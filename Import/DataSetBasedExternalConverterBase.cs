#region DEMO_REMOVE

using System.Data.Common;
using System.Data.OleDb;
using DevExpress.XtraReports.UI;

namespace DevExpress.XtraReports.Import {
    public abstract class DataSetBasedExternalConverterBase : ExternalConverterBase {
        protected DataSetBasedExternalConverterBase() {
        }

        protected void AssignDataAdapter(DataAdapter dataAdapter) {
            TargetReport.DataAdapter = dataAdapter;
        }

        static protected OleDbDataAdapter CreateOleDBDataAdapter(OleDbCommand selectCommand, string tableName) {
            OleDbDataAdapter dataAdapter = new OleDbDataAdapter(selectCommand);
            CreateTableMapping(dataAdapter, tableName);
            return dataAdapter;
        }

        protected static void CreateTableMapping(DataAdapter dataAdapter, string tableName) {
            DataTableMapping tableMapping = new DataTableMapping("Table", tableName);
            dataAdapter.TableMappings.Add(tableMapping);
        }

        protected void BindDataToControl(XRControl control, string property, string dataMember) {
            BindDataToControl(control, property, dataMember, string.Empty);
        }

        protected static string ParseTableName(string sql) {
            string from = " FROM ";
            sql = sql.ToUpper();
            sql = sql.Replace((char)0xD, ' ');
            sql = sql.Replace((char)0xA, ' ');
            int fromIndex = sql.IndexOf(from);
            if(fromIndex < 0)
                return string.Empty;
            int count = sql.Length;
            fromIndex += from.Length;
            for(int i = fromIndex; i < count; i++) {
                if(!IsWhiteSpace(sql[i]))
                    break;
                fromIndex++;
            }

            if(fromIndex >= count)
                return string.Empty;

            int len = 0;
            for(int i = fromIndex; i < count; i++) {
                if(IsWhiteSpace(sql[i]) || sql[i] == ',')
                    break;
                len++;
            }

            return sql.Substring(fromIndex, len);
        }

        static bool IsWhiteSpace(char ch) {
            return char.IsWhiteSpace(ch) || ch == '\n' || ch == '\r';
        }
    }
}

#endregion