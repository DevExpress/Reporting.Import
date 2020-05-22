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
using DevExpress.Data.Filtering;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Native;
using DevExpress.XtraReports.Design.Import.CrystalFormula;
using DevExpress.XtraReports.Native;
using DevExpress.XtraReports.Parameters;
using DevExpress.XtraReports.UI;
using Cr = CrystalDecisions.CrystalReports.Engine;
using CrData = CrystalDecisions.ReportAppServer.DataDefModel;
using CrReport = CrystalDecisions.ReportAppServer.ReportDefModel;

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

            public static Type GetXRParameterType(ParameterValueKind kind, string parameterName) {
                switch(kind) {
                    case ParameterValueKind.BooleanParameter:
                        return typeof(bool);
                    case ParameterValueKind.CurrencyParameter:
                        return typeof(decimal);
                    case ParameterValueKind.DateParameter:
                    case ParameterValueKind.DateTimeParameter:
                    case ParameterValueKind.TimeParameter:
                        return typeof(DateTime);
                    case ParameterValueKind.NumberParameter:
                        return typeof(double);
                    case ParameterValueKind.StringParameter:
                        return typeof(string);
                    default:
                        Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.Warning_ParameterType_NotSupported_Format, parameterName));
                        return typeof(string);
                }
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

            public static ConditionType ConvertToConditionType(CrData.CrTableJoinTypeEnum joinType) {
                switch(joinType) {
                    case CrData.CrTableJoinTypeEnum.crTableJoinTypeEqualJoin:
                        return ConditionType.Equal;
                    case CrData.CrTableJoinTypeEnum.crTableJoinTypeGreaterOrEqualJoin:
                        return ConditionType.GreaterOrEqual;
                    case CrData.CrTableJoinTypeEnum.crTableJoinTypeGreaterThanJoin:
                        return ConditionType.Greater;
                    case CrData.CrTableJoinTypeEnum.crTableJoinTypeLessOrEqualJoin:
                        return ConditionType.LessOrEqual;
                    case CrData.CrTableJoinTypeEnum.crTableJoinTypeLessThanJoin:
                        return ConditionType.Less;
                    case CrData.CrTableJoinTypeEnum.crTableJoinTypeNotEqualJoin:
                        return ConditionType.NotEqual;
                    case CrData.CrTableJoinTypeEnum.crTableJoinTypeOuterJoin:
                    case CrData.CrTableJoinTypeEnum.crTableJoinTypeAdvance:
                    case CrData.CrTableJoinTypeEnum.crTableJoinTypeLeftOuterJoin:
                    case CrData.CrTableJoinTypeEnum.crTableJoinTypeRightOuterJoin:
                    default:
                        return ConditionType.Equal;
                }
            }
            public static Xpo.DB.JoinType ConvertToJoinType(CrData.CrTableJoinTypeEnum joinType) {
                switch(joinType) {
                    case CrData.CrTableJoinTypeEnum.crTableJoinTypeLeftOuterJoin:
                        return Xpo.DB.JoinType.LeftOuter;
                    case CrData.CrTableJoinTypeEnum.crTableJoinTypeEqualJoin:
                    case CrData.CrTableJoinTypeEnum.crTableJoinTypeGreaterOrEqualJoin:
                    case CrData.CrTableJoinTypeEnum.crTableJoinTypeGreaterThanJoin:
                    case CrData.CrTableJoinTypeEnum.crTableJoinTypeLessOrEqualJoin:
                    case CrData.CrTableJoinTypeEnum.crTableJoinTypeLessThanJoin:
                    case CrData.CrTableJoinTypeEnum.crTableJoinTypeNotEqualJoin:
                        return Xpo.DB.JoinType.Inner;
                    case CrData.CrTableJoinTypeEnum.crTableJoinTypeOuterJoin:
                    case CrData.CrTableJoinTypeEnum.crTableJoinTypeRightOuterJoin:
                    case CrData.CrTableJoinTypeEnum.crTableJoinTypeAdvance:
                    default:
                        return Xpo.DB.JoinType.Inner;
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
            public static byte[] GetPrintedBmp(string crystalReportFileName, CrReport.ISCRReportObject pictureObject, short indexOnSection) {
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

        internal static class ExpressionFactory {
            public static string CreateExpression(string crystalFormula, Dictionary<string, string> columnsMap, Func<string, CalculatedField> getCalculatedFieldByFormula, string calculatedFieldName, string dataMember, out Formula formula) {
                formula = null;
                crystalFormula = crystalFormula?.Trim();
                if(string.IsNullOrEmpty(crystalFormula))
                    return string.Empty;
                string result;
                if(TryGetSimpleBindingPath(crystalFormula, columnsMap, getCalculatedFieldByFormula, calculatedFieldName, dataMember, out result))
                    return result;
                formula = ParseFormula(crystalFormula, calculatedFieldName);
                return string.Format("Iif(True, '{0}', '{1}')", FormulaParser.NotSupportedStub, crystalFormula.Replace("'", "''"));
            }
            static bool StringHasSingleChar(string value, char searchChar) {
                return CountChars(value, searchChar) == 1;
            }
            static bool IsSimpleBindingPath(string crystalFormula) {
                return crystalFormula.Length > 2
                    && crystalFormula[0] == '{'
                    && crystalFormula[crystalFormula.Length - 1] == '}'
                    && StringHasSingleChar(crystalFormula, '{')
                    && StringHasSingleChar(crystalFormula, '}');
            }
            static bool TryGetSimpleBindingPath(string crystalFormula, Dictionary<string, string> columnsMap, Func<string, CalculatedField> getCalculatedFieldByFormula, string calculatedFieldName, string dataMember, out string result) {
                if(!IsSimpleBindingPath(crystalFormula)) {
                    result = null;
                    return false;
                }
                result = crystalFormula.Substring(1, crystalFormula.Length - 2);
                string mappedColumn;
                if(columnsMap.TryGetValue(result, out mappedColumn))
                    result = mappedColumn;
                else if(result.Length > 1 && result[0] == '@') {
                    string formulaName = result.Substring(1);
                    CalculatedField calculatedField = getCalculatedFieldByFormula(result.Substring(1));
                    if(calculatedField == null)
                        throw new FormulaParserException(string.Format(Messages.Warning_CalculatedField_FormulaNotFound_Format, formulaName, calculatedFieldName));
                    result = calculatedField.Name;
                }
                if(result.StartsWith(dataMember + "."))
                    result = result.Substring(dataMember.Length + 1);
                return true;
            }
            static Formula ParseFormula(string crystalFormula, string calculatedFieldName) {
                using(var reader = new StringReader(crystalFormula)) {
                    var lexer = new FormulaLexer(reader);
                    try {
                        Formula formula = FormulaParser.Parse(
                            lexer,
                            CrystalConverter.UnrecognizedFunctionBehavior == UnrecognizedFunctionBehavior.Ignore,
                            x => Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.Warning_CalculatedField_UncategorizedFunction_Format, x, calculatedFieldName)));
                        return formula;
                    } catch(Exception e) {
                        string message = AugmentExceptionText(calculatedFieldName, e.Message, crystalFormula, lexer.Line, lexer.Col, lexer.Position);
                        if(!(e is FormulaParserException))
                            message += Environment.NewLine + e.ToString();
                        Tracer.TraceError(NativeSR.TraceSource, message);
                        return null;
                    }
                }
            }
            static string AugmentExceptionText(string calculatedFieldName, string exceptionMessage, string failedQuery, int failedLine, int failedCol, int failedPosition) {
                string grammarCatchAllErrorMessage = "Parser error at calculated field '{0}' line {1}, character {2}: '{3}'" + Environment.NewLine + "{4}" + Environment.NewLine;
                string malformedQuery = failedQuery;
                try {
                    malformedQuery = malformedQuery.Substring(0, failedPosition) + DevExpress.Data.Filtering.Exceptions.FilteringExceptionsText.ErrorPointer + malformedQuery.Substring(failedPosition);
                } catch { }
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, grammarCatchAllErrorMessage, calculatedFieldName, failedLine, failedCol, exceptionMessage, malformedQuery);
            }
            internal static int CountChars(string text, char ch) {
                int result = 0;
                foreach(char textChar in text)
                    if(textChar == ch)
                        result++;
                return result;
            }
        }
        #endregion

        public static UnrecognizedFunctionBehavior UnrecognizedFunctionBehavior { get; set; } = UnrecognizedFunctionBehavior.InsertWarning;
        public static bool DefaultSelectFullTableSchema { get; set; } = true;
        public static string FilterString { get { return "Crystal Reports (*.rpt)|*.rpt"; } }
        static readonly char[] DotSeparator = { '.' };

        static CrystalConverter() {
            SubscribeAssemblyResolveEventStatic("CrystalDecisions.");
        }

        public CrystalConverter() {
        }

        public bool? SelectFullTableSchema { get; set; }
        public event CrystalConverterSubreportGeneratedHandler SubreportGenerated;

        readonly Dictionary<string, Band> bandsByOriginalNames = new Dictionary<string, Band>(StringComparer.OrdinalIgnoreCase);
        readonly Dictionary<string, Parameter> parametersByOriginalNames = new Dictionary<string, Parameter>(StringComparer.OrdinalIgnoreCase);
        readonly Dictionary<string, Tuple<SqlDataSource, SqlQuery>> sqlQueriesByCrystalTableNames = new Dictionary<string, Tuple<SqlDataSource, SqlQuery>>(StringComparer.OrdinalIgnoreCase);
        readonly Dictionary<string, Tuple<CalculatedField, Formula>> calculatedFieldsByFormulae = new Dictionary<string, Tuple<CalculatedField, Formula>>(StringComparer.OrdinalIgnoreCase);
        readonly Dictionary<string, string> sqlSelectQueryColumnsByCrystalTableColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        readonly Dictionary<SqlQuery, object> crystalStoredProceduresByStoredProcQueries = new Dictionary<SqlQuery, object>(); // Value=CrData.ISCRProcedure; do not use types from CrystalDecisions.*.dll assemblies in the class fields
        readonly List<Action> onEnd = new List<Action>();

        ConversionResult Convert(ReportDocument crystalReport) {
            PerformConversion(() => ConvertInternalCore(crystalReport));
            return new ConversionResult(TargetReport, SourceReport);
        }

        protected override void ConvertInternal(string fileName) {
            var crystalReport = new ReportDocument();
            try {
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
                sqlQueriesByCrystalTableNames.Clear();
                calculatedFieldsByFormulae.Clear();
                sqlSelectQueryColumnsByCrystalTableColumns.Clear();
                crystalStoredProceduresByStoredProcQueries.Clear();
                onEnd.Clear();

                string filePath = !crystalReport.IsSubreport ? crystalReport.FilePath : string.Empty;
                TargetReport.ReportUnit = ReportUnit.HundredthsOfAnInch;
                TargetReport.Name = crystalReport.Name;

                GenerateDataSources(crystalReport.Database, filePath);
                if(!crystalReport.IsSubreport) {
                    ConvertReportParameters(crystalReport.ParameterFields, crystalReport.Database.Tables);
                    AssignStoredProcedureParameters();
                }
                ConvertCalculatedFields(crystalReport.DataDefinition.FormulaFields);
                PostConvertCalculatedFields();
                ConvertGroups(crystalReport);
                if(!crystalReport.IsSubreport)
                    ConvertPageSettings(crystalReport.PrintOptions);
                ISupportInitialize supportInit = TargetReport;
                supportInit.BeginInit();
                try {
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

        void GenerateDataSources(Database crystalDatabase, string originalReportPath = null) {
            SqlDataSource[] sqlDataSources = crystalDatabase.Tables
                .Cast<Cr.Table>()
                .GroupBy(x => x.LogOnInfo.ConnectionInfo, CrystalConnectionInfoEqualityComparer.Instance)
                .Select(x => GenerateSqlDataSource(x.Key, x, crystalDatabase.Links, originalReportPath))
                .Where(x => x != null)
                .ToArray();
            TargetReport.ComponentStorage.AddRange(sqlDataSources);
            SqlDataSource firstDataSource = sqlDataSources.FirstOrDefault();
            if(sqlDataSources.Length > 1)
                Tracer.TraceWarning(NativeSR.TraceSource, Messages.Warning_DataSource_Limitation);
            TargetReport.DataSource = firstDataSource;
            TargetReport.DataMember = firstDataSource?.Queries.FirstOrDefault()?.Name ?? string.Empty;
        }

        SqlDataSource GenerateSqlDataSource(ConnectionInfo connection, IEnumerable<Cr.Table> crystalTables, TableLinks crystalTableLinks, string originalReportPath = null) {
            DataConnectionParametersBase dataConnectionParameters = GenerateConnectionParameters(connection, originalReportPath);
            if(dataConnectionParameters == null)
                return null;
            var sqlDataSource = new SqlDataSource(dataConnectionParameters);
            NamingMapper.GenerateAndAssignXRControlName(sqlDataSource, connection.DatabaseName);
            var schemaProvider = new FieldListResultSchemaProvider(sqlDataSource.Name);
            var crystalTablesList = crystalTables.ToList();
            foreach(Cr.Table crystalTable in crystalTablesList)
                sqlQueriesByCrystalTableNames[crystalTable.Name] = Tuple.Create(sqlDataSource, (SqlQuery)null);
            Dictionary<SqlQuery, Tuple<Dictionary<string, Type>, Cr.Table>> queryDefinitions = GenerateQueryDefinitions(crystalTablesList, crystalTableLinks);
            foreach(KeyValuePair<SqlQuery, Tuple<Dictionary<string, Type>, Cr.Table>> queryDefinition in queryDefinitions) {
                Tuple<Dictionary<string, Type>, Cr.Table> dataSourceQueryDefinition = queryDefinition.Value;
                SqlQuery sqlQuery = queryDefinition.Key;
                if(dataSourceQueryDefinition.Item2 != null)
                    sqlQueriesByCrystalTableNames[dataSourceQueryDefinition.Item2.Name] = Tuple.Create(sqlDataSource, sqlQuery);
                schemaProvider.AddView(sqlQuery.Name, dataSourceQueryDefinition.Item1);
                sqlDataSource.Queries.Add(sqlQuery);
            }
            sqlDataSource.ResultSchemaSerializable = schemaProvider.GetResultSchemaSerializable();
            return sqlDataSource;
        }

        Dictionary<SqlQuery, Tuple<Dictionary<string, Type>, Cr.Table>> GenerateQueryDefinitions(List<Cr.Table> crystalTables, TableLinks crystalTableLinks = null, Func<Cr.Table, DatabaseFieldDefinition, bool> columnsFilter = null) {
            var result = new Dictionary<SqlQuery, Tuple<Dictionary<string, Type>, Cr.Table>>();
            Cr.Table singleCrystalTable = crystalTables.Count == 1 ? crystalTables[0] : null;
            string singleCrystalTableOriginalName = singleCrystalTable?.Location;
            var selectQuery = new SelectQuery {
                Name = NamingMapper.GenerateSafeName<SelectQuery>(singleCrystalTableOriginalName)
            };
            var selectQueryColumns = new Dictionary<string, Type>();
            var selectQueryDisplayColumnNames = new HashSet<string>();
            var selectQueryCrystalTableList = new List<Cr.Table>();
            foreach(Cr.Table crystalTable in crystalTables) {
                var rasTable = GetRasObject<CrystalDecisions.ReportAppServer.DataDefModel.ISCRTable>(crystalTable);
                if(rasTable.ClassName == "CrystalReports.Procedure" || rasTable.ClassName == "CrystalReports.CommandTable") {
                    if(singleCrystalTable == null)
                        Tracer.TraceWarning(NativeSR.TraceSource, Messages.Warning_DataSourceSP_Limitation);
                    Tuple<SqlQuery, Dictionary<string, Type>> spQueryDefinition = GenerateStoredProcQueryDefinition(crystalTable, columnsFilter);
                    result.Add(spQueryDefinition.Item1, Tuple.Create(spQueryDefinition.Item2, crystalTable));
                } else {
                    FillSelectQueryDefinition(selectQuery, selectQueryColumns, selectQueryDisplayColumnNames, crystalTable, columnsFilter);
                    selectQueryCrystalTableList.Add(crystalTable);
                }
            }
            if(selectQuery.Tables.Count > 0) {
                if(crystalTableLinks != null) {
                    foreach(TableLink crystalTableLink in crystalTableLinks) {
                        DataAccess.Sql.Table parentTable = selectQuery.Tables.FirstOrDefault(x => x.Name == crystalTableLink.SourceTable.Location);
                        DataAccess.Sql.Table nestedTable = selectQuery.Tables.FirstOrDefault(x => x.Name == crystalTableLink.DestinationTable.Location);
                        if(parentTable != null && nestedTable != null) {
                            var rasTableLink = GetRasObject<CrystalDecisions.ReportAppServer.DataDefModel.ISCRTableLink>(crystalTableLink);
                            Xpo.DB.JoinType joinType = CrystalTypeConverter.ConvertToJoinType(rasTableLink.JoinType);
                            RelationColumnInfo[] relationColumnInfo = GenerateRelationColumnInfo(
                                crystalTableLink.SourceFields,
                                crystalTableLink.DestinationFields,
                                CrystalTypeConverter.ConvertToConditionType(rasTableLink.JoinType));
                            selectQuery.AddRelation(parentTable, nestedTable, joinType, relationColumnInfo);
                        }
                    }
                }
                Cr.Table singleSelectQueryCrystalTable = selectQueryCrystalTableList.Count == 1 ? selectQueryCrystalTableList[0] : null;
                result.Add(selectQuery, Tuple.Create(selectQueryColumns, singleSelectQueryCrystalTable));
            }
            return result;
        }

        void FillSelectQueryDefinition(SelectQuery selectQuery, Dictionary<string, Type> selectQueryColumns, HashSet<string> selectQueryDisplayColumnNames, Cr.Table crystalTable, Func<Cr.Table, DatabaseFieldDefinition, bool> columnsFilter = null) {
            DataAccess.Sql.Table table = selectQuery.AddTable(crystalTable.Name);
            foreach(DatabaseFieldDefinition crystalTableColumn in crystalTable.Fields) {
                if(columnsFilter?.Invoke(crystalTable, crystalTableColumn) == false
                    || (columnsFilter == null && !SelectFullTableSchema.GetValueOrDefault(DefaultSelectFullTableSchema) && crystalTableColumn.UseCount == 0))
                    continue;
                string dispayColumnName = crystalTableColumn.Name;
                string crystalColumnAlias = null;
                if(selectQueryDisplayColumnNames.Contains(dispayColumnName)) {
                    crystalColumnAlias = crystalTableColumn.TableName + "_" + crystalTableColumn.Name;
                    dispayColumnName = crystalColumnAlias;
                }
                selectQuery.SelectColumn(table, crystalTableColumn.Name, crystalColumnAlias);
                selectQueryColumns.Add(dispayColumnName, CrystalTypeConverter.ConvertToType(crystalTableColumn.ValueType));
                selectQueryDisplayColumnNames.Add(dispayColumnName);
                string crystalBindingPath = crystalTable.Name + "." + crystalTableColumn.Name;
                if(!sqlSelectQueryColumnsByCrystalTableColumns.ContainsKey(crystalBindingPath))
                    sqlSelectQueryColumnsByCrystalTableColumns.Add(crystalBindingPath, selectQuery.Name + "." + dispayColumnName);
            }
        }

        Tuple<SqlQuery, Dictionary<string, Type>> GenerateStoredProcQueryDefinition(Cr.Table crystalTable, Func<Cr.Table, DatabaseFieldDefinition, bool> columnsFilter = null) {
            var rasProcedure = GetRasObject<CrData.ISCRProcedure>(crystalTable);
            SqlQuery result = rasProcedure.ClassName == "CrystalReports.CommandTable"
                ? (SqlQuery)new CustomSqlQuery {
                    Sql = ((CrData.ISCRCommandTable)rasProcedure).CommandText
                }
                : new StoredProcQuery {
                    StoredProcName = crystalTable.Location.Split(new[] { ';' }, 2)[0]
                };
            result.Name = NamingMapper.GenerateSafeName<SqlQuery>(crystalTable.Name);
            crystalStoredProceduresByStoredProcQueries.Add(result, rasProcedure);
            var columns = new Dictionary<string, Type>();
            foreach(DatabaseFieldDefinition crystalTableColumn in crystalTable.Fields) {
                if(columnsFilter != null && !columnsFilter(crystalTable, crystalTableColumn))
                    continue;
                string dispayColumnName = crystalTableColumn.Name;
                columns.Add(dispayColumnName, CrystalTypeConverter.ConvertToType(crystalTableColumn.ValueType));
                sqlSelectQueryColumnsByCrystalTableColumns[crystalTable.Name + "." + crystalTableColumn.Name] = result.Name + "." + dispayColumnName;
            }
            return Tuple.Create(result, columns);
        }

        string GenerateDataMember(SqlDataSource sqlDataSource, Cr.Tables crystalTables, string crystalTableName, string[] crystalColumnNames) {
            Cr.Table crystalTable = crystalTables[crystalTableName];
            KeyValuePair<SqlQuery, Tuple<Dictionary<string, Type>, Cr.Table>> dataSourceQueryDefinition = GenerateQueryDefinitions(
                    new List<Cr.Table> { crystalTable },
                    columnsFilter: (_, x) => crystalColumnNames.Contains(x.Name, StringComparer.OrdinalIgnoreCase))
                .FirstOrDefault();
            System.Diagnostics.Debug.Assert(dataSourceQueryDefinition.Key != null);
            SqlQuery sqlQuery = dataSourceQueryDefinition.Key;
            if(dataSourceQueryDefinition.Value.Item2 != null)
                sqlQueriesByCrystalTableNames[dataSourceQueryDefinition.Value.Item2.Name] = Tuple.Create(sqlDataSource, sqlQuery);
            FieldListResultSchemaProvider.AppendViewToSchema(sqlDataSource, sqlQuery.Name, dataSourceQueryDefinition.Value.Item1);
            sqlDataSource.Queries.Add(sqlQuery);
            return sqlQuery.Name;
        }

        static RelationColumnInfo[] GenerateRelationColumnInfo(DatabaseFieldDefinitions sourceFields, DatabaseFieldDefinitions destinationFields, ConditionType conditionType) {
            var result = sourceFields
                .Cast<DatabaseFieldDefinition>()
                .Zip(destinationFields.Cast<DatabaseFieldDefinition>(), (s, d) => new RelationColumnInfo(s.Name, d.Name, conditionType))
                .ToArray();
            return result;
        }

        static DataConnectionParametersBase GenerateConnectionParameters(ConnectionInfo connectionInfo, string originalReportPath = null) {
            NameValuePairs2 attributes = connectionInfo.Attributes.Collection;
            var databaseDll = attributes.Lookup(DbConnectionAttributes.CONNINFO_DATABASE_DLL) as string;
            NameValuePairs2 logonProperties = connectionInfo.LogonProperties;
            if(databaseDll == DbConnectionAttributes.DATABASE_DLL_CRDB_ADO || databaseDll == DbConnectionAttributes.DATABASE_DLL_CRDB_ADOPLUS) {
                var oledbProvider = logonProperties.Lookup("Provider") as string;
                var msSqlProviders = new[] { "SQLOLEDB", "SQLNCLI", "SQLNCLI10", "SQLNCLI11" };
                if(Array.IndexOf(msSqlProviders, oledbProvider) >= 0) {
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
            } else if(databaseDll == "crdb_fielddef.dll") {
                string optionalSearchDirectory = !string.IsNullOrEmpty(originalReportPath) ? Path.GetDirectoryName(originalReportPath) : null;
                string ttxXmlFilePath = GenerateTtxXmlFilePath(connectionInfo.ServerName, optionalSearchDirectory);
                if(!string.IsNullOrEmpty(ttxXmlFilePath))
                    return new XmlFileConnectionParameters(ttxXmlFilePath);
            } else if(databaseDll == DbConnectionAttributes.DATABASE_DLL_CRDB_ODBC) {
                var odbcDsnName = logonProperties.Lookup("UseDSNProperties") as string;
                object useDSNPropertiesObj = logonProperties.Lookup("UseDSNProperties");
                if(useDSNPropertiesObj is bool && (bool)useDSNPropertiesObj)
                    Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.Warning_Connection_DatabaseOdbcDsnNotSupported_Format, odbcDsnName));
                return new MsSqlConnectionParameters(
                    connectionInfo.ServerName,
                    connectionInfo.DatabaseName,
                    connectionInfo.UserID,
                    connectionInfo.Password,
                    connectionInfo.IntegratedSecurity ? MsSqlAuthorizationType.Windows : MsSqlAuthorizationType.SqlServer);
            }
            Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.Warning_Connection_DatabaseDllNotSupported_Format, databaseDll));
            return null;
        }

        static string GenerateTtxXmlFilePath(string originalFilePath, string optionalSearchDirectory = null) {
            if(string.IsNullOrEmpty(originalFilePath))
                return null;
            const string TtxFileExt = ".ttx";
            if(!originalFilePath.EndsWith(TtxFileExt, StringComparison.OrdinalIgnoreCase))
                return null;
            originalFilePath = originalFilePath.Substring(0, originalFilePath.Length - TtxFileExt.Length) + "_ttx.xml";
            if(File.Exists(originalFilePath))
                return originalFilePath;
            if(!string.IsNullOrEmpty(optionalSearchDirectory)) {
                string fileName = Path.GetFileName(originalFilePath);
                string optionalSearchFilePath = Path.Combine(optionalSearchDirectory, fileName);
                if(File.Exists(optionalSearchFilePath))
                    return optionalSearchFilePath;
            }
            return originalFilePath;
        }

        void ConvertCalculatedFields(FormulaFieldDefinitions crystalFormulae) {
            List<SqlQuery> allQueries = TargetReport.ComponentStorage
                .OfType<SqlDataSource>()
                .SelectMany(x => x.Queries)
                .ToList();
            string dataMember = allQueries.Count == 1 ? allQueries[0].Name : string.Empty;
            foreach(FormulaFieldDefinition crystalFormula in crystalFormulae) {
                var result = new CalculatedField {
                    DataMember = dataMember,
                    FieldType = CrystalTypeConverter.ConvertToFieldType(crystalFormula.ValueType)
                };
                GenerateName(result, crystalFormula.Name);
                Formula formula;
                result.Expression = CreateExpression(crystalFormula.Text, result.Name, dataMember, out formula);
                calculatedFieldsByFormulae[crystalFormula.Name] = Tuple.Create(result, formula);
                TargetReport.CalculatedFields.Add(result);
            }
        }

        string CreateExpression(string crystalFormula, string currentName, string dataMember, out Formula formula) {
            return ExpressionFactory.CreateExpression(
                    crystalFormula,
                    sqlSelectQueryColumnsByCrystalTableColumns,
                    GetCalculatedFieldByFormula,
                    currentName,
                    dataMember,
                    out formula);
        }

        void GenerateName(CalculatedField calculatedField, string formulaName) {
            string name = NamingMapper.GenerateSafeName<CalculatedField>(
                formulaName,
                x => DictionaryContainsValueIgnoreCase(sqlSelectQueryColumnsByCrystalTableColumns, EmbeddedFieldsHelper.GetDataMember(calculatedField.DataMember, x)));
            NamingMapper.GenerateAndAssignXRControlName(calculatedField, name);
        }

        static bool DictionaryContainsValueIgnoreCase<T>(IDictionary<T, string> dictionary, string value) {
            foreach(var dictioanaryValue in dictionary.Values)
                if(string.Equals(dictioanaryValue, value, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        void ConvertReportParameters(ParameterFields parameterFields, Cr.Tables crystalDatabaseTables) {
            IEnumerable<KeyValuePair<string, Parameter>> parametersByName = parameterFields
                .Cast<ParameterField>()
                .Where(x => x.ReportParameterType == CrystalDecisions.Shared.ParameterType.ReportParameter || x.ReportParameterType == CrystalDecisions.Shared.ParameterType.StoreProcedureParameter)
                .Select(x => ConvertParameter(x, crystalDatabaseTables));
            foreach(KeyValuePair<string, Parameter> pair in parametersByName) {
                parametersByOriginalNames[pair.Key] = pair.Value;
                TargetReport.Parameters.Add(pair.Value);
            }
        }

        void AssignStoredProcedureParameters() {
            foreach(KeyValuePair<SqlQuery, object> pair in crystalStoredProceduresByStoredProcQueries) {
                SqlQuery query = pair.Key;
                var rasProcedure = (CrData.ISCRProcedure)pair.Value;

                var customSqlQuery = query as CustomSqlQuery;
                if(customSqlQuery != null)
                    customSqlQuery.Sql = UpdateSqlParameterNames(customSqlQuery.Sql);
                foreach(CrData.ISCRField crystalParameter in rasProcedure.Parameters) {
                    Parameter parameter;
                    if(!parametersByOriginalNames.TryGetValue(crystalParameter.Name, out parameter)) {
                        Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.Warning_DataSourceSP_ParameterNotFound_Format, crystalParameter.Name, query.Name));
                        continue;
                    }
                    query.Parameters.Add(new QueryParameter(
                        crystalParameter.Name,
                        typeof(DataAccess.Expression),
                        new DataAccess.Expression("?" + parameter.Name, parameter.Type)));
                }
            }
        }
        string UpdateSqlParameterNames(string sql) {
            // 'where ID = {?p1}' should be replaced by 'where ID = @p1'
            System.Text.RegularExpressions.MatchCollection matches = System.Text.RegularExpressions.Regex.Matches(sql, @"\{\?(\w+)\}");
            foreach(System.Text.RegularExpressions.Match match in matches) {
                if(match.Groups.Count == 2) {
                    Parameter parameter;
                    if(parametersByOriginalNames.TryGetValue(match.Groups[1].Value, out parameter))
                        sql = sql.Replace(match.Groups[0].Value, "@" + parameter.Name);
                }
            }
            return sql;
        }

        void PostConvertCalculatedFields() {
            foreach(KeyValuePair<string, Tuple<CalculatedField, Formula>> pair in calculatedFieldsByFormulae) {
                Formula formula = pair.Value.Item2;
                if(formula == null || ReferenceEquals(formula.Statement, null))
                    continue;
                CalculatedField calculatedField = pair.Value.Item1;
                try {
                    CriteriaOperator convertedStatement = formula.Statement.Accept(new FormulaConverter(
                        calculatedField,
                        formula.Formulae,
                        GetCalculatedFieldByFormula,
                        sqlSelectQueryColumnsByCrystalTableColumns,
                        parametersByOriginalNames));
                    calculatedField.Expression = convertedStatement.ToString();
                } catch(Exception e) {
                    Tracer.TraceError(NativeSR.TraceSource, e);
                }
            }
        }

        CalculatedField GetCalculatedFieldByFormula(string formula) {
            Tuple<CalculatedField, Formula> value;
            if(calculatedFieldsByFormulae.TryGetValue(formula, out value))
                return value.Item1;
            return null;
        }

        void ConvertGroups(ReportDocument crystalReport) {
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
                Cr.Group crystalGroup = groups[i];
                string crystalConditionFieldName = crystalGroup.ConditionField.Name;
                GroupField groupField = new GroupField(crystalConditionFieldName);
                // todo: (DateTimeCondition)crystalGroup.GroupOptions.Condition
                foreach(SortField field in dataDefinition.SortFields) {
                    if(field.Field.Name == crystalConditionFieldName) {
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
                        control = ConvertFieldObject(band, (FieldObject)reportObject);
                        break;
                    case ReportObjectKind.FieldHeadingObject:
                    case ReportObjectKind.TextObject:
                        control = ConvertTextObject(band, (TextObject)reportObject);
                        break;
                    case ReportObjectKind.BlobFieldObject:
                        control = ConvertBlobFieldObject(band, (BlobFieldObject)reportObject);
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
            NamingMapper.GenerateAndAssignXRControlName(band, area.Name);
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
            band.PrintAcrossBands = section.SectionFormat.EnableUnderlaySection;
            ConvertSectionControls(band, section, crystalReportFileName);
        }

        XRControl ConvertFieldObject(Band band, FieldObject fieldObject) {
            XRControl control;
            if(HasAccessToDataSource(fieldObject))
                control = ConvertFieldObjectCore(band, fieldObject);
            else
                control = ConvertFieldObjectFallback(band, GetRasObject<CrReport.ISCRFieldObject>(fieldObject));
            if(control == null) {
                control = CreateXRControl<XRLabel>(band, fieldObject.Name);
                control.Text = Messages.Control_Untranslated;
            }

            control.ForeColor = fieldObject.Color;
            control.Font = (Font)fieldObject.Font.Clone();
            return control;
        }

        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
        static bool HasAccessToDataSource(FieldObject fieldObject) {
            try {
                var ignore = fieldObject.DataSource;
                return true;
            } catch(AccessViolationException) {
                return false;
            }
        }

        XRLabel ConvertFieldObjectFallback(Band band, CrReport.ISCRFieldObject fieldObject) {
            Formula ignore;
            string expression = CreateExpression(
                    fieldObject.DataSource,
                    "Expression",
                    band.Report.DataMember,
                    out ignore);
            var label = CreateXRControl<XRLabel>(band, fieldObject.Name);
            label.ExpressionBindings.Add(new ExpressionBinding("Text", expression));
            return label;
        }

        XRControl ConvertFieldObjectCore(Band band, FieldObject fieldObject) {
            XRControl control = null;
            FieldDefinition fieldDefinition = fieldObject.DataSource;
            if(fieldDefinition != null) {
                switch(fieldDefinition.Kind) {
                    case FieldKind.DatabaseField:
                        control = CreateXRControl<XRLabel>(band, fieldObject.Name);
                        AddDataBinding(control, "Text", fieldDefinition.FormulaName);
                        break;
                    case FieldKind.ParameterField:
                        control = CreateXRControl<XRLabel>(band, fieldObject.Name);
                        AddParameterBinding(control, fieldDefinition);
                        break;
                    case FieldKind.SpecialVarField:
                        var specialVarFieldDefinition = (SpecialVarFieldDefinition)fieldDefinition;
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
                        AddDataBinding(control, "Text", fieldDefinition.FormulaName, isGrouping: true);
                        break;
                    case FieldKind.SummaryField:
                        var summaryDefinition = (SummaryFieldDefinition)fieldDefinition;
                        control = GenerateSummarySafe(band, fieldObject.Name, summaryDefinition.Operation, false, SummaryRunning.Report, summaryDefinition.SummarizedField.FormulaName);
                        break;
                    case FieldKind.RunningTotalField:
                        var runningTotalField = (RunningTotalFieldDefinition)fieldDefinition;
                        SummaryRunning summaryRunning = runningTotalField.EvaluationConditionType == RunningTotalCondition.OnChangeOfGroup
                            ? SummaryRunning.Group
                            : SummaryRunning.Report;
                        control = GenerateSummarySafe(band, fieldObject.Name, runningTotalField.Operation, true, summaryRunning, runningTotalField.SummarizedField.FormulaName);
                        break;
                    case FieldKind.FormulaField:
                        control = CreateXRControl<XRLabel>(band, fieldDefinition.Name);
                        AddDataBinding(control, "Text", fieldDefinition.FormulaName, isFormula: true);
                        break;
                    default:
                        Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.Warning_FieldObject_Kind_NotSupported_Format, fieldObject.Name, fieldDefinition.Kind));
                        break;
                }
            }
            return control;
        }

        XRControl ConvertBlobFieldObject(Band band, BlobFieldObject fieldObject) {
            XRControl control = null;
            FieldDefinition fieldDefinition = fieldObject.DataSource;
            if(fieldDefinition != null) {
                switch(fieldDefinition.Kind) {
                    case FieldKind.DatabaseField:
                        control = CreateXRControl<XRPictureBox>(band, fieldObject.Name);
                        AddDataBinding(control, "Image", fieldDefinition.FormulaName);
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
            var rasPicture = GetRasObject<CrReport.ISCRPictureObject>(fieldObject);
            byte[] bmpBytes = GetBmpBytes(rasPicture, crystalReportFileName, indexOnSection);
            if(bmpBytes != null) {
                var stream = new MemoryStream(bmpBytes);
                result.Image = new Bitmap(stream);
            } else
                Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.Warning_PictureContent_NotSupported_Format, fieldObject.Name));
            return result;
        }

        static byte[] GetBmpBytes(CrReport.ISCRPictureObject crystalPicture, string crystalReportFileName, short indexOnSection) {
            if(crystalPicture == null || string.IsNullOrEmpty(crystalReportFileName))
                return null;
            ByteArray pictureData = crystalPicture.PictureData;
            if(pictureData != null && pictureData.ByteArray != null)
                return pictureData.ByteArray;
            return PrintHelper.GetPrintedBmp(crystalReportFileName, crystalPicture, indexOnSection);
        }

        XRLabel GenerateSummarySafe(Band band, string controlName, SummaryOperation crystalSummaryOperation, bool useRunningSummary, SummaryRunning summaryRunning, string crystalFormulaName) {
            XRLabel result = null;
            SummaryFunc runningSummaryFunc;
            if(CrystalTypeConverter.TryConvertSummaryFunc(crystalSummaryOperation, useRunningSummary, out runningSummaryFunc)) {
                result = CreateXRControl<XRLabel>(band, controlName, x => x.Summary = new XRSummary(summaryRunning, runningSummaryFunc, string.Empty));
                AddDataBinding(result, "Text", crystalFormulaName);
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
                if(converterResult.TargetReport != null) {
                    control.ReportSource = converterResult.TargetReport;
                    SubreportGenerated?.Invoke(this, new CrystalConverterSubreportGeneratedEventArgs(subreportObject.SubreportName, control, converterResult.TargetReport));
                }
            }
            return control;
        }

        void SetControlBorder(XRControl control, Border border) {
            if(border.BackgroundColor != Color.White)
                control.BackColor = border.BackgroundColor;
            if(border.BorderColor != Color.Black)
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
            if(string.IsNullOrEmpty(control.Name))
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
            control.CanGrow = reportObject.ObjectFormat.EnableCanGrow;
            control.Visible = !reportObject.ObjectFormat.EnableSuppress;

            control.TextAlignment = CrystalTypeConverter.GetTextAlignment(reportObject.ObjectFormat.HorizontalAlignment);
            SetControlBorder(control, reportObject.Border);

            string navigationUrl;
            if(TryGetNavigationUrl(reportObject.ObjectFormat, out navigationUrl)) {
                control.NavigateUrl = navigationUrl;
            }
        }

        void AddDataBinding(XRControl control, string property, string crystalFormulaName, bool isGrouping = false, bool isFormula = false) {
            string optimizedDataMember = GetActualDataMember(crystalFormulaName, control.Name, isGrouping, isFormula);
            if(optimizedDataMember != null)
                control.DataBindings.Add(property, TargetReport.DataSource, optimizedDataMember);
        }

        string GetActualDataMember(string crystalFormulaName, string controlName, bool isGrouping = false, bool isFormula = false) {
            string dataMember = crystalFormulaName;
            if(isGrouping)
                dataMember = dataMember.Replace("GroupName ", string.Empty).Trim('(', ')');
            int indexOfComma = dataMember.IndexOf(",");
            if(indexOfComma >= 0) {
                string groupBy = dataMember.Substring(indexOfComma + 1).Trim(' ');
                dataMember = dataMember.Substring(0, indexOfComma);
                Tracer.TraceError(NativeSR.TraceSource, string.Format(Messages.Warning_DataBinding_GroupByKindNotSupported_Format, controlName, groupBy));
            }
            dataMember = dataMember.Trim('{', '}');
            string resultDataMember;
            if(isFormula) {
                if(dataMember.StartsWith("%")) {
                    Tracer.TraceError(NativeSR.TraceSource, string.Format(Messages.Warning_DataBinding_SqlExpressionFieldNotSupported_Format, dataMember, controlName));
                    return null;
                }
                string formulaName = dataMember.TrimStart('@');
                CalculatedField calculatedField = GetCalculatedFieldByFormula(formulaName);
                if(calculatedField == null) {
                    Tracer.TraceError(NativeSR.TraceSource, string.Format(Messages.Warning_CalculatedField_FormulaNotFound_Format, formulaName, controlName));
                    return null;
                }
                return EmbeddedFieldsHelper.GetDataMember(calculatedField.DataMember, calculatedField.Name);
            } else if(sqlSelectQueryColumnsByCrystalTableColumns.TryGetValue(dataMember, out resultDataMember))
                return resultDataMember;
            string[] parts = dataMember.Split(DotSeparator, 2);
            resultDataMember = parts.Length == 2
                ? parts[1]
                : dataMember;
            return EmbeddedFieldsHelper.GetDataMember(TargetReport.DataMember, resultDataMember);
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

        KeyValuePair<string, Parameter> ConvertParameter(ParameterField crystalParameter, Tables crystalDatabaseTables) {
            var result = new Parameter {
                Type = CrystalTypeConverter.GetXRParameterType(crystalParameter.ParameterValueType, crystalParameter.Name),
                Visible = crystalParameter.ParameterFieldUsage2.HasFlag(ParameterFieldUsage2.ShowOnPanel),
                Description = crystalParameter.PromptText,
                MultiValue = crystalParameter.EnableAllowMultipleValue,
                Value = GetXRParameterValue(crystalParameter),
                AllowNull = crystalParameter.EnableNullValue
            };
            string parameterName = crystalParameter.Name.TrimStart('@');
            NamingMapper.GenerateAndAssignXRControlName(result, parameterName);
            if(crystalParameter.DefaultValues != null && crystalParameter.DefaultValues.Count > 0) {
                if(crystalParameter.DefaultValues.Count == 1 && crystalParameter.DefaultValues[0] is ParameterDiscreteValue) {
                    result.Value = ((ParameterDiscreteValue)crystalParameter.DefaultValues[0]).Value;
                } else {
                    var settings = new StaticListLookUpSettings();
                    IEnumerable<LookUpValue> lookupValues = crystalParameter.DefaultValues
                        .OfType<ParameterDiscreteValue>()
                        .Select(x => new LookUpValue(x.Value, x.Description));
                    settings.LookUpValues.AddRange(lookupValues);
                    result.LookUpSettings = settings;
                }
            } else if(crystalParameter.Attributes.ContainsKey("FieldID")) {
                var fieldId = (string)crystalParameter.Attributes["FieldID"];
                string[] parts = fieldId.Split(DotSeparator, 2);
                string crystalTableName = parts[0];
                Tuple<SqlDataSource, SqlQuery> queryDefenition;
                sqlQueriesByCrystalTableNames.TryGetValue(crystalTableName, out queryDefenition);
                if(queryDefenition != null && parts.Length == 2) {
                    string dataMember = queryDefenition.Item2?.Name
                        ?? GenerateDataMember(queryDefenition.Item1, crystalDatabaseTables, crystalTableName, new[] { parts[1] });
                    string valueMember = parts[1];
                    string mappedQueryColumnName;
                    if(sqlSelectQueryColumnsByCrystalTableColumns.TryGetValue(fieldId, out mappedQueryColumnName)) {
                        string[] mappedQueryColumnNameArray = mappedQueryColumnName.Split(DotSeparator, 2);
                        if(mappedQueryColumnNameArray.Length == 2)
                            valueMember = mappedQueryColumnNameArray[1];
                    }
                    var settings = new DynamicListLookUpSettings {
                        DataSource = queryDefenition.Item1,
                        DataMember = dataMember,
                        ValueMember = valueMember,
                        DisplayMember = valueMember
                    };
                    result.LookUpSettings = settings;
                } else {
                    Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.Warning_ParameterLookups_CanNotFindTable_Format, crystalTableName, result));
                }
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
            var value = GetRasObject<CrReport.ISCRObjectFormat>(objectFormat);
            if(value != null && value.HyperlinkType == CrReport.CrHyperlinkTypeEnum.crHyperlinkTypeWebsite && !string.IsNullOrEmpty(value.HyperlinkText)) {
                navigationUrl = value.HyperlinkText;
                return true;
            }
            return false;
        }
    }

    public enum UnrecognizedFunctionBehavior {
        InsertWarning,
        Ignore
    }
}
#endif

#endregion
