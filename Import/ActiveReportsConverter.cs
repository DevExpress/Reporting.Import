#region DEMO_REMOVE

#if Active
using System;
using System.Collections;
using System.ComponentModel;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Xml;
using DevExpress.Drawing;
using DevExpress.Drawing.Internal;
using DevExpress.Drawing.Printing;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Drawing;
using DevExpress.XtraPrinting.Native;
using DevExpress.XtraPrinting.Shape;
using DevExpress.XtraReports.UI;
using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.Document.Section;
using ARPageSettings = GrapeCity.ActiveReports.PageSettings;

namespace DevExpress.XtraReports.Import {
    public sealed partial class ActiveReportsConverter : DataSetBasedExternalConverterBase {
        static readonly Hashtable sectionBandMap = new Hashtable();
        static readonly Hashtable controlsMap = new Hashtable();
        static readonly Hashtable lineStyleMap = new Hashtable();
        static readonly Hashtable picSizeModeMap = new Hashtable();
        static readonly Hashtable summaryTypeMap = new Hashtable();
        static readonly Hashtable summaryFuncMap = new Hashtable();
        static readonly Hashtable barCodeSymbologyMap = new Hashtable();

        SectionReport sourceReport;
        string bindingPath = string.Empty;
        static bool staticFieldsFilled = false;

        static void PopulateStatic() {
            sectionBandMap[SectionType.ReportHeader] = typeof(ReportHeaderBand);
            sectionBandMap[SectionType.PageHeader] = typeof(PageHeaderBand);
            sectionBandMap[SectionType.GroupHeader] = typeof(GroupHeaderBand);
            sectionBandMap[SectionType.Detail] = typeof(DetailBand);
            sectionBandMap[SectionType.GroupFooter] = typeof(GroupFooterBand);
            sectionBandMap[SectionType.PageFooter] = typeof(PageFooterBand);
            sectionBandMap[SectionType.ReportFooter] = typeof(ReportFooterBand);

            controlsMap[typeof(GrapeCity.ActiveReports.SectionReportModel.Label)] = typeof(XRLabel);
            controlsMap[typeof(GrapeCity.ActiveReports.SectionReportModel.TextBox)] = typeof(XRLabel);
            controlsMap[typeof(GrapeCity.ActiveReports.SectionReportModel.CheckBox)] = typeof(XRCheckBox);
            controlsMap[typeof(GrapeCity.ActiveReports.SectionReportModel.Line)] = typeof(XRLine);
            controlsMap[typeof(GrapeCity.ActiveReports.SectionReportModel.Barcode)] = typeof(XRBarCode);
            controlsMap[typeof(GrapeCity.ActiveReports.SectionReportModel.Picture)] = typeof(XRPictureBox);
            controlsMap[typeof(GrapeCity.ActiveReports.SectionReportModel.RichTextBox)] = typeof(XRRichText);
            controlsMap[typeof(GrapeCity.ActiveReports.SectionReportModel.PageBreak)] = typeof(XRPageBreak);
            controlsMap[typeof(GrapeCity.ActiveReports.SectionReportModel.Shape)] = typeof(XRShape);
            //controlsMap[typeof(GrapeCity.ActiveReports.SectionReportModel.SubReport)] = typeof(SubReport);

            lineStyleMap[GrapeCity.ActiveReports.SectionReportModel.LineStyle.Dash] = System.Drawing.Drawing2D.DashStyle.Dash;
            lineStyleMap[GrapeCity.ActiveReports.SectionReportModel.LineStyle.DashDot] = System.Drawing.Drawing2D.DashStyle.DashDot;
            lineStyleMap[GrapeCity.ActiveReports.SectionReportModel.LineStyle.DashDotDot] = System.Drawing.Drawing2D.DashStyle.DashDotDot;
            lineStyleMap[GrapeCity.ActiveReports.SectionReportModel.LineStyle.Dot] = System.Drawing.Drawing2D.DashStyle.Dot;
            lineStyleMap[GrapeCity.ActiveReports.SectionReportModel.LineStyle.Solid] = System.Drawing.Drawing2D.DashStyle.Solid;
            lineStyleMap[GrapeCity.ActiveReports.SectionReportModel.LineStyle.Transparent] = System.Drawing.Drawing2D.DashStyle.Custom;

            picSizeModeMap[GrapeCity.ActiveReports.SectionReportModel.SizeModes.Clip] = ImageSizeMode.Normal;
            picSizeModeMap[GrapeCity.ActiveReports.SectionReportModel.SizeModes.Stretch] = ImageSizeMode.StretchImage;
            picSizeModeMap[GrapeCity.ActiveReports.SectionReportModel.SizeModes.Zoom] = ImageSizeMode.ZoomImage;

            summaryTypeMap[GrapeCity.ActiveReports.SectionReportModel.SummaryType.GrandTotal] = DevExpress.XtraReports.UI.SummaryRunning.Report;
            summaryTypeMap[GrapeCity.ActiveReports.SectionReportModel.SummaryType.SubTotal] = DevExpress.XtraReports.UI.SummaryRunning.Group;
            summaryTypeMap[GrapeCity.ActiveReports.SectionReportModel.SummaryType.PageCount] = DevExpress.XtraReports.UI.SummaryRunning.None;
            summaryTypeMap[GrapeCity.ActiveReports.SectionReportModel.SummaryType.PageTotal] = DevExpress.XtraReports.UI.SummaryRunning.None;
            summaryTypeMap[GrapeCity.ActiveReports.SectionReportModel.SummaryType.None] = DevExpress.XtraReports.UI.SummaryRunning.None;

            summaryFuncMap[GrapeCity.ActiveReports.SectionReportModel.SummaryFunc.Avg] = DevExpress.XtraReports.UI.SummaryFunc.Avg;
            summaryFuncMap[GrapeCity.ActiveReports.SectionReportModel.SummaryFunc.Count] = DevExpress.XtraReports.UI.SummaryFunc.Count;
            summaryFuncMap[GrapeCity.ActiveReports.SectionReportModel.SummaryFunc.DAvg] = DevExpress.XtraReports.UI.SummaryFunc.DAvg;
            summaryFuncMap[GrapeCity.ActiveReports.SectionReportModel.SummaryFunc.DCount] = DevExpress.XtraReports.UI.SummaryFunc.DCount;
            summaryFuncMap[GrapeCity.ActiveReports.SectionReportModel.SummaryFunc.DStdDev] = DevExpress.XtraReports.UI.SummaryFunc.DStdDev;
            summaryFuncMap[GrapeCity.ActiveReports.SectionReportModel.SummaryFunc.DStdDevP] = DevExpress.XtraReports.UI.SummaryFunc.DStdDevP;
            summaryFuncMap[GrapeCity.ActiveReports.SectionReportModel.SummaryFunc.DSum] = DevExpress.XtraReports.UI.SummaryFunc.DSum;
            summaryFuncMap[GrapeCity.ActiveReports.SectionReportModel.SummaryFunc.DVar] = DevExpress.XtraReports.UI.SummaryFunc.DVar;
            summaryFuncMap[GrapeCity.ActiveReports.SectionReportModel.SummaryFunc.DVarP] = DevExpress.XtraReports.UI.SummaryFunc.DVarP;
            summaryFuncMap[GrapeCity.ActiveReports.SectionReportModel.SummaryFunc.Max] = DevExpress.XtraReports.UI.SummaryFunc.Max;
            summaryFuncMap[GrapeCity.ActiveReports.SectionReportModel.SummaryFunc.Min] = DevExpress.XtraReports.UI.SummaryFunc.Min;
            summaryFuncMap[GrapeCity.ActiveReports.SectionReportModel.SummaryFunc.StdDev] = DevExpress.XtraReports.UI.SummaryFunc.StdDev;
            summaryFuncMap[GrapeCity.ActiveReports.SectionReportModel.SummaryFunc.StdDevP] = DevExpress.XtraReports.UI.SummaryFunc.StdDevP;
            summaryFuncMap[GrapeCity.ActiveReports.SectionReportModel.SummaryFunc.Sum] = DevExpress.XtraReports.UI.SummaryFunc.Sum;
            summaryFuncMap[GrapeCity.ActiveReports.SectionReportModel.SummaryFunc.Var] = DevExpress.XtraReports.UI.SummaryFunc.Var;
            summaryFuncMap[GrapeCity.ActiveReports.SectionReportModel.SummaryFunc.VarP] = DevExpress.XtraReports.UI.SummaryFunc.VarP;

            barCodeSymbologyMap[GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle.Ansi39] = new DevExpress.XtraPrinting.BarCode.Code39Generator(); //XRBarCodeSymbology.Code39;
            barCodeSymbologyMap[GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle.Ansi39x] = new DevExpress.XtraPrinting.BarCode.Code39ExtendedGenerator(); //XRBarCodeSymbology.Code39Extended;
            barCodeSymbologyMap[GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle.Codabar] = new DevExpress.XtraPrinting.BarCode.CodabarGenerator(); //XRBarCodeSymbology.Codabar;
            barCodeSymbologyMap[GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle.Code25intlv] = new DevExpress.XtraPrinting.BarCode.Interleaved2of5Generator(); //XRBarCodeSymbology.Interleaved2of5;
            barCodeSymbologyMap[GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle.Matrix_2_of_5] = new DevExpress.XtraPrinting.BarCode.Matrix2of5Generator(); //XRBarCodeSymbology.Matrix2of5;
            barCodeSymbologyMap[GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle.Code39] = new DevExpress.XtraPrinting.BarCode.Code39Generator(); //XRBarCodeSymbology.Code39;
            barCodeSymbologyMap[GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle.Code39x] = new DevExpress.XtraPrinting.BarCode.Code39ExtendedGenerator(); //XRBarCodeSymbology.Code39Extended;
                                                                                                                                                                  //barCodeSymbologyMap[BarCodeStyle.Code49] = XRBarCodeSymbology.Code39Extended;
            barCodeSymbologyMap[GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle.Code93x] = new DevExpress.XtraPrinting.BarCode.Code93ExtendedGenerator(); //XRBarCodeSymbology.Code93Extended;
            barCodeSymbologyMap[GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle.Code_128_A] = new DevExpress.XtraPrinting.BarCode.Code128Generator(); //XRBarCodeSymbology.Code128;
            barCodeSymbologyMap[GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle.Code_128_B] = new DevExpress.XtraPrinting.BarCode.Code128Generator(); //XRBarCodeSymbology.Code128;
            barCodeSymbologyMap[GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle.Code_128_C] = new DevExpress.XtraPrinting.BarCode.Code128Generator(); //XRBarCodeSymbology.Code128;
            barCodeSymbologyMap[GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle.Code_128auto] = new DevExpress.XtraPrinting.BarCode.Code128Generator(); //XRBarCodeSymbology.Code128;
            barCodeSymbologyMap[GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle.Code_2_of_5] = new DevExpress.XtraPrinting.BarCode.Industrial2of5Generator(); //XRBarCodeSymbology.Industrial2of5;
            barCodeSymbologyMap[GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle.Code_93] = new DevExpress.XtraPrinting.BarCode.Code93Generator(); //XRBarCodeSymbology.Code93;
            barCodeSymbologyMap[GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle.EAN_13] = new DevExpress.XtraPrinting.BarCode.EAN13Generator(); //XRBarCodeSymbology.EAN13;
            barCodeSymbologyMap[GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle.EAN_8] = new DevExpress.XtraPrinting.BarCode.EAN8Generator(); //XRBarCodeSymbology.EAN8;
                                                                                                                                                      //barCodeSymbologyMap[BarCodeStyle.JapanesePostal] = XRBarCodeSymbology.Code39Extended;
            barCodeSymbologyMap[GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle.MSI] = new DevExpress.XtraPrinting.BarCode.CodeMSIGenerator(); //XRBarCodeSymbology.MSI;
            barCodeSymbologyMap[GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle.None] = new DevExpress.XtraPrinting.BarCode.Code128Generator(); //XRBarCodeSymbology.Code128;
                                                                                                                                                        //barCodeSymbologyMap[BarCodeStyle.Pdf417] = XRBarCodeSymbology.Code39Extended;
            barCodeSymbologyMap[GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle.PostNet] = new DevExpress.XtraPrinting.BarCode.PostNetGenerator(); //XRBarCodeSymbology.PostNet;
                                                                                                                                                           //barCodeSymbologyMap[BarCodeStyle.QRCode] = XRBarCodeSymbology.Code39Extended;
            barCodeSymbologyMap[GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle.RM4SCC] = new DevExpress.XtraPrinting.BarCode.Code39ExtendedGenerator(); //XRBarCodeSymbology.Code39Extended;
            barCodeSymbologyMap[GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle.UCCEAN128] = new DevExpress.XtraPrinting.BarCode.EAN8Generator(); //XRBarCodeSymbology.EAN128;
            barCodeSymbologyMap[GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle.UPC_A] = new DevExpress.XtraPrinting.BarCode.UPCAGenerator(); //XRBarCodeSymbology.UPCA;
            barCodeSymbologyMap[GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle.UPC_E0] = new DevExpress.XtraPrinting.BarCode.UPCE0Generator(); //XRBarCodeSymbology.UPCE0;
            barCodeSymbologyMap[GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle.UPC_E1] = new DevExpress.XtraPrinting.BarCode.UPCE1Generator(); //XRBarCodeSymbology.UPCE1;
        }

        public ActiveReportsConverter() {
            BeforeConvert();
            if(!staticFieldsFilled) {
                staticFieldsFilled = true;
                PopulateStatic();
            }
        }

        partial void BeforeConvert();

        public static string FilterString {
            get { return "GrapeCity ActiveReports (*.rpx)|*.rpx"; }
        }

        static int InchesToHOI(float value) {
            return System.Convert.ToInt32(value * 100);
        }

        void ConvertPageSettings() {
            ARPageSettings ps = sourceReport.PageSettings;
            TargetReport.PaperKind = (DXPaperKind)ps.PaperKind;
            if(ps.PaperKind == PaperKind.Custom) {
                float paperWidth = sourceReport.PrintWidth + ps.Margins.Left + ps.Margins.Right;
                if(paperWidth < ps.PaperWidth)
                    paperWidth = ps.PaperWidth;
                TargetReport.PageWidth = InchesToHOI(paperWidth);
                TargetReport.PageHeight = InchesToHOI(ps.PaperHeight);
            }
            TargetReport.Margins.Left = InchesToHOI(ps.Margins.Left);
            TargetReport.Margins.Right = InchesToHOI(ps.Margins.Right);
            TargetReport.Margins.Top = InchesToHOI(ps.Margins.Top);
            TargetReport.Margins.Bottom = InchesToHOI(ps.Margins.Bottom);
        }

        void ConvertWatermark() {
            if(sourceReport.Watermark != null) {
                TargetReport.Watermark.ImageViewMode = ToImageViewMode(sourceReport.WatermarkSizeMode);
                if(sourceReport.Watermark != null)
                    TargetReport.Watermark.Image = (Image)sourceReport.Watermark.Clone();
                TargetReport.Watermark.PageRange = sourceReport.WatermarkPrintOnPages;
            }
        }

        XRControl CreateXRControl(GrapeCity.ActiveReports.SectionReportModel.ARControl src) {
            if(src is GrapeCity.ActiveReports.SectionReportModel.SubReport)
                return null;

            Type xrType = (Type)controlsMap[src.GetType()];
            if(xrType == null)
                xrType = typeof(XRControl);

            return CreateXRControl(xrType);
        }

        static DevExpress.XtraPrinting.TextAlignment MakeXRAlignment(GrapeCity.ActiveReports.Document.Section.TextAlignment horz, VerticalTextAlignment vert) {
            DevExpress.XtraPrinting.TextAlignment xrAlign = DevExpress.XtraPrinting.TextAlignment.TopLeft;
            switch(horz) {
                case GrapeCity.ActiveReports.Document.Section.TextAlignment.Left: {
                        switch(vert) {
                            case VerticalTextAlignment.Top:
                                xrAlign = DevExpress.XtraPrinting.TextAlignment.TopLeft;
                                break;
                            case VerticalTextAlignment.Middle:
                                xrAlign = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
                                break;
                            case VerticalTextAlignment.Bottom:
                                xrAlign = DevExpress.XtraPrinting.TextAlignment.BottomLeft;
                                break;
                        }
                        break;
                    }
                case GrapeCity.ActiveReports.Document.Section.TextAlignment.Right: {
                        switch(vert) {
                            case VerticalTextAlignment.Top:
                                xrAlign = DevExpress.XtraPrinting.TextAlignment.TopRight;
                                break;
                            case VerticalTextAlignment.Middle:
                                xrAlign = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
                                break;
                            case VerticalTextAlignment.Bottom:
                                xrAlign = DevExpress.XtraPrinting.TextAlignment.BottomRight;
                                break;
                        }
                        break;
                    }
                case GrapeCity.ActiveReports.Document.Section.TextAlignment.Center: {
                        switch(vert) {
                            case VerticalTextAlignment.Top:
                                xrAlign = DevExpress.XtraPrinting.TextAlignment.TopCenter;
                                break;
                            case VerticalTextAlignment.Middle:
                                xrAlign = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
                                break;
                            case VerticalTextAlignment.Bottom:
                                xrAlign = DevExpress.XtraPrinting.TextAlignment.BottomCenter;
                                break;
                        }
                        break;
                    }
                case GrapeCity.ActiveReports.Document.Section.TextAlignment.Justify: {
                        switch(vert) {
                            case VerticalTextAlignment.Top:
                                xrAlign = DevExpress.XtraPrinting.TextAlignment.TopJustify;
                                break;
                            case VerticalTextAlignment.Middle:
                                xrAlign = DevExpress.XtraPrinting.TextAlignment.MiddleJustify;
                                break;
                            case VerticalTextAlignment.Bottom:
                                xrAlign = DevExpress.XtraPrinting.TextAlignment.BottomJustify;
                                break;
                        }
                        break;
                    }
            }
            return xrAlign;
        }

        static DevExpress.XtraPrinting.TextAlignment MakeXRAlignment(StringAlignment horz, GrapeCity.ActiveReports.SectionReportModel.BarCodeCaptionPosition vert) {
            DevExpress.XtraPrinting.TextAlignment xrAlign = DevExpress.XtraPrinting.TextAlignment.BottomLeft;
            switch(horz) {
                case StringAlignment.Near: {
                        switch(vert) {
                            case GrapeCity.ActiveReports.SectionReportModel.BarCodeCaptionPosition.Above:
                                xrAlign = DevExpress.XtraPrinting.TextAlignment.TopLeft;
                                break;
                            case GrapeCity.ActiveReports.SectionReportModel.BarCodeCaptionPosition.Below:
                                xrAlign = DevExpress.XtraPrinting.TextAlignment.BottomLeft;
                                break;
                        }
                        break;
                    }
                case StringAlignment.Center: {
                        switch(vert) {
                            case GrapeCity.ActiveReports.SectionReportModel.BarCodeCaptionPosition.Above:
                                xrAlign = DevExpress.XtraPrinting.TextAlignment.TopCenter;
                                break;
                            case GrapeCity.ActiveReports.SectionReportModel.BarCodeCaptionPosition.Below:
                                xrAlign = DevExpress.XtraPrinting.TextAlignment.BottomCenter;
                                break;
                        }
                        break;
                    }
                case StringAlignment.Far: {
                        switch(vert) {
                            case GrapeCity.ActiveReports.SectionReportModel.BarCodeCaptionPosition.Above:
                                xrAlign = DevExpress.XtraPrinting.TextAlignment.TopRight;
                                break;
                            case GrapeCity.ActiveReports.SectionReportModel.BarCodeCaptionPosition.Below:
                                xrAlign = DevExpress.XtraPrinting.TextAlignment.BottomRight;
                                break;
                        }
                        break;
                    }
            }
            return xrAlign;
        }

        static DXDashStyle MakeLineStyle(GrapeCity.ActiveReports.SectionReportModel.LineStyle style) {
            return (DXDashStyle)lineStyleMap[style];
        }

        static ImageSizeMode MakeSizeMode(GrapeCity.ActiveReports.SectionReportModel.SizeModes sizeMode) {
            return (ImageSizeMode)picSizeModeMap[sizeMode];
        }

        static LineDirection MakeLineDirection(int x1, int y1, int x2, int y2) {
            if(x1 == x2)
                return LineDirection.Vertical;
            if(y1 == y2)
                return LineDirection.Horizontal;
            if((x2 > x1 && y2 > y1) || (x1 > x2 && y1 > y2))
                return LineDirection.BackSlant;
            else
                return LineDirection.Slant;

        }

        /*
        static int MakeBorderWidth(Border border) {
            return 1;
        }
        static XRBorderSide MakeBorderSide(Border border) {
            return (XRBorderSide)0;
        }
        */
        static BorderSide MakeBorderSide(GrapeCity.ActiveReports.SectionReportModel.LineStyle style) {
            return style == GrapeCity.ActiveReports.SectionReportModel.LineStyle.Transparent ? BorderSide.None : BorderSide.All;
        }
        static DevExpress.XtraReports.UI.SummaryRunning MakeSummaryRunning(GrapeCity.ActiveReports.SectionReportModel.SummaryType type) {
            return (DevExpress.XtraReports.UI.SummaryRunning)summaryTypeMap[type];
        }
        static DevExpress.XtraReports.UI.SummaryFunc MakeSummaryFunc(GrapeCity.ActiveReports.SectionReportModel.SummaryFunc type) {
            return (DevExpress.XtraReports.UI.SummaryFunc)summaryFuncMap[type];
        }
        static string MakeOutputFormat(string arOutputFormat) {
            if(string.IsNullOrEmpty(arOutputFormat))
                return string.Empty;
            if(arOutputFormat.ToUpper() == "CURRENCY")
                arOutputFormat = "C";

            return "{0:" + arOutputFormat + '}';
        }
        static DevExpress.XtraPrinting.BarCode.BarCodeGeneratorBase MakeSymbology(GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle style) {
            DevExpress.XtraPrinting.BarCode.BarCodeGeneratorBase symbology = barCodeSymbologyMap[style] as DevExpress.XtraPrinting.BarCode.BarCodeGeneratorBase;
            return symbology != null ? symbology : new DevExpress.XtraPrinting.BarCode.Code39ExtendedGenerator(); //XRBarCodeSymbology.Code39Extended;
        }
        static DevExpress.XtraPrinting.BarCode.Code128Charset MakeCode128Charset(GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle style) {
            DevExpress.XtraPrinting.BarCode.Code128Charset charSet = DevExpress.XtraPrinting.BarCode.Code128Charset.CharsetAuto;
            if(style == GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle.Code_128_A)
                charSet = DevExpress.XtraPrinting.BarCode.Code128Charset.CharsetA;
            else if(style == GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle.Code_128_B)
                charSet = DevExpress.XtraPrinting.BarCode.Code128Charset.CharsetB;
            else if(style == GrapeCity.ActiveReports.SectionReportModel.BarCodeStyle.Code_128_C)
                charSet = DevExpress.XtraPrinting.BarCode.Code128Charset.CharsetC;
            return charSet;
        }

        static ImageViewMode ToImageViewMode(GrapeCity.ActiveReports.SectionReportModel.SizeModes val) {
            switch(val) {
                case GrapeCity.ActiveReports.SectionReportModel.SizeModes.Zoom:
                    return ImageViewMode.Zoom;
                case GrapeCity.ActiveReports.SectionReportModel.SizeModes.Stretch:
                    return ImageViewMode.Stretch;
                //case SizeModes.Clip:
                default:
                    return ImageViewMode.Clip;
            }
        }

        static ColumnLayout ConvertMultiColumnDirection(GrapeCity.ActiveReports.SectionReportModel.ColumnDirection val) {
            if(val == GrapeCity.ActiveReports.SectionReportModel.ColumnDirection.AcrossDown)
                return ColumnLayout.AcrossThenDown;
            else
                return ColumnLayout.DownThenAcross;
        }

        void ConvertARLabelToXRLabel(GrapeCity.ActiveReports.SectionReportModel.Label src, XRLabel tgt) {
            tgt.Angle = (float)src.Angle / 10.0f;
            tgt.BackColor = src.BackColor;
            tgt.CanGrow = false;
            tgt.CanShrink = false;
            tgt.Font = SystemDrawingConverter.CreateDXFont((Font)src.Font.Clone());
            tgt.ForeColor = src.ForeColor;
            tgt.Multiline = src.MultiLine;
            tgt.NavigateUrl = src.HyperLink;
            tgt.Text = src.Text;
            tgt.TextAlignment = MakeXRAlignment(src.Alignment, src.VerticalAlignment);
            tgt.WordWrap = src.WrapMode != WrapMode.NoWrap;


            BindDataToControl(tgt, "Text", src.DataField);

            /*
            src.ClassName;
            src.Style;
            */
        }
        protected override void BindDataToControl(XRControl control, string property, string dataMember, string formatString = null) {
            string s = bindingPath != "" && dataMember != null
                ? string.Format("{0}.{1}", bindingPath, dataMember)
                : dataMember;
            base.BindDataToControl(control, property, s, string.Empty);
        }

        void ConvertARTextBoxToXRLabel(GrapeCity.ActiveReports.SectionReportModel.TextBox src, XRLabel tgt) {
            tgt.BackColor = src.BackColor;
            tgt.CanGrow = src.CanGrow;
            tgt.CanShrink = src.CanShrink;
            tgt.Font = SystemDrawingConverter.CreateDXFont((Font)src.Font.Clone());
            tgt.ForeColor = src.ForeColor;
            tgt.Multiline = src.MultiLine;
            tgt.NavigateUrl = src.HyperLink;
            tgt.Text = src.Text;
            tgt.TextAlignment = MakeXRAlignment(src.Alignment, src.VerticalAlignment);
            tgt.WordWrap = src.WrapMode != WrapMode.NoWrap;

            string formatString = MakeOutputFormat(src.OutputFormat);

            BindDataToControl(tgt, "Text", src.DataField, formatString);

            //tgt.Summary.Func;
            tgt.Summary.Running = MakeSummaryRunning(src.SummaryType);
            tgt.Summary.Func = MakeSummaryFunc(src.SummaryFunc);
            tgt.Summary.FormatString = formatString;

            /*
            src.DistinctField;
            *src.SummaryGroup;
            *src.SummaryRunning;
            *src.SummaryType;
            src.ClassName;
            src.Style;
            */
        }

        void ConvertPageBreak(GrapeCity.ActiveReports.SectionReportModel.PageBreak src, XRPageBreak tgt) {
            tgt.Visible = src.Enabled;
        }

        void ConvertCheckBox(GrapeCity.ActiveReports.SectionReportModel.CheckBox src, XRCheckBox tgt) {
            tgt.Checked = src.Checked;
            tgt.WordWrap = src.WrapMode != WrapMode.NoWrap;
            tgt.BackColor = src.BackColor;
            tgt.ForeColor = src.ForeColor;
            tgt.Font = SystemDrawingConverter.CreateDXFont((Font)src.Font.Clone());

            BindDataToControl(tgt, "CheckState", src.DataField);
            /*
            src.CheckAlignment;
            src.ClassName;
            src.Style;
            */
        }
        void ConvertRichTextBox(GrapeCity.ActiveReports.SectionReportModel.RichTextBox src, XRRichText tgt) {
            if(src.BackColor != Color.Transparent)
                tgt.BackColor = src.BackColor;
            tgt.CanGrow = src.CanGrow;
            tgt.CanShrink = src.CanShrink;
            tgt.Font = SystemDrawingConverter.CreateDXFont((Font)src.Font.Clone());
            tgt.ForeColor = src.ForeColor;
            tgt.Rtf = src.RTF;

            BindDataToControl(tgt, "Rtf", src.DataField);
            /*
            src.ClassName;
            src.Style;
            */
        }

        void ConvertPictureBox(GrapeCity.ActiveReports.SectionReportModel.Picture src, XRPictureBox tgt) {
            tgt.BackColor = src.BackColor;
            tgt.NavigateUrl = src.HyperLink;
            if(src.Image != null)
                tgt.Image = (Image)src.Image.Clone();
            tgt.Sizing = MakeSizeMode(src.SizeMode);
            tgt.BorderWidth = (int)src.LineWeight;
            tgt.BorderColor = src.LineColor;
            tgt.Borders = MakeBorderSide(src.LineStyle);

            BindDataToControl(tgt, "Image", src.DataField);
            //src.PictureAlignment;
        }

        void ConvertLine(GrapeCity.ActiveReports.SectionReportModel.Line src, XRLine tgt) {
            tgt.ForeColor = src.LineColor;
            tgt.LineStyle = MakeLineStyle(src.LineStyle);
            tgt.LineWidth = (int)src.LineWeight;
            tgt.LineDirection = MakeLineDirection(InchesToHOI(src.X1), InchesToHOI(src.Y1), InchesToHOI(src.X2), InchesToHOI(src.Y2));
        }

        void ConvertShape(GrapeCity.ActiveReports.SectionReportModel.Shape src, XRShape tgt) {
            tgt.BackColor = src.BackColor;
            tgt.BorderWidth = (int)src.LineWeight;
            tgt.BorderColor = src.LineColor;
            tgt.Borders = MakeBorderSide(src.LineStyle);
            tgt.Shape = new ShapeRectangle {
                Fillet = (int)src.RoundingRadius.Default.GetValueOrDefault()
            };
        }

        void ConvertBarCode(GrapeCity.ActiveReports.SectionReportModel.Barcode src, XRBarCode tgt) {
            int module = (int)src.NarrowBarWidth;
            if(module <= 0)
                module = 1;

            tgt.Symbology = MakeSymbology(src.Style);
            DevExpress.XtraPrinting.BarCode.Code128Generator gen = tgt.Symbology as DevExpress.XtraPrinting.BarCode.Code128Generator;
            if(gen != null)
                gen.CharacterSet = MakeCode128Charset(src.Style);

            tgt.Symbology.CalcCheckSum = src.CheckSumEnabled;

            tgt.BackColor = src.BackColor;
            tgt.Font = SystemDrawingConverter.CreateDXFont((Font)src.Font.Clone());
            tgt.ForeColor = src.ForeColor;
            tgt.Text = src.Text;
            tgt.TextAlignment = MakeXRAlignment(src.Alignment, src.CaptionPosition);
            if(src.CaptionPosition == GrapeCity.ActiveReports.SectionReportModel.BarCodeCaptionPosition.None)
                tgt.ShowText = false;
            tgt.Module = module;
            tgt.Padding = PaddingInfo.Empty;

            BindDataToControl(tgt, "Text", src.DataField);
        }

        void ConvertARControlToXRControl(GrapeCity.ActiveReports.SectionReportModel.ARControl src, XRControl tgt) {
            SetControlName(tgt, src.Name);

            tgt.Location = new Point(InchesToHOI(src.Location.X), InchesToHOI(src.Location.Y));
            tgt.Size = new Size(InchesToHOI(src.Size.Width), InchesToHOI(src.Size.Height));
            tgt.Tag = src.Tag;
            tgt.Visible = src.Visible;
            //tgt.BorderWidth = MakeBorderWidth(src.Border);
            //tgt.BorderSide = MakeBorderSide(src.Border);

            SetParentStyleUsing(tgt, false);

            if(src is GrapeCity.ActiveReports.SectionReportModel.Label)
                ConvertARLabelToXRLabel((GrapeCity.ActiveReports.SectionReportModel.Label)src, (XRLabel)tgt);
            else if(src is GrapeCity.ActiveReports.SectionReportModel.TextBox)
                ConvertARTextBoxToXRLabel((GrapeCity.ActiveReports.SectionReportModel.TextBox)src, (XRLabel)tgt);
            else if(src is GrapeCity.ActiveReports.SectionReportModel.PageBreak)
                ConvertPageBreak((GrapeCity.ActiveReports.SectionReportModel.PageBreak)src, (XRPageBreak)tgt);
            else if(src is GrapeCity.ActiveReports.SectionReportModel.CheckBox)
                ConvertCheckBox((GrapeCity.ActiveReports.SectionReportModel.CheckBox)src, (XRCheckBox)tgt);
            else if(src is GrapeCity.ActiveReports.SectionReportModel.RichTextBox)
                ConvertRichTextBox((GrapeCity.ActiveReports.SectionReportModel.RichTextBox)src, (XRRichText)tgt);
            else if(src is GrapeCity.ActiveReports.SectionReportModel.Picture)
                ConvertPictureBox((GrapeCity.ActiveReports.SectionReportModel.Picture)src, (XRPictureBox)tgt);
            else if(src is GrapeCity.ActiveReports.SectionReportModel.Line)
                ConvertLine((GrapeCity.ActiveReports.SectionReportModel.Line)src, (XRLine)tgt);
            else if(src is GrapeCity.ActiveReports.SectionReportModel.Barcode)
                ConvertBarCode((GrapeCity.ActiveReports.SectionReportModel.Barcode)src, (XRBarCode)tgt);
            else if(src is GrapeCity.ActiveReports.SectionReportModel.Shape)
                ConvertShape((GrapeCity.ActiveReports.SectionReportModel.Shape)src, (XRShape)tgt);
        }

        void ConvertControls(GrapeCity.ActiveReports.SectionReportModel.Section section, Band band) {
            if(section.Controls.Count <= 0)
                return;

            foreach(GrapeCity.ActiveReports.SectionReportModel.ARControl ctrl in section.Controls) {
                XRControl xrControl = CreateXRControl(ctrl);
                if(xrControl != null) {
                    band.Controls.Add(xrControl);
                    try {
                        ConvertARControlToXRControl(ctrl, xrControl);
                    } catch {
                    }
                }
            }
        }

        void ConvertSectionToBand(GrapeCity.ActiveReports.SectionReportModel.Section section, Band band) {
            SetControlName(band, section.Name);

            band.Height = InchesToHOI(section.Height);
            band.BackColor = section.BackColor;
            band.Visible = section.Visible;

            SetParentStyleUsing(band, false);

            ConvertControls(section, band);

            GrapeCity.ActiveReports.SectionReportModel.Detail srcDetail = section as GrapeCity.ActiveReports.SectionReportModel.Detail;
            if(srcDetail != null) {
                DetailBand tgtDetail = band as DetailBand;
                tgtDetail.MultiColumn.ColumnCount = srcDetail.ColumnCount;
                tgtDetail.MultiColumn.Layout = ConvertMultiColumnDirection(srcDetail.ColumnDirection);
            }

            GrapeCity.ActiveReports.SectionReportModel.GroupHeader groupHeader = section as GrapeCity.ActiveReports.SectionReportModel.GroupHeader;
            GroupHeaderBand tgtHeader = band as GroupHeaderBand;
            if(groupHeader != null) {
                if(groupHeader.DataField != null)
                    tgtHeader.GroupFields.Add(new GroupField(groupHeader.DataField));
                tgtHeader.Level = 0;
            }
            /*
                        GroupFooterBand tgtFooter = band as GroupFooterBand;
                        if (tgtFooter != null)
                            tgtFooter.Level = int.MaxValue;
            */
        }

        void ConvertSection(GrapeCity.ActiveReports.SectionReportModel.Section section) {
            Type bandType = (Type)sectionBandMap[section.Type];
            Band band = GetOrCreateBandByType(bandType);
            ConvertSectionToBand(section, band);
        }

        void ConvertSections() {
            int count = sourceReport.Sections.Count;
            for(int i = 0; i < count; i++)
                ConvertSection(sourceReport.Sections[i]);
        }

        void PerformConvert() {
            try {
                CursorStorage.SetCursor(System.Windows.Forms.Cursors.WaitCursor);

                GrapeCity.ActiveReports.Data.OleDBDataSource oleDS = sourceReport.DataSource as GrapeCity.ActiveReports.Data.OleDBDataSource;
                if(oleDS != null) {
                    OleDbConnection oleConnection = new OleDbConnection(oleDS.ConnectionString);
                    OleDbCommand oleSelectCommand = new OleDbCommand(oleDS.SQL, oleConnection);
                    bindingPath = ParseTableName(oleDS.SQL);
                    AssignDataAdapter(CreateOleDBDataAdapter(oleSelectCommand, bindingPath));
                }
                GrapeCity.ActiveReports.Data.SqlDBDataSource sqlDS = sourceReport.DataSource as GrapeCity.ActiveReports.Data.SqlDBDataSource;
                if(sqlDS != null) {
                    SqlConnection sqlConnection = new SqlConnection(sqlDS.ConnectionString);
                    SqlCommand sqlSelectCommand = new SqlCommand(sqlDS.SQL, sqlConnection);
                    AssignDataAdapter(CreateSqlDataAdapter(sqlSelectCommand, ParseTableName(sqlDS.SQL)));
                }
                GrapeCity.ActiveReports.Data.XMLDataSource xmlDS = sourceReport.DataSource as GrapeCity.ActiveReports.Data.XMLDataSource;
                if(xmlDS != null) {
                    TargetReport.DataMember = xmlDS.RecordsetPattern;
                    //NOTE: reflection for DataSourceSchema
                    SetPropertyValue(TargetReport, "XmlDataPath", xmlDS.FileURL);
                }
                TargetReport.ReportUnit = ReportUnit.HundredthsOfAnInch;

                ConvertPageSettings();
                ConvertWatermark();
                ConvertSections();
            } finally {
                CursorStorage.RestoreCursor();
            }
        }
        PropertyDescriptor GetProperty(object obj, string propName) {
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(obj)[propName];
            if(descriptor == null)
                throw new ArgumentNullException(propName);
            return descriptor;
        }

        void SetPropertyValue(object obj, string propName, object value) {
            PropertyDescriptor descriptor = GetProperty(obj, propName);
            descriptor.SetValue(obj, value);
        }

        public ConversionResult Convert(SectionReport activeReport) {
            sourceReport = activeReport;
            PerformConversion(PerformConvert);
            return new ConversionResult(TargetReport, SourceReport);
        }

        protected override void ConvertInternal(string fileName) {
            bindingPath = string.Empty;
            sourceReport = new SectionReport();
            using(var reader = XmlReader.Create(fileName)) {
                sourceReport.LoadLayout(reader);
                PerformConvert();
            }
        }

        SqlDataAdapter CreateSqlDataAdapter(SqlCommand selectCommand, string tableName) {
            var dataAdapter = new SqlDataAdapter(selectCommand);
            CreateTableMapping(dataAdapter, tableName);
            return dataAdapter;
        }
    }
}
#endif

#endregion
