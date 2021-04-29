using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Native;
using DevExpress.XtraReports.UI;

namespace DevExpress.XtraReports.Import.ReportingServices.Tablix {
    partial class TablixConverter : ITableConverter {
        readonly IReportingServicesConverter rootConverter;
        public TablixConverter(IReportingServicesConverter rootConverter) {
            this.rootConverter = rootConverter;
        }
        public ConvertionResult Convert(XElement element, XRControl container, float yBodyOffset, IReportingServicesConverter rootConverter) {
            var model = Model.Parse(element, this.rootConverter.UnitConverter, rootConverter);
            bool hasAnyRowGroups = model.RowHierarchy.HasAnyGroup();
            bool hasAnyColumnGroups = model.ColumnHierarchy.HasAnyGroup();
            bool hasSingleColumnGroup = model.ColumnHierarchy.HasSingleGroup();
            bool shouldStartNewBand;
            try {
                if(!(hasAnyRowGroups || hasAnyColumnGroups)) {
                    new TablixToStaticTableConverter(this.rootConverter, this).ConvertStaticTable(model, container, yBodyOffset);
                    shouldStartNewBand = false;
                } else if(hasAnyRowGroups && !hasAnyColumnGroups)
                    shouldStartNewBand = new TablixToBandsConverter(rootConverter, this, model).Convert(container);
                else if(!hasAnyRowGroups && hasSingleColumnGroup)
                    shouldStartNewBand = new TablixToVBandsConverter(rootConverter, this, model).Convert(container);
                else {
                    new TablixToCrossTabConverter(rootConverter, this, model, container.Report).Convert(container, yBodyOffset);
                    shouldStartNewBand = false;
                }
            } catch(Exception e) {
                Tracer.TraceWarning(NativeSR.TraceSource, e);
                GenerateStub(model, container);
                shouldStartNewBand = false;
            }
            return new ConvertionResult(shouldStartNewBand, model.Bounds.Height);
        }
        public void ConvertTableColumns(IEnumerable<RowModel> rowModels, int rowModelCellIndexOffset, IList<float> columns, XRTable xrTable, HeaderModel header = null) {
            if(header != null) {
                var xrRowHeader = new XRTableRow();
                xrRowHeader.Dpi = xrTable.Dpi;
                xrRowHeader.HeightF = header.Size;
                xrTable.Rows.Add(xrRowHeader);
                var xrCell = new XRTableCell();
                xrCell.Dpi = xrRowHeader.Dpi;
                xrRowHeader.Cells.Add(xrCell);
                ProcessTablixCell(header.Cell, xrCell, columns[0], columns);
            }
            foreach(RowModel rowModel in rowModels)
                ConvertTableColumn(rowModelCellIndexOffset, columns, xrTable, rowModel);
        }
        void ConvertTableColumn(int rowModelCellIndexOffset, IList<float> columns, XRTable xrTable, RowModel rowModel) {
            var xrRow = new XRTableRow();
            xrRow.Dpi = xrTable.Dpi;
            xrRow.HeightF = rowModel.Height;
            xrTable.Rows.Add(xrRow);
            for(int i = 0; i < columns.Count; i++) {
                XElement xCell = rowModel.Cells[rowModelCellIndexOffset + i];
                float columnWidth = columns[i];
                XRTableCell xrCell;
                if(xCell == null) {
                    if(xrRow.Cells.Count == 0) {
                        xrCell = new XRTableCell {
                            Dpi = xrRow.Dpi,
                            WidthF = columnWidth
                        };
                        xrRow.Cells.Add(xrCell);
                    } else {
                        xrCell = xrRow.Cells[xrRow.Cells.Count - 1];
                        xrCell.WidthF += columnWidth;
                    }
                    continue;
                }
                xrCell = new XRTableCell();
                xrCell.Dpi = xrRow.Dpi;
                xrRow.Cells.Add(xrCell);
                ProcessTablixCell(xCell, xrCell, columnWidth, columns);
            }
        }
        public void ProcessTablixCell(XElement element, XRTableCell cell, float? columnWidth = null, IList<float> columns = null) {
            if(columnWidth.HasValue)
                cell.WidthF = columnWidth.Value;
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
                        if(columns != null) {
                            cell.WidthF = columns.Skip(cell.Index).Take(int.Parse(e.Value)).Sum();
                            if(cell.Controls.Count == 1) {
                                cell.Controls[0].WidthF = cell.WidthF;
                            }
                        }
                        break;
                    default:
                        var yBodyOffset = 0f;
                        rootConverter.ProcessReportItem(e, cell, ref yBodyOffset);
                        break;
                }
            });
        }
        public void ConvertTableRows(IList<RowModel> rowModels, IList<float> columns, XRTable xrTable, bool useExistTableRow, HeaderModel header = null) {
            for(int i = 0; i < rowModels.Count; i++) {
                XRTableRow xrTableRow;
                if(useExistTableRow && xrTable.Rows.Count > 0) {
                    int rowIndex = Math.Min(i, xrTable.Rows.Count - 1);
                    xrTableRow = xrTable.Rows[rowIndex];
                } else {
                    xrTableRow = new XRTableRow();
                    xrTableRow.Dpi = xrTable.Dpi;
                    xrTable.Rows.Add(xrTableRow);
                }
                ConvertTableRow(rowModels[i], columns, xrTableRow, header);
            }
        }
        public void ConvertTableRow(RowModel rowModel, IList<float> columns, XRTableRow xrRow, HeaderModel header = null) {
            xrRow.HeightF = rowModel.Height;
            if(header != null) {
                var xrCell = new XRTableCell();
                xrCell.Dpi = xrRow.Dpi;
                xrRow.Cells.Add(xrCell);
                ProcessTablixCell(header.Cell, xrCell, columnWidth: header.Size, columns: columns);
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
                xrCell.Dpi = xrRow.Dpi;
                xrRow.Cells.Add(xrCell);
                ProcessTablixCell(xCell, xrCell, columnWidth, columns);
            }
        }
        static void GenerateStub(Model model, XRControl container) {
            var stubLabel = new XRLabel {
                Name = model.Name,
                Text = Messages.NotConvertedControl_Stub,
                Dpi = container.Dpi,
                BoundsF = model.Bounds
            };
            container.Controls.Add(stubLabel);
        }
    }
    interface ITableConverter {
        void ConvertTableRows(IList<RowModel> rowModels, IList<float> columns, XRTable xrTable, bool useExistTableRow, HeaderModel header = null);
        void ConvertTableRow(RowModel rowModel, IList<float> columns, XRTableRow xrRow, HeaderModel header = null);
        void ConvertTableColumns(IEnumerable<RowModel> rowModels, int startModelColumnIndex, IList<float> columns, XRTable xrTable, HeaderModel header = null);
        void ProcessTablixCell(XElement element, XRTableCell cell, float? columnWidth = null, IList<float> columns = null);
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
