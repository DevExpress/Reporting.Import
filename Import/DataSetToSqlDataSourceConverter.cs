using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.DataAccess.Wizard.Services;
using DevExpress.Utils;
using DevExpress.Xpo.DB;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Native;

namespace DevExpress.XtraReports.Import {
    public class DataSetToSqlDataSourceConverter {
        const string tableAdapterManagerName = "TableAdapterManagerName";

        public bool UseManagedOracleDataProvider { get; set; } = true;

        readonly DataSet dataSet;
        readonly ITypeResolutionService typeResolver;
        readonly string adaptersNamespace;
        readonly Dictionary<string, IDbCommand> commands = new Dictionary<string, IDbCommand>();
        readonly IDbConnection connection;

        string ExpectedTableAdapterManagerTypeName => $"{adaptersNamespace}.{tableAdapterManagerName}";

        public DataSetToSqlDataSourceConverter(DataSet dataSet, ITypeResolutionService typeResolver = null) {
            Guard.ArgumentNotNull(dataSet, nameof(dataSet));
            this.dataSet = dataSet;
            this.typeResolver = typeResolver;
            adaptersNamespace = $"{dataSet.GetType().Namespace}.{dataSet.GetType().Name}TableAdapters";
            Dictionary<string, object> tableAdapters = ResolveTableAdapters();
            foreach(KeyValuePair<string, object> tableAdapter in tableAdapters) {
                var dataAdapter = GetDataAdapter(tableAdapter.Value);
                var command = dataAdapter?.SelectCommand ?? GetCommand(tableAdapter.Value);
                if(command != null)
                    commands[tableAdapter.Key] = command;
            }
            var tableAdapterManager = ResolveTableAdapterManager();
            connection = GetConnectionPropertyValueFromInstance(tableAdapterManager);
            if(connection == null) {
                foreach(var tableAdapter in tableAdapters.Values) {
                    if(connection != null)
                        break;
                    connection = GetConnectionPropertyValueFromInstance(tableAdapter);
                }
            }
        }

        public DataSetToSqlDataSourceConverter(DataSet dataSet, IDbConnection connection, Dictionary<string, IDbCommand> commands, ITypeResolutionService typeResolver = null) {
            Guard.ArgumentNotNull(dataSet, nameof(dataSet));
            Guard.ArgumentNotNull(connection, nameof(connection));
            Guard.ArgumentNotNull(commands, nameof(commands));
            this.dataSet = dataSet;
            this.connection = connection;
            this.commands = commands;
            this.typeResolver = typeResolver;
        }

        public bool CanConvert() {
            return connection != null && commands.Any();
        }

        public SqlDataSource Convert() {
            if(!CanConvert())
                return null;
            return ConvertCore();
        }

        SqlDataSource ConvertCore() {
            var sqlDataSource = new SqlDataSource();
            foreach(var command in commands) {
                AddQuery(command.Key, command.Value, sqlDataSource);
            }
            ApplyConnectionParameters(connection, sqlDataSource, UseManagedOracleDataProvider);
            ConvertSchema(dataSet, sqlDataSource);
            return sqlDataSource;
        }

        #region connection parameters
        void ApplyConnectionParameters(IDbConnection connection, SqlDataSource sqlDataSource, bool useManagedOracleDataProvider) {
            var connectionParameters = GetConnectionParameters(connection, useManagedOracleDataProvider);
            sqlDataSource.ConnectionName = "Connection";
            sqlDataSource.ConnectionParameters = connectionParameters;
            sqlDataSource.ConnectionOptions.CommandTimeout = connection.ConnectionTimeout;
        }

        static DataConnectionParametersBase GetConnectionParameters(IDbConnection connection, bool useManagedOracleDataProvider) {
            string connectionString = GetConnectionString(connection, useManagedOracleDataProvider);
            return DataAccessConnectionHelper.ParseConnectionParameters(connectionString);
        }

        static string GetConnectionString(IDbConnection connection, bool useManagedOracleDataProvider) {
            var connectionString =
                GetConnectionString(connection as SqlConnection) ??
                GetConnectionString(connection as OleDbConnection, useManagedOracleDataProvider) ??
                connection.ConnectionString;
            return connectionString;
        }

        static string GetConnectionString(SqlConnection connection) {
            if(connection == null)
                return null;
            var connectionString = connection.ConnectionString;
            return $"XpoProvider={MSSqlConnectionProvider.XpoProviderTypeString};{connectionString}";
        }

        public static void ConvertSchema(DataSet dataSet, SqlDataSource dataSource) {
            foreach(DataTable table in dataSet.Tables) {
                var name = table.TableName;
                var columns = table.Columns.Cast<DataColumn>().ToDictionary(x => x.ColumnName, x => x.DataType);
                FieldListResultSchemaProvider.AppendViewToSchema(dataSource, name, columns);
            }
            ConvertRelations(dataSet.Relations, dataSource);
            FieldListResultSchemaProvider.UpdateResultSchemaRelations(dataSource);
        }

        static void ConvertRelations(DataRelationCollection relations, SqlDataSource dataSource) {
            List<MasterDetailInfo> infos = new List<MasterDetailInfo>();
            foreach(DataRelation relation in relations) {
                var parentTable = relation.ParentTable.TableName;
                var childTable = relation.ChildTable.TableName;
                var columns = relation.ParentColumns.Zip(relation.ChildColumns, (DataColumn p, DataColumn c) => new RelationColumnInfo(p.ColumnName, c.ColumnName));
                infos.Add(new MasterDetailInfo(parentTable, childTable, columns) { Name = relation.RelationName });
            }
            dataSource.Relations.AddRange(infos.ToArray());
            FieldListResultSchemaProvider.UpdateResultSchemaRelations(dataSource);
        }

        static void AddQuery(string queryName, IDbCommand command, SqlDataSource sqlDataSource) {
            var query = CreateSqlQuery(queryName, command);
            if(query != null)
                sqlDataSource.Queries.Add(query);
        }

        public static SqlQuery CreateSqlQuery(string name, IDbCommand command) {
            SqlQuery query;
            var commandText = command.CommandText.Trim();
            switch(command.CommandType) {
                case CommandType.Text:
                    string message = null;
                    var validator = new CustomQueryValidator();
                    if(!validator.Validate(null, commandText, ref message)) {
                        Tracer.TraceWarning(NativeSR.TraceSource, $"Can't process '{name}' data set: {message}");
                        return null;
                    }
                    query = new CustomSqlQuery(name, commandText);
                    break;
                case CommandType.StoredProcedure:
                    query = new StoredProcQuery(name, commandText);
                    break;
                case CommandType.TableDirect:
                    query = SelectQueryFluentBuilder.AddTable(commandText).SelectAllColumns().Build(name);
                    break;
                default:
                    throw new InvalidOperationException();
            }
            foreach(IDbDataParameter parameter in command.Parameters) {
                if(parameter.Direction == ParameterDirection.Output || parameter.Direction == ParameterDirection.ReturnValue)
                    continue;
                var columnType = ConnectionProviderSql.GetColumnType(parameter.DbType, true);
                var parameterName = parameter.ParameterName;
                if(parameterName.StartsWith("@"))
                    parameterName = parameterName.Substring(1);
                var type = DBColumn.GetType(columnType);
                var queryParameter = new QueryParameter(parameterName, type, parameter.Value);
                query.Parameters.Add(queryParameter);
            }
            return query;
        }
        #endregion

        #region resolve types / objects
        object ResolveTableAdapterManager() {
            var type = ResolveType(ExpectedTableAdapterManagerTypeName, typeResolver);
            var instance = ResolveInstance(type);
            if(instance == null) {
                Tracer.TraceWarning(NativeSR.TraceSource, $"Can't resolve table adapter manager for the '{dataSet.DataSetName}' data set.");
            }
            return instance;
        }

        Dictionary<string, object> ResolveTableAdapters() {
            Dictionary<string, object> tableAdapters = new Dictionary<string, object>();
            foreach(DataTable table in dataSet.Tables) {
                var adapter = ResolveTableAdapter(table.TableName);
                if(adapter != null) {
                    tableAdapters[table.TableName] = adapter;
                }
            }
            return tableAdapters;
        }

        object ResolveTableAdapter(string tableName) {
            var typeName = $"{adaptersNamespace}.{tableName}TableAdapter";
            var type = ResolveType(typeName, typeResolver);
            var tableAdapter = ResolveInstance(type);
            if(tableAdapter == null) {
                Tracer.TraceWarning(NativeSR.TraceSource, $"Can't resolve table adapter for the '{tableName}' data table.");
            }
            return tableAdapter;
        }

        object ResolveInstance(Type type) {
            object instance = null;
            if(type != null) {
                try {
                    instance = Activator.CreateInstance(type);
                } catch (MemberAccessException) { }
            }
            return instance;
        }

        public static Type ResolveType(string typeName, ITypeResolutionService typeResolver, Predicate<Type> condition = null) {
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

        IDbDataAdapter GetDataAdapter(object tableAdapter) {
            var sqlAdapterProperty = tableAdapter.GetType().GetProperty("Adapter", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if(sqlAdapterProperty != null) {
                return sqlAdapterProperty.GetValue(tableAdapter) as IDbDataAdapter;
            }
            return null;
        }

        static IDbConnection GetConnectionPropertyValueFromInstance(object instance) {
            if(instance == null)
                return null;
            var property = instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(x => typeof(IDbConnection).IsAssignableFrom(x.PropertyType));
            return property?.GetValue(instance) as IDbConnection;
        }

        IDbCommand GetCommand(object tableAdapter) {
            var commandCollectionProperty = tableAdapter.GetType().GetProperty("CommandCollection", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if(commandCollectionProperty != null) {
                var commands = (commandCollectionProperty.GetValue(tableAdapter) as IEnumerable)?.Cast<IDbCommand>();
                return commands?.FirstOrDefault();
            }
            return null;
        }
        #endregion

        //static void Clear(SqlDataSource target) {
        //    target.ConnectionParameters = null;
        //    target.Queries.Clear();
        //    target.Relations.Clear();
        //    target.RebuildResultSchema();
        //    FieldListResultSchemaProvider.UpdateResultSchemaRelations(target);
        //}
    }
}
