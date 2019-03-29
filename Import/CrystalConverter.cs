#region DEMO_REMOVE

#if Crystal
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.ReportAppServer.CommonObjectModel;
using CrystalDecisions.Shared;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Native;
using DevExpress.XtraReports.Parameters;
using DevExpress.XtraReports.UI;
using Cr = CrystalDecisions.CrystalReports.Engine;

namespace DevExpress.XtraReports.Import {
    public class CrystalConverter : ExternalConverterBase {
        #region inner types
        class CrystalConnectionInfoEqualityComparer : IEqualityComparer<ConnectionInfo> {
            public static readonly CrystalConnectionInfoEqualityComparer Instance = new CrystalConnectionInfoEqualityComparer();

            public bool Equals(ConnectionInfo x, ConnectionInfo y) {
                return NameValuePairs2Equals(x.Attributes.Collection, y.Attributes.Collection);
            }

            public int GetHashCode(ConnectionInfo obj) {
                return 0;
            }

            static bool NameValuePairs2Equals(NameValuePairs2 x, NameValuePairs2 y) {
                if(x.Count != y.Count) {
                    return false;
                }
                foreach(NameValuePair2 xItem in x) {
                    var value = y.Lookup(xItem.Name);
                    var xDbConnectionAttributes = xItem.Value as DbConnectionAttributes;
                    var yDbConnectionAttributes = value as DbConnectionAttributes;

                    if(xDbConnectionAttributes != null && yDbConnectionAttributes != null) {
                        if(NameValuePairs2Equals(xDbConnectionAttributes.Collection, yDbConnectionAttributes.Collection)) {
                            continue;
                        } else {
                            return false;
                        }
                    }
                    if(value == null || !value.Equals(xItem.Value)) {
                        return false;
                    }
                }
                return true;
            }
        }

        static class CrystalTypeConverter {
            public static TextAlignment GetTextAlignment(Alignment align) {
                switch(align) {
                    case Alignment.HorizontalCenterAlign:
                        return TextAlignment.TopCenter;
                    case Alignment.Justified:
                        return TextAlignment.TopJustify;
                    case Alignment.RightAlign:
                        return TextAlignment.TopRight;
                    default:
                        return TextAlignment.TopLeft;
                }
            }

            public static PaperKind GetPaperKind(CrystalDecisions.Shared.PaperSize paperSize) {
                switch(paperSize) {
                    case CrystalDecisions.Shared.PaperSize.Paper10x14:
                        return PaperKind.Standard10x14;
                    case CrystalDecisions.Shared.PaperSize.Paper11x17:
                        return PaperKind.Standard11x17;
                    case CrystalDecisions.Shared.PaperSize.PaperA4:
                        return PaperKind.A4;
                    case CrystalDecisions.Shared.PaperSize.PaperA4Small:
                        return PaperKind.A4Small;
                    case CrystalDecisions.Shared.PaperSize.PaperA5:
                        return PaperKind.A5;
                    case CrystalDecisions.Shared.PaperSize.PaperB4:
                        return PaperKind.B4;
                    case CrystalDecisions.Shared.PaperSize.PaperB5:
                        return PaperKind.B5;
                    case CrystalDecisions.Shared.PaperSize.PaperCsheet:
                        return PaperKind.CSheet;
                    case CrystalDecisions.Shared.PaperSize.PaperDsheet:
                        return PaperKind.DSheet;
                    case CrystalDecisions.Shared.PaperSize.PaperEnvelope10:
                        return PaperKind.Number10Envelope;
                    case CrystalDecisions.Shared.PaperSize.PaperEnvelope11:
                        return PaperKind.Number11Envelope;
                    case CrystalDecisions.Shared.PaperSize.PaperEnvelope12:
                        return PaperKind.Number12Envelope;
                    case CrystalDecisions.Shared.PaperSize.PaperEnvelope14:
                        return PaperKind.Number14Envelope;
                    case CrystalDecisions.Shared.PaperSize.PaperEnvelope9:
                        return PaperKind.Number9Envelope;
                    case CrystalDecisions.Shared.PaperSize.PaperEnvelopeB4:
                        return PaperKind.B4Envelope;
                    case CrystalDecisions.Shared.PaperSize.PaperEnvelopeB5:
                        return PaperKind.B5Envelope;
                    case CrystalDecisions.Shared.PaperSize.PaperEnvelopeB6:
                        return PaperKind.B6Envelope;
                    case CrystalDecisions.Shared.PaperSize.PaperEnvelopeC3:
                        return PaperKind.C3Envelope;
                    case CrystalDecisions.Shared.PaperSize.PaperEnvelopeC4:
                        return PaperKind.C4Envelope;
                    case CrystalDecisions.Shared.PaperSize.PaperEnvelopeC5:
                        return PaperKind.C5Envelope;
                    case CrystalDecisions.Shared.PaperSize.PaperEnvelopeC6:
                        return PaperKind.C6Envelope;
                    case CrystalDecisions.Shared.PaperSize.PaperEnvelopeC65:
                        return PaperKind.C65Envelope;
                    case CrystalDecisions.Shared.PaperSize.PaperEnvelopeDL:
                        return PaperKind.DLEnvelope;
                    case CrystalDecisions.Shared.PaperSize.PaperEnvelopeItaly:
                        return PaperKind.ItalyEnvelope;
                    case CrystalDecisions.Shared.PaperSize.PaperEnvelopeMonarch:
                        return PaperKind.MonarchEnvelope;
                    case CrystalDecisions.Shared.PaperSize.PaperEsheet:
                        return PaperKind.ESheet;
                    case CrystalDecisions.Shared.PaperSize.PaperExecutive:
                        return PaperKind.Executive;
                    case CrystalDecisions.Shared.PaperSize.PaperFanfoldLegalGerman:
                        return PaperKind.GermanLegalFanfold;
                    case CrystalDecisions.Shared.PaperSize.PaperFanfoldStdGerman:
                        return PaperKind.GermanStandardFanfold;
                    case CrystalDecisions.Shared.PaperSize.PaperFanfoldUS:
                        return PaperKind.USStandardFanfold;
                    case CrystalDecisions.Shared.PaperSize.PaperFolio:
                        return PaperKind.Folio;
                    case CrystalDecisions.Shared.PaperSize.PaperLedger:
                        return PaperKind.Ledger;
                    case CrystalDecisions.Shared.PaperSize.PaperLegal:
                        return PaperKind.Legal;
                    case CrystalDecisions.Shared.PaperSize.PaperLetter:
                        return PaperKind.Letter;
                    case CrystalDecisions.Shared.PaperSize.PaperLetterSmall:
                        return PaperKind.LetterSmall;
                    case CrystalDecisions.Shared.PaperSize.PaperNote:
                        return PaperKind.Note;
                    case CrystalDecisions.Shared.PaperSize.PaperQuarto:
                        return PaperKind.Quarto;
                    case CrystalDecisions.Shared.PaperSize.PaperStatement:
                        return PaperKind.Statement;
                    case CrystalDecisions.Shared.PaperSize.PaperTabloid:
                        return PaperKind.Tabloid;
                    default:
                        return PaperKind.Custom;
                };
            }

            public static Type GetBandTypeByAreaSectionKind(AreaSectionKind areaSectionKind, bool excludeGroups = false) {
                switch(areaSectionKind) {
                    case AreaSectionKind.ReportHeader:
                        return typeof(ReportHeaderBand);
                    case AreaSectionKind.PageHeader:
                        return typeof(PageHeaderBand);
                    case AreaSectionKind.GroupHeader:
                        return excludeGroups ? null : typeof(GroupHeaderBand);
                    case AreaSectionKind.Detail:
                        return typeof(DetailBand);
                    case AreaSectionKind.GroupFooter:
                        return excludeGroups ? null : typeof(GroupFooterBand);
                    case AreaSectionKind.PageFooter:
                        return typeof(PageFooterBand);
                    case AreaSectionKind.ReportFooter:
                        return typeof(ReportFooterBand);
                    default:
                        return null;
                };
            }

            public static DashStyle GetLineStyle(LineStyle style) {
                switch(style) {
                    case LineStyle.DotLine:
                        return DashStyle.DashDot;
                    case LineStyle.DashLine:
                        return DashStyle.Dash;
                    default:
                        return DashStyle.Solid;
                }
            }

            public static bool TryGetXRParameterType(ParameterValueKind kind, out Type type) {
                switch(kind) {
                    case ParameterValueKind.BooleanParameter:
                        type = typeof(bool);
                        break;
                    case ParameterValueKind.CurrencyParameter:
                        type = typeof(decimal);
                        break;
                    case ParameterValueKind.DateParameter:
                    case ParameterValueKind.DateTimeParameter:
                    case ParameterValueKind.TimeParameter:
                        type = typeof(DateTime);
                        break;
                    case ParameterValueKind.NumberParameter:
                        type = typeof(double);
                        break;
                    case ParameterValueKind.StringParameter:
                        type = typeof(string);
                        break;
                    default:
                        type = null;
                        break;
                }
                return type != null;
            }

            public static bool TryConvertSummaryFunc(SummaryOperation crystalSummaryOperation, bool useRunningSummary, out SummaryFunc summaryFunc) {
                summaryFunc = default(SummaryFunc);
                switch(crystalSummaryOperation) {
                    case SummaryOperation.Average:
                        summaryFunc = SummaryFunc.Avg;
                        break;
                    case SummaryOperation.Count:
                        summaryFunc = SummaryFunc.Count;
                        break;
                    case SummaryOperation.DistinctCount:
                        summaryFunc = SummaryFunc.DCount;
                        break;
                    case SummaryOperation.Maximum:
                        summaryFunc = SummaryFunc.Max;
                        break;
                    case SummaryOperation.Median:
                        summaryFunc = SummaryFunc.Median;
                        break;
                    case SummaryOperation.Minimum:
                        summaryFunc = SummaryFunc.Min;
                        break;
                    case SummaryOperation.PopStandardDeviation:
                        summaryFunc = SummaryFunc.StdDevP;
                        break;
                    case SummaryOperation.PopVariance:
                        summaryFunc = SummaryFunc.VarP;
                        break;
                    case SummaryOperation.SampleVariance:
                        summaryFunc = SummaryFunc.Var;
                        break;
                    case SummaryOperation.SampleStandardDeviation:
                        summaryFunc = SummaryFunc.StdDev;
                        break;
                    case SummaryOperation.Sum:
                        summaryFunc = useRunningSummary ? SummaryFunc.RunningSum : SummaryFunc.Sum;
                        break;
                    default:
                        return false;
                }
                return true;
            }

            public static FieldType ConvertToFieldType(FieldValueType crystalFieldValueType) {
                switch(crystalFieldValueType) {
                    case FieldValueType.BitmapField:
                    case FieldValueType.BlobField:
                    case FieldValueType.IconField:
                    case FieldValueType.PictureField:
                        return FieldType.Byte;
                    case FieldValueType.BooleanField:
                        return FieldType.Boolean;
                    case FieldValueType.CurrencyField:
                        return FieldType.Decimal;
                    case FieldValueType.DateField:
                    case FieldValueType.DateTimeField:
                    case FieldValueType.TimeField:
                        return FieldType.DateTime;
                    case FieldValueType.Int16sField:
                    case FieldValueType.Int16uField:
                        return FieldType.Int16;
                    case FieldValueType.Int32sField:
                    case FieldValueType.Int32uField:
                    case FieldValueType.NumberField:
                        return FieldType.Int32;
                    case FieldValueType.Int8sField:
                    case FieldValueType.Int8uField:
                        return FieldType.Byte;
                    case FieldValueType.PersistentMemoField:
                    case FieldValueType.StringField:
                    case FieldValueType.TransientMemoField:
                        return FieldType.String;
                    case FieldValueType.UnknownField:
                    case FieldValueType.SameAsInputField:
                    case FieldValueType.ChartField:
                    case FieldValueType.OleField:
                    default:
                        return FieldType.None;
                }
            }

            public static Type ConvertToType(FieldValueType crystalFieldValueType) {
                switch(crystalFieldValueType) {
                    case FieldValueType.BitmapField:
                    case FieldValueType.BlobField:
                    case FieldValueType.IconField:
                    case FieldValueType.PictureField:
                        return typeof(byte[]);
                    case FieldValueType.BooleanField:
                        return typeof(bool);
                    case FieldValueType.CurrencyField:
                        return typeof(decimal);
                    case FieldValueType.DateField:
                    case FieldValueType.DateTimeField:
                    case FieldValueType.TimeField:
                        return typeof(DateTime);
                    case FieldValueType.Int16sField:
                        return typeof(short);
                    case FieldValueType.Int16uField:
                        return typeof(ushort);
                    case FieldValueType.Int32sField:
                        return typeof(int);
                    case FieldValueType.Int32uField:
                    case FieldValueType.NumberField:
                        return typeof(uint);
                    case FieldValueType.Int8sField:
                        return typeof(sbyte);
                    case FieldValueType.Int8uField:
                        return typeof(byte);
                    case FieldValueType.PersistentMemoField:
                    case FieldValueType.StringField:
                    case FieldValueType.TransientMemoField:
                        return typeof(string);
                    case FieldValueType.UnknownField:
                    case FieldValueType.SameAsInputField:
                    case FieldValueType.ChartField:
                    case FieldValueType.OleField:
                    default:
                        return typeof(string);
                }
            }
        }

        static class Interop {
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct PictureObjectStruct {
                public short StructSize;
                public short Unknown2;
                public IntPtr Handle;
                public int Length;
            }

            [DllImport("crpe32.dll", EntryPoint = "PEOpenPrintJob")]
            public static extern short OpenPrintJob(string name);

            [DllImport("crpe32.dll", EntryPoint = "PEGetNthObjectInSection")]
            public static extern short GetNthObjectInSection(short reportId, short sectionCode, short elementIndex);

            [DllImport("crpe32.dll", CharSet = CharSet.Unicode, EntryPoint = "PEGetPictureObjectInfo")]
            public static extern bool GetPictureObjectInfo(short reportId, int pictureId, ref PictureObjectStruct info);
        }

        static class PrintHelper {
            public static byte[] GetPrintedBmp(string crystalReportFileName, CrystalDecisions.ReportAppServer.ReportDefModel.ISCRReportObject pictureObject, short indexOnSection) {
                short reportId = Interop.OpenPrintJob(crystalReportFileName);
                short nthObjectInSection = Interop.GetNthObjectInSection(reportId, (short)pictureObject.SectionCode, indexOnSection);
                var pictureObjectStruct = new Interop.PictureObjectStruct {
                    StructSize = (short)Marshal.SizeOf<Interop.PictureObjectStruct>()
                };
                Interop.GetPictureObjectInfo(reportId, nthObjectInSection, ref pictureObjectStruct);
                if(pictureObjectStruct.Handle == IntPtr.Zero)
                    return null;

                const int bmpHeaderSize = 14;
                byte[] bmpArray = new byte[bmpHeaderSize + pictureObjectStruct.Length];
                Marshal.Copy(pictureObjectStruct.Handle, bmpArray, bmpHeaderSize, pictureObjectStruct.Length);
                PopulateBmpData(bmpArray, bmpHeaderSize);
                return bmpArray;
            }

            static void PopulateBmpData(byte[] bmpData, int bmpHeaderSize) {
                int index = 0;
                bmpData[index++] = (byte)'B';
                bmpData[index++] = (byte)'M';
                index = WriteInt32(bmpData, index, bmpData.Length);
                index += 4;
                int size = BitConverter.ToInt32(bmpData, bmpHeaderSize);
                WriteInt32(bmpData, index, bmpHeaderSize + size);
            }

            public static int WriteInt32(byte[] data, int offset, int value) {
                data[offset] = (byte)value;
                data[offset + 1] = (byte)(value >> 8);
                data[offset + 2] = (byte)(value >> 16);
                data[offset + 3] = (byte)(value >> 24);
                return offset + 4;
            }
        }

        static class Messages {
            internal static string
                Information_Started = "SAP Crystal Reports to XtraReports Converter - Conversion started.",
                Information_Completed = "SAP Crystal Reports to XtraReports Converter - Conversion finished.",
                Information_CompletedWithError = "SAP Crystal Reports to XtraReports Converter - Conversion finished with error.",
                Error_Generic_Format = "Cannot complete conversion because of the following exception: '{0}'.",
                Warning_Connection_OleDbProviderNotSupported_Format = "Connection - Cannot generate a report data source because the following OLE DB provider is not supported: '{0}'.",
                Warning_Connection_OleDbProviderNotSpecified = "Connection - Cannot generate a report data source because the OLE DB connection has no provider.",
                Warning_Connection_DatabaseDllNotSupported_Format = "Connection - Cannot generate a report data source because the following database DLL is not supported: '{0}'.",
                Warning_Formula_NotSupported = "Formula - Cannot properly convert formulas. Calculated fields with the unspecified Expression are created instead.",
                Warning_FieldObject_Kind_NotSupported_Format = "FieldObject named '{0}' with definition kind '{1}' is not currently supported.",
                Warning_FieldObject_SpecialVarType_NotSupported_Format = "FieldObject named '{0}' with special field '{1}' is not currently supported.",
                Warning_SummaryOperation_NotSupported_Format = "FieldObject named '{0}' with summary operation '{1}' is not currently supported.",
                Warning_PictureContent_NotSupported_Format = "PictureObject named '{0}' cannot be properly converted with the image content due to API limitations of SAP Crystal Reports.",
                Warning_Subreport_NotSupported_Format = "SubreportObject named '{0}' has not been converted.",
                Warning_Chart_NotSupported_Format = "ChartObject named '{0}' has not been converted.",
                Warning_CrossTab_NotSupported_Format = "CrossTabObject named '{0}' has not been converted.",
                Warning_ReportObjectKind_NotSupported_Format = "Report Object named '{0}' with kind '{1}' has not been converted.",
                Warning_ReportParameter_NotSupported_Format = "Report Parameter named '{0}' with type '{1}' is not supported.",
                Warning_Binding_CanNotResolve_Format = "Report Object named '{0}' has unsupported binding '{1}'.",
                Control_Untranslated = "Untranslated",
                Control_CantResolveBinding = "Cannot resolve binding"
            ;
        }
        #endregion

        public static bool DefaultSelectFullTableSchema { get; set; }
        public static string FilterString { get { return "Crystal Reports (*.rpt)|*.rpt"; } }
        static readonly char[] DotSeparator = { '.' };

        static CrystalConverter() {
            SubscribeAssemblyResolveEventStatic("CrystalDecisions.");
        }

        public CrystalConverter() {
        }

        public bool? SelectFullTableSchema { get; set; }

        readonly Dictionary<string, Band> bandsByOriginalNames = new Dictionary<string, Band>();
        readonly Dictionary<string, Parameter> parametersByOriginalNames = new Dictionary<string, Parameter>();
        readonly Dictionary<string, SqlDataSource> sqlDataSourcesByTableNames = new Dictionary<string, SqlDataSource>();
        readonly Dictionary<Tuple<SqlDataSource, string>, SelectQuery> singleQueriesByDataSourcesAndTables = new Dictionary<Tuple<SqlDataSource, string>, SelectQuery>();
        readonly List<Action> onEnd = new List<Action>();

        ConversionResult Convert(ReportDocument crystalReport) {
            PerformConversion(() => ConvertInternalCore(crystalReport));
            return new ConversionResult(TargetReport, SourceReport);
        }

        protected override void ConvertInternal(string fileName) {
            ReportDocument crystalReport = null;
            try {
                crystalReport = new ReportDocument();
                crystalReport.Load(fileName);
            } catch {
                Tracer.TraceInformation(NativeSR.TraceSource, Messages.Information_CompletedWithError);
                throw;
            }
            ConvertInternalCore(crystalReport);
        }

        void ConvertInternalCore(ReportDocument crystalReport) {
            Tracer.TraceInformation(NativeSR.TraceSource, Messages.Information_Started);
            bool hasException = false;
            try {
                bandsByOriginalNames.Clear();
                parametersByOriginalNames.Clear();
                sqlDataSourcesByTableNames.Clear();
                onEnd.Clear();

                TargetReport.ReportUnit = ReportUnit.HundredthsOfAnInch;

                GenerateDataSources(crystalReport.Database);
                ConvertCalcFields(crystalReport.DataDefinition.FormulaFields);
                if(!crystalReport.IsSubreport)
                    ConvertReportParameters(crystalReport.ParameterFields, crystalReport.Database.Tables);
                ConvertGroups(crystalReport);
                if(!crystalReport.IsSubreport)
                    ConvertPageSettings(crystalReport.PrintOptions);
                ISupportInitialize supportInit = TargetReport;
                supportInit.BeginInit();
                try {
                    string filePath = !crystalReport.IsSubreport ? crystalReport.FilePath : string.Empty;
                    ConvertAreas(crystalReport.ReportDefinition.Areas, filePath);
                    onEnd.ForEach(x => x());
                } finally {
                    supportInit.EndInit();
                }
            } catch {
                hasException = true;
                throw;
            } finally {
                Tracer.TraceInformation(NativeSR.TraceSource, hasException ? Messages.Information_CompletedWithError : Messages.Information_Completed);
            }
        }

        void GenerateDataSources(Database crystalDatabase) {
            SqlDataSource[] sqlDataSources = crystalDatabase.Tables
                .Cast<Cr.Table>()
                .GroupBy(x => x.LogOnInfo.ConnectionInfo, CrystalConnectionInfoEqualityComparer.Instance)
                .Select(x => GenerateSqlDataSource(x.Key, x, crystalDatabase.Links))
                .Where(x => x != null)
                .ToArray();
            TargetReport.ComponentStorage.AddRange(sqlDataSources);
            SqlDataSource firstDataSource = sqlDataSources.FirstOrDefault();
            TargetReport.DataSource = firstDataSource;
            SqlQuery firstQuery = firstDataSource != null ? firstDataSource.Queries.FirstOrDefault() : null;
            TargetReport.DataMember = firstQuery != null ? firstQuery.Name : string.Empty;
        }

        SqlDataSource GenerateSqlDataSource(ConnectionInfo connection, IEnumerable<Cr.Table> crystalTables, TableLinks crystalTableLinks) {
            DataConnectionParametersBase dataConnectionParameters = GenerateConnectionParameters(connection);
            if(dataConnectionParameters == null)
                return null;
            var sqlDataSource = new SqlDataSource(dataConnectionParameters);
            NamingMapper.GenerateAndAssignXRControlName(sqlDataSource, connection.DatabaseName);
            var schemaProvider = new FieldListResultSchemaProvider(sqlDataSource.Name);
            Tuple<SelectQuery, Dictionary<string, Type>> queryDefinition = GenerateQueryDefinition(crystalTables, crystalTableLinks);
            SelectQuery selectQuery = queryDefinition.Item1;
            sqlDataSource.Queries.Add(selectQuery);
            foreach(string tableName in crystalTables.Select(x => x.Location)) {
                sqlDataSourcesByTableNames[tableName] = sqlDataSource;
                if(selectQuery.Tables.Count == 1) {
                    singleQueriesByDataSourcesAndTables[Tuple.Create(sqlDataSource, tableName)] = selectQuery;
                }
            }
            schemaProvider.AddView(selectQuery.Name, queryDefinition.Item2);
            sqlDataSource.ResultSchemaSerializable = schemaProvider.GetResultSchemaSerializable();
            return sqlDataSource;
        }

        Tuple<SelectQuery, Dictionary<string, Type>> GenerateQueryDefinition(IEnumerable<Cr.Table> crystalTables, TableLinks crystalTableLinks) {
            Cr.Table singleCrystalTable = crystalTables.Count() == 1 ? crystalTables.First() : null;
            var result = new SelectQuery();
            string originalName = singleCrystalTable != null ? singleCrystalTable.Location : null;
            result.Name = NamingMapper.GenerateSafeName<SelectQuery>(originalName);
            var columns = new Dictionary<string, Type>();
            var displayColumnNames = new HashSet<string>();
            foreach(Cr.Table crystalTable in crystalTables) {
                DataAccess.Sql.Table table = result.AddTable(crystalTable.Name);
                foreach(DatabaseFieldDefinition crystalTableColumn in crystalTable.Fields) {
                    if(!SelectFullTableSchema.GetValueOrDefault(DefaultSelectFullTableSchema) && crystalTableColumn.UseCount == 0)
                        continue;
                    string dispayColumnName = crystalTableColumn.Name;
                    string crystalColumnAlias = null;

                    if(displayColumnNames.Contains(dispayColumnName)) {
                        crystalColumnAlias = crystalTableColumn.TableName + "_" + crystalTableColumn.Name;
                        dispayColumnName = crystalColumnAlias;
                    }
                    result.SelectColumn(table, crystalTableColumn.Name, crystalColumnAlias);
                    columns.Add(dispayColumnName, CrystalTypeConverter.ConvertToType(crystalTableColumn.ValueType));
                    displayColumnNames.Add(dispayColumnName);
                }
            }
            if(singleCrystalTable == null) {
                foreach(TableLink crystalTableLink in crystalTableLinks) {
                    DataAccess.Sql.Table sourceTable = result.Tables.FirstOrDefault(x => x.Name == crystalTableLink.SourceTable.Location);
                    DataAccess.Sql.Table destinationTable = result.Tables.FirstOrDefault(x => x.Name == crystalTableLink.DestinationTable.Location);
                    if(sourceTable != null && destinationTable != null) {
                        RelationColumnInfo[] relationColumnInfo = GenerateRelationColumnInfo(crystalTableLink.DestinationFields, crystalTableLink.SourceFields);
                        result.AddRelation(sourceTable, destinationTable, relationColumnInfo);
                    }
                }
            }
            return Tuple.Create(result, columns);
        }

        static RelationColumnInfo[] GenerateRelationColumnInfo(DatabaseFieldDefinitions destinationFields, DatabaseFieldDefinitions sourceFields) {
            var result = sourceFields
                .Cast<DatabaseFieldDefinition>()
                .Zip(destinationFields.Cast<DatabaseFieldDefinition>(), (s, d) => new RelationColumnInfo(s.Name, d.Name))
                .ToArray();
            return result;
        }

        static DataConnectionParametersBase GenerateConnectionParameters(ConnectionInfo connectionInfo) {
            NameValuePairs2 attributes = connectionInfo.Attributes.Collection;
            var databaseDll = attributes.Lookup(DbConnectionAttributes.CONNINFO_DATABASE_DLL) as string;
            if(databaseDll == DbConnectionAttributes.DATABASE_DLL_CRDB_ADO || databaseDll == DbConnectionAttributes.DATABASE_DLL_CRDB_ADOPLUS) {
                NameValuePairs2 logonProperties = connectionInfo.LogonProperties;
                var oledbProvider = logonProperties.Lookup("Provider") as string;
                if(oledbProvider == "SQLOLEDB") {
                    return new MsSqlConnectionParameters(connectionInfo.ServerName, connectionInfo.DatabaseName, connectionInfo.UserID, connectionInfo.Password, connectionInfo.IntegratedSecurity ? MsSqlAuthorizationType.Windows : MsSqlAuthorizationType.SqlServer);
                } else if(oledbProvider == "MSDAORA") {
                    return new OracleConnectionParameters(connectionInfo.ServerName, connectionInfo.UserID, connectionInfo.Password);
                } else if(oledbProvider == null) {
                    string filePath = logonProperties.Lookup("File Path") as string ?? logonProperties.Lookup("File Path ") as string;
                    if(string.Equals(Path.GetExtension(filePath), ".xml", StringComparison.OrdinalIgnoreCase)) {
                        return new XmlFileConnectionParameters(filePath);
                    } else {
                        Tracer.TraceWarning(NativeSR.TraceSource, Messages.Warning_Connection_OleDbProviderNotSpecified);
                    }
                } else {
                    Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.Warning_Connection_OleDbProviderNotSupported_Format, oledbProvider));
                }
            } else if(databaseDll == "crdb_oracle.dll") {
                return new OracleConnectionParameters(connectionInfo.ServerName, connectionInfo.UserID, connectionInfo.Password);
            } else {
                Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.Warning_Connection_DatabaseDllNotSupported_Format, databaseDll));
            }
            return null;
        }

        void ConvertCalcFields(FormulaFieldDefinitions crystalfields) {
            if(crystalfields.Count > 0) {
                Tracer.TraceWarning(NativeSR.TraceSource, Messages.Warning_Formula_NotSupported);
            }
            object dataSource = TargetReport.DataSource;
            List<SqlQuery> allQueries = TargetReport.ComponentStorage.OfType<SqlDataSource>().SelectMany(x => x.Queries).ToList();
            string dataMember = allQueries.Count == 1 ? allQueries[0].Name : string.Empty;
            CalculatedField[] calcFields = crystalfields.Cast<FormulaFieldDefinition>()
                .Select(x => new CalculatedField {
                    Name = x.Name,
                    DataSource = dataSource,
                    DataMember = dataMember,
                    Expression = !string.IsNullOrEmpty(x.Text) ? string.Format("Iif(True, '', '{0}')", x.Text.Replace("'", "''")) : string.Empty,
                    FieldType = CrystalTypeConverter.ConvertToFieldType(x.ValueType)
                })
                .Select(x => NamingMapper.GenerateAndAssignXRControlName(x, x.Name))
                .ToArray();
            TargetReport.CalculatedFields.AddRange(calcFields);
        }

        void ConvertReportParameters(ParameterFields parameterFields, Tables crystalDatabaseTables) {
            IEnumerable<KeyValuePair<string, Parameter>> parametersByName = parameterFields
                .Cast<ParameterField>()
                .Where(x => x.ReportParameterType == CrystalDecisions.Shared.ParameterType.ReportParameter)
                .Select(x => ConvertParameter(x, crystalDatabaseTables));
            foreach(KeyValuePair<string, Parameter> pair in parametersByName) {
                parametersByOriginalNames[pair.Key] = pair.Value;
                TargetReport.Parameters.Add(pair.Value);
            }
        }

        void ConvertGroups(ReportDocument crystalReport) {
            //ClearGroupBands();
            var filePath = !crystalReport.IsSubreport
                ? crystalReport.FilePath
                : string.Empty;
            ConvertGroupHeaderBands(crystalReport.DataDefinition, GenerateGroupHeaderBands(crystalReport.ReportDefinition.Areas, filePath));
            ConvertGroupFooterBands(crystalReport.ReportDefinition.Areas, filePath);
        }

        void ConvertGroupHeaderBands(DataDefinition dataDefinition, IList<GroupHeaderBand> reportBands) {
            Groups groups = dataDefinition.Groups;
            if(reportBands.Count == 0 || groups.Count != reportBands.Count) {
                return;
            }
            for(int i = reportBands.Count - 1; i >= 0; i--) {
                GroupField groupField = new GroupField(groups[i].ConditionField.Name);
                foreach(SortField field in dataDefinition.SortFields) {
                    if(field.Field.Name == groups[i].ConditionField.Name) {
                        groupField.SortOrder = field.SortDirection == SortDirection.AscendingOrder
                            ? XRColumnSortOrder.Ascending
                            : XRColumnSortOrder.Descending;
                        break;
                    }
                }
                reportBands[i].GroupFields.Add(groupField);
                TargetReport.Bands.Add(reportBands[i]);
            }
        }

        List<GroupHeaderBand> GenerateGroupHeaderBands(Areas crystalAreas, string crystalReportFileName) {
            var bands = crystalAreas
                .Cast<Area>()
                .Where(x => CrystalTypeConverter.GetBandTypeByAreaSectionKind(x.Kind) == typeof(GroupHeaderBand))
                .Select(x => ConvertAreaSections(GetOrCreateBandByType<GroupHeaderBand>(), x, crystalReportFileName))
                .ToList();
            return bands;
        }

        void ConvertGroupFooterBands(Areas crystalAreas, string crystalReportFileName) {
            var groupFooters = crystalAreas
                .Cast<Area>()
                .Where(x => CrystalTypeConverter.GetBandTypeByAreaSectionKind(x.Kind) == typeof(GroupFooterBand))
                .Select(x => ConvertAreaSections(GetOrCreateBandByType<GroupFooterBand>(), x, crystalReportFileName))
                .ToArray();
            TargetReport.Bands.AddRange(groupFooters);
        }

        static Margins MakePageMargins(PageMargins pageMargins) {
            return new Margins(
                TwipsToHOI(pageMargins.leftMargin),
                TwipsToHOI(pageMargins.rightMargin),
                TwipsToHOI(pageMargins.topMargin),
                TwipsToHOI(pageMargins.bottomMargin));
        }

        void ConvertPageSettings(PrintOptions printOptions) {
            TargetReport.PaperKind = CrystalTypeConverter.GetPaperKind(printOptions.PaperSize);
            if(TargetReport.PaperKind == PaperKind.Custom) {
                TargetReport.PageHeight = TwipsToHOI(printOptions.PageContentHeight) + TwipsToHOI(printOptions.PageMargins.topMargin) + TwipsToHOI(printOptions.PageMargins.bottomMargin);
                TargetReport.PageWidth = TwipsToHOI(printOptions.PageContentWidth) + TwipsToHOI(printOptions.PageMargins.leftMargin) + TwipsToHOI(printOptions.PageMargins.rightMargin);
            }
            TargetReport.Margins = MakePageMargins(printOptions.PageMargins);
            TargetReport.Landscape = printOptions.PaperOrientation == PaperOrientation.Landscape;
        }

        void ConvertAreas(Areas crystalAreas, string crystalReportFileName) {
            var pairs = crystalAreas
                .Cast<Area>()
                .Select(x => new { Area = x, BandType = CrystalTypeConverter.GetBandTypeByAreaSectionKind(x.Kind, excludeGroups: true) })
                .Where(x => x.BandType != null);
            foreach(var pair in pairs) {
                ConvertAreaSections(GetOrCreateBandByType(pair.BandType), pair.Area, crystalReportFileName);
            }
        }

        static int TwipsToHOI(int val) {
            return 100 * val / 1440;
        }

        static float TwipsToHOIF(int val) {
            return 100f * val / 1440f;
        }

        void ConfigureCrossBandControl(XRCrossBandControl control, DrawingObject drawingObject) {
            control.StartPointF = new PointF(TwipsToHOIF(drawingObject.Left), TwipsToHOIF(drawingObject.Top));
            onEnd.Add(() => {
                Band endBand;
                if(bandsByOriginalNames.TryGetValue(drawingObject.EndSectionName, out endBand)) {
                    control.EndBand = endBand;
                }
                control.EndPointF = new PointF(control.EndPointF.X, TwipsToHOIF(drawingObject.Bottom));
            });
        }

        void ConvertSectionControls(Band band, Section section, string crystalReportFileName) {
            for(short i = 0; i < section.ReportObjects.Count; i++) {
                ReportObject reportObject = section.ReportObjects[i];
                XRControl control = null;
                switch(reportObject.Kind) {
                    case ReportObjectKind.FieldObject:
                        control = ConvertFieldObject(TargetReport, band, (FieldObject)reportObject);
                        break;
                    case ReportObjectKind.FieldHeadingObject:
                    case ReportObjectKind.TextObject:
                        control = ConvertTextObject(band, (TextObject)reportObject);
                        break;
                    case ReportObjectKind.BlobFieldObject:
                        control = ConvertBlobFieldObject(TargetReport, band, (BlobFieldObject)reportObject);
                        break;
                    case ReportObjectKind.PictureObject:
                        control = CreatePictureObject(band, (PictureObject)reportObject, crystalReportFileName, i);
                        break;
                    case ReportObjectKind.LineObject:
                        control = ConvertLine(section, band, (LineObject)reportObject);
                        break;
                    case ReportObjectKind.BoxObject:
                        control = ConvertBoxObject(band, (BoxObject)reportObject);
                        break;
                    case ReportObjectKind.SubreportObject:
                        Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.Warning_Subreport_NotSupported_Format, reportObject.Name));
                        control = ConvertSubreport(band, (SubreportObject)reportObject);
                        break;
                    case ReportObjectKind.ChartObject:
                        Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.Warning_Chart_NotSupported_Format, reportObject.Name));
                        control = CreateXRControl<XRChart>(band, reportObject.Name);
                        break;
                    case ReportObjectKind.CrossTabObject:
                        Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.Warning_CrossTab_NotSupported_Format, reportObject.Name));
                        control = CreateXRControl<XRPivotGrid>(band, reportObject.Name);
                        break;
                    default:
                        Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.Warning_ReportObjectKind_NotSupported_Format, reportObject.Name, reportObject.Kind));
                        break;
                }
                if(control == null) {
                    control = CreateXRControl<XRLabel>(band, reportObject.Name);
                    control.Text = Messages.Control_Untranslated;
                }
                ConvertReportObjectProperties(control, reportObject);
            }
        }

        TBand ConvertAreaSections<TBand>(TBand band, Area area, string crystalReportFileName)
            where TBand : Band {
            bandsByOriginalNames[area.Name] = band;
            if(area.Sections.Count == 1) {
                ConfigureBand(band, area.Sections[0], crystalReportFileName);
            } else {
                band.Height = 0;
                foreach(Section section in area.Sections) {
                    SubBand subBand = CreateXRControl<SubBand>(band, section.Name);
                    ConfigureBand(subBand, section, crystalReportFileName);
                }
            }
            return band;
        }

        void ConfigureBand(Band band, Section section, string crystalReportFileName) {
            bandsByOriginalNames[section.Name] = band;
            band.Height = TwipsToHOI(section.Height);
            if(section.SectionFormat.EnableNewPageAfter)
                band.PageBreak = PageBreak.AfterBand;
            if(section.SectionFormat.EnableNewPageBefore)
                band.PageBreak = PageBreak.BeforeBand;
            if(section.SectionFormat.EnableKeepTogether)
                band.KeepTogether = true;
            band.Visible = !section.SectionFormat.EnableSuppress;
            ConvertSectionControls(band, section, crystalReportFileName);
        }

        XRControl ConvertFieldObject(XtraReport targetReport, Band band, FieldObject fieldObject) {
            XRControl control = null;
            FieldDefinition fieldDefinition = fieldObject.DataSource;
            if(fieldDefinition != null) {
                switch(fieldDefinition.Kind) {
                    case FieldKind.DatabaseField:
                        control = CreateXRControl<XRLabel>(band, fieldObject.Name);
                        AddDataBinding(targetReport, control, "Text", fieldDefinition.FormulaName);
                        break;
                    case FieldKind.ParameterField:
                        control = CreateXRControl<XRLabel>(band, fieldObject.Name);
                        AddParameterBinding(control, fieldDefinition);
                        break;
                    case FieldKind.SpecialVarField:
                        var specialVarFieldDefinition = (SpecialVarFieldDefinition)fieldObject.DataSource;
                        switch(specialVarFieldDefinition.SpecialVarType) {
                            case SpecialVarType.PageNumber:
                                control = CreateXRControl<XRPageInfo>(band, fieldObject.Name, x => x.PageInfo = PageInfo.Number);
                                break;
                            case SpecialVarType.PageNofM:
                                control = CreateXRControl<XRPageInfo>(band, fieldObject.Name, x => x.PageInfo = PageInfo.NumberOfTotal);
                                break;
                            case SpecialVarType.TotalPageCount:
                                control = CreateXRControl<XRPageInfo>(band, fieldObject.Name, x => x.PageInfo = PageInfo.Total);
                                break;
                            case SpecialVarType.ModificationDate:
                            case SpecialVarType.PrintDate:
                            case SpecialVarType.DataDate:
                                control = CreateXRControl<XRPageInfo>(band, fieldObject.Name, x => x.PageInfo = PageInfo.DateTime);
                                break;
                            case SpecialVarType.DataTime:
                            case SpecialVarType.PrintTime:
                            case SpecialVarType.ModificationTime:
                                control = CreateXRControl<XRPageInfo>(band, fieldObject.Name, x => {
                                    x.PageInfo = PageInfo.DateTime;
                                    x.Format = "{0:t}";
                                });
                                break;
                            default:
                                Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.Warning_FieldObject_SpecialVarType_NotSupported_Format, fieldObject.Name, specialVarFieldDefinition.SpecialVarType));
                                break;
                        }
                        break;
                    case FieldKind.GroupNameField:
                        control = CreateXRControl<XRLabel>(band, fieldObject.Name);
                        AddDataBinding(targetReport, control, "Text", fieldDefinition.FormulaName, isGrouping: true);
                        break;
                    case FieldKind.SummaryField:
                        var summaryDefinition = (SummaryFieldDefinition)fieldObject.DataSource;
                        control = GenerateSummarySafe(targetReport, band, fieldObject.Name, summaryDefinition.Operation, false, SummaryRunning.Report, summaryDefinition.SummarizedField.FormulaName);
                        break;
                    case FieldKind.RunningTotalField:
                        var runningTotalField = (RunningTotalFieldDefinition)fieldObject.DataSource;
                        SummaryRunning summaryRunning = runningTotalField.EvaluationConditionType == RunningTotalCondition.OnChangeOfGroup
                            ? SummaryRunning.Group
                            : SummaryRunning.Report;
                        control = GenerateSummarySafe(targetReport, band, fieldObject.Name, runningTotalField.Operation, true, summaryRunning, runningTotalField.SummarizedField.FormulaName);
                        break;
                    case FieldKind.FormulaField:
                        control = CreateXRControl<XRLabel>(band, fieldDefinition.Name);
                        AddDataBinding(targetReport, control, "Text", fieldObject.DataSource.FormulaName, isFormula: true);
                        break;
                    default:
                        Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.Warning_FieldObject_Kind_NotSupported_Format, fieldObject.Name, fieldDefinition.Kind));
                        break;
                }
            }

            if(control == null) {
                control = CreateXRControl<XRLabel>(band, fieldObject.Name);
                control.Text = Messages.Control_Untranslated;
            }

            //fieldObject.FieldFormat;
            control.ForeColor = fieldObject.Color;
            control.Font = (Font)fieldObject.Font.Clone();
            return control;
        }

        XRControl ConvertBlobFieldObject(XtraReport targetReport, Band band, BlobFieldObject fieldObject) {
            XRControl control = null;
            FieldDefinition fieldDefinition = fieldObject.DataSource;
            if(fieldDefinition != null) {
                switch(fieldDefinition.Kind) {
                    case FieldKind.DatabaseField:
                        control = CreateXRControl<XRPictureBox>(band, fieldObject.Name);
                        AddDataBinding(targetReport, control, "Image", fieldDefinition.FormulaName);
                        break;
                    default:
                        Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.Warning_FieldObject_Kind_NotSupported_Format, fieldObject.Name, fieldDefinition.Kind));
                        break;
                }
            }
            if(control == null)
                control = CreateXRControl<XRPictureBox>(band, fieldObject.Name);
            return control;
        }

        XRPictureBox CreatePictureObject(Band band, PictureObject fieldObject, string crystalReportFileName, short indexOnSection) {
            XRPictureBox result = CreateXRControl<XRPictureBox>(band, fieldObject.Name);
            var rasPicture = GetRasObject<CrystalDecisions.ReportAppServer.ReportDefModel.ISCRPictureObject>(fieldObject);
            byte[] bmpBytes = GetBmpBytes(rasPicture, crystalReportFileName, indexOnSection);
            if(bmpBytes != null) {
                var stream = new MemoryStream(bmpBytes);
                result.Image = new Bitmap(stream);
            } else
                Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.Warning_PictureContent_NotSupported_Format, fieldObject.Name));
            return result;
        }

        static byte[] GetBmpBytes(CrystalDecisions.ReportAppServer.ReportDefModel.ISCRPictureObject crystalPicture, string crystalReportFileName, short indexOnSection) {
            if(crystalPicture == null || string.IsNullOrEmpty(crystalReportFileName))
                return null;
            ByteArray pictureData = crystalPicture.PictureData;
            if(pictureData != null && pictureData.ByteArray != null)
                return pictureData.ByteArray;
            return PrintHelper.GetPrintedBmp(crystalReportFileName, crystalPicture, indexOnSection);
        }

        XRLabel GenerateSummarySafe(XtraReport targetReport, Band band, string controlName, SummaryOperation crystalSummaryOperation, bool useRunningSummary, SummaryRunning summaryRunning, string crystalFormulaName) {
            XRLabel result = null;
            SummaryFunc runningSummaryFunc;
            if(CrystalTypeConverter.TryConvertSummaryFunc(crystalSummaryOperation, useRunningSummary, out runningSummaryFunc)) {
                result = CreateXRControl<XRLabel>(band, controlName, x => x.Summary = new XRSummary(summaryRunning, runningSummaryFunc, string.Empty));
                AddDataBinding(targetReport, result, "Text", crystalFormulaName);
            } else {
                Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.Warning_SummaryOperation_NotSupported_Format, controlName, crystalSummaryOperation));
            }
            return result;
        }

        XRControl ConvertLine(Section section, Band band, LineObject lineObject) {
            if(section.Name == lineObject.EndSectionName) {
                XRLine line = CreateXRControl<XRLine>(band, lineObject.Name);
                line.LineStyle = CrystalTypeConverter.GetLineStyle(lineObject.LineStyle);
                line.ForeColor = lineObject.LineColor;
                line.Borders = BorderSide.None;
                int lineWidth = XRConvert.Convert(lineObject.LineThickness, GraphicsDpi.Twips, GraphicsDpi.Pixel);
                line.LineWidth = Math.Max(lineWidth, 1);
                line.LineDirection = lineObject.Width > 0
                    ? LineDirection.Horizontal
                    : LineDirection.Vertical;
                return line;
            }
            XRCrossBandLine xbandLine = CreateXRControl<XRCrossBandLine>(band, lineObject.Name);
            xbandLine.ForeColor = lineObject.LineColor;
            ConfigureCrossBandControl(xbandLine, lineObject);
            return xbandLine;
        }

        XRLabel ConvertTextObject(Band band, TextObject textObject) {
            XRLabel control = CreateXRControl<XRLabel>(band, textObject.Name);
            control.Text = textObject.Text;
            control.ForeColor = textObject.Color;
            control.Font = (Font)textObject.Font.Clone();
            return control;
        }

        XRCrossBandBox ConvertBoxObject(Band band, BoxObject boxObject) {
            XRCrossBandBox control = CreateXRControl<XRCrossBandBox>(band, boxObject.Name);
            control.BorderWidth = XRConvert.Convert(boxObject.LineThickness, GraphicsDpi.Twips, GraphicsDpi.Pixel);
            control.BackColor = boxObject.FillColor;
            ConfigureCrossBandControl(control, boxObject);
            return control;
        }

        XRControl ConvertSubreport(Band band, SubreportObject subreportObject) {
            var control = CreateXRControl<XRSubreport>(band, subreportObject.Name);
            ReportDocument crystalSubreport = subreportObject.OpenSubreport(subreportObject.SubreportName);
            if(crystalSubreport != null) {
                var subCrystalConverter = new CrystalConverter();
                ConversionResult converterResult = subCrystalConverter.Convert(crystalSubreport);
                if(converterResult.TargetReport != null)
                    control.ReportSource = converterResult.TargetReport;
            }
            return control;
        }

        void SetControlBorder(XRControl control, Border border) {
            control.BackColor = border.BackgroundColor;
            control.BorderColor = border.BorderColor;

            if(border.BottomLineStyle != LineStyle.NoLine)
                control.Borders |= BorderSide.Bottom;
            if(border.TopLineStyle != LineStyle.NoLine)
                control.Borders |= BorderSide.Top;
            if(border.LeftLineStyle != LineStyle.NoLine)
                control.Borders |= BorderSide.Left;
            if(border.RightLineStyle != LineStyle.NoLine)
                control.Borders |= BorderSide.Right;
        }

        void ConvertReportObjectProperties(XRControl control, ReportObject reportObject) {
            SetControlName(control, reportObject.Name);
            SetParentStyleUsing(control, false);

            control.LeftF = TwipsToHOIF(reportObject.Left);
            control.TopF = TwipsToHOIF(reportObject.Top);
            if(control is XRCrossBandLine && reportObject is LineObject) {
                control.WidthF = TwipsToHOIF(((LineObject)reportObject).LineThickness);
            } else {
                control.WidthF = TwipsToHOIF(reportObject.Width);
            }
            control.HeightF = TwipsToHOIF(reportObject.Height);
            control.BackColor = reportObject.Border.BackgroundColor;
            control.CanGrow = reportObject.ObjectFormat.EnableCanGrow;
            control.Visible = !reportObject.ObjectFormat.EnableSuppress;

            control.TextAlignment = CrystalTypeConverter.GetTextAlignment(reportObject.ObjectFormat.HorizontalAlignment);
            SetControlBorder(control, reportObject.Border);

            string navigationUrl = null;
            if(TryGetNavigationUrl(reportObject.ObjectFormat, out navigationUrl)) {
                control.NavigateUrl = navigationUrl;
            }
        }

        void AddDataBinding(XtraReport targetReport, XRControl control, string property, string crystalFormulaName, bool isGrouping = false, bool isFormula = false) {
            string dataMember = crystalFormulaName;
            if(isGrouping)
                dataMember = dataMember.Replace("GroupName ", string.Empty).Trim('(', ')');
            dataMember = dataMember.Trim('{', '}');
            if(isFormula)
                dataMember = dataMember.TrimStart('@');
            var parts = dataMember.Split(DotSeparator, 2);
            string optimizedDataMember = parts.Length < 2
                ? dataMember
                : parts[1];
            var dataMemberPrefix = string.IsNullOrEmpty(targetReport.DataMember) ? string.Empty : targetReport.DataMember + ".";
            optimizedDataMember = dataMemberPrefix + optimizedDataMember;
            control.DataBindings.Add(property, targetReport.DataSource, optimizedDataMember);
        }

        void AddParameterBinding(XRControl control, FieldDefinition fieldDefinition) {
            string dataMember = fieldDefinition.FormulaName.Trim('{', '?', '}');
            Parameter parameter;
            if(parametersByOriginalNames.TryGetValue(dataMember, out parameter)) {
                control.DataBindings.Add(new XRBinding(parameter, "Text", string.Empty));
            } else {
                Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.Warning_Binding_CanNotResolve_Format, fieldDefinition.Name, fieldDefinition.FormulaName));
                control.Text = Messages.Control_CantResolveBinding;
            }
        }

        string GetOrGenerateDataMember(SqlDataSource dataSource, Cr.Table crystalTable, string columnName) {
            string crystalTableName = crystalTable.Name;
            Tuple<SqlDataSource, string> key = Tuple.Create(dataSource, crystalTableName);
            SelectQuery selectQuery = null;
            if(!singleQueriesByDataSourcesAndTables.TryGetValue(key, out selectQuery)) {
                selectQuery = SelectQueryFluentBuilder
                    .AddTable(crystalTableName)
                    .SelectColumn(columnName)
                    .Build(crystalTableName);
                dataSource.Queries.Add(selectQuery);
                singleQueriesByDataSourcesAndTables.Add(key, selectQuery);
                DatabaseFieldDefinition crystalColumn = crystalTable.Fields.Cast<DatabaseFieldDefinition>().First(x => x.Name == columnName);
                FieldListResultSchemaProvider.AppendViewToSchema(dataSource, crystalTable.Location, columnName, CrystalTypeConverter.ConvertToType(crystalColumn.ValueType));
            }
            return selectQuery.Name;
        }

        KeyValuePair<string, Parameter> ConvertParameter(ParameterField crystalParameter, Tables crystalDatabaseTables) {
            Type xrParameterType;
            if(!CrystalTypeConverter.TryGetXRParameterType(crystalParameter.ParameterValueType, out xrParameterType)) {
                Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.Warning_ReportParameter_NotSupported_Format, crystalParameter.Name));
                xrParameterType = typeof(string);
            }
            var result = new Parameter {
                Type = xrParameterType,
                Visible = crystalParameter.ParameterFieldUsage2.HasFlag(ParameterFieldUsage2.ShowOnPanel),
                Description = crystalParameter.PromptText,
                MultiValue = crystalParameter.EnableAllowMultipleValue,
                Value = GetXRParameterValue(crystalParameter)
            };
            NamingMapper.GenerateAndAssignXRControlName(result, crystalParameter.Name);
            if(crystalParameter.DefaultValues != null && crystalParameter.DefaultValues.Count > 0) {
                var settings = new StaticListLookUpSettings();
                IEnumerable<LookUpValue> lookupValues = crystalParameter.DefaultValues
                    .OfType<ParameterDiscreteValue>()
                    .Select(x => new LookUpValue(x.Value, x.Description));
                settings.LookUpValues.AddRange(lookupValues);
                result.LookUpSettings = settings;
            } else if(crystalParameter.Attributes.ContainsKey("FieldID")) {
                var fieldId = (string)crystalParameter.Attributes["FieldID"];
                string[] parts = fieldId.Split(DotSeparator, 2);
                string crystalTableName = parts[0];
                SqlDataSource sqlDataSource = sqlDataSourcesByTableNames[crystalTableName];
                Cr.Table crystalTable = crystalDatabaseTables.Cast<Cr.Table>().First(x => x.Name == crystalTableName);
                string dataMember = GetOrGenerateDataMember(sqlDataSource, crystalTable, parts[1]);
                var settings = new DynamicListLookUpSettings {
                    DataSource = sqlDataSource,
                    DataMember = dataMember,
                    ValueMember = parts[1],
                    DisplayMember = parts[1]
                };
                result.LookUpSettings = settings;
            }
            return new KeyValuePair<string, Parameter>(crystalParameter.Name, result);
        }

        static object GetXRParameterValue(ParameterField crystalParameter) {
            IEnumerable<object> values = crystalParameter.CurrentValues
                .OfType<ParameterDiscreteValue>()
                .Select(x => x.Value);
            return crystalParameter.EnableAllowMultipleValue
                ? values.ToArray()
                : values.FirstOrDefault();
        }

        static T GetRasObject<T>(EngineObjectBase crystalObjectBase)
            where T : class {
            var rasObjectProperty = typeof(EngineObjectBase).GetProperty("RasObject", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            T value = rasObjectProperty.GetValue(crystalObjectBase, null) as T;
            return value;
        }

        static bool TryGetNavigationUrl(EngineObjectBase objectFormat, out string navigationUrl) {
            navigationUrl = null;
            var value = GetRasObject<CrystalDecisions.ReportAppServer.ReportDefModel.ISCRObjectFormat>(objectFormat);
            if(value != null && value.HyperlinkType == CrystalDecisions.ReportAppServer.ReportDefModel.CrHyperlinkTypeEnum.crHyperlinkTypeWebsite && !string.IsNullOrEmpty(value.HyperlinkText)) {
                navigationUrl = value.HyperlinkText;
                return true;
            }
            return false;
        }
    }
}
#endif

#endregion
