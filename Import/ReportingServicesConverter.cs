using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DevExpress.Data.Browsing;
using DevExpress.Data.Filtering;
using DevExpress.Utils;
using DevExpress.Utils.Internal;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Native;
using DevExpress.XtraReports.Import.ReportingServices;
using DevExpress.XtraReports.Import.ReportingServices.DataSources;
using DevExpress.XtraReports.Import.ReportingServices.Expressions;
using DevExpress.XtraReports.Import.ReportingServices.Tablix;
using DevExpress.XtraReports.Native;
using DevExpress.XtraReports.Parameters;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.UI.CrossTab;
using DevExpress.XtraRichEdit;
using DevExpress.XtraRichEdit.API.Native;

namespace DevExpress.XtraReports.Import {
    public class ReportingServicesConverter : ExternalConverterBase, IReportingServicesConverter {
        const string defaultUnit = "Inch";
        readonly static Dictionary<string, Type> dataTypesMap = new Dictionary<string, Type>() {
            { "String", typeof(string) },
            { "DateTime", typeof(DateTime) },
            { "Integer", typeof(int) },
            { "Float", typeof(float) },
            { "Boolean", typeof(bool) },
        };

        readonly static string[] supportedNamespaces = {
            "http://schemas.microsoft.com/sqlserver/reporting/2005/01/reportdefinition",
            "http://schemas.microsoft.com/sqlserver/reporting/2008/01/reportdefinition",
            "http://schemas.microsoft.com/sqlserver/reporting/2010/01/reportdefinition",
            "http://schemas.microsoft.com/sqlserver/reporting/2016/01/reportdefinition",
        };

        public UnrecognizedFunctionBehavior UnrecognizedFunctionBehavior { get; set; } = UnrecognizedFunctionBehavior.InsertWarning;
        public bool IgnoreQueryValidation { get; set; } = false;

        UnitConverter unitConverter;
        string reportFolder;
        XNamespace xmlns;
        readonly XNamespace rdns = XNamespace.Get("http://schemas.microsoft.com/SQLServer/reporting/reportdesigner");

        readonly ITypeResolutionService typeResolver;
        readonly IDesignerHost designerHost;
        readonly string currentProjectRootNamespace;
        UnitConverter IReportingServicesConverter.UnitConverter => unitConverter;
        string IReportingServicesConverter.ReportFolder => reportFolder;
        Dictionary<string, DataPair> dataSetToDataPairMap;

        public ReportingServicesConverter(ITypeResolutionService typeResolver = null, IDesignerHost designerHost = null, string currentProjectRootNamespace = null) {
            this.typeResolver = typeResolver;
            this.designerHost = designerHost;
            this.currentProjectRootNamespace = currentProjectRootNamespace;
        }

        protected override void ConvertInternal(string fileName) {
            reportFolder = Path.GetDirectoryName(fileName);
            TargetReport.DisplayName = Path.GetFileNameWithoutExtension(fileName);
            using(FileStream fileStream = File.OpenRead(fileName)) {
                XDocument rdlcDocument = SafeXml.CreateXDocument(fileStream);
                xmlns = rdlcDocument.Root.GetDefaultNamespace();
                if(!supportedNamespaces.Contains(xmlns.NamespaceName))
                    throw new NotSupportedException(Messages.InvalidFormat_Error);
                ProcessRoot(rdlcDocument.Root);
                dataSetToDataPairMap.Clear();
            }
        }
        protected override XtraReport CreateTargetReport() {
            return new XtraReport() {
                Bands = {
                    new TopMarginBand(),
                    new BottomMarginBand()
                },
                PaperKind = System.Drawing.Printing.PaperKind.Custom,
                Landscape = false,
                Margins = new System.Drawing.Printing.Margins(),
                Font = new Font("Arial", 10)
            };
        }

        void ProcessRoot(XElement root) {
            ProcessMeasureUnit(root.Element(rdns + "ReportUnitType"));
            ProcessDataSources(root);

            IterateElements(root, (e, name) => {
                switch(name) {
                    case "ReportSections":
                        ProcessReportSection(e.Element(xmlns + "ReportSection"));
                        break;
                    case "ReportParameters":
                        ProcessReportParameters(e);
                        break;
                    case "EmbeddedImages":
                        ProcessEmbeddedImages(e);
                        break;
                    case "Variables":
                        TraceInfo(Messages.Variables_NotSupported);
                        break;
                    case "Code":
                        TraceInfo(Messages.Code_NotSupported);
                        break;
                    case "CodeModules":
                        TraceInfo(Messages.CodeModules_NotSupported);
                        break;
                    case "Body":                      // < 2008
                        ProcessBody(e);
                        break;
                    case "Page":                      // < 2008
                        ProcessPage(e);
                        break;
                    case "DefaultFontFamily":
                        ProcessFont(e.Value, TargetReport, "FontFamily", null);
                        break;
                    case "SnapToGrid":
                        TargetReport.SnappingMode = e.Value.Equals("true") ? SnappingMode.SnapToGridAndSnapLines : SnappingMode.SnapLines;
                        break;
                    case "DrawGrid":
                        TargetReport.DrawGrid = e.Value.Equals("true");
                        break;
                    case "PageHeader":
                        ProcessPageBand(e, BandKind.PageHeader);
                        break;
                    case "PageFooter":
                        ProcessPageBand(e, BandKind.PageFooter);
                        break;
                    case "PageWidth":
                        TargetReport.PageWidth = unitConverter.ToInt(e.Value);
                        break;
                    case "PageHeight":
                        TargetReport.PageHeight = unitConverter.ToInt(e.Value);
                        break;
                    case "Language":                  //not supported
                    case "ConsumeContainerWhitespace"://not supported
                    case "ReportUnitType":            //handled
                    case "DataSources":               //handled
                    case "DataSets":                  //handled
                    case "ReportID":                  //not supported
                    case "ReportParametersLayout":    //not supported
                    case "AutoRefresh":               //not supported
                    case "Author":                    //not supported
                    case "Width":                     // < 2008
                    case "ReportTemplate":
                    case "InteractiveHeight":
                    case "InteractiveWidth":
                        break;
                    case "Description":
                        TargetReport.Tag = e.Value;
                        break;
                    default:
#if DEBUG
                        TraceInfo(Messages.RootElement_NotSupported_Format, name);
#endif
                        break;
                }
            });
        }

        void ProcessMeasureUnit(XElement reportUnit) {
            var measureUnit = reportUnit?.Value ?? defaultUnit;
            unitConverter = new UnitConverter(measureUnit);
            TargetReport.ReportUnit = unitConverter.ReportUnit;
        }

        void ProcessReportSection(XElement reportSection) {
            Guard.ArgumentNotNull(reportSection, nameof(reportSection));
            IterateElements(reportSection, (e, name) => {
                switch(name) {
                    case "Body":
                        ProcessBody(e);
                        break;
                    case "Page":
                        ProcessPage(e);
                        break;
                    case "Width":                     //not supported
                        break;
                    default:
                        TraceInfo(Messages.ReportSectionElement_NotSupported_Format, name);
                        break;
                }
            });
        }

        void ProcessPage(XElement page) {
            IterateElements(page, (e, name) => {
                switch(name) {
                    case "PageHeader":
                        ProcessPageBand(e, BandKind.PageHeader);
                        break;
                    case "PageFooter":
                        ProcessPageBand(e, BandKind.PageFooter);
                        break;
                    case "PageWidth":
                        TargetReport.PageWidth = unitConverter.ToInt(e.Value);
                        break;
                    case "PageHeight":
                        TargetReport.PageHeight = unitConverter.ToInt(e.Value);
                        break;
                    case "LeftMargin":
                        TargetReport.Margins.Left = unitConverter.ToInt(e.Value);
                        break;
                    case "RightMargin":
                        TargetReport.Margins.Right = unitConverter.ToInt(e.Value);
                        break;
                    case "TopMargin":
                        TargetReport.Bands[BandKind.TopMargin].HeightF = unitConverter.ToFloat(e.Value);
                        break;
                    case "BottomMargin":
                        TargetReport.Bands[BandKind.BottomMargin].HeightF = unitConverter.ToInt(e.Value);
                        break;
                    case "Columns":
                        var columns_detailBand = EnsureEmptyBand<DetailBand>(TargetReport, BandKind.Detail);
                        columns_detailBand.MultiColumn.ColumnCount = int.Parse(e.Value);
                        break;
                    case "ColumnSpacing":
                        var columnSpacing_detailBand = (DetailBand)TargetReport.Bands[BandKind.Detail];
                        if(columnSpacing_detailBand != null)
                            columnSpacing_detailBand.MultiColumn.ColumnSpacing = unitConverter.ToFloat(e.Value);
                        break;
                    case "InteractiveHeight":        //not supported
                    case "InteractiveWidth":         //not supported
                        break;
                    case "Style":                    //not supported in page
                        if(!e.HasElements)
                            break;
                        TraceInfo(Messages.StyleElement_NotSupported);
                        break;
                    default:
                        TraceInfo(Messages.PageElement_NotSupported_Format, name);
                        break;
                }
            });
            TargetReport.PaperKind = PageSizeInfo.GetAppropriatePaperKind(TargetReport.PageWidth, TargetReport.PageHeight);
        }

        #region bands
        void ProcessBody(XElement body) {
            IterateElements(body, (e, name) => {
                switch(name) {
                    case "ReportItems":
                        ProcessReportItems(e, TargetReport);
                        break;
                    case "Height":
                        break;
                    case "Style":
                        ProcessStyle(e, TargetReport);
                        break;
                    default:
                        TraceInfo(Messages.BodyElement_NotSupported_Format, name);
                        break;
                }
            });
            PostProcessClean(TargetReport);
        }

        void ProcessPageBand(XElement pageBandElement, BandKind bandKind) {
            Guard.ArgumentMatch(bandKind, nameof(bandKind), x => x == BandKind.PageHeader || x == BandKind.PageFooter);
            var band = EnsureEmptyBand<PageBand>(TargetReport, bandKind);
            ApplyPrintPageBandOnPage(band, pageBandElement.Element(xmlns + "PrintOnFirstPage"), PrintOnPages.NotWithReportHeader);
            ApplyPrintPageBandOnPage(band, pageBandElement.Element(xmlns + "PrintOnLastPage"), PrintOnPages.NotWithReportFooter);

            IterateElements(pageBandElement, (e, name) => {
                switch(name) {
                    case "Height":
                        band.HeightF = unitConverter.ToFloat(e.Value);
                        break;
                    case "Style":
                        ProcessStyle(e, band);
                        break;
                    case "ReportItems":
                        ProcessReportItems(e, band);
                        break;
                    default:
                        break;
                }
            });
        }

        void ApplyPrintPageBandOnPage(PageBand band, XElement printOnPage, PrintOnPages excludingMode) {
            bool shouldPrintOnPage = string.Equals(printOnPage?.Value, "true", StringComparison.InvariantCultureIgnoreCase);
            if(!shouldPrintOnPage) {
                var newBandKind = excludingMode == PrintOnPages.NotWithReportHeader
                    ? BandKind.ReportHeader
                    : BandKind.ReportFooter;
                EnsureEmptyBand<Band>(band.Report, newBandKind);
                band.PrintOn |= excludingMode;
            }
        }
        #endregion

        #region controls
        void ProcessReportItems(XElement items, XtraReportBase report) {
            Band detailBand = EnsureEmptyBand<Band>(report, BandKind.Detail);
            ProcessReportItems(items, detailBand);
        }
        void ProcessReportItems(XElement items, XRControl container) {
            float yBodyOffset = 0;
            IterateReportItemsElements(items, (e, name) => ProcessReportItem(e, ref container, ref yBodyOffset));
        }

        void ProcessReportItem(XElement reportItem, ref XRControl container, ref float yBodyOffset) {
            switch(reportItem.Name.LocalName) {
                case "Textbox":
                    ProcessTextboxControl(reportItem, container, yBodyOffset);
                    break;
                case "Rectangle":
                    ProcessRectangleControl(reportItem, container, yBodyOffset);
                    break;
                case "Table":
                    TraceInfo(Messages.TableControl_NotSupported);
                    break;
                case "Tablix":
                    ProcessTablixControl(reportItem, ref container, ref yBodyOffset);
                    break;
                case "Image":
                    ProcessImageControl(reportItem, container, yBodyOffset);
                    break;
                case "List":
                    TraceInfo(Messages.ListControl_NotSupported);
                    break;
                case "Line":
                    ProcessLineControl(reportItem, container, yBodyOffset);
                    break;
                case "Subreport":
                    ProcessSubreportControl(reportItem, container, yBodyOffset);
                    break;
                case "Chart":
                    ProcessChartControl(reportItem, container, yBodyOffset);
                    break;
                default:
                    TraceWarning(Messages.ReportItemsElement_NotSupported_Format, reportItem.Name.LocalName);
                    break;
            }
        }

        void IReportingServicesConverter.ProcessReportItem(XElement chartElement, XRControl container, ref float yBodyOffset) => ProcessReportItem(chartElement, ref container, ref yBodyOffset);

        #region TextBox control
        void ProcessTextboxControl(XElement textBoxElement, XRControl container, float yBodyOffset) {
            IEnumerable<XElement> runs = textBoxElement.Descendants(xmlns + "TextRun");
            if(runs.Count() > 1) {
                ProcessTextBoxAsRichText(textBoxElement, container, yBodyOffset);
                return;
            }
            XRLabel control = container as XRLabel;
            if(control == null) {
                control = CreateXRControl<XRLabel>(container);
                control.Multiline = true;
                control.CanGrow = true;
            }
            ProcessTextBoxAsLabel(textBoxElement, control, yBodyOffset);
        }
        public void ProcessTextBoxAsLabel(XElement textBoxElement, XRLabel control, float yBodyOffset) {
            var runs = textBoxElement.Descendants(xmlns + "TextRun");
            if(runs.Count() > 1) {
                throw new NotSupportedException("Label can be converted from single TextRun.");
            }
            this.SetComponentName(control, textBoxElement);
            control.TextAlignment = TextAlignment.TopLeft;
            IterateElements(textBoxElement, (e, name) => {
                ExpressionParserResult expressionParserResult;
                TryGetExpression(e, control, true, out expressionParserResult);
                switch(name) {
                    case "Value":
                        if(expressionParserResult != null) {
                            if(expressionParserResult.HasSummary)
                                control.Summary.Running = SummaryRunning.Group;
                            control.ExpressionBindings.Add(expressionParserResult.ToExpressionBinding(nameof(control.Text)));
                            UpdateControlsReportDataSource(control, expressionParserResult);
                        } else
                            control.Text = e.Value;
                        break;
                    case "Paragraphs":
                        ProcessSingleParagraphAsText(e, control);
                        break;
                    default:
                        ProcessCommonControlProperties(e, control, yBodyOffset);
                        break;
                }
            });
        }
        public void SetComponentName<T>(T component, string name = null) {
            NamingMapper.GenerateAndAssignXRControlName(component, name);
        }

        void ProcessTextBoxAsRichText(XElement textBoxElement, XRControl container, float yBodyOffset) {
            var control = CreateXRControl<XRRichText>(container, textBoxElement);
            IterateElements(textBoxElement, (e, name) => {
                switch(name) {
                    case "Paragraphs":
                        using(var documentServer = CreateRichDocumentServer(container.GetEffectiveFont(), container.GetEffectiveForeColor())) {
                            documentServer.BeginUpdate();
                            ProcessRichParagraphs(e, control, documentServer);
                            documentServer.EndUpdate();
                            control.Rtf = documentServer.RtfText;
                        }
                        break;
                    default:
                        ProcessCommonControlProperties(e, control, yBodyOffset);
                        break;
                }
            });
        }

        void ProcessRichParagraphs(XElement paragraphs, XRRichText control, RichEditDocumentServer richDocumentServer) {
            bool isFirstParagraph = true;
            IterateElements(paragraphs, xmlns + "Paragraph", (e, name) => {
                if(isFirstParagraph)
                    isFirstParagraph = false;
                else
                    richDocumentServer.Document.Paragraphs.Append();
                ProcessRichParagraph(e, control, richDocumentServer);
            });
        }

        void ProcessRichParagraph(XElement paragraph, XRRichText control, RichEditDocumentServer documentServer) {
            var paragraphProperties = documentServer.Document.BeginUpdateParagraphs(documentServer.Document.Paragraphs.Last().Range);
            IterateElements(paragraph, (e, name) => {
                switch(name) {
                    case "TextRuns":
                        ProcessRichTextRuns(e, control, documentServer);
                        break;
                    case "SpaceAfter":
                        paragraphProperties.SpacingAfter = float.Parse(unitConverter.CutUnits(e.Value));
                        break;
                    case "Style":
                        ProcessRichParagraphStyle(e, paragraphProperties, documentServer);
                        break;
                    case "LeftIndent":
                        paragraphProperties.FirstLineIndent = float.Parse(unitConverter.CutUnits(e.Value));
                        break;
                    default:
                        TraceWarning(Messages.RichParagraphElement_NotSupported_Format, name);
                        break;
                }
            });
            documentServer.Document.EndUpdateParagraphs(paragraphProperties);
        }

        readonly static Dictionary<string, ParagraphAlignment?> richTextAlignmentMap = new Dictionary<string, ParagraphAlignment?>() {
            { "General", null },
            { "Left", ParagraphAlignment.Left },
            { "Center", ParagraphAlignment.Center },
            { "Right", ParagraphAlignment.Right },
        };
        void ProcessRichParagraphStyle(XElement style, ParagraphProperties paragraphProperties, RichEditDocumentServer documentServer) {
            IterateElements(style, (e, name) => {
                switch(name) {
                    case "TextAlign":
                        paragraphProperties.Alignment = richTextAlignmentMap.GetValueOrDefault(e.Value, null);
                        break;
                    case "Format":    // not supported in rich
                        break;
                    default:
                        TraceInfo(Messages.RichParagraphElement_NotSupported_Format, name);
                        break;
                }
            });
        }

        void ProcessRichTextRuns(XElement textRuns, XRRichText control, RichEditDocumentServer documentServer) {
            IterateElements(textRuns, xmlns + "TextRun", (e, name) => {
                ProcessRichTextRun(e, control, documentServer);
            });
        }

        void ProcessRichTextRun(XElement textRun, XRRichText control, RichEditDocumentServer documentServer) {
            string value = textRun.Element(xmlns + "Value")?.Value;
            if(string.IsNullOrEmpty(value))
                return;
            DocumentRange range;
            ExpressionParserResult expressionResult;
            if(TryGetExpression(value, control, false, out expressionResult)) {
                TraceInfo(Messages.RichTextRunExpression_NotSupported_Format, control.Name, expressionResult.Expression);
                range = documentServer.Document.AppendText(string.Format(Messages.RichTextRunExpression_NotSupported_Format, control.Name, expressionResult.Expression));
            } else {
                bool isHtml = textRun.Element(xmlns + "MarkupType")?.Value == "HTML";
                range = isHtml
                    ? documentServer.Document.AppendHtmlText(value)
                    : documentServer.Document.AppendText(value);
            }
            IterateElements(textRun, (e, name) => {
                switch(name) {
                    case "Style":
                        ProcessRichTextRunStyle(e, range, documentServer);
                        break;
                    case "MarkupType":    // handled
                    case "Value":         // handled
                    case "Label":         // not supported
                    case "ActionInfo":    // not supported
                        break;
                    default:
                        TraceInfo(string.Format(Messages.RichTextRunElement_NotSupported_Format, name));
                        break;
                }
            });
        }

        void ProcessRichTextRunStyle(XElement style, DocumentRange range, RichEditDocumentServer documentServer) {
            var characterProperties = documentServer.Document.BeginUpdateCharacters(range);
            characterProperties.Assign(documentServer.Document.DefaultCharacterProperties);
            IterateElements(style, (e, name) => {
                switch(name) {
                    case "FontFamily":
                        characterProperties.FontName = e.Value;
                        break;
                    case "FontSize":
                        characterProperties.FontSize = float.Parse(unitConverter.CutUnits(e.Value));
                        break;
                    case "FontWeight":
                        characterProperties.Bold = fontBoldValues.Contains(e.Value);
                        break;
                    case "Color":
                        characterProperties.ForeColor = ParseColor(e.Value);
                        break;
                    case "FontStyle":
                        characterProperties.Italic = e.Value == "Italic";
                        break;
                    case "TextDecoration":
                        if(e.Value == "Underline")
                            characterProperties.Underline = UnderlineType.Single;
                        else if(e.Value == "LineThrought")
                            characterProperties.Strikeout = StrikeoutType.Single;
                        break;
                    default:
                        TraceInfo(Messages.RichTextRunStyle_NotSupported_Format, name);
                        break;
                }
            });
            documentServer.Document.EndUpdateCharacters(characterProperties);
        }

        void ProcessSingleParagraphAsText(XElement paragraphs, XRLabel label) {
            var paragraph = paragraphs.Element(xmlns + "Paragraph");
            IterateElements(paragraph, (e, name) => {
                switch(name) {
                    case "Style":
                        ProcessStyle(e, label);
                        break;
                    case "TextRuns":
                        var run = e.Element(xmlns + "TextRun");
                        IterateElements(run, (textRunElement, textRunElementName) => {
                            ExpressionParserResult expressionParserResult;
                            TryGetExpression(textRunElement, label, true, out expressionParserResult);
                            switch(textRunElementName) {
                                case "Value":
                                    if(expressionParserResult != null) {
                                        if(expressionParserResult.HasSummary)
                                            label.Summary.Running = SummaryRunning.Group;
                                        label.ExpressionBindings.Add(expressionParserResult.ToExpressionBinding(nameof(label.Text)));
                                        UpdateControlsReportDataSource(label, expressionParserResult);
                                    } else
                                        label.Text = textRunElement.Value;
                                    break;
                                case "Style":
                                    ProcessStyle(textRunElement, label);
                                    break;
                                case "Label":
                                    if(expressionParserResult == null)
                                        label.NullValueText = e.Value;
                                    break;
                                case "MarkupType":
                                case "ActionInfo":
                                    break;
                                default:
                                    TraceInfo(Messages.RichTextRunElement_NotSupported_Format, textRunElementName);
                                    break;
                            }
                        });
                        break;
                    case "LeftIndent":
                        label.Padding = new PaddingInfo(label.Padding, label.Dpi) { Left = unitConverter.ToInt(e.Value) };
                        break;
                    case "SpaceBefore":
                        label.Padding = new PaddingInfo(label.Padding, label.Dpi) { Top = unitConverter.ToInt(e.Value) };
                        break;
                    case "SpaceAfter":
                        label.Padding = new PaddingInfo(label.Padding, label.Dpi) { Bottom = unitConverter.ToInt(e.Value) };
                        break;
                    default:
                        TraceInfo(Messages.RichTextParagraphElement_NotSupported_Format, name);
                        break;
                }
            });
        }

        RichEditDocumentServer CreateRichDocumentServer(Font font, Color foreColor) {
            var server = new RichEditDocumentServer();
            var characterProperties = server.Document.BeginUpdateCharacters(server.Document.Range);
            server.Document.DefaultCharacterProperties.FontName = characterProperties.FontName = font.Name;
            server.Document.DefaultCharacterProperties.FontSize = characterProperties.FontSize = font.Size;
            server.Document.DefaultCharacterProperties.Bold = characterProperties.Bold = font.Bold;
            server.Document.DefaultCharacterProperties.Italic = characterProperties.Italic = font.Italic;
            server.Document.DefaultCharacterProperties.Strikeout = characterProperties.Strikeout = font.Strikeout ? StrikeoutType.Single : StrikeoutType.None;
            server.Document.DefaultCharacterProperties.Underline = characterProperties.Underline = font.Underline ? UnderlineType.Single : UnderlineType.None;
            server.Document.DefaultCharacterProperties.ForeColor = characterProperties.ForeColor = foreColor;
            server.Document.EndUpdateCharacters(characterProperties);
            return server;
        }
        #endregion

        void ProcessRectangleControl(XElement rectangleElement, XRControl container, float yBodyOffset) {
            var control = CreateXRControl<XRPanel>(container, rectangleElement);
            IterateElements(rectangleElement, (e, name) => {
                switch(name) {
                    case "ReportItems":
                        ProcessReportItems(e, control);
                        break;
                    default:
                        ProcessCommonControlProperties(e, control, yBodyOffset);
                        break;
                }
            });
        }

        void ProcessTablixControl(XElement e, ref XRControl container, ref float yBodyOffset) {
            var matrixConverter = new TablixConverter(this);
            XtraReportBase report = container.Report;
            ConvertionResult convertionResult = matrixConverter.Convert(e, container, yBodyOffset, this);
            if(convertionResult.ShouldStartNewBand) {
                yBodyOffset += convertionResult.MatrixHeight;
                var detailReportBand = new DetailReportBand();
                detailReportBand.Dpi = report.Dpi;
                report.Bands.Add(detailReportBand);
                container = EnsureEmptyBand<DetailBand>(detailReportBand, BandKind.Detail);
            }
        }

        #region Image Control
        readonly static Dictionary<string, ImageSizeMode> imageSizingMap = new Dictionary<string, ImageSizeMode>() {
            { "AutoSize", ImageSizeMode.AutoSize },
            { "FitProportional", ImageSizeMode.ZoomImage },
            { "Fit", ImageSizeMode.StretchImage },
            { "Clip", ImageSizeMode.Normal }
        };
        public void ProcessImageControl(XElement imageElement, XRControl container, float yBodyOffset) {
            var control = CreateXRControl<XRPictureBox>(container, imageElement);
            control.Sizing = ImageSizeMode.AutoSize;
            var sourceType = imageElement.Element(xmlns + "Source").Value;
            var value = imageElement.Element(xmlns + "Value");
            ProcessImageControlSource(control, sourceType, value);

            IterateElements(imageElement, (e, name) => {
                switch(name) {
                    case "Sizing":
                        control.Sizing = imageSizingMap.GetValueOrDefault(e.Value, control.Sizing);
                        break;
                    case "MIMEType":
                    case "Source":                    // handled
                    case "Value":                     // handled
                        break;
                    default:
                        ProcessCommonControlProperties(e, control, yBodyOffset);
                        break;
                }
            });
        }

        void ProcessImageControlSource(XRPictureBox control, string sourceType, XElement valueElement) {
            ExpressionParserResult expressionParserResult;
            TryGetExpression(valueElement, control, false, out expressionParserResult);
            switch(sourceType) {
                case "Embedded":
                    if(!string.IsNullOrEmpty(valueElement.Value))
                        control.ExpressionBindings.Add(new ExpressionBinding(nameof(control.ImageSource), $"[Images.{valueElement.Value}]"));
                    break;
                case "External":
                    if(expressionParserResult != null)
                        control.ExpressionBindings.Add(expressionParserResult.ToExpressionBinding(nameof(control.ImageUrl)));
                    else
                        control.ImageUrl = valueElement.Value;
                    break;
                case "Database":
                    if(expressionParserResult != null)
                        control.ExpressionBindings.Add(expressionParserResult.ToExpressionBinding(nameof(control.ImageSource)));
                    break;
                default:
                    TraceInfo(Messages.ImageSourceType_NotSupported_Format, sourceType);
                    break;
            }
        }
        #endregion

        void ProcessLineControl(XElement lineElement, XRControl container, float yBandOffset) {
            var control = CreateXRControl<XRShape>(container, lineElement);
            control.Shape = new XtraPrinting.Shape.ShapeLine();
            float top = 0;
            float left = 0;
            float width = 0;
            float height = 0;
            IterateElements(lineElement, (e, name) => {
                switch(name) {
                    case "Top":
                        top = unitConverter.ToFloat(e.Value) - yBandOffset;
                        break;
                    case "Left":
                        left = unitConverter.ToFloat(e.Value);
                        break;
                    case "Width":
                        width = unitConverter.ToFloat(e.Value);
                        break;
                    case "Height":
                        height = unitConverter.ToFloat(e.Value);
                        break;
                    case "ZIndex":                         //no need to handle ?
                        break;
                    case "Style":
                        ProcessStyle(e, control);
                        break;
                    case "Visibility":
                        ProcessControlVisibility(e, control);
                        break;
                    default:
                        TraceInfo(Messages.LineElement_NotSupported_Format, control.Name, name);
                        break;
                }
            });
            control.Angle = 90 - (int)(Math.Atan2(height, width) * (180 / Math.PI));
            if(width < 0) {
                left += width;
                width = -width;
            }
            if(height < 0) {
                top += height;
                height = -height;
            }
            control.BoundsF = new RectangleF(left, top, width, height);
        }

        #region Subreport Control
        void ProcessSubreportControl(XElement subreportElement, XRControl container, float yBodyOffset) {
            var control = CreateXRControl<XRSubreport>(container, subreportElement);
            IterateElements(subreportElement, (e, name) => {
                switch(name) {
                    case "ReportName":
                        control.ReportSourceUrl = e.Value;
                        break;
                    case "Parameters":
                        ProcessSubreportParameters(e, control);
                        break;
                    default:
                        ProcessCommonControlProperties(e, control, yBodyOffset);
                        break;
                }
            });
        }

        T CreateXRControl<T>(XRControl container, XElement element = null)
            where T : XRControl, new() {
            T control = new T { Dpi = container.Dpi };
            if(element != null)
                this.SetComponentName(control, element);
            container.Controls.Add(control);
            return control;
        }

        void ProcessSubreportParameters(XElement parametersElement, XRSubreport control) {
            IterateElements(parametersElement, xmlns + "Parameter", (e, name) => {
                string parameterName = e.Attribute("Name").Value;
                string value = e.Element(xmlns + "Value").Value;
                var parameter = new Parameter() { Visible = false, Name = parameterName };
                ExpressionParserResult expressionResult;
                if(TryGetExpression(value, control, false, out expressionResult)) {
                    parameter.ExpressionBindings.Add(expressionResult.ToBasicExpressionBinding());
                } else {
                    parameter.Value = value;
                }
                TargetReport.Parameters.Add(parameter);
                control.ParameterBindings.Add(new ParameterBinding(parameterName, parameter));
            });
        }
        #endregion

        void ProcessChartControl(XElement chartElement, XRControl container, float yBodyOffset) {
            var chart = CreateXRControl<XRChart>(container, chartElement);
            chart.Series.Add(new XtraCharts.Series("Series Stub", XtraCharts.ViewType.Line));
            IterateElements(chartElement, (e, name) => {
                switch(name) {
                    default:
                        ProcessCommonControlProperties(e, chart, yBodyOffset);
                        break;
                }
            });
        }

        public void ProcessCommonControlProperties(XElement element, XRControl control, float yBodyOffset, bool traceOn = true) {
            if(control.Parent is XRTableCell)
                control.BoundsF = BestSizeEstimator.GetBoundsToFitContainer(control);
            string name = element.Name.LocalName;
            switch(name) {
                case "Left":
                case "Top":
                case "Width":
                case "Height":
                    ProcessControlLayout(element.Value, name, control, yBodyOffset);
                    break;
                case "Style":
                    ProcessStyle(element, control);
                    break;
                case "Visibility":
                    ProcessControlVisibility(element, control);
                    break;
                case "KeepTogether":
                    control.KeepTogether = bool.Parse(element.Value);
                    break;
                case "CanShrink":
                    control.CanShrink = bool.Parse(element.Value);
                    break;
                case "CanGrow":
                    control.CanGrow = bool.Parse(element.Value);
                    break;
                case "Bookmark":
                    ProcessBookmark(element, control);
                    break;
                case "ActionInfo":
                    ProcessActionInfo(element, control);
                    break;
                case "HideDuplicates":
                    break;
                case "ZIndex":
                case "RepeatWith":
                case "DefaultName":                    // not supported
                case "WatermarkTextbox":               // not supported
                case "ToolTip":                        // not supported
                case "TablixBody":                     // handled
                case "TablixColumnHierarchy":          // handled
                case "TablixRowHierarchy":             // handled
                case "DataSetName":                    // handled
                    break;
                default:
                    TraceInfo(Messages.ControlProperty_NotSupported_Format, name);
                    break;
            }
        }

        void ProcessControlLayout(string value, string property, XRControl control, float yBodyOffset) {
            if(control is XRShape)
                throw new ArgumentException(Messages.ProcessControlLayout_Shape_Error);
            if(control.Parent is XRTableCell)
                return;
            switch(property) {
                case "Left":
                    control.LocationFloat = new PointFloat(unitConverter.ToFloat(value), control.LocationFloat.Y);
                    break;
                case "Top":
                    control.LocationFloat = new PointFloat(control.LocationFloat.X, unitConverter.ToFloat(value) - yBodyOffset);
                    break;
                case "Width":
                    control.SizeF = new SizeF(unitConverter.ToFloat(value), control.SizeF.Height);
                    break;
                case "Height":
                    control.SizeF = new SizeF(control.SizeF.Width, unitConverter.ToFloat(value));
                    break;
                default:
                    TraceInfo(Messages.ControlLayoutProperty_NotSupported_Format, property);
                    break;
            }
        }

        void ProcessControlVisibility(XElement visibility, XRControl control) {
            IterateElements(visibility, (e, name) => {
                ExpressionParserResult expressionParserResult;
                TryGetExpression(e, control, false, out expressionParserResult);
                switch(name) {
                    case "Hidden":
                        if(expressionParserResult != null) {
                            control.ExpressionBindings.Add(expressionParserResult.ToExpressionBinding(nameof(control.Visible)));
                            UpdateControlsReportDataSource(control, expressionParserResult);
                            break;
                        } else control.Visible = bool.Parse(e.Value);
                        break;
                    default:
                        TraceInfo(Messages.VisibilityProperty_NotSupported_Format, name, control.Name);
                        break;
                }
            });
        }

        void ProcessBookmark(XElement bookmark, XRControl control) {
            ExpressionParserResult expressionParserResult;
            TryGetExpression(bookmark, control, false, out expressionParserResult);
            if(expressionParserResult != null) {
                control.ExpressionBindings.Add(expressionParserResult.ToExpressionBinding(nameof(control.Bookmark)));
                UpdateControlsReportDataSource(control, expressionParserResult);
            } else
                control.Bookmark = bookmark.Value;
        }

        void ProcessActionInfo(XElement actionInfo, XRControl control) {
            foreach(var action in actionInfo.Descendants(xmlns + "Action")) {
                var actionCore = action.Elements().Single();
                if(actionCore.Name.LocalName == "Hyperlink") {
                    ExpressionParserResult expressionResult;
                    if(actionCore != null && TryGetExpression(actionCore, control, false, out expressionResult)) {
                        control.ExpressionBindings.Add(expressionResult.ToExpressionBinding(nameof(control.NavigateUrl)));
                        UpdateControlsReportDataSource(control, expressionResult);
                    } else
                        control.NavigateUrl = actionCore.Value;
                } else {
                    TraceInfo(Messages.ActionInfo_NotSupported_Format, control.Name, actionCore.Name.LocalName);
                }
            }
        }
        #endregion

        #region appearance
        void ProcessStyle(XElement style, XRControl control) {
            var textAlignment = style.Element(xmlns + "TextAlign")?.Value;
            var verticalAlignment = style.Element(xmlns + "VerticalAlign")?.Value;
            ProcessTextAlignment(textAlignment, verticalAlignment, control);
            var bordersModel = new BordersModel(this, control);
            IterateElements(style, (e, name) => {
                ExpressionParserResult expressionParserResult;
                TryGetExpression(e, control, false, out expressionParserResult);
                switch(name) {
                    case "Border":
                        bordersModel.Parse(e);
                        break;
                    case "LeftBorder":
                        bordersModel.Parse(e, BorderSide.Left);
                        break;
                    case "TopBorder":
                        bordersModel.Parse(e, BorderSide.Top);
                        break;
                    case "RightBorder":
                        bordersModel.Parse(e, BorderSide.Right);
                        break;
                    case "BottomBorder":
                        bordersModel.Parse(e, BorderSide.Bottom);
                        break;
                    case "BorderColor":          //rdl 2005
                        bordersModel.ParseBorderColor(e);
                        break;
                    case "BorderStyle":          //rdl 2005
                        bordersModel.ParseBorderStyle(e);
                        break;
                    case "BorderWidth":          //rdl 2005
                        bordersModel.ParseBorderWidth(e);
                        break;
                    case "Color":
                        ProcessColor(e.Value, control, nameof(XRControl.ForeColor), expressionParserResult);
                        break;
                    case "BackgroundColor":
                        ProcessColor(e.Value, control, nameof(XRControl.BackColor), expressionParserResult);
                        break;
                    case "PaddingLeft":
                    case "PaddingRight":
                    case "PaddingTop":
                    case "PaddingBottom":
                        ProcessPadding(e.Value, name, control, expressionParserResult);
                        break;
                    case "FontFamily":
                    case "FontSize":
                    case "FontWeight":
                    case "FontStyle":
                    case "TextDecoration":
                        ProcessFont(e.Value, control, name, expressionParserResult);
                        break;
                    case "Format":
                        ProcessFormat(e.Value, control, expressionParserResult);
                        break;
                    case "WritingMode":
                        ProcessWritingMode(e, control, expressionParserResult);
                        break;
                    case "VerticalAlign":        // handled
                    case "TextAlign":            // handled
                        break;
                    case "Language":             // not supported
                    case "NumeralLanguage":      // not supported
                    case "LineHeight":           // not supported
                    case "Calendar":             // not supported
                        break;
                    case "BackgroundGradientType":
                        if(e.Value != "None")
                            TraceInfo(Messages.StyleProperty_NotSupported_Format, name, e.Value);
                        break;
                    default:
                        TraceInfo(Messages.StyleProperty_NotSupported_Format, name, e.Value);
                        break;
                }
            });
            bordersModel.ApplyToControl();
        }

        #region TextAlignment
        void ProcessTextAlignment(string horizontalAlignment, string verticalAlignment, XRControl control) {
            var effectiveAlignment = control.GetEffectiveTextAlignment();
            if(IsExpression(horizontalAlignment)) {
                TraceInfo(Messages.TextAlign_Expression_NotSupported);
                horizontalAlignment = TextAlignmentToTextAlign(effectiveAlignment);
            } else if(string.IsNullOrEmpty(horizontalAlignment) || horizontalAlignment == "General" || horizontalAlignment == "Default") {
                horizontalAlignment = TextAlignmentToTextAlign(effectiveAlignment);
            }

            if(IsExpression(verticalAlignment)) {
                TraceInfo(NativeSR.TraceSource, Messages.VerticalAlign_Expression_NotSupported);
                verticalAlignment = TextAlignmentToVerticalAlign(effectiveAlignment);
            } else if(string.IsNullOrEmpty(verticalAlignment) || verticalAlignment == "Default") {
                verticalAlignment = TextAlignmentToVerticalAlign(effectiveAlignment);
            }

            if(horizontalAlignment == "Left") {
                if(verticalAlignment == "Top")
                    control.TextAlignment = TextAlignment.TopLeft;
                if(verticalAlignment == "Center")
                    control.TextAlignment = TextAlignment.MiddleLeft;
                if(verticalAlignment == "Bottom")
                    control.TextAlignment = TextAlignment.BottomLeft;
            } else if(horizontalAlignment == "Center") {
                if(verticalAlignment == "Top")
                    control.TextAlignment = TextAlignment.TopCenter;
                if(verticalAlignment == "Center")
                    control.TextAlignment = TextAlignment.MiddleCenter;
                if(verticalAlignment == "Bottom")
                    control.TextAlignment = TextAlignment.BottomCenter;
            } else if(horizontalAlignment == "Right") {
                if(verticalAlignment == "Top")
                    control.TextAlignment = TextAlignment.TopRight;
                if(verticalAlignment == "Center")
                    control.TextAlignment = TextAlignment.MiddleRight;
                if(verticalAlignment == "Bottom")
                    control.TextAlignment = TextAlignment.BottomRight;
            }
        }

        static string TextAlignmentToVerticalAlign(TextAlignment alignment) {
            switch(alignment) {
                case TextAlignment.TopLeft:
                case TextAlignment.TopCenter:
                case TextAlignment.TopRight:
                case TextAlignment.TopJustify:
                    return "Top";
                case TextAlignment.MiddleLeft:
                case TextAlignment.MiddleCenter:
                case TextAlignment.MiddleRight:
                case TextAlignment.MiddleJustify:
                    return "Center";
                case TextAlignment.BottomLeft:
                case TextAlignment.BottomCenter:
                case TextAlignment.BottomRight:
                case TextAlignment.BottomJustify:
                    return "Bottom";
                default:
                    throw new InvalidOperationException();
            }
        }

        static string TextAlignmentToTextAlign(TextAlignment alignment) {
            switch(alignment) {
                case TextAlignment.TopLeft:
                case TextAlignment.MiddleLeft:
                case TextAlignment.BottomLeft:
                case TextAlignment.TopJustify:
                case TextAlignment.MiddleJustify:
                case TextAlignment.BottomJustify:
                    return "Left";
                case TextAlignment.TopCenter:
                case TextAlignment.MiddleCenter:
                case TextAlignment.BottomCenter:
                    return "Center";
                case TextAlignment.TopRight:
                case TextAlignment.MiddleRight:
                case TextAlignment.BottomRight:
                    return "Right";
                default:
                    throw new InvalidOperationException();
            }
        }
        #endregion
        #region borders
        class BordersModel {
            public class BorderModel {
                public Color? Color { get; set; }
                public BorderDashStyle? Style { get; set; }
                public float? Width { get; set; }
            }

            readonly static Dictionary<string, BorderSide> borderSideMap = new Dictionary<string, BorderSide>() {
                { "Default", BorderSide.All },
                { "Left", BorderSide.Left },
                { "Top", BorderSide.Top },
                { "Right", BorderSide.Right },
                { "Bottom", BorderSide.Bottom },
            };
            readonly static Dictionary<string, BorderDashStyle?> borderDashStyleMap = new Dictionary<string, BorderDashStyle?>() {
                { "Default", BorderDashStyle.Solid},
                { "Dotted", BorderDashStyle.Dot},
                { "Dashed", BorderDashStyle.Dash},
                { "Solid", BorderDashStyle.Solid},
                { "DashDot", BorderDashStyle.DashDot},
                { "DashDotDot", BorderDashStyle.DashDotDot},
                { "Double", BorderDashStyle.Double},
                { "None", null},
            };
            readonly XRControl control;
            readonly IReportingServicesConverter converter;
            readonly Dictionary<BorderSide, BorderModel> borderModels = new Dictionary<BorderSide, BorderModel>() {
                { BorderSide.All, new BorderModel() },
                { BorderSide.Left, new BorderModel() },
                { BorderSide.Top, new BorderModel() },
                { BorderSide.Right, new BorderModel() },
                { BorderSide.Bottom, new BorderModel() }
            };

            public ExpressionParserResult ColorExpression { get; set; }
            public ExpressionParserResult StyleExpression { get; set; }
            public ExpressionParserResult WidthExpression { get; set; }

            public BordersModel(IReportingServicesConverter converter, XRControl control) {
                this.control = control;
                this.converter = converter;
            }

            public void ApplyToControl() {
                SetControlProperty(control, nameof(XRControl.BorderDashStyle), GetStyle(), StyleExpression);
                var colorProperty = control is XRShape ? nameof(XRShape.ForeColor) : nameof(XRControl.BorderColor);
                SetControlProperty(control, colorProperty, GetColor(), ColorExpression);
                if(control is XRShape)
                    SetControlProperty(control, nameof(XRShape.LineWidth), (int)Math.Round(GetWidth()), WidthExpression);
                else
                    SetControlProperty(control, nameof(XRControl.BorderWidth), GetWidth(), WidthExpression);
                control.Borders = GetBorderSides();
            }

            public void Parse(XElement borderElement, BorderSide side = BorderSide.All) {
                if(borderElement == null)
                    return;
                var borderModel = borderModels[side];
                IterateElements(borderElement, (e, name) => {
                    ExpressionParserResult expressionParserResult;
                    if(converter.TryGetExpression(e.Value, control.Name, out expressionParserResult)) {
                        TraceInfo(Messages.Border_Expression_NotSupported_Format, name);
                        return;
                    }
                    switch(name) {
                        case "Style":
                            SetModelProperty(borderModel, nameof(BorderModel.Style), borderDashStyleMap[e.Value], expressionParserResult);
                            break;
                        case "Color":
                            SetModelProperty(borderModel, nameof(BorderModel.Color), ParseColor(e.Value), expressionParserResult);
                            break;
                        case "Width":
                            SetModelProperty(borderModel, nameof(BorderModel.Width), converter.UnitConverter.ToFloat(e.Value), expressionParserResult);
                            break;
                        default:
                            TraceInfo(string.Format(Messages.BorderProperty_NotSupported_Format, name, e.Value));
                            break;
                    }
                });
            }
            public void ParseBorderColor(XElement borderColorElement) {
                ParseBorderPropertyCore(borderColorElement, nameof(BorderModel.Color), ParseColor);
            }
            public void ParseBorderStyle(XElement borderStyleElement) {
                ParseBorderPropertyCore(borderStyleElement, nameof(BorderModel.Style), x=> borderDashStyleMap[x]);
            }
            public void ParseBorderWidth(XElement borderWidthElement) {
                ParseBorderPropertyCore(borderWidthElement, nameof(BorderModel.Width), converter.UnitConverter.ToFloat);
            }
            void ParseBorderPropertyCore<T>(XElement element, string propertyName, Func<string, T> convertValue) {
                if(element == null)
                    return;
                IterateElements(element, (e, name) => {
                    ExpressionParserResult expressionParserResult;
                    converter.TryGetExpression(e.Value, control.Name, out expressionParserResult);
                    BorderSide? side = borderSideMap[name];
                    if(expressionParserResult != null) {
                        if(side == BorderSide.All)
                            XRAccessor.SetProperty(this, propertyName + "Expression", expressionParserResult);
                    } else {
                        XRAccessor.SetProperty(borderModels[side.Value], propertyName, convertValue(e.Value));
                    }
                });
            }

            Color GetColor() {
                return borderModels.Values.Select(x => x.Color).FirstOrDefault(x => x.HasValue) ?? Color.Black;
            }
            float GetWidth() {
                return borderModels.Values.Select(x => x.Width).FirstOrDefault(x => x.HasValue) ?? 1f;
            }
            BorderDashStyle GetStyle() {
                return borderModels.Values.Select(x => x.Style).FirstOrDefault(x => x.HasValue) ?? BorderDashStyle.Solid;
            }
            BorderSide GetBorderSides() {
                var sides = borderModels[BorderSide.All].Style.HasValue ? BorderSide.All : BorderSide.None;
                foreach(var modelItem in borderModels.Skip(1)) {
                    if(modelItem.Value.Style.HasValue)
                        sides |= modelItem.Key;
                }
                return sides;
            }

            static void SetModelProperty<T>(BorderModel model, string propertyName, T value, ExpressionParserResult expressionParserResult) {
                if(expressionParserResult != null)
                    XRAccessor.SetProperty(model, propertyName + "Expression", expressionParserResult);
                else
                    XRAccessor.SetProperty(model, propertyName, value);
            }
            void SetControlProperty<T>(XRControl control, string propertyName, T value, ExpressionParserResult expressionParserResult) {
                if(expressionParserResult != null) {
                    control.ExpressionBindings.Add(expressionParserResult.ToExpressionBinding(propertyName));
                    converter.UpdateControlsReportDataSource(control, expressionParserResult);
                }
                XRAccessor.SetProperty(control, propertyName, value);
            }
        }
        #endregion

        void ProcessPadding(string value, string side, XRControl control, ExpressionParserResult expressionParserResult) {
            var hasExpression = expressionParserResult != null;
            int paddingValue = !hasExpression
                ? unitConverter.ToInt(value)
                : 0;
            string expressionPropertyName = null;
            PaddingInfo padding = control.Padding.Dpi == unitConverter.TargetDpi
                    ? control.Padding
                    : new PaddingInfo(control.Padding, unitConverter.TargetDpi);
            switch(side) {
                case "PaddingLeft":
                    if(hasExpression)
                        expressionPropertyName = nameof(padding.Left);
                    else
                        padding.Left = paddingValue;
                    break;
                case "PaddingRight":
                    if(hasExpression)
                        expressionPropertyName = nameof(padding.Right);
                    else
                        padding.Right = paddingValue;
                    break;
                case "PaddingTop":
                    if(hasExpression)
                        expressionPropertyName = nameof(padding.Top);
                    else
                        padding.Top = paddingValue;
                    break;
                case "PaddingBottom":
                    if(hasExpression)
                        expressionPropertyName = nameof(padding.Bottom);
                    else
                        padding.Bottom = paddingValue;
                    break;
                default:
                    TraceInfo(Messages.PaddingProperty_NotSupported_Format, side);
                    break;
            }
            if(hasExpression)
                control.ExpressionBindings.Add(expressionParserResult.ToExpressionBinding($"{nameof(control.Padding)}.{expressionPropertyName}"));
            else
                control.Padding = padding;
        }

        static void ProcessColor(string value, XRControl component, string propertyName, ExpressionParserResult expressionParserResult) {
            if(expressionParserResult != null) {
                component.ExpressionBindings.Add(expressionParserResult.ToExpressionBinding(propertyName));
                return;
            }
            var property = XRAccessor.GetPropertyDescriptor(component, propertyName);
            var defaultValue = (Color)property.GetValue(component);
            var color = ParseColor(value, defaultValue);
            property.SetValue(component, color);
        }

        static Color ParseColor(string value, Color defaultValue) {
            return ParseColor(value) ?? defaultValue;
        }

        static Color? ParseColor(string value) {
            value = value.Replace("Grey", "Gray");
            var converter = new ColorConverter();
            try {
                return (Color)converter.ConvertFrom(value);
            } catch (Exception e) {
                Tracer.TraceWarning(NativeSR.TraceSource, e.Message);
                return null;
            }
        }

        void ProcessBorderWidth(string value, XRControl control, ExpressionParserResult expressionParserResult) {
            bool hasExpression = expressionParserResult != null;
            string expressionProperty = null;
            var shape = control as XRShape;
            if(shape != null) {
                if(hasExpression)
                    expressionProperty = nameof(shape.LineWidth);
                else
                    shape.LineWidth = (int)Math.Round(unitConverter.ToFloat(value, GraphicsDpi.Pixel));
            } else {
                if(hasExpression)
                    expressionProperty = nameof(control.BorderWidth);
                else
                    control.BorderWidth = !hasExpression ? unitConverter.ToFloat(value) : 0;
            }
            if(hasExpression)
                control.ExpressionBindings.Add(expressionParserResult.ToExpressionBinding(expressionProperty));
        }

        readonly static string[] fontBoldValues = new[] { "Medium", "SemiBold", "Bold", "ExtraBold", "Heavy" };

        void ProcessFont(string value, XRControl control, string propertyName, ExpressionParserResult expressionParserResult) {
            var surrogate = FontSurrogate.FromFont(control.GetEffectiveFont());
            string expressionPropertyName = "";
            switch(propertyName) {
                case "FontFamily":
                    if(expressionParserResult != null)
                        expressionPropertyName = nameof(Font.Name);
                    else surrogate.Name = value;
                    break;
                case "FontSize":
                    if(expressionParserResult != null)
                        expressionPropertyName = nameof(Font.Name);
                    else surrogate.Size = float.Parse(unitConverter.CutUnits(value));
                    break;
                case "FontWeight":
                    if(expressionParserResult != null)
                        expressionPropertyName = nameof(Font.Name);
                    else surrogate.Bold = fontBoldValues.Contains(value);
                    break;
                case "FontStyle":
                    if(expressionParserResult != null)
                        expressionPropertyName = nameof(Font.Name);
                    else surrogate.Italic = value.ToLower() == "italic";
                    break;
                case "TextDecoration":
                    if(expressionParserResult == null) {
                        if(value == "Underline")
                            surrogate.Underline = true;
                        else if(value == "LineThrought")
                            surrogate.Strikeout = true;
                    }
                    break;
                default:
                    break;
            }
            if(expressionParserResult != null) {
                expressionPropertyName = $"{nameof(XRControl.Font)}.{expressionPropertyName}";
                control.ExpressionBindings.Add(expressionParserResult.ToExpressionBinding(expressionPropertyName));
            }
            control.Font = FontSurrogate.ToFont(surrogate);
        }

        static void ProcessFormat(string value, XRControl control, ExpressionParserResult expressionParserResult) {
            if(expressionParserResult != null)
                control.ExpressionBindings.Add(expressionParserResult.ToExpressionBinding(nameof(control.TextFormatString), ProcessFormatCriteria));
            else
                control.TextFormatString = "{0:" + value + "}";
        }
        static CriteriaOperator ProcessFormatCriteria(CriteriaOperator criteria) {
            var left = new BinaryOperator(new ConstantValue("{0:"), criteria, BinaryOperatorType.Plus);
            var result = new BinaryOperator(left, new ConstantValue("}"), BinaryOperatorType.Plus);
            return result;
        }

        readonly static Dictionary<string, int> writingModeToTextAngleMap = new Dictionary<string, int>() {
            { "Horizontal", 0 },
            { "Vertical", 270 },
            { "Rotate270", 90 },
        };
        void ProcessWritingMode(XElement writingMode, XRControl control, ExpressionParserResult expressionParserResult) {
            if(expressionParserResult != null) {
                TraceInfo(string.Format(Messages.StyleExpression_NotSupported_Format, writingMode.Name.LocalName));
            } else {
                var textAngle = writingModeToTextAngleMap.GetValueOrDefault(writingMode.Value, 0);
                if(control is XRLabel)
                    ((XRLabel)control).Angle = textAngle;
                else if(control is XRCrossTabCell)
                    ((XRCrossTabCell)control).Angle = textAngle;
                else
                    TraceInfo(string.Format(Messages.CannotApplyStyleProperty_Format, writingMode.Name.LocalName, control.Name));
            }
        }
        #endregion

        #region parameters

        void ProcessReportParameters(XElement parametersElement) {
            IterateElements(parametersElement, xmlns + "ReportParameter", (e, name) => ProcessReportParameter(e));
        }

        void ProcessReportParameter(XElement parameterElement) {
            var parameter = new Parameter();
            parameter.Name = NamingMapper.GenerateSafeName<Parameter>(parameterElement.Attribute("Name").Value);
            parameter.Description = parameterElement.Attribute("Prompt")?.Value;
            var dataTypeElement = parameterElement.Element(xmlns + "DataType");
            parameter.Type = GetTypeFromDataType(dataTypeElement.Value);
            var multiValue = parameterElement.Element(xmlns + "MultiValue")?.Value ?? "false";
            parameter.MultiValue = bool.Parse(multiValue);
            var nullable = parameterElement.Element(xmlns + "Nullable")?.Value ?? "false";
            parameter.AllowNull = bool.Parse(nullable);
            IterateElements(parameterElement, (e, name) => {
                ExpressionParserResult expressionParserResult;
                TryGetExpression(e, parameter.Name, false, out expressionParserResult);
                switch(name) {
                    case "DefaultValue":
                        ProcessParameterValue(e, parameter, expressionParserResult);
                        break;
                    case "Hidden":
                        parameter.Visible = !bool.Parse(e.Value.ToLower());
                        break;
                    case "Prompt":
                        parameter.Description = e.Value;
                        break;
                    case "ValidValues":
                        parameter.ValueSourceSettings = ProcessParameterValueSource(e.Elements().Single(), parameter.Type);
                        break;
                    case "DataType":          //handled
                    case "AllowBlank":        //not supported
                    case "MultiValue":        //handled
                    case "Nullable":          //handled
                        break;
                    default:
                        TraceInfo(Messages.ParameterProperty_NotSupported_Format, name);
                        break;
                }
                TargetReport.Parameters.Add(parameter);
            });
        }
        void ProcessParameterValue(XElement defaultValue, Parameter parameter, ExpressionParserResult expressionParserResult) {
            if(expressionParserResult != null) {
                parameter.ExpressionBindings.Add(expressionParserResult.ToBasicExpressionBinding());
                return;
            }
            if(!defaultValue.HasElements) {
                parameter.Value = defaultValue.Value;
            } else {
                var dataSetReference = defaultValue.Element(xmlns + "DataSetReference");
                if(dataSetReference != null) {
                    TraceInfo(Messages.Parameter_DefaultValueProperty_NotSupported_Format, "DataSetReference");
                    return;
                }
                var values = defaultValue.Descendants(xmlns + "Value")
                    .Select(x => x.Value)
                    .Where(x => !string.IsNullOrEmpty(x));
                var firstValue = values.FirstOrDefault();
                if(parameter.MultiValue) {
                    parameter.Value = values;
                } else if(firstValue != null) {
                    ExpressionParserResult expressionResult;
                    if(TryGetExpression(values.First(), parameter.Name, false, out expressionResult))
                        parameter.ExpressionBindings.Add(expressionResult.ToBasicExpressionBinding());
                    else if(firstValue == "<ALL>" && parameter.MultiValue)
                        parameter.SelectAllValues = true;
                    else
                        parameter.Value = values.First();
                }
            }
        }
        ValueSourceSettings ProcessParameterValueSource(XElement valueSourceElement, Type valueType) {
            string name = valueSourceElement.Name.LocalName;
            switch(name) {
                case "ParameterValues":
                    return ProcessStaticListLookUpSettings(valueSourceElement, valueType);
                case "DataSetReference":
                    return ProcessDynamicListLookUpSettings(valueSourceElement);
                default:
                    TraceInfo(Messages.ParameterValueSource_NotSupported_Format, name);
                    return null;
            }
        }

        StaticListLookUpSettings ProcessStaticListLookUpSettings(XElement staticListElement, Type valueType) {
            var lookups = new StaticListLookUpSettings();

            var lookUpValues = staticListElement.Elements(xmlns + "ParameterValue")
                .Select(x => {
                    var value = ParameterHelper.GetDefaultValue(valueType);
                    var valueString = x.Element(xmlns + "Value")?.Value ?? "";
                    if(!IsExpression(valueString))
                        value = ParameterHelper.ConvertFrom(valueString, valueType, value);

                    var description = x.Element(xmlns + "Label")?.Value;
                    return new LookUpValue(value, description);
                });
            lookups.LookUpValues.AddRange(lookUpValues);
            return lookups;
        }

        DynamicListLookUpSettings ProcessDynamicListLookUpSettings(XElement dynamicListElement) {
            var settings = new DynamicListLookUpSettings();
            IterateElements(dynamicListElement, (e, name) => {
                switch(name) {
                    case "DataSetName":
                        DataPair dataPair;
                        if(dataSetToDataPairMap.TryGetValue(e.Value, out dataPair)) {
                            settings.DataSource = dataPair.Source;
                            settings.DataMember = dataPair.Member;
                        }
                        break;
                    case "ValueField":
                        settings.ValueMember = e.Value;
                        break;
                    case "LabelField":
                        settings.DisplayMember = e.Value;
                        break;
                    default:
                        TraceInfo(Messages.Parameter_DynamicListProperty_NotSupported_Format, name);
                        break;
                }
            });
            return settings;
        }

        #endregion

        #region image resources
        void ProcessEmbeddedImages(XElement images) {
            IterateElements(images, (e, name) => {
                if(name == "EmbeddedImage") {
                    string imageName = e.Attribute("Name").Value;
                    Image image = ConvertBase64ToImage(e.Element(xmlns + "ImageData")?.Value);
                    if(!string.IsNullOrEmpty(imageName) && image != null)
                        TargetReport.ImageResources.Add(imageName, new XtraPrinting.Drawing.ImageSource(image));
                } else {
                    TraceInfo(Messages.ImageElement_NotSupported_Format, name);
                }
            });
        }
        #endregion

        #region data
        void ProcessDataSources(XElement root) {
            var dataSourceConverter = new DataSourceConverter(this, typeResolver, designerHost, currentProjectRootNamespace);
            dataSetToDataPairMap = dataSourceConverter.Convert(root.Element(xmlns + "DataSources"), root.Element(xmlns + "DataSets"));
            foreach(IComponent dataSource in dataSetToDataPairMap.Values.Select(x => x.Source)) {
                TargetReport.ComponentStorage.Add(dataSource);
            }
        }
        #endregion

        internal static T EnsureEmptyBand<T>(XtraReportBase report, BandKind bandKind, Action<T> onNew = null)
            where T : Band {
            T band = null;
            if(!(typeof(GroupBand).IsAssignableFrom(typeof(T))))
                band = report.Bands[bandKind] as T;
            if(band == null) {
                band = (T)XtraReportBase.CreateBand(bandKind);
                band.Dpi = report.Dpi;
                band.HeightF = 0f;
                if(band is VerticalBand)
                    band.WidthF = 0f;
                else if(band is DetailBand) {
                    DetailBand detailBand = band as DetailBand;
                    detailBand.MultiColumn.ColumnSpacing = 50f;
                }
                report.Bands.Add(band);
                onNew?.Invoke(band);
            }
            return band;
        }

        static Image ConvertBase64ToImage(string base64String) {
            if(string.IsNullOrEmpty(base64String))
                return null;
            var bytes = System.Convert.FromBase64String(base64String);
            return Image.FromStream(new MemoryStream(bytes));
        }

        internal static Type GetTypeFromDataType(string value) {
            Type type;
            if(dataTypesMap.TryGetValue(value, out type))
                return type;
            TraceInfo(Messages.DataType_NotSupported_Format, value);
            return typeof(string);
        }
        internal static bool IsExpression(string value) {
            return !string.IsNullOrEmpty(value) && value[0] == '=';
        }
        bool TryGetExpression(XElement element, XRControl control, bool useReportingSummary, out ExpressionParserResult expressionParserResult) {
            return TryGetExpression(element, control.Name, useReportingSummary, out expressionParserResult);
        }
        public bool TryGetExpression(XElement element, string componentName, bool useReportingSummary, out ExpressionParserResult expressionParserResult) {
            if(element.HasElements) {
                expressionParserResult = null;
                return false;
            }
            return TryGetExpression(element.Value, componentName, useReportingSummary, out expressionParserResult);
        }
        bool TryGetExpression(string value, XRControl control, bool useReportingSummary, out ExpressionParserResult expressionParserResult) {
            return TryGetExpression(value, control.Name, useReportingSummary, out expressionParserResult);
        }
        bool IReportingServicesConverter.TryGetExpression(string value, string componentName, out ExpressionParserResult expressionParserResult) {
            return TryGetExpression(value, componentName, false, out expressionParserResult);
        }
        public bool TryGetExpression(string value, string componentName, bool useReportingSummary, out ExpressionParserResult expressionParserResult) {
            value = value.Trim();
            if(!IsExpression(value)) {
                expressionParserResult = null;
                return false;
            }
            expressionParserResult = ExpressionParser.ParseSafe(
                value.Substring(1),
                componentName,
                useReportingSummary,
                UnrecognizedFunctionBehavior == UnrecognizedFunctionBehavior.Ignore);
            return true;
        }

        void IterateReportItemsElements(XElement root, Action<XElement, string> process) {
            IterateElementsCore(root, unitConverter, process);
        }

        bool IReportingServicesConverter.TryGetDataPair(string dataSetName, out DataPair pair) {
            if(string.IsNullOrEmpty(dataSetName)) {
                pair = null;
                return false;
            }
            return dataSetToDataPairMap.TryGetValue(dataSetName, out pair);
        }

        internal static void IterateElements(XElement root, Action<XElement, string> process) {
            IterateElementsCore(root, null, process);
        }
        static void IterateElementsCore(XElement root, UnitConverter converter, Action<XElement, string> process) {
            IEnumerable<XElement> elements = root.Elements();
            if(converter != null) {
                XNamespace ns = root.GetDefaultNamespace();
                elements = elements
                    .OrderBy(x => {
                        string value = x.Element(ns + "Top")?.Value;
                        return string.IsNullOrEmpty(value)
                            ? 0
                            : converter.ToFloat(value);
                    })
                    .ToList();
            }
            foreach(XElement element in elements) {
                try {
                    process(element, element.Name.LocalName);
                } catch(Exception e) {
                    TraceError(element, root, e);
                }
            }
        }
        static void IterateElements(XElement root, XName name, Action<XElement, string> process) {
            foreach(var element in root.Elements(name)) {
                try {
                    process(element, element.Name.LocalName);
                } catch(Exception e) {
                    TraceError(element, root, e);
                }
            }
        }
        public void UpdateControlsReportDataSource(XRControl control, ExpressionParserResult result) {
            if(result.UsedScopes.Count == 0)
                return;
            if(result.UsedScopes.Count > 1)
                TraceInfo(Messages.ControlMultipleDataMembers_NotSupported_Format, control.Name);
            string scope = result.UsedScopes[0];
            XtraReportBase report = control.Report;
            DataPair pair;
            if(!dataSetToDataPairMap.TryGetValue(scope, out pair)) {
                TraceInfo(Messages.ControlUsesInvalidScope_Format, control.Name, scope);
                return;
            }
            if(report.DataSource == pair.Source && Equals(report.DataMember, scope))
                return;
            if(report.DataSource == null && string.IsNullOrEmpty(report.DataMember)) {
                report.DataSource = pair.Source;
                report.DataMember = pair.Member;
            } else {
                TraceInfo(Messages.ControlUsesDifferentScope_NotSupported_Format, control.Name, scope, report.DataMember);
            }
        }

        static void PostProcessClean(XtraReportBase report) {
            BandCollection bands = report.Bands;
            for(int i = bands.Count - 1; i >= 0; i--) {
                Band band = bands[i];
                var detailReportBand = band as DetailReportBand;
                if(detailReportBand != null && detailReportBand.Bands.Count == 1) {
                    var detailBand = detailReportBand.Bands[0] as DetailBand;
                    if(detailBand != null && detailBand.Controls.Count == 0) {
                        detailReportBand.Dispose();
                        break;
                    }
                }
            }
        }

        static void TraceInfo(string format, params object[] args) {
            Tracer.TraceInformation(NativeSR.TraceSource, new FormattableString(format, args));
        }
        static void TraceWarning(string format, params object[] args) {
            Tracer.TraceWarning(NativeSR.TraceSource, new FormattableString(format, args));
        }
        static void TraceError(XElement element, XElement root, Exception exception) {
            TraceError(Messages.LayoutGenericError_Format, element, root, exception);
        }
        static void TraceError(string format, params object[] args) {
            Tracer.TraceError(NativeSR.TraceSource, new FormattableString(format, args));
        }
    }
    [Serializable]
    class FormattableString {
        public string Format { get; }
        public object[] Args { get; }
        public FormattableString(string format, params object[] args) {
            Format = format;
            Args = args;
        }
        public override string ToString() {
            return string.Format(Format, Args);
        }
        public FormattableString Append(FormattableString other) {
            return new FormattableString("{0} {1}", this, other);
        }
    }
    interface IReportingServicesConverter {
        UnitConverter UnitConverter { get; }
        string ReportFolder { get; }
        bool IgnoreQueryValidation { get; }
        void ProcessCommonControlProperties(XElement element, XRControl control, float yBodyOffset, bool throwException = true);
        void SetComponentName<T>(T component, string name = null);
        void ProcessReportItem(XElement reportItem, XRControl container, ref float yBodyOffset);
        bool TryGetExpression(string value, string componentName, out ExpressionParserResult result);
        bool TryGetDataPair(string dataSetName, out DataPair pair);
        void UpdateControlsReportDataSource(XRControl control, ExpressionParserResult expressionParserResult);
    }
    static class ReportingServicesConverterExtensions {
        public static void SetComponentName<T>(this IReportingServicesConverter converter, T component, XElement element) {
            converter.SetComponentName(component, element.Attribute("Name")?.Value);
        }
        public static CriteriaOperator ParseExpression(this IReportingServicesConverter converter, string value, string componentName) {
            ExpressionParserResult expressionParserResult;
            if(converter.TryGetExpression(value, componentName, out expressionParserResult))
                return expressionParserResult.Criteria;
            return null;
        }
    }
}
