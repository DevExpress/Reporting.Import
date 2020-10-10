using System;
using System.ComponentModel.Design;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Native;
using CommandType = System.Data.CommandType;
using DataSetConversionState = DevExpress.XtraReports.Import.ReportingServices.DataSources.DataSourceConverter.DataSetConversionState;

namespace DevExpress.XtraReports.Import.ReportingServices.DataSources {
    class ExternalDataSetConverter {
        readonly XNamespace rdns = XNamespace.Get("http://schemas.microsoft.com/SQLServer/reporting/reportdesigner");
        readonly IDataSourceConverter dataSourceConverter;
        readonly ITypeResolutionService typeResolver;
        readonly IDesignerHost designerHost;
        readonly string currentProjectRootNamespace;

        public ExternalDataSetConverter(IDataSourceConverter dataSourceConverter, ITypeResolutionService typeResolver = null, IDesignerHost designerHost = null, string currentProjectRootNamespace = null) {
            this.dataSourceConverter = dataSourceConverter;
            this.typeResolver = typeResolver;
            this.designerHost = designerHost;
            this.currentProjectRootNamespace = currentProjectRootNamespace;
        }

        public void Convert(XElement dataSetInfo, DataSetConversionState state) {
            var dataSetName = dataSetInfo.Element(rdns + "DataSetName").Value;
            if(string.IsNullOrEmpty(dataSetName))
                return;
            var dataSetSchemaPath = dataSetInfo.Element(rdns + "SchemaPath")?.Value ?? dataSetName;
            var tableName = dataSetInfo.Element(rdns + "TableName").Value;
            var dataSetType = ResolveDataSetType(dataSetName);
            DataSet dataSet = null;
            if(dataSetType != null) {
                try {
                    dataSet = Activator.CreateInstance(dataSetType) as DataSet;
                } catch { }
            }

            if(!TryConvertWithConverter(dataSet, state)) {
                var schemaDocument = dataSourceConverter.GetSharedResourceDocument(dataSetSchemaPath, "xsd");
                if(schemaDocument != null) {
                    Tracer.TraceInformation(NativeSR.TraceSource, new FormattableString(Messages.DataSource_CannotResolveDataSet_Format, dataSetName));
                    ProcessSchema(schemaDocument, state);
                } else {
                    Tracer.TraceInformation(NativeSR.TraceSource, new FormattableString(Messages.DataSource_CannotProcessDataSet_Format, dataSetName));
                    return;
                }
            }
            state.Query = state.DataSource.Queries.SingleOrDefault(x => x.Name == tableName);
        }

        bool TryConvertWithConverter(DataSet dataSet, DataSetConversionState state) {
            if(dataSet == null)
                return false;
            var sqlDataSourceConverter = new DataSetToSqlDataSourceConverter(dataSet) { UseManagedOracleDataProvider = dataSourceConverter.UseManagedOracleDataProvider };
            if(!sqlDataSourceConverter.CanConvert())
                return false;
            var result = sqlDataSourceConverter.Convert();
            var serializedResult = result.SaveToXml();
            state.DataSource.LoadFromXml(serializedResult);
            return true;
        }

        Type ResolveDataSetType(string typeName) {
            Func<string, Type> resolveType = name => ResolveType(name, typeof(DataSet).IsAssignableFrom);
            Type type = resolveType(typeName);
            if(type == null && currentProjectRootNamespace != null) {
                string rootNamespaceTypeName = $"{currentProjectRootNamespace}.{typeName}";
                type = resolveType(rootNamespaceTypeName);
            }
            if(type == null && designerHost != null) {
                string documentNamespaceTypeName = $"{designerHost.RootComponent.GetType().Namespace}.{typeName}";
                type = resolveType(documentNamespaceTypeName);
            }
            return type;
        }

        void ProcessSchema(XDocument document, DataSetConversionState state) {
            var root = document.Root;
            var ns = root.GetDefaultNamespace();
            var dataSourceElement = root.Descendants().SingleOrDefault(x => x.Name.LocalName == "DataSource");
            var dsNs = dataSourceElement.GetDefaultNamespace();

            var connectionElement = dataSourceElement.Descendants(dsNs + "Connection").Single();
            var parameterPrefix = connectionElement?.Attribute("ParameterPrefix").Value ?? "@";
            ProcessConnection(connectionElement, state);

            foreach(XElement table in root.Descendants(dsNs + "TableAdapter")) {
                var queryName = table.Attribute("Name").Value;
                var command = table.Descendants(dsNs + "SelectCommand").SingleOrDefault()?.Element(dsNs + "DbCommand");
                if(command == null)
                    return;
                var commandType = command.Attribute("CommandType").Value;
                var commandText = command.Element(dsNs + "CommandText").Value;
                var commandTypeEnum = (CommandType)Enum.Parse(typeof(CommandType), commandType);
                var sqlCommand = new SqlCommand(commandText) { CommandType = commandTypeEnum };
                var parameters = command.Descendants(dsNs + "Parameter").Where(x => x.Attribute("Direction").Value == "Input");
                foreach(XElement parameter in parameters) {
                    var name = parameter.Attribute("ParameterName").Value;
                    if(name.StartsWith(parameterPrefix))
                        name = name.Substring(1);
                    var type = GetDBType(parameter.Attribute("DbType").Value);
                    sqlCommand.Parameters.Add(new SqlParameter() { ParameterName = name, DbType = type });
                }
                var query = DataSetToSqlDataSourceConverter.CreateSqlQuery(queryName, sqlCommand);
                if(query == null)
                    return;

                state.DataSource.Queries.Add(query);

                foreach(XElement mapping in table.Descendants(dsNs + "Mapping")) {
                    var source = mapping.Attribute("SourceColumn").Value;
                    var result = mapping.Attribute("DataSetColumn").Value;
                    if(source != result)
                        Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.DataSource_MissingMapping_Format, source, result));
                }
            }
            var dataSet = new DataSet();
            var schemaString = document.ToString();
            using(var stream = new MemoryStream(Encoding.Default.GetBytes(schemaString))) {
                dataSet.ReadXmlSchema(stream);
            }
            DataSetToSqlDataSourceConverter.ConvertSchema(dataSet, state.DataSource);
        }

        Type ResolveType(string typeName, Predicate<Type> condition = null) {
            Type type = null;
            if(typeResolver != null) {
                type = typeResolver.GetType(typeName, false);
            }
            if(type == null) {
                condition = condition ?? (t => true);
                foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                    if(type != null)
                        break;
                    Type foundedType = assembly.GetType(typeName, false);
                    if(foundedType != null && condition(foundedType))
                        type = foundedType;
                }
            }
            return type;
        }

        static void ProcessConnection(XElement connectionElement, DataSetConversionState state) {
            if(connectionElement == null) {
                Tracer.TraceWarning(NativeSR.TraceSource, Messages.DataSource_ExternalConnection_NotSupported);
            }
            var parameterPrefix = connectionElement?.Attribute("ParameterPrefix").Value ?? "@";
            if(connectionElement.Attribute("IsAppSettingsProperty").Value == "true") {
                var appSettingsObject = connectionElement.Attribute("AppSettingsObjectName").Value;
                var appSettingsPropertyName = connectionElement.Attribute("AppSettingsPropertyName").Value;
                var propertyReferencePatterns = connectionElement.Attribute("PropertyReference").Value.Split('.');
                state.DataSource.ConnectionName = $"{propertyReferencePatterns[1]}.Properties.{appSettingsObject}.{appSettingsPropertyName}";
            } else {
                state.DataSource.ConnectionParameters = new CustomStringConnectionParameters(connectionElement.Attribute("ConnectionStringObject").Value);
            }
        }

        static DbType GetDBType(string dbTypeString) {
            DbType dbType;
            if(!Enum.TryParse(dbTypeString, out dbType))
                dbType = DbType.Object;
            return dbType;
        }
    }
}
