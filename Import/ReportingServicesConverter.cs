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
            "http://schemas.microsoft.com/sqlserver/reporting/2008/01/reportdefinition",
            "http://schemas.microsoft.com/sqlserver/reporting/2010/01/reportdefinition",
            "http://schemas.microsoft.com/sqlserver/reporting/2016/01/reportdefinition",
        };

        public UnrecognizedFunctionBehavior UnrecognizedFunctionBehavior { get; set; } = UnrecognizedFunctionBehavior.InsertWarning;

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
                    case "Language":                  // ???
                    case "ConsumeContainerWhitespace":// ???
                    case "ReportUnitType":            //handled
                    case "DataSources":               //handled
                    case "DataSets":                  //handled
                    case "ReportID":                  //not supported
                    case "ReportParametersLayout":    //not supported
                    case "AutoRefresh":               //not supported
                    case "Author":                    //not supported
                    case "Width":                     // < 2008        ??????????? todo
                    case "ReportTemplate":
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
                    case "Width":                     //??????? todo
                        break;
                    default:
                        TraceInfo(Messages.ReportSectionElement_NotSupported_Format, name);
                        break;
                }
            });
        }

        void ProcessPage(XElement page) {
            var detailBand = EnsureEmptyBand<DetailBand>(TargetReport, BandKind.Detail);
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
                        detailBand.MultiColumn.ColumnCount = int.Parse(e.Value);
                        break;
                    case "ColumnSpacing":
                        detailBand.MultiColumn.ColumnSpacing = unitConverter.ToFloat(e.Value);
                        break;
                    case "InteractiveHeight":        //?????
                    case "InteractiveWidth":
                        break;
                    case "Style":                    //todo crossband or notSupported?
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
                        break; // skip
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
                    TraceInfo(Messages.ReportItemsElement_NotSupported_Format, reportItem.Name.LocalName);
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
                control = new XRLabel { Multiline = true, CanGrow = true };
                container.Controls.Add(control);
            }
            ProcessTextBoxAsLabel(textBoxElement, control, yBodyOffset);
        }
        public void ProcessTextBoxAsLabel(XElement textBoxElement, XRLabel control, float yBodyOffset) {
            var runs = textBoxElement.Descendants(xmlns + "TextRun");
            if(runs.Count() > 1) {
                throw new NotSupportedException("Label can be converted from single TextRun.");
            }
            this.SetControlName(control, textBoxElement);
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
        public void SetControlName<T>(T control, string name) {
            NamingMapper.GenerateAndAssignXRControlName(control, name);
        }

        void ProcessTextBoxAsRichText(XElement textBoxElement, XRControl container, float yBodyOffset) {
            var control = new XRRichText();
            this.SetControlName(control, textBoxElement);
            container.Controls.Add(control);
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
                    default:
                        TraceInfo(Messages.RichParagraphElement_NotSupported_Format, name);
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
                const string format = "RichText with Run Expression '{0}' is not supported.";
                TraceInfo(format, expressionResult.Expression);
                range = documentServer.Document.AppendText(string.Format(format, expressionResult.Expression));
            } else {
                var isHtml = textRun.Element(xmlns + "MarkupType")?.Value == "HTML";
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
                    case "Label":         //not supported in rich ???
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
                                    break;
                                default:
                                    TraceInfo(Messages.RichTextRunElement_NotSupported_Format, textRunElementName);
                                    break;
                            }
                        });
                        break;
                    case "SpaceBefore":
                    case "SpaceAfter":
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
            var control = new XRPanel();
            container.Controls.Add(control);
            this.SetControlName(control, rectangleElement);
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
            ConvertionResult convertionResult = matrixConverter.Convert(e, container, yBodyOffset, this);
            if(convertionResult.ShouldStartNewBand) {
                yBodyOffset += convertionResult.MatrixHeight;
                var report = container.Report;
                var detailReportBand = new DetailReportBand();
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
            var control = new XRPictureBox() { Sizing = ImageSizeMode.AutoSize };
            this.SetControlName(control, imageElement);
            container.Controls.Add(control);
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
            var control = new XRShape() { Shape = new XtraPrinting.Shape.ShapeLine() };
            container.Controls.Add(control);
            this.SetControlName(control, lineElement);
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
                    default:
                        TraceInfo("Line Element '{0}' for  is not supported.", name);
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
            var control = new XRSubreport();
            this.SetControlName(control, subreportElement);
            container.Controls.Add(control);
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
            var chart = new XRChart();
            container.Controls.Add(chart);
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
                case "ZIndex":                         // no need to handle ?
                case "RepeatWith":                     // not supported
                case "DataElementOutput":              // not supported
                case "DefaultName":                    // not supported
                case "WatermarkTextbox":               // not supported
                case "ToolTip":                        // not supported
                case "HideDuplicates":                 // todo???
                case "PageBreak":                      // todo!!!
                    break;
                default:
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
                            return;
                        } else control.Visible = bool.Parse(e.Value);
                        break;
                    case "ToggleItem":                // not supported
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
            if(expressionParserResult != null)
                control.ExpressionBindings.Add(expressionParserResult.ToExpressionBinding(nameof(control.Bookmark)));
            else control.Bookmark = bookmark.Value;
        }
        #endregion

        #region appearance
        void ProcessStyle(XElement style, XRControl control) {
            var textAlignment = style.Element(xmlns + "TextAlign")?.Value;
            var verticalAlignment = style.Element(xmlns + "VerticalAlign")?.Value;
            ProcessTextAlignment(textAlignment, verticalAlignment, control);

            IterateElements(style, (e, name) => {
                ExpressionParserResult expressionParserResult;
                TryGetExpression(e, control, false, out expressionParserResult);
                switch(name) {
                    case "Border":
                        ProcessBorder(e, control);
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
                    case "BottomBorder":         // not supported ???
                    case "TopBorder":            // not supported ???
                    case "LeftBorder":           // not supported ???
                    case "RightBorder":          // not supported ???
                    case "Language":             // not supported ???
                    case "NumeralLanguage":      // not supported ???
                    case "LineHeight":           // not supported ???
                    case "Calendar":             // not supported ???
                    default:
                        TraceInfo(Messages.StyleProperty_NotSupported_Format, name, e.Value);
                        break;
                }
            });
        }

        #region TextAlignment
        void ProcessTextAlignment(string horizontalAlignment, string verticalAlignment, XRControl control) {
            var effectiveAlignment = control.GetEffectiveTextAlignment();
            if(IsExpression(horizontalAlignment)) {
                Tracer.TraceWarning(NativeSR.TraceSource, Messages.TextAlign_Expression_NotSupported);
                horizontalAlignment = TextAlignmentToTextAlign(effectiveAlignment);
            } else if(string.IsNullOrEmpty(horizontalAlignment) || horizontalAlignment == "General" || horizontalAlignment == "Default") {
                horizontalAlignment = TextAlignmentToTextAlign(effectiveAlignment);
            }

            if(IsExpression(verticalAlignment)) {
                Tracer.TraceWarning(NativeSR.TraceSource, Messages.VerticalAlign_Expression_NotSupported);
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

        readonly static Dictionary<string, BorderDashStyle> borderDashStyleMap = new Dictionary<string, BorderDashStyle>() {
            { "Default", BorderDashStyle.Solid},
            { "Dotted", BorderDashStyle.Dot},
            { "Dashed", BorderDashStyle.Dash},
            { "Solid", BorderDashStyle.Solid},
            { "DashDot", BorderDashStyle.DashDot},
            { "DashDotDot", BorderDashStyle.DashDotDot},
        };

        void ProcessBorder(XElement border, XRControl control) {
            IterateElements(border, (e, name) => {
                ExpressionParserResult expressionParserResult;
                TryGetExpression(e, control, false, out expressionParserResult);
                switch(name) {
                    case "Style":
                        ProcessBorderDashStyle(e.Value, control, expressionParserResult);
                        break;
                    case "Color":
                        var colorProperty = control is XRShape ? nameof(XRShape.ForeColor) : nameof(XRControl.BorderColor);
                        ProcessColor(e.Value, control, colorProperty, expressionParserResult);
                        break;
                    case "Width":
                        ProcessBorderWidth(e.Value, control, expressionParserResult);
                        break;
                    default:
                        Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.BorderProperty_NotSupported_Format, name, e.Value));
                        break;
                }
            });
        }

        static void ProcessBorderDashStyle(string value, XRControl control, ExpressionParserResult expressionParserResult) {
            if(expressionParserResult != null)
                control.ExpressionBindings.Add(expressionParserResult.ToExpressionBinding(nameof(control.BorderDashStyle)));
            else
                control.BorderDashStyle = borderDashStyleMap.GetValueOrDefault(value, BorderDashStyle.Solid);
        }

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
                Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.StyleExpression_NotSupported_Format, writingMode.Name.LocalName));
            } else {
                var textAngle = writingModeToTextAngleMap.GetValueOrDefault(writingMode.Value, 0);
                if(control is XRLabel)
                    ((XRLabel)control).Angle = textAngle;
                else if(control is XRCrossTabCell)
                    ((XRCrossTabCell)control).Angle = textAngle;
                else
                    Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.CannotApplyStyleProperty_Format, writingMode.Name.LocalName, control.Name));
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
            IterateElements(parameterElement, (e, name) => {
                ExpressionParserResult expressionParserResult;
                TryGetExpression(e, parameter.Name, false, out expressionParserResult);
                switch(name) {
                    case "DefaultValue":
                        ProcessParameterValue(e.Value, parameter, expressionParserResult);
                        break;
                    case "Hidden":
                        parameter.Visible = !bool.Parse(e.Value.ToLower());
                        break;
                    case "MultiValue":
                        parameter.MultiValue = bool.Parse(e.Value.ToLower());
                        break;
                    case "Nullable":
                        parameter.AllowNull = bool.Parse(e.Value.ToLower());
                        break;
                    case "Prompt":
                        parameter.Description = e.Value;
                        break;
                    case "ValidValues":
                        parameter.ValueSourceSettings = ProcessParameterValueSource(e.Elements().Single(), parameter.Type);
                        break;
                    case "DataType":          //handled
                    case "AllowBlank":        //not supported
                        break;
                    default:
                        TraceInfo(Messages.ParameterProperty_NotSupported_Format, name);
                        break;
                }
                TargetReport.Parameters.Add(parameter);
            });
        }
        static void ProcessParameterValue(string value, Parameter parameter, ExpressionParserResult expressionParserResult) {
            if(expressionParserResult != null)
                parameter.ExpressionBindings.Add(expressionParserResult.ToBasicExpressionBinding());
            else
                parameter.Value = ParameterHelper.ConvertFrom(value, parameter.Type, ParameterHelper.GetDefaultValue(parameter.Type));
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
            TraceWarning(Messages.ParameterDynamicLookups_NotSupported);
            return null;
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

        internal static T EnsureEmptyBand<T>(XtraReportBase report, BandKind kind, Action<T> onNew = null)
            where T : Band {
            T band = report.Bands[kind] as T;
            if(band == null) {
                band = (T)XtraReportBase.CreateBand(kind);
                band.Dpi = report.Dpi;
                band.HeightF = 0f;
                if(band is DetailBand) {
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

        public static void TraceInfo(string format, params object[] args) {
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
        struct FormattableString {
            public string Format { get; }
            public object[] Args { get; }
            public FormattableString(string format, params object[] args) {
                Format = format;
                Args = args;
            }
            public override string ToString() {
                return string.Format(Format, Args);
            }
        }
    }
    interface IReportingServicesConverter {
        UnitConverter UnitConverter { get; }
        string ReportFolder { get; }
        void ProcessCommonControlProperties(XElement element, XRControl control, float yBodyOffset, bool throwException = true);
        void SetControlName<T>(T control, string name);
        void ProcessReportItem(XElement reportItem, XRControl container, ref float yBodyOffset);
        bool TryGetExpression(string value, string componentName, out ExpressionParserResult result);
        bool TryGetDataPair(string dataSetName, out DataPair pair);
    }
    static class ReportingServicesConverterExtensions {
        public static void SetControlName<T>(this IReportingServicesConverter converter, T control, XElement element) {
            converter.SetControlName(control, element.Attribute("Name")?.Value);
        }
        public static CriteriaOperator ParseExpression(this IReportingServicesConverter converter, string value, string componentName) {
            ExpressionParserResult expressionParserResult;
            if(converter.TryGetExpression(value, componentName, out expressionParserResult))
                return expressionParserResult.Criteria;
            return null;
        }
    }
}
