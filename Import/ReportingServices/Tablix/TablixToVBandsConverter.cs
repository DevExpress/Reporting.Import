using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DevExpress.XtraReports.UI;

namespace DevExpress.XtraReports.Import.ReportingServices.Tablix {
    class TablixToVBandsConverter : TablixToBandsConverterBase<float> {
        Band containerToDispose;
        public TablixToVBandsConverter(IReportingServicesConverter converter, ITableConverter tableConverter, Model model)
            : base(new TablixMemberVConductor(), converter, tableConverter, model, shouldUpdateOffsetOnDetailBand: true) {
        }
        protected override bool UseColumnsHierarchy {
            get { return true; }
        }
        protected override IEnumerable<float> ModelItems {
            get { return model.Columns; }
        }
        protected override bool BeforeConvert_ShouldFillExistContainer(Band container) {
            return container is VerticalDetailBand;
        }
        protected override bool BeforeConvert_ShouldCreateDetailReportBand(Band container) {
            XtraReportBase report = container.Report;
            return report.Bands.OfType<GroupBand>().Any() || container.Controls.Count > 0;
        }
        protected override bool BeforeConvert_ShouldStartNewBand(Band container) {
            containerToDispose = container;
            return false;
        }
        protected override bool CanReuseGeneratedBands(TablixMember member) {
            return true;
        }
        protected override Band BeforeTableConvertGroupBand(TablixMember member, XtraReportBase report, IEnumerable<Band> parentGeneratedBands, out HashSet<SortExpressionMember> usedSorts) {
            usedSorts = null;
            return GetOrCreateGroupBand(member, report, parentGeneratedBands);
        }
        VerticalBand GetOrCreateGroupBand(TablixMember member, XtraReportBase report, IEnumerable<Band> parentGeneratedBands) {
            VerticalBand groupBand = !DetailBandExists
                ? GetBandOrCreateGroupBand<VerticalHeaderBand>(member, report, parentGeneratedBands)
                : GetBandOrCreateGroupBand<VerticalTotalBand>(member, report, parentGeneratedBands);
            return groupBand;
        }
        VerticalBand GetBandOrCreateGroupBand<T>(TablixMember member, XtraReportBase report, IEnumerable<Band> parentGeneratedBands)
            where T : VerticalBand, new() {
            VerticalBand band = parentGeneratedBands
                .OfType<VerticalBand>()
                .FirstOrDefault();
            if(band == null) {
                band = new T();
                InitializeNewBand(band, member, report);
            }
            return band;
        }
        protected override Band BeforeTableConvertDetailBand(TablixMember member, XtraReportBase report, ICollection<SortExpressionMember> usedSorts, IEnumerable<Band> parentGeneratedBands) {
            VerticalDetailBand detailBand = parentGeneratedBands
                .OfType<VerticalDetailBand>()
                .FirstOrDefault()
                ?? ReportingServicesConverter.EnsureEmptyBand<VerticalDetailBand>(report, BandKind.VerticalDetail,
                    x => InitializeNewBand(x, member, report));
            GroupField[] sortFields = member
                .SortExpressions
                .Where(x => usedSorts == null || !usedSorts.Contains(x))
                .Select(x => new GroupField(x.GetMemberOrGenerateCalculatedField(report, member.GroupName, rootConverter), x.SortOrder))
                .ToArray();
            detailBand.SortFields.AddRange(sortFields);
            return detailBand;
        }
        protected override float ConvertTableCore(TablixMember member, Band band, TableSource tableSource, float offset, List<float> spanModelItems) {
            List<float> columns = spanModelItems;
            float columnsWidth = columns.Sum();
            XRTable xrTable = band.Controls.Count == 1
                ? band.Controls[0] as XRTable
                : null;
            bool xrTableExist = xrTable != null;
            HeaderModel header = member.Header;
            RectangleF tableBounds;
            switch(tableSource) {
                case TableSource.CellContents | TableSource.Header:
                    tableBounds = new RectangleF(model.Bounds.X, offset, columnsWidth, model.Bounds.Height - offset);
                    break;
                case TableSource.CellContents:
                    tableBounds = new RectangleF(0, offset, columnsWidth, model.Bounds.Height - offset);
                    break;
                case TableSource.Header:
                    tableBounds = new RectangleF(model.Bounds.X, offset, columnsWidth, header.Size);
                    break;
                default:
                    throw new NotSupportedException(tableSource.ToString());
            }
            float? appendTableHeight = null;
            if(xrTableExist)
                appendTableHeight = tableBounds.Height;
            else {
                xrTable = new XRTable { BoundsF = tableBounds };
                xrTable.Dpi = band.Dpi;
                band.Controls.Add(xrTable);
            }
            xrTable.BeginInit();
            xrTable.WidthF = Math.Max(xrTable.WidthF, columnsWidth);
            if(appendTableHeight.HasValue)
                xrTable.HeightF += appendTableHeight.Value;
            band.SizeF = new SizeF(xrTable.BoundsF.Right, xrTable.BoundsF.Bottom);
            try {
                HeaderModel actualHeader = tableSource.HasFlag(TableSource.Header)
                    ? header
                    : null;
                IEnumerable<RowModel> rows = tableSource.HasFlag(TableSource.CellContents)
                    ? model.Rows
                    : Enumerable.Empty<RowModel>();
                tableConverter.ConvertTableColumns(rows, conductor.Index, columns, xrTable, actualHeader);
            } finally {
                xrTable.EndInit();
            }
            return offset + xrTable.BottomF;
        }
        protected override void AfterConvert(XtraReportBase currentReport) {
            containerToDispose?.Dispose();
        }
    }
}
