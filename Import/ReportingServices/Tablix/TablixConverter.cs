using System;
using System.Collections.Generic;
using System.Xml.Linq;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Native;
using DevExpress.XtraReports.UI;

namespace DevExpress.XtraReports.Import.ReportingServices.Tablix {
    partial class TablixConverter : ITablixConverter {
        readonly IReportingServicesConverter rootConverter;
        public TablixConverter(IReportingServicesConverter rootConverter) {
            this.rootConverter = rootConverter;
        }
        public ConvertionResult Convert(XElement element, XRControl container, float yBodyOffset, IReportingServicesConverter converter) {
            var model = Model.Parse(element, rootConverter.UnitConverter, converter);
            bool anyRowGroups = model.RowHierarchy.AnyGroup();
            bool anyColumnGroups = model.ColumnHierarchy.AnyGroup();
            bool shouldStartNewBand = true;
            try {
                if(!(anyRowGroups || anyColumnGroups)) {
                    ConvertStaticTable(model, container, yBodyOffset);
                    return new ConvertionResult(false, model.Bounds.Height);
                } else if(anyRowGroups)
                    shouldStartNewBand = new BandsConverter(rootConverter, this, model).ConvertDetailReport(container);
                else if(anyColumnGroups)
                    new BandsConverter(rootConverter, this, model).ConvertVBands(container);
                else {
                    bool xtabSuccess = TryConvertCrossTab(model, container);
                    if(!xtabSuccess) {
                        Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.Tablix_CannotConvert_Format, element.Name.LocalName));
                        GenerateStub(model, container);
                        return new ConvertionResult(false, model.Bounds.Height);
                    }
                }
            } catch(Exception e) {
                Tracer.TraceWarning(NativeSR.TraceSource, e);
                GenerateStub(model, container);
                return new ConvertionResult(false, model.Bounds.Height);
            }
            return new ConvertionResult(shouldStartNewBand, model.Bounds.Height);
        }
        void ConvertStaticTable(Model model, XRControl container, float yBodyOffset) {
            var table = new XRTable {
                BoundsF = model.Bounds,
                Dpi = container.Dpi
            };
            rootConverter.SetControlName(table, model.Element);
            table.BeginInit();
            ReportingServicesConverter.IterateElements(
                model.Element,
                (x, _) => rootConverter.ProcessCommonControlProperties(x, table, yBodyOffset, false));

            for(int i = 0; i < model.Rows.Count; i++) {
                RowModel modelRow = model.Rows[i];
                XRTableRow xrRow = new XRTableRow();
                table.Rows.Add(xrRow);
                ConvertTableRow(modelRow, model.Columns, xrRow);
            }

            table.EndInit();
            container.Controls.Add(table);
        }
        void ProcessTablixCell(XElement element, XRTableCell cell, float columnWidth) {
            cell.WidthF = columnWidth;
            ReportingServicesConverter.IterateElements(element, (e, name) => {
                switch(name) {
                    case "RowSpan":
                        cell.RowSpan = int.Parse(e.Value);
                        break;
                    case "Selected":
                        break;
                    case "Subreport":
                    case "Tablix":
                        string warningMessage = string.Format(Messages.Tablix_NotSupportedInsideTableCell_Format, name);
                        Tracer.TraceWarning(NativeSR.TraceSource, warningMessage + $" '{cell.Name}'.");
                        cell.Text = warningMessage + ".";
                        break;
                    case "ColSpan":
                        break;        //handled
                    default:
                        var yBodyOffset = 0f;
                        rootConverter.ProcessReportItem(e, cell, ref yBodyOffset);
                        break;
                }
            });
        }
        public void ConvertTableRow(RowModel rowModel, IList<float> columns, XRTableRow xrRow, HeaderModel header = null) {
            xrRow.HeightF = rowModel.Height;
            if(header != null) {
                var xrCell = new XRTableCell();
                xrRow.Cells.Add(xrCell);
                ProcessTablixCell(header.Cell, xrCell, header.Size);
            }
            for(int i = 0; i < columns.Count; i++) {
                XElement xCell = rowModel.Cells[i];
                float columnWidth = columns[i];
                XRTableCell xrCell;
                if(xCell == null) {
                    xrCell = xrRow.Cells[xrRow.Cells.Count - 1];
                    xrCell.WidthF += columnWidth;
                    continue;
                }
                xrCell = new XRTableCell();
                xrRow.Cells.Add(xrCell);
                ProcessTablixCell(xCell, xrCell, columnWidth);
            }
        }
        static bool TryConvertCrossTab(Model model, XRControl container) {
            throw new NotImplementedException();
        }
        static void GenerateStub(Model model, XRControl container) {
            var stubLabel = new XRLabel {
                Name = model.Name,
                Text = Messages.NotConvertedControl_Stub,
                BoundsF = model.Bounds
            };
            container.Controls.Add(stubLabel);
        }
    }
    interface ITablixConverter {
        void ConvertTableRow(RowModel rowModel, IList<float> columns, XRTableRow xrRow, HeaderModel header = null);
    }
    struct ConvertionResult {
        public bool ShouldStartNewBand { get; }
        public float MatrixHeight { get; }
        public ConvertionResult(bool shouldStartNewBand, float matrixHeight) {
            ShouldStartNewBand = shouldStartNewBand;
            MatrixHeight = matrixHeight;
        }
    }
}
