using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DevExpress.XtraReports.UI;

namespace DevExpress.XtraReports.Import.ReportingServices.Tablix {
    class TablixToBandsConverter : TablixToBandsConverterBase<RowModel> {
        int groupLevel = int.MaxValue;
        protected override bool UseColumnsHierarchy {
            get { return false; }
        }
        protected override IEnumerable<RowModel> ModelItems {
            get { return model.Rows; }
        }
        public TablixToBandsConverter(IReportingServicesConverter converter, ITableConverter tableConverter, Model model)
            : base(new TablixMemberConductor(), converter, tableConverter, model, shouldUpdateOffsetOnDetailBand: false) {
        }
        protected override bool BeforeConvert_ShouldFillExistContainer(Band container) {
            return model.Element.Parent.Elements().Count() == 1 && container.Controls.Count == 0;
        }
        protected override bool BeforeConvert_ShouldCreateDetailReportBand(Band container) {
            return true;
        }
        protected override Band BeforeTableConvertGroupBand(TablixMember member, XtraReportBase report, IEnumerable<Band> parentGeneratedBands, out HashSet<SortExpressionMember> usedSorts) {
            usedSorts = null;
            GroupBand groupBand = GetOrCreateGroupBand(member, report, parentGeneratedBands);
            var groupHeaderBand = groupBand as GroupHeaderBand;
            if(groupHeaderBand != null) {
                usedSorts = new HashSet<SortExpressionMember>();
                foreach(ExpressionMember groupExpression in member.GroupExpressions) {
                    SortExpressionMember sortExpression;
                    if(member.TryGetSortExpressionMember(groupExpression.Expression, out sortExpression))
                        usedSorts.Add(sortExpression);
                    string dataMember = groupExpression.GetMemberOrGenerateCalculatedField(report, member.GroupName, rootConverter);
                    var sortOrder = sortExpression?.SortOrder ?? XRColumnSortOrder.None;
                    var groupField = new GroupField(dataMember, sortOrder);
                    groupHeaderBand.GroupFields.Add(groupField);
                }
            }
            return groupBand;
        }
        GroupBand GetOrCreateGroupBand(TablixMember member, XtraReportBase report, IEnumerable<Band> parentGeneratedBands) {
            GroupBand groupBand = !DetailBandExists
                ? GetOrCreateGroupBand<GroupHeaderBand>(member, report, parentGeneratedBands)
                : GetOrCreateGroupBand<GroupFooterBand>(member, report, parentGeneratedBands);
            return groupBand;
        }
        GroupBand GetOrCreateGroupBand<T>(TablixMember member, XtraReportBase report, IEnumerable<Band> parentGeneratedBands)
            where T : GroupBand, new() {
            T groupBand = parentGeneratedBands
                .OfType<T>()
                .FirstOrDefault();
            if(groupBand == null) {
                groupBand = new T {
                    Level = groupLevel,
                    PrintAcrossBands = member.GetRowGroupPrintAcrossBands(),
                    RepeatEveryPage = member.RepeatOnNewPage
                };
                groupLevel += DetailBandExists ? 1 : -1;
                InitializeNewBand(groupBand, member, report);
            }
            return groupBand;
        }
        protected override Band BeforeTableConvertDetailBand(TablixMember member, XtraReportBase report, ICollection<SortExpressionMember> usedSorts, IEnumerable<Band> parentGeneratedBands) {
            DetailBand detailBand = parentGeneratedBands
                .OfType<DetailBand>()
                .FirstOrDefault()
                ?? ReportingServicesConverter.EnsureEmptyBand<DetailBand>(report, BandKind.Detail,
                    x => InitializeNewBand(x, member, report));
            GroupField[] sortFields = member
                .SortExpressions
                .Where(x => usedSorts == null || !usedSorts.Contains(x))
                .Select(x => new GroupField(x.GetMemberOrGenerateCalculatedField(report, member.GroupName, rootConverter), x.SortOrder))
                .ToArray();
            detailBand.SortFields.AddRange(sortFields);
            return detailBand;
        }
        protected override void AfterTableConvertDetailBand(TablixMember member) {
            groupLevel = 0;
        }
        protected override float ConvertTableCore(TablixMember member, Band band, TableSource tableSource, float offset, List<RowModel> spanModelItems) {
            List<RowModel> rowModels = spanModelItems;
            float rowsHeight = rowModels
                .Select(x => x.Height)
                .Sum();
            XRTable xrTable = band.Controls.Count == 1
                ? band.Controls[0] as XRTable
                : null;
            bool xrTableExist = xrTable != null;
            band.HeightF = Math.Max(band.HeightF, rowsHeight);

            HeaderModel header = member.Header;
            RectangleF tableBounds;
            switch(tableSource) {
                case TableSource.CellContents:
                case TableSource.CellContents | TableSource.Header:
                    tableBounds = new RectangleF(model.Bounds.X + offset, 0, model.Bounds.Width - offset, rowsHeight);
                    break;
                case TableSource.Header:
                    tableBounds = new RectangleF(model.Bounds.X + offset, 0, header.Size, rowsHeight);
                    break;
                default:
                    throw new NotSupportedException(tableSource.ToString());
            }
            float? appendTableWidth = null;
            if(xrTableExist) {
                xrTable = (XRTable)band.Controls[0];
                appendTableWidth = tableBounds.Width;
            } else {
                xrTable = new XRTable { BoundsF = tableBounds };
                if(!string.IsNullOrEmpty(member.GroupName))
                    rootConverter.SetComponentName(xrTable, member.GroupName + "_table");
                xrTable.Dpi = band.Dpi;
                band.Controls.Add(xrTable);
            }
            xrTable.BeginInit();
            try {
                if(appendTableWidth.HasValue)
                    xrTable.WidthF += appendTableWidth.Value;
                IList<float> columns = tableSource.HasFlag(TableSource.CellContents)
                    ? (IList<float>)model.Columns
                    : new float[0];
                HeaderModel actualHeader = tableSource.HasFlag(TableSource.Header)
                    ? header
                    : null;
                tableConverter.ConvertTableRows(rowModels, columns, xrTable, xrTableExist, actualHeader);
            } finally {
                xrTable.EndInit();
            }
            return xrTable.RightF;
        }
        protected override bool CanReuseGeneratedBands(TablixMember member) {
            return !member.HasGroupRecursive()
                && member.HasHeaderRecursive(false);
        }
        protected override void AfterConvert(XtraReportBase currentReport) {
            BandCollection bands = currentReport.Bands;
            UpdateGroupLevels(bands.OfType<GroupHeaderBand>().Cast<GroupBand>().ToList());
            UpdateGroupLevels(bands.OfType<GroupFooterBand>().Cast<GroupBand>().ToList());
        }
        static void UpdateGroupLevels(IList<GroupBand> bands) {
            List<GroupBand> orderedBands = bands
                .OrderBy(x => x.Level)
                .ToList();
            for(int i = 0; i < orderedBands.Count; i++)
                orderedBands[i].Level = i;
        }
    }
}
