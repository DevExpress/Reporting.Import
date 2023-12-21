#region DEMO_REMOVE

#if Access
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.OleDb;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DevExpress.Drawing;
using DevExpress.Drawing.Printing;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Native;
using DevExpress.XtraReports.UI;
using AccessInterop = DevExpress.XtraReports.Import.Interop.Access;

namespace DevExpress.XtraReports.Import {
    public class AccessConverter : DataSetBasedExternalConverterBase {
        class Messages {
            public const string Trace_LevelWithExpressionIsNotSupported_Format = "Level with Expression '{0}' is not supported.";
            public const string Text_UnsupportedControl_Format = "The '{0}' control is not supported.";
            public const string MessageBox_InstallPrinter = "At least one printer needs to be installed before reports can be imported from Microsoft Access.\nDo you want to install a printer now?";
            public const string CommandLine_ReportIndex_Warning = "[ReportIndex] argument should be from 0 to {0}.";
        }

        AccessInterop._Application app = null;
        AccessReportBase accessReport;
        protected AccessReportBase AccessReport {
            get {
                if(accessReport == null)
                    accessReport = AccessReportBase.CreateInstance(app.Reports[0]);
                return accessReport;
            }
        }

        readonly string reportName; // used in Console Import only
        readonly int? reportIndex; // used in Console Import only
        public AccessConverter() {
        }
        public AccessConverter(string reportName, int? reportIndex) {
            this.reportName = reportName;
            this.reportIndex = reportIndex;
        }

        static readonly Type[] bandTypes = {
            typeof(DetailBand),
            typeof(ReportHeaderBand),
            typeof(ReportFooterBand),
            typeof(PageHeaderBand),
            typeof(PageFooterBand),
            typeof(GroupHeaderBand),
            typeof(GroupFooterBand),
            typeof(GroupHeaderBand),
            typeof(GroupFooterBand)
        };

        bool assignNavigateUrlUseReflection;
        void AssignNavigateUrl(XRControl xrControl, AccessInterop.Hyperlink hyperlink) {
            if(hyperlink != null) {
                xrControl.NavigateUrl = hyperlink.GetAddress(ref assignNavigateUrlUseReflection);
            }
        }
        static void AssignBorderStyle(XRControl xrControl, int borderColor, byte borderStyle, byte borderWidth) {
            xrControl.BorderColor = ToColor(borderColor);
            xrControl.Borders = ToBorderSide(borderStyle);
            xrControl.BorderWidth = Math.Max(1, PointsToPixel(borderWidth));
        }
        static void AssignBackColor(XRControl xrControl, int backColor, byte backStyle, Color? defaultBackColor = null) {
            xrControl.BackColor = backStyle == 0 ? (defaultBackColor ?? Color.Transparent) : ToColor(backColor);
        }
        static void AssignBounds(XRControl xrControl, short x, short y, short width, short height) {
            xrControl.Bounds = XRConvert.Convert(new Rectangle(x, y, width, height), GraphicsDpi.Twips, GraphicsDpi.HundredthsOfAnInch);
        }
        static void AssingPadding(XRLabel tgt, int left, int top, int right, int bottom) {
            PaddingInfo padding = new PaddingInfo(left, right, top, bottom, GraphicsDpi.Twips);
            padding.Dpi = GraphicsDpi.HundredthsOfAnInch;
            tgt.Padding = padding;
        }
        static Color ToColor(int color) {
            return ColorTranslator.FromOle(color);
        }
        static int PointsToPixel(int val) {
            return XRConvert.Convert(val, GraphicsDpi.Point, GraphicsDpi.Pixel);
        }
        static int TwipsToHOI(int val) {
            return XRConvert.Convert(val, GraphicsDpi.Twips, GraphicsDpi.HundredthsOfAnInch);
        }
        static DXFontStyle MakeFontStyle(short bold, bool italic, bool underline) {
            DXFontStyle fs = DXFontStyle.Regular;
            if(bold != 0)
                fs |= DXFontStyle.Bold;
            if(italic)
                fs |= DXFontStyle.Italic;
            if(underline)
                fs |= DXFontStyle.Underline;
            return fs;
        }
        static BorderSide ToBorderSide(byte borderStyle) {
            return (borderStyle == 0) ? BorderSide.None : BorderSide.All;
        }
        static DXDashStyle MakeLineStyle(byte style) {
            switch(style) {
                case 0:
                    return DXDashStyle.Custom;
                case 1:
                    return DXDashStyle.Solid;
                case 2:
                    return DXDashStyle.Dash;
                case 3:
                    return DXDashStyle.Dash;
                case 4:
                    return DXDashStyle.Dot;
                case 5:
                    return DXDashStyle.Dot;
                case 6:
                    return DXDashStyle.DashDot;
                case 7:
                    return DXDashStyle.DashDotDot;
                case 8:
                    return DXDashStyle.Solid;
                default:
                    return DXDashStyle.Custom;
            }
        }
        static TextAlignment MakeTextAlignment(byte textAlign) {
            switch(textAlign) {
                case 2:
                    return TextAlignment.TopCenter;
                case 3:
                    return TextAlignment.TopRight;
                case 4:
                    return TextAlignment.TopJustify;
                default:
                    return TextAlignment.TopLeft;
            }
        }
        static ImageSizeMode ToImageSizeMode(byte sizeMode) {
            switch(sizeMode) {
                case 0:
                    return ImageSizeMode.Normal;
                case 1:
                    return ImageSizeMode.StretchImage;
                case 3:
                    return ImageSizeMode.ZoomImage;
                default:
                    return ImageSizeMode.StretchImage;
            }
        }
        //static ImageViewMode ToImageViewMode(byte sizeMode) {
        //    switch(sizeMode) {
        //        case 0:
        //            return ImageViewMode.Clip;
        //        case 1:
        //            return ImageViewMode.Stretch;
        //        case 3:
        //            return ImageViewMode.Zoom;
        //        default:
        //            return ImageViewMode.Stretch;
        //    }
        //}
        string GetFormName(AccessInterop.Form form) {
            try {
                return form.Name;
            } catch {
                return (string)form.GetType().InvokeMember("Name", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public, null, form, new object[] { });
            }
        }
        System.Drawing.Image MakeImage(object pictureData) {
            byte[] bytes = pictureData as byte[];
            if(bytes == null)
                return null;
            AccessInterop.Form dummyForm = app.CreateForm(null, null);
            string formName = GetFormName(dummyForm);
            app.DoCmd.OpenForm(formName, AccessInterop.AcFormView.acDesign, "", "", AccessInterop.AcFormOpenDataMode.acFormPropertySettings, AccessInterop.AcWindowMode.acWindowNormal, null);
            AccessInterop.Image imgCtrl = app.CreateControl(formName, AccessInterop.AcControlType.acImage, AccessInterop.AcSection.acDetail, null, null, null, null, null, null) as AccessInterop.Image;
            imgCtrl.PictureData = pictureData;
            app.DoCmd.DoMenuItem(3, 1, 7, null, 70);
            app.DoCmd.DoMenuItem(3, 1, 2, null, 70);
            System.Windows.Forms.IDataObject data = System.Windows.Forms.Clipboard.GetDataObject();
            Bitmap bmp = data.GetData(typeof(Bitmap)) as Bitmap;
            app.DoCmd.Close(AccessInterop.AcObjectType.acForm, formName, AccessInterop.AcCloseSave.acSaveNo);
            System.Windows.Forms.Clipboard.SetDataObject("");

            return bmp;
        }
        static string MakeFormat(string format, byte decimalPlaces) {
            string upperFormat = format.ToUpper();
            if(upperFormat == "CURRENCY" || upperFormat == "EURO") {
                format = "C";
                if(decimalPlaces > 0 && decimalPlaces != 255)
                    format += decimalPlaces.ToString().Trim();
                else
                    format += '2';
            } else if(upperFormat == "GENERAL NUMBER" || upperFormat == "STANDARD")
                format = String.Empty;
            else if(upperFormat == "PERCENT") {
                format = "0";
                if(decimalPlaces > 0 && decimalPlaces != 255) {
                    format += '.';
                    format += new String('0', decimalPlaces);
                } else
                    format += ".00";
                format += '%';
            } else if(upperFormat == "SCIENTIFIC") {
                format = "E";
                if(decimalPlaces > 0 && decimalPlaces != 255)
                    format += decimalPlaces.ToString().Trim();
            } else if(upperFormat == "LONG DATE")
                format = "D";
            else if(upperFormat == "MEDIUM DATE")
                format = "d";
            else if(upperFormat == "SHORT DATE")
                format = "d";
            else if(upperFormat == "LONG TIME")
                format = "T";
            else if(upperFormat == "MEDIUM TIME")
                format = "t";
            else if(upperFormat == "SHORT TIME")
                format = "t";

            if(format == string.Empty)
                return "{0}";

            return "{0:" + format + '}';
        }
        void CloseDatabase() {
            if(app != null) {
                app.CloseCurrentDatabase();
                accessReport = null;
            }
        }
        XRControl CreateXRControl(AccessInterop.Control src) {
            Type xrType = src is AccessInterop.PageBreak
                ? typeof(XRPageBreak)
                : src is AccessInterop.Image
                ? typeof(XRPictureBox)
                : src is AccessInterop.Line
                ? typeof(XRLine)
                : src is AccessInterop.CheckBox
                ? typeof(XRCheckBox)
                : src is AccessInterop.Rectangle
                ? typeof(XRPanel)
                : src is AccessInterop.TextBox || src is AccessInterop.Label
                ? typeof(XRLabel)
                : null;
            if(xrType == null) {
                Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.Text_UnsupportedControl_Format, src.GetType().Name));
                xrType = typeof(XRLabel);
            }
            return CreateXRControl(xrType);
        }

        void ConvertPageSettings(AccessReportBase report) {
            try {
                MemoryStream stream = new MemoryStream((byte[])report.GetPrintingSettings());
                BinaryReader reader = new BinaryReader(stream);
                /*
                int leftMargin = reader.ReadInt32();
                int topMargin = reader.ReadInt32();
                int rightMargin = reader.ReadInt32();
                int bottomMargin = reader.ReadInt32();
                int dataOnly = reader.ReadInt32();
                int itemsAcross = reader.ReadInt32();
                int rowSpacing = reader.ReadInt32();
                int columnSpacing = reader.ReadInt32();
                int defaultSize = reader.ReadInt32();
                int itemSizeWidth = reader.ReadInt32();
                int itemSizeHeight = reader.ReadInt32();
                int itemLayout = reader.ReadInt32();
                int fastPrint = reader.ReadInt32();
                int dataSheet = reader.ReadInt32();
                */
                //const int PM_AcrossThenDown = 1953;
                //const int PM_DownThenAcross = 1954;
                int leftMargin = reader.ReadInt32();
                int topMargin = reader.ReadInt32();
                int rightMargin = reader.ReadInt32();
                int bottomMargin = reader.ReadInt32();
                int dataOnly = reader.ReadInt32();
                int itemSizeWidth = reader.ReadInt32();
                int itemSizeHeight = reader.ReadInt32();
                int defaultSize = reader.ReadInt32();
                int itemsAcross = reader.ReadInt32();
                int rowSpacing = reader.ReadInt32();
                int columnSpacing = reader.ReadInt32();
                int itemLayout = reader.ReadInt32();
                int fastPrint = reader.ReadInt32();
                int dataSheet = reader.ReadInt32();
                stream.Close();

                TargetReport.PaperKind = DXPaperKind.Custom;

                TargetReport.PageWidth = TwipsToHOI(report.LogicalPageWidth + leftMargin + rightMargin);
                int logicalPageHeight;
                try {
                    logicalPageHeight = report.LogicalPageHeight;
                } catch(COMException) {
                    logicalPageHeight = 7 * report.LogicalPageWidth / 5;
                }
                TargetReport.PageHeight = TwipsToHOI(logicalPageHeight + topMargin + bottomMargin);

                TargetReport.Margins.Top = TwipsToHOI(topMargin);
                TargetReport.Margins.Bottom = TwipsToHOI(bottomMargin);
                TargetReport.Margins.Right = TwipsToHOI(rightMargin);
                TargetReport.Margins.Left = TwipsToHOI(leftMargin);

                /*
                DetailBand detail = (DetailBand)fTargetReport.Bands[BandKind.Detail];
                detail.MultiColumn.ColumnCount = itemsAcross;
                detail.MultiColumn.Direction = itemLayout == PM_AcrossThenDown ? ColumnDirection.AcrossThenDown : ColumnDirection.DownThenAcross;
                */
            } catch(Exception e) {
                Tracer.TraceError(NativeSR.TraceSource, e);
            }
        }

        void ConvertLabelToXRLabel(AccessInterop.Label source, XRLabel target) {
            SetControlName(target, source.Name);
            AssignBackColor(target, source.BackColor, source.BackStyle);
            AssignBorderStyle(target, source.BorderColor, source.BorderStyle, source.BorderWidth);
            AssignBounds(target, source.Left, source.Top, source.Width, source.Height);
            AssingPadding(target, source.LeftMargin, source.TopMargin, source.RightMargin, source.BottomMargin);
            target.Text = source.Caption;
            target.Font = new DXFont(source.FontName, source.FontSize, MakeFontStyle(source.FontBold, source.FontItalic, source.FontUnderline));
            target.ForeColor = ToColor(source.ForeColor);
            target.TextAlignment = MakeTextAlignment(source.TextAlign);
            target.Visible = source.Visible;
            target.CanGrow = false;
            AssignNavigateUrl(target, source.Hyperlink);
        }
        void ConvertUnsupportedLabel(AccessInterop.Control src, XRLabel target) {
            SetControlName(target, src.Name);
            if(src is AccessInterop.SubForm) {
                var subform = (AccessInterop.SubForm)src;
                AssignBounds(target,
                    GetPropertyValue(() => subform.Left, (short)0),
                    GetPropertyValue(() => subform.Top, (short)0),
                    GetPropertyValue(() => subform.Width, (short)150),
                    GetPropertyValue(() => subform.Height, (short)50));
            } else if(src is AccessInterop.BoundObjectFrame) {
                var boundObjectFrame = (AccessInterop.BoundObjectFrame)src;
                AssignBounds(target,
                    GetPropertyValue(() => boundObjectFrame.Left, (short)0),
                    GetPropertyValue(() => boundObjectFrame.Top, (short)0),
                    GetPropertyValue(() => boundObjectFrame.Width, (short)150),
                    GetPropertyValue(() => boundObjectFrame.Height, (short)50));
            } else
                target.Size = new Size(150, 50);
            target.Text = string.Format(Messages.Text_UnsupportedControl_Format, src.GetType().Name);
        }
        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
        static T GetPropertyValue<T>(Func<T> getValue, T defaultValue) {
            try {
                return getValue();
            } catch(Exception e) {
                if(e is AccessViolationException || e is COMException)
                    return defaultValue;
                throw;
            }
        }
        void ConvertTextBoxToXRLabel(AccessInterop.TextBox src, XRLabel tgt) {
            SetControlName(tgt, src.Name);
            AssignBackColor(tgt, src.BackColor, src.BackStyle);
            AssignBorderStyle(tgt, src.BorderColor, src.BorderStyle, src.BorderWidth);
            AssingPadding(tgt, src.LeftMargin, src.TopMargin, src.RightMargin, src.BottomMargin);
            tgt.CanGrow = src.CanGrow;
            tgt.CanShrink = src.CanShrink;
            tgt.Font = new DXFont(src.FontName, src.FontSize, MakeFontStyle(src.FontBold, src.FontItalic, src.FontUnderline));
            tgt.Summary.FormatString = MakeFormat(src.Format, src.DecimalPlaces);
            //src.FormatConditions;
            //src.FormatPictureText;
            AssignBounds(tgt, src.Left, src.Top, src.Width, src.Height);
            //src.InputMask;
            //src.LabelAlign;
            //src.LabelX;
            //src.LabelY;
            //src.PostalAddress;
            tgt.TextAlignment = MakeTextAlignment(src.TextAlign);
            //src.TextFontCharSet;
            if(src.Vertical)
                tgt.Angle = -90;
            tgt.Visible = src.Visible;

            BindAccessDataToControl(tgt, nameof(tgt.Text), src.ControlSource, MakeFormat(src.Format, src.DecimalPlaces));
            AssignNavigateUrl(tgt, src.Hyperlink);
        }

        void ConvertPageBreak(AccessInterop.PageBreak src, XRPageBreak tgt) {
            SetControlName(tgt, src.Name);
            tgt.Left = TwipsToHOI(src.Left);
            tgt.Top = TwipsToHOI(src.Top);
            tgt.Visible = src.Visible;
        }

        void ConvertPictureBox(AccessInterop.Image src, XRPictureBox tgt) {
            SetControlName(tgt, src.Name);
            AssignBackColor(tgt, src.BackColor, src.BackStyle);
            AssignBorderStyle(tgt, src.BorderColor, src.BorderStyle, src.BorderWidth);
            AssignBounds(tgt, src.Left, src.Top, src.Width, src.Height);
            //src.ImageHeight;
            //src.ImageWidth;
            //src.Picture;
            //src.PictureAlignment;
            //src.PictureData;
            //src.PictureTiling;
            //src.PictureType;
            tgt.Sizing = ToImageSizeMode(src.SizeMode);
            tgt.Image = MakeImage(src.PictureData);
            tgt.Visible = src.Visible;
            AssignNavigateUrl(tgt, src.Hyperlink);
        }

        void ConvertLine(AccessInterop.Line src, XRLine tgt) {
            SetControlName(tgt, src.Name);
            AssignBorderStyle(tgt, src.BorderColor, 0, src.BorderWidth);
            AssignBounds(tgt, src.Left, src.Top, src.Width, src.Height);
            tgt.LineWidth = (int)tgt.BorderWidth;
            tgt.LineStyle = MakeLineStyle(src.BorderStyle);
            tgt.ForeColor = tgt.BorderColor;
            tgt.Visible = src.Visible;
            if(tgt.Width < tgt.BorderWidth)
                tgt.Width = (int)tgt.BorderWidth;
            if(tgt.Height < tgt.BorderWidth)
                tgt.Height = (int)tgt.BorderWidth;

            tgt.LineDirection = src.LineSlant ? LineDirection.Slant : LineDirection.BackSlant;

            if(tgt.Width <= tgt.BorderWidth || src.Width == 0)
                tgt.LineDirection = LineDirection.Vertical;
            if(tgt.Height <= tgt.BorderWidth || src.Height == 0)
                tgt.LineDirection = LineDirection.Horizontal;
        }

        void ConvertRectangle(AccessInterop.Rectangle src, XRPanel tgt) {
            SetControlName(tgt, src.Name);
            AssignBackColor(tgt, src.BackColor, src.BackStyle, Color.Black);
            AssignBorderStyle(tgt, src.BorderColor, 1, src.BorderWidth);
            AssignBounds(tgt, src.Left, src.Top, src.Width, src.Height);
            tgt.Visible = src.Visible;
        }

        void ConvertCheckBox(AccessInterop.CheckBox src, XRCheckBox tgt) {
            SetControlName(tgt, src.Name);
            AssignBorderStyle(tgt, src.BorderColor, src.BorderStyle, src.BorderWidth);
            AssignBounds(tgt, src.Left, src.Top, src.Width, src.Height);
            //src.ControlSource;
            //src.LabelAlign;
            //src.LabelX;
            //src.LabelY;
            //src.TripleState;
            tgt.Visible = src.Visible;

            BindAccessDataToControl(tgt, nameof(tgt.CheckBoxState), src.ControlSource);
        }
        void BindAccessDataToControl(XRControl tgt, string propertyName, string accessControlSource, string formatString = "") {
            accessControlSource = accessControlSource.Trim();
            if(!string.IsNullOrEmpty(accessControlSource) && accessControlSource[0] == '=') {
                string exscapedExpressionBindingStub = string.Format("Iif(True, '{0}', '{1}')", Design.Import.CrystalFormula.FormulaParser.NotSupportedStub, accessControlSource.Replace("'", "''"));
                tgt.ExpressionBindings.Add(new ExpressionBinding(propertyName, exscapedExpressionBindingStub));
            } else {
                BindDataToControl(tgt, propertyName, accessControlSource, formatString);
            }
        }

        void ConvertAccessControlToXRControl(AccessInterop.Control src, XRControl tgt) {
            if(src is AccessInterop.TextBox)
                ConvertTextBoxToXRLabel((AccessInterop.TextBox)src, (XRLabel)tgt);
            else if(src is AccessInterop.PageBreak)
                ConvertPageBreak((AccessInterop.PageBreak)src, (XRPageBreak)tgt);
            else if(src is AccessInterop.Image)
                ConvertPictureBox((AccessInterop.Image)src, (XRPictureBox)tgt);
            else if(src is AccessInterop.Line)
                ConvertLine((AccessInterop.Line)src, (XRLine)tgt);
            else if(src is AccessInterop.CheckBox)
                ConvertCheckBox((AccessInterop.CheckBox)src, (XRCheckBox)tgt);
            else if(src is AccessInterop.Rectangle)
                ConvertRectangle((AccessInterop.Rectangle)src, (XRPanel)tgt);
            else if(src is AccessInterop.Label)
                ConvertLabelToXRLabel((AccessInterop.Label)src, (XRLabel)tgt);
            else
                ConvertUnsupportedLabel(src, (XRLabel)tgt);
        }

        void ConvertControls(AccessInterop._Section src, Band tgt) {
            foreach(AccessInterop.Control ctrl in src.Controls) {
                XRControl xrControl = CreateXRControl(ctrl);
                tgt.Controls.Add(xrControl);
                try {
                    ConvertAccessControlToXRControl(ctrl, xrControl);
                } catch(Exception e) {
                    Tracer.TraceError(NativeSR.TraceSource, e);
                }
            }
            //			for(int i = 0; i < src.Controls.Count; i++) {
            //				AccessInterop.Control ctrl = src.Controls[i] as AccessInterop.Control;
            //                XRControl xrControl = CreateXRControl(ctrl);
            //                if(xrControl != null) {
            //                    tgt.Controls.Add(xrControl);
            //                    try {
            //                        ConvertAccessControlToXRControl(ctrl, xrControl);
            //                    } catch {
            //                    }
            //                }
            //            }
        }

        void ConvertSection(AccessInterop._Section src, Band tgt) {
            SetControlName(tgt, src.Name);

            //tgt.BackColor = ToColor(src.BackColor);
            tgt.BackColor = Color.Transparent;
            tgt.Visible = src.Visible;
            tgt.Height = TwipsToHOI(src.Height);

            SetParentStyleUsing(tgt, false);
            ConvertControls(src, tgt);
        }

        GroupHeaderBand ConvertGroupHeaderSection(AccessInterop.GroupLevel level, int sectionIndex) {
            AccessInterop._Section section = AccessReport.GetSection(sectionIndex);
            GroupHeaderBand header = GetOrCreateBandByType<GroupHeaderBand>();
            if(section != null)
                ConvertSection(section, header);
            else
                header.HeightF = 0;
            string levelControlSource = level.ControlSource;
            if(!string.IsNullOrEmpty(levelControlSource) && (levelControlSource.Contains("[") || levelControlSource.Contains("&") || levelControlSource.Contains("=")))
                Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.Trace_LevelWithExpressionIsNotSupported_Format, levelControlSource));
            header.GroupFields.Add(new GroupField(levelControlSource, level.SortOrder ? XRColumnSortOrder.Descending : XRColumnSortOrder.Ascending));
            return header;
        }

        void ConvertGroupFooterSection(int sectionIndex) {
            AccessInterop._Section section = AccessReport.GetSection(sectionIndex);
            if(section != null) {
                GroupFooterBand footer = GetOrCreateBandByType<GroupFooterBand>();
                ConvertSection(section, footer);
            }
        }

        void ConvertGrouping() {
            int sectionIndex = 5;
            for(int i = 0; i < 10; i++) {
                AccessInterop.GroupLevel level = AccessReport.GetGroupLevel(i);
                if(level == null)
                    break;
                GroupHeaderBand header = ConvertGroupHeaderSection(level, sectionIndex++);
                ConvertGroupFooterSection(sectionIndex++);
                if(header != null)
                    header.Level = 0;
            }
        }
        void ConvertSections() {
            for(int i = 0; i <= 4; i++) {
                AccessInterop._Section section = AccessReport.GetSection(i);
                if(section != null) {
                    Band band = GetOrCreateBandByType(bandTypes[i]);
                    ConvertSection(section, band);
                }
            }
            ConvertGrouping();
        }
        void PrepareDataSource(string fileName) {
            string commandText = "";
            string tableName = GetTableName(ref commandText);
            CloseDatabase();

            if(tableName.Length == 0)
                return;

            const string connectionStringFormat = @"Provider=Microsoft.Jet.OLEDB.4.0;User ID=Admin;Data Source={0};Mode=Share Deny None;Extended Properties="""";Jet OLEDB:System database="""";Jet OLEDB:Registry Path="""";Jet OLEDB:Database Password="""";Jet OLEDB:Engine Type=5;Jet OLEDB:Database Locking Mode=1;Jet OLEDB:Global Partial Bulk Ops=2;Jet OLEDB:Global Bulk Transactions=1;Jet OLEDB:New Database Password="""";Jet OLEDB:Create System Database=False;Jet OLEDB:Encrypt Database=False;Jet OLEDB:Don't Copy Locale on Compact=False;Jet OLEDB:Compact Without Replica Repair=False;Jet OLEDB:SFP=False";
            string connectionString = string.Format(connectionStringFormat, fileName);
            OleDbConnection connection = new OleDbConnection(connectionString);

            OleDbCommand selectCommand = new OleDbCommand {
                Connection = connection,
                CommandText = commandText
            };

            OleDbDataAdapter dataAdapter = CreateOleDBDataAdapter(selectCommand, tableName);
            AssignDataAdapter(dataAdapter);
        }
        private string GetTableName(ref string commandText) {
            string recordSource = AccessReport.GetRecordSource();
            if(GetObjectByName(app.CurrentData.AllTables, recordSource) != null) {
                commandText = "SELECT * FROM " + recordSource;
                return recordSource;
            } else if(GetObjectByName(app.CurrentData.AllQueries, recordSource) != null) {
                commandText = GetSql(recordSource);
                return ParseTableName(commandText);
            }
            commandText = recordSource;
            return ParseTableName(recordSource);
        }
        private string GetSql(string name) {
            object db = app.GetType().InvokeMember("CurrentDb", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, app, new object[] { });
            object queryDefs = db.GetType().InvokeMember("QueryDefs", BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, db, new object[] { });
            object query = queryDefs.GetType().InvokeMember("Item", BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, queryDefs, new object[] { name });
            string sql = (string)query.GetType().InvokeMember("SQL", BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, query, new object[] { });

            //DaoInterop.Database db = app.CurrentDb();
            ////DaoInterop.QueryDef query = db.OpenQueryDef(name);
            //DaoInterop.QueryDef query = db.QueryDefs[name];
            //string sql = query.SQL;
            sql = sql.Replace((char)0xD, ' ');
            sql = sql.Replace((char)0xA, ' ');
            db = null;
            queryDefs = null;
            query = null;
            return sql;
        }
        AccessInterop.AccessObject GetObjectByName(AccessInterop.AllObjects allObjects, string name) {
            for(int i = 0; i < allObjects.Count; i++) {
                if(allObjects[i].Name == name)
                    return allObjects[i];
            }
            return null;
        }
        void PerformConvert(string fileName, string reportName) {
            PrepareDataSource(fileName);

            OpenCurrentDatabase(fileName);
            OpenReport(reportName);
            try {
                CursorStorage.SetCursor(Cursors.WaitCursor);

                TargetReport.ReportUnit = ReportUnit.HundredthsOfAnInch;
                ConvertPageSettings(AccessReport);
                SetControlName(TargetReport, AccessReport.Name);
                if(TargetReport.Name != AccessReport.Name)
                    TargetReport.DisplayName = AccessReport.Name;
                ConvertSections();
            } finally {
                CursorStorage.RestoreCursor();
            }
        }

        void OpenCurrentDatabase(string fileName) {
            try {
                app.GetType().InvokeMember("OpenCurrentDatabase", BindingFlags.InvokeMethod | BindingFlags.Instance, null, app, new object[] { fileName, false, string.Empty });
            } catch {
                try {
                    app.GetType().InvokeMember("OpenCurrentDatabase", BindingFlags.InvokeMethod | BindingFlags.Instance, null, app, new object[] { fileName, false });
                } catch {
                }
            }
            accessReport = null;
        }

        void OpenReport(string reportName) {
            try {
                app.DoCmd.GetType().InvokeMember("OpenReport", BindingFlags.InvokeMethod | BindingFlags.Instance, null, app.DoCmd, new object[] { reportName, AccessInterop.AcView.acViewDesign, "", "", AccessInterop.AcWindowMode.acHidden, null });
            } catch {
                try {
                    app.DoCmd.GetType().InvokeMember("OpenReport", BindingFlags.InvokeMethod | BindingFlags.Instance, null, app.DoCmd, new object[] { reportName, AccessInterop.AcView.acViewDesign, "", "" });
                } catch(Exception e) {
                    Tracer.TraceError(NativeSR.TraceSource, e);
                }
            }
            accessReport = null;
        }

        protected override void ConvertInternal(string fileName) {
            if(PrinterSettings.InstalledPrinters.Count == 0) {
                if(MessageBox.Show(Messages.MessageBox_InstallPrinter,
                                    "Import", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo = new System.Diagnostics.ProcessStartInfo("rundll32",
                    "printui.dll,PrintUIEntry /il");
                process.Start();
                process.WaitForExit();
                process.Close();
                if(PrinterSettings.InstalledPrinters.Count == 0)
                    return;
            }

            //Type accessAppType = Type.GetTypeFromProgID("Access.Application", false);
            //app = Activator.CreateInstance(accessAppType) as AccessInterop._Application;
            app = (AccessInterop._Application)new AccessInterop.ApplicationClass();
            try {
                app.Visible = false;

                OpenCurrentDatabase(fileName);

                AccessInterop.AllObjects allReports = app.GetAllReports();
                string[] reportNames = GetReportNames(allReports);
                string reportName = "";
                if(!string.IsNullOrEmpty(this.reportName)) {
                    reportName = this.reportName;
                } else if(reportIndex.HasValue) {
                    if(reportIndex.Value < 0 || reportIndex.Value >= reportNames.Length)
                        Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.CommandLine_ReportIndex_Warning, reportNames.Length - 1));
                    else
                        reportName = reportNames[reportIndex.Value];
                } else if(reportNames.Length == 1) {
                    reportName = reportNames[0];
                } else {
                    using(var dlg = new AccessReportSelectionForm()) {
                        var designerHost = TargetReport.Site.GetService<IDesignerHost>();
                        LookAndFeel.DesignService.DesignLookAndFeelHelper.SetParentLookAndFeel(dlg, designerHost);
                        dlg.SetReportsList(reportNames);
                        IWin32Window owner = DialogRunner.GetOwnerWindow();
                        if(dlg.ShowDialog(owner) == DialogResult.OK && !string.IsNullOrEmpty(dlg.SelectedReport)) {
                            reportName = dlg.SelectedReport;
                        }
                    }
                }
                if(!string.IsNullOrEmpty(reportName)) {
                    OpenReport(reportName);
                    PerformConvert(fileName, reportName);
                }
            }
            /*
            catch (Exception e) {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
            */
            finally {
                CloseDatabase();
                app.Quit(AccessInterop.AcQuitOption.acQuitSaveNone);
                app = null;
            }
        }

        string[] GetReportNames(AccessInterop.AllObjects allReports) {
            var reports = new List<string>(allReports.Count);
            for(int i = 0; i < allReports.Count; i++) {
                AccessInterop.AccessObject rpt = allReports[i];
                if(rpt != null)
                    reports.Add(rpt.Name);
            }
            return reports.ToArray();
        }
    }

    static class AccessApplicationExtension {
        public static AccessInterop.AllObjects GetAllReports(this AccessInterop._Application application) {
            var project = application.CurrentProject;
            var allReports = GetProperty<AccessInterop.AllObjects>(project, "AllReports");
            return allReports;
        }

        public static T GetProperty<T>(object obj, string propertyName) {
            Type type = obj.GetType();
            T result = (T)type.InvokeMember(propertyName, BindingFlags.GetProperty, null, obj, null);
            return result;
        }

        public static string GetAddress(this AccessInterop.Hyperlink hyperlink, ref bool useReflection) {
            if(useReflection) {
                try {
                    return GetProperty<string>(hyperlink, "Address");
                } catch {
                    return "";
                }
            }
            try {
                return hyperlink.get_Address();
            } catch {
                useReflection = true;
                return GetAddress(hyperlink, ref useReflection);
            }
        }
    }
}
#endif

#endregion