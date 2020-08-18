using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using DevExpress.Data.Browsing;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Native.Sql;
using DevExpress.DataAccess.Native.Sql.ConnectionProviders;
using DevExpress.DataAccess.Sql;
using DevExpress.Xpo.DB;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Native;
using DevExpress.XtraReports.Import.ReportingServices.Expressions;

namespace DevExpress.XtraReports.Import.ReportingServices.DataSources {
    interface IDataSourceConverter {
        XDocument GetSharedResourceDocument(string resourceName, string extension);
        bool UseManagedOracleDataProvider { get; }
    }

    class DataSourceConverter : IDataSourceConverter {
        public class DataSetConversionState {
            public string DataSetName { get; set; }
            public SqlDataSource DataSource { get; set; }
            public List<QueryParameter> Parameters => Query.Parameters;
            public SqlQuery Query { get; set; }
        }

        readonly XNamespace rdns = XNamespace.Get("http://schemas.microsoft.com/SQLServer/reporting/reportdesigner");
        readonly static string[] msSqlProviders = { "SQLNCLI", "SQLNCLI10", "SQLNCLI11" };
        readonly IReportingServicesConverter converter;
        readonly ITypeResolutionService typeResolver;
        readonly IDesignerHost designerHost;
        readonly string currentProjectRootNamespace;
        readonly Dictionary<string, SqlDataSource> dataSources = new Dictionary<string, SqlDataSource>();
        readonly Dictionary<string, string> dataSourceReferenceToDataSourceNameMap = new Dictionary<string, string>();
        readonly Dictionary<string, DataPair> dataSetToDataSourceMap = new Dictionary<string, DataPair>();
        bool useManagedOracleDataProvider = false;

        public DataSourceConverter(IReportingServicesConverter converter, ITypeResolutionService typeResolver = null, IDesignerHost designerHost = null, string currentProjectRootNamespace = null) {
            this.converter = converter;
            this.typeResolver = typeResolver;
            this.designerHost = designerHost;
            this.currentProjectRootNamespace = currentProjectRootNamespace;
        }

        public Dictionary<string, DataPair> Convert(XElement dataSourcesElement, XElement dataSetsElement) {
            ProcessDataSources(dataSourcesElement);
            ProcessDataSets(dataSetsElement);
            dataSources.Clear();
            dataSourceReferenceToDataSourceNameMap.Clear();
            return dataSetToDataSourceMap;
        }

        #region DataSource

        void ProcessDataSources(XElement dataSources) {
            if(dataSources == null)
                return;
            foreach(XElement dataSource in dataSources.Elements(dataSources.GetDefaultNamespace() + "DataSource"))
                ProcessDataSource(dataSource);
        }

        void ProcessDataSource(XElement dataSourceElement) {
            var ns = dataSourceElement.GetDefaultNamespace();
            var dataSourceName = dataSourceElement.Attribute("Name").Value;
            var dataSourceReference = dataSourceElement.Element(ns + "DataSourceReference");
            if(dataSourceReference != null) {
                string storedDataSourceName;
                if(dataSourceReferenceToDataSourceNameMap.TryGetValue(dataSourceReference.Value, out storedDataSourceName))
                    return;
                else dataSourceReferenceToDataSourceNameMap[dataSourceReference.Value] = dataSourceName;
            }
            var sqlDataSource = GetOrAddDataSource(dataSourceElement.Attribute("Name")?.Value);
            ReportingServicesConverter.IterateElements(dataSourceElement, (e, name) => {
                switch(name) {
                    case "DataSourceReference":
                        ProcessDataSourceReference(e, sqlDataSource);
                        break;
                    case "ConnectionProperties":
                        if(dataSourceReference == null)
                            ProcessConnectionProperties(e, sqlDataSource);
                        break;
                    case "SecurityType":  //handled
                    case "DataSourceID":
                        break;
                    default:
                        throw new NotSupportedException($"DataSource element '{name}' not supported.");
                }
            });
        }
        #endregion

        #region DataSet
        void ProcessDataSets(XElement dataSets) {
            if(dataSets == null)
                return;
            var ns = dataSets.GetDefaultNamespace();
            useManagedOracleDataProvider = IsVersionHigherThan2016(ns.NamespaceName);
            var orderedDataSets = dataSets.Elements(ns + "DataSet")
                .OrderBy(x => x.Element(ns + "SharedDataSet") != null);
            foreach(XElement dataSet in orderedDataSets)
                ProcessDataSet(dataSet);
        }
        void ProcessDataSet(XElement dataSetElement) {
            var dataSetName = dataSetElement.Attribute("Name").Value;
            var conversionState = new DataSetConversionState() { DataSetName = dataSetName };
            ProcessDataSetCore(dataSetElement, conversionState, dataSetName);

            var queryName = conversionState.Query?.Name ?? conversionState.DataSetName;
            dataSetToDataSourceMap[dataSetName] = new DataPair(conversionState.DataSource, queryName);
        }

        void ProcessDataSetCore(XElement dataSetElement, DataSetConversionState conversionState, string componentName) {
            var ns = dataSetElement.GetDefaultNamespace();
            var dataSetInfo = dataSetElement.Element(rdns + "DataSetInfo");
            var sharedDataSet = dataSetElement.Element(ns + "SharedDataSet");
            if(sharedDataSet != null) {
                ProcessSharedDataSet(sharedDataSet, conversionState, componentName);
            } else {
                var dataSourceName = dataSetElement.Descendants(ns + "DataSourceName").SingleOrDefault()?.Value;
                if(dataSourceName == null) {
                    var dataSourceReference = dataSetElement.Descendants(ns + "DataSourceReference").SingleOrDefault();
                    if(dataSourceReference != null) {
                        if(!dataSourceReferenceToDataSourceNameMap.TryGetValue(dataSourceReference.Value, out dataSourceName))
                        dataSourceReferenceToDataSourceNameMap[dataSourceReference.Value] = dataSourceName = dataSourceReference.Value;
                    }
                }
                conversionState.DataSource = GetOrAddDataSource(dataSourceName);
                if(dataSetInfo != null) {
                    new ExternalDataSetConverter(this, typeResolver, designerHost, currentProjectRootNamespace).Convert(dataSetInfo, conversionState);
                    return;
                }
            }

            var fieldsElement = dataSetElement.Element(ns + "Fields");
            if(fieldsElement != null)
                ProcessDataSetFields(fieldsElement, conversionState);

            var queryElement = dataSetElement.Element(ns + "Query");
            if(queryElement != null && !conversionState.DataSource.Queries.Any(x => x.Name == conversionState.DataSetName))
                ProcessQuery(queryElement, conversionState, componentName);
        }

        void ProcessSharedDataSet(XElement sharedDataSet, DataSetConversionState state, string componentName) {
            var dataSetName = sharedDataSet.Element(sharedDataSet.GetDefaultNamespace() + "SharedDataSetReference").Value;
            var document = GetSharedResourceDocument(converter.ReportFolder, dataSetName, "rsd");
            if(document == null) {
                Tracer.TraceWarning(NativeSR.TraceSource, $"Unable to find the '{dataSetName}' shared data set.");
                return;
            }
            var ns = document.Root.GetDefaultNamespace();
            var dataSourceReference = document.Root.Descendants(ns + "DataSourceReference").Single();
            var referenceName = dataSourceReference.Value;
            string dataSourceName;
            if(dataSourceReferenceToDataSourceNameMap.TryGetValue(referenceName, out dataSourceName)) {
                state.DataSource = GetOrAddDataSource(dataSourceName);
            } else {
                state.DataSource = GetOrAddDataSource(referenceName);
                dataSourceReferenceToDataSourceNameMap[referenceName] = state.DataSource.Name;
                ProcessDataSourceReference(dataSourceReference, state.DataSource);
            }
            var dataSet = document.Root.Element(ns + "DataSet");
            ProcessDataSetCore(dataSet, state, componentName);
        }

        void ProcessDataSetFields(XElement fields, DataSetConversionState state) {
            var ns = fields.GetDefaultNamespace();
            if(FieldListResultSchemaProvider.ResultSetContains(state.DataSource, state.DataSetName))
                return;
            var columns = fields.Elements(ns + "Field")
                .Select(field => {
                    var name = field.Attribute("Name").Value;
                    var fieldName = field.Element(ns + "DataField");
                    var typeName = field.Element(rdns + "TypeName")?.Value;
                    var type = string.IsNullOrEmpty(typeName) ? typeof(object) : Type.GetType(typeName);
                    if(type == null) {
                        Tracer.TraceWarning(NativeSR.TraceSource, $"Can't resolve the '{typeName}' column type.");
                        type = typeof(object);
                    }
                    return new { Name = name, Type = type };
                }).ToDictionary(x => x.Name, x => x.Type);
            FieldListResultSchemaProvider.AppendViewToSchema(state.DataSource, state.DataSetName, columns);
        }
        #endregion

        #region Connection properties
        void ProcessDataSourceReference(XElement reference, SqlDataSource dataSource) {
            var document = GetSharedResourceDocument(converter.ReportFolder, reference.Value, "rds");
            if(document == null) {
                Tracer.TraceWarning(NativeSR.TraceSource, $"Unable to find the '{reference.Value}' data source reference.");
            } else {
                var ns = document.Root.GetDefaultNamespace();
                ProcessConnectionProperties(document.Root.Element(ns + "ConnectionProperties"), dataSource);
            }
        }

        void ProcessConnectionProperties(XElement connection, SqlDataSource dataSource) {
            var ns = connection.GetDefaultNamespace();
            var dataProvider = connection.Element(ns + "DataProvider")?.Value ?? connection.Element(ns + "Extension")?.Value;
            var connectionString = connection.Element(ns + "ConnectString").Value;
            var integratedSecurity = connection.Element(ns + "IntegratedSecurity")?.Value == "true";
            if(ReportingServicesConverter.IsExpression(connectionString))
                Tracer.TraceWarning(NativeSR.TraceSource, Messages.DataSource_ConnectionParameters_Expression_NotSupported);
            else
                dataSource.ConnectionParameters = CreateConnectionParameters(dataProvider, connectionString, integratedSecurity, is2016OrHigher: IsVersionHigherThan2016(ns.NamespaceName));
        }

        static DataConnectionParametersBase CreateConnectionParameters(string dataProvider, string connectionString, bool integratedSecurity, bool is2016OrHigher) {
            string xpoProvider = null;
            switch(dataProvider) {
                case "System.Data.DataSet":    //handled
                    return null;
                case "XML":
                    return new XmlFileConnectionParameters(connectionString);
                case "SQL":
                case "SQLAZURE":
                    xpoProvider = MSSqlConnectionProvider.XpoProviderTypeString;
                    if(integratedSecurity)
                        connectionString += ";integrated security=SSPI";
                    break;
                case "ORACLE":
                    xpoProvider = is2016OrHigher ? ODPManagedConnectionProvider.XpoProviderTypeString : OracleConnectionProvider.XpoProviderTypeString;
                    break;
                case "TERADATA":
                    xpoProvider = DataAccessTeradataConnectionProvider.XpoProviderTypeString;
                    break;
                case "OLEDB":
                    var builder = new OleDbConnectionStringBuilder(connectionString);
                    object oleDBProvider;
                    if(builder.TryGetValue("Provider", out oleDBProvider)) {
                        const string StrJetOledb4 = "Microsoft.Jet.OLEDB.4.0";
                        const string StrAceOledb12 = "Microsoft.ACE.OLEDB.12.0";
                        if(oleDBProvider.Equals(StrJetOledb4) || oleDBProvider.Equals(StrAceOledb12))
                            xpoProvider = AccessConnectionProvider.XpoProviderTypeString;
                        else if(msSqlProviders.Contains(oleDBProvider))
                            xpoProvider = MSSqlConnectionProvider.XpoProviderTypeString;
                        else if(oleDBProvider.Equals("MSDAORA"))
                            xpoProvider = is2016OrHigher ? ODPManagedConnectionProvider.XpoProviderTypeString : OracleConnectionProvider.XpoProviderTypeString;
                        else
                            Tracer.TraceWarning(NativeSR.TraceSource, $"The '{oleDBProvider}' Ole DB provider is not supported.");
                    } else {
                        Tracer.TraceWarning(NativeSR.TraceSource, $"Unable to determine Ole DB provider type by connection string.");
                    }
                    break;
                default:
                    Tracer.TraceWarning(NativeSR.TraceSource, $"The '{dataProvider}' data provider is not supported");
                    break;
            }
            if(string.IsNullOrEmpty(xpoProvider))
                return new CustomStringConnectionParameters(connectionString);

            connectionString = $"XpoProvider={xpoProvider};{connectionString}";
            var parameters = DataAccessConnectionHelper.ParseConnectionParameters(connectionString);
            return parameters;
        }
        #endregion

        #region Query
        void ProcessQuery(XElement queryElement, DataSetConversionState state, string componentName) {
            var ns = queryElement.GetDefaultNamespace();
            var commandType = queryElement.Element(ns + "CommandType")?.Value ?? "Text";
            var commandText = queryElement.Element(ns + "CommandText").Value;
            if(commandText == "/* Local Query */")
                return;
            state.Query = GetOrAddQuery(state, commandType, commandText);
            if(state.Query == null)
                return;
            ProcessDataSetParameters(queryElement.Element(ns + "DataSetParameters"), state);
            ProcessQueryParameters(queryElement.Element(ns + "QueryParameters"), state, componentName);

        }

        void ProcessQueryParameters(XElement queryParameters, DataSetConversionState state, string componentName) {
            if(queryParameters == null)
                return;
            var ns = queryParameters.GetDefaultNamespace();
            IEnumerable<XElement> queryParameterElements = queryParameters.Elements(ns + "QueryParameter");
            foreach(var queryParameterElement in queryParameterElements) {
                QueryParameter queryParameter = GetOrAddQueryParameter(queryParameterElement, state);
                var value = queryParameterElement.Element(ns + "Value").Value;

                ExpressionParserResult expressionResult;
                queryParameter.Value = converter.TryGetExpression(value, $"{componentName}.{queryParameter.Name}", out expressionResult)
                    ? expressionResult.ToDataAccessExpression()
                    : (object)value;
            }
        }

        void ProcessDataSetParameters(XElement dataSetParameters, DataSetConversionState state) {
            if(dataSetParameters == null)
                return;
            var ns = dataSetParameters.GetDefaultNamespace();
            foreach(XElement dataSetParameter in dataSetParameters.Elements(ns + "DataSetParameter"))
                ProcessDataSetParameter(dataSetParameter, state);
        }

        void ProcessDataSetParameter(XElement dataSetParameter, DataSetConversionState state) {
            var parameter = GetOrAddQueryParameter(dataSetParameter, state);
            ReportingServicesConverter.IterateElements(dataSetParameter, (e, name) => {
                switch(name) {
                    case "DbType":
                        parameter.Type = Type.GetType(e.Value);
                        break;
                    case "ReadOnly":                  // not supported 
                    case "Nullable":                  // not supported
                    case "OmitFromQuery":             // not supported
                    case "UserDefined":               // not supported
                        break;
                    case "DefaultValue":               // todo
                        break;
                    default:
                        throw new NotSupportedException();
                }
            });
        }

        static SqlQuery GetOrAddQuery(DataSetConversionState state, string commandType, string commandText) {
            var query = state.DataSource.Queries.SingleOrDefault(x => x.Name == state.DataSetName);
            if(query == null) {
                var commandTypeEnum = (CommandType)Enum.Parse(typeof(CommandType), commandType);
                var command = new SqlCommand() { CommandType = commandTypeEnum, CommandText = commandText };
                query = DataSetToSqlDataSourceConverter.CreateSqlQuery(state.DataSetName, command);
                if(query != null) {
                    state.DataSource.Queries.Add(query);
                }
            }
            return query;
        }

        static QueryParameter GetOrAddQueryParameter(XElement parameter, DataSetConversionState state) {
            var name = parameter.Attribute("Name").Value;
            if(name.StartsWith("@"))
                name = name.Substring(1);
            var queryParameter = state.Parameters.SingleOrDefault(x => x.Name == name);
            if(queryParameter == null) {
                queryParameter = new QueryParameter() { Name = name };
                state.Parameters.Add(queryParameter);
            }
            return queryParameter;
        }

        #endregion

        SqlDataSource GetOrAddDataSource(string name) {
            SqlDataSource dataSource;
            if(!dataSources.TryGetValue(name, out dataSource)) {
                dataSource = new SqlDataSource() { ConnectionName = "Connection" };
                converter.SetControlName(dataSource, name);
                dataSources.Add(dataSource.Name, dataSource);
            }
            return dataSource;
        }

        static string GetSharedResourcePath(string basePath, string resourcePath, string extension) {
            if(!Path.IsPathRooted(resourcePath))
                resourcePath = Path.Combine(basePath, resourcePath);
            if(!Path.HasExtension(resourcePath))
                resourcePath = $"{resourcePath}.{extension}";
            return resourcePath;
        }

        static XDocument GetSharedResourceDocument(string basePath, string resourcePath, string extension) {
            resourcePath = GetSharedResourcePath(basePath, resourcePath, extension);
            if(!File.Exists(resourcePath))
                return null;
            using(FileStream stream = File.OpenRead(resourcePath))
                return Utils.SafeXml.CreateXDocument(stream);
        }

        static bool IsVersionHigherThan2016(string defaultNamespace) {
            var match = Regex.Match(defaultNamespace, "(?<key>http://schemas.microsoft.com/sqlserver/reporting/)([0-9]{4})/([0-9]{2})(?<key>/reportdefinition)");
            if(match.Success) {
                var versionText = match.Groups[1].Value;
                int version;
                return int.TryParse(versionText, out version) && version >= 2016;
            }
            return false;
        }

        #region IDataSourceConverter
        XDocument IDataSourceConverter.GetSharedResourceDocument(string resourceName, string extension) {
            return GetSharedResourceDocument(converter.ReportFolder, resourceName, extension);
        }
        bool IDataSourceConverter.UseManagedOracleDataProvider => useManagedOracleDataProvider;
        #endregion
    }
}
