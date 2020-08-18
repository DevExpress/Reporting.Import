using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DevExpress.Data.Browsing;
using DevExpress.Data.Filtering;
using DevExpress.XtraReports.UI;

namespace DevExpress.XtraReports.Import.ReportingServices.Tablix {
    class BandsConverter {
        readonly IReportingServicesConverter converter;
        readonly ITablixConverter tableConverter;
        readonly Model model;
        int modelRowIndex = 0;
        int groupLevel = int.MaxValue;
        bool detailBandExists = false;
        public BandsConverter(IReportingServicesConverter converter, ITablixConverter tableConverter, Model model) {
            this.converter = converter;
            this.tableConverter = tableConverter;
            this.model = model;
        }
        public bool ConvertDetailReport(XRControl container) {
            if(!(container is Band))
                throw new NotSupportedException($"Cannot convert Matrix to Detail band inside the '{container.Name}' {container.GetType().Name} container, Band only supported.");
            return ConvertDetailReportCore(container);
        }

        private bool ConvertDetailReportCore(XRControl container) {
            XtraReportBase currentReport;
            bool shouldStartNewBand;
            if(model.Element.Parent.Elements().Count() == 1 && container.Controls.Count == 0) {
                currentReport = container.Report;
                converter.SetControlName(container, model.Name);
                shouldStartNewBand = false;
            } else {
                var detailReport = new DetailReportBand();
                converter.SetControlName(detailReport, model.Name);
                // todo: set upper container Height for space before Matrix
                container.Report.Bands.Add(detailReport);
                currentReport = detailReport;
                shouldStartNewBand = true;
            }
            if(currentReport.DataSource == null && string.IsNullOrEmpty(currentReport.DataMember)) {
                DataPair dataPair;
                if(converter.TryGetDataPair(model.DataSetName, out dataPair)) {
                    currentReport.DataSource = dataPair.Source;
                    currentReport.DataMember = dataPair.Member;
                } else {
                    currentReport.DataMember = model.DataSetName;
                }
            }
            XtraReport rootReport = currentReport.RootReport;
            rootReport.BeginUpdate();
            try {
                Convert(model.RowHierarchy.Members, currentReport);
                UpdateGroupLevels(currentReport);
            } finally {
                rootReport.EndUpdate();
            }
            currentReport.FilterString = model.FilterExpression?.ToString() ?? "";
            return shouldStartNewBand;
        }
        void Convert(IEnumerable<TablixMember> members, XtraReportBase report, IEnumerable<Band> parentGeneratedBands = null, float leftOffset = 0) {
            parentGeneratedBands = parentGeneratedBands ?? Enumerable.Empty<Band>();
            foreach(TablixMember member in members) {
                float currentLeftOffset = leftOffset;
                List<Band> currentlyGeneratedBands = ConvertCore(member, report, parentGeneratedBands, ref currentLeftOffset);
                if(member.CanRecursiveIterate()) {
                    IEnumerable<Band> actualGeneratedBands = member.CanReuseGeneratedBands()
                        ? currentlyGeneratedBands
                        : Enumerable.Empty<Band>();
                    Convert(member.Members, report, actualGeneratedBands, currentLeftOffset);
                }
            }
        }
        List<Band> ConvertCore(TablixMember member, XtraReportBase report, IEnumerable<Band> parentGeneratedBands, ref float leftOffset) {
            HashSet<SortExpressionMember> usedSorts = null;
            var result = new List<Band>(1);
            if(member.CanConvertGroupBand())
                result.Add(ConvertGroupBand(member, report, parentGeneratedBands, ref leftOffset, out usedSorts));
            if(member.CanConvertDetailBand())
                result.Add(ConvertDetailBand(member, report, usedSorts, leftOffset, parentGeneratedBands));
            return result;
        }
        Band ConvertGroupBand(TablixMember member, XtraReportBase report, IEnumerable<Band> parentGeneratedBands, ref float leftOffset, out HashSet<SortExpressionMember> usedSorts) {
            usedSorts = null;
            bool isNew;
            GroupBand groupBand = GetOrCreateGroupBand(member, report, parentGeneratedBands, out isNew);
            var groupHeaderBand = groupBand as GroupHeaderBand;
            if(groupHeaderBand != null) {
                usedSorts = new HashSet<SortExpressionMember>();
                foreach(ExpressionMember groupExpression in member.GroupExpressions) {
                    SortExpressionMember sortExpression = member
                        .SortExpressions
                        .FirstOrDefault(x => Equals(x.Expression, groupExpression.Expression));
                    if(sortExpression != null && !sortExpression.IsEmpty)
                        usedSorts.Add(sortExpression);
                    string dataMember = groupExpression.GetMemberOrGenerateCalculatedField(report, member.GroupName, converter);
                    var sortOrder = sortExpression?.SortOrder ?? XRColumnSortOrder.None;
                    var groupField = new GroupField(dataMember, sortOrder);
                    groupHeaderBand.GroupFields.Add(groupField);
                }
            }
            TablixMemberGroupInfo info = member.GetGroupInfo();
            ConvertTable(member, groupBand, info.TableSource, ref leftOffset);
            return groupBand;
        }
        GroupBand GetOrCreateGroupBand(TablixMember member, XtraReportBase report, IEnumerable<Band> parentGeneratedBands, out bool isNew) {
            GroupBand groupBand = !detailBandExists
                ? GetOrCreateGroupBand<GroupHeaderBand>(member, report, parentGeneratedBands, out isNew)
                : GetOrCreateGroupBand<GroupFooterBand>(member, report, parentGeneratedBands, out isNew);
            return groupBand;
        }
        GroupBand GetOrCreateGroupBand<T>(TablixMember member, XtraReportBase report, IEnumerable<Band> parentGeneratedBands, out bool isNew)
            where T : GroupBand, new() {
            isNew = false;
            T groupBand = parentGeneratedBands
                .OfType<T>()
                .FirstOrDefault();
            if(groupBand == null) {
                TablixMemberGroupInfo info = member.GetGroupInfo();
                groupBand = new T {
                    Level = groupLevel,
                    PrintAcrossBands = info.PrintAcrossBands
                };
                groupLevel += detailBandExists ? 1 : -1;
                InitializeNewBand(groupBand, member, report);
                isNew = true;
            }
            return groupBand;
        }
        Band ConvertDetailBand(TablixMember member, XtraReportBase report, HashSet<SortExpressionMember> usedSorts, float leftOffset, IEnumerable<Band> parentGeneratedBands) {
            bool isNew = false;
            DetailBand detailBand = parentGeneratedBands
                .OfType<DetailBand>()
                .FirstOrDefault()
                ?? ReportingServicesConverter.EnsureEmptyBand<DetailBand>(report, BandKind.Detail,
                    x => InitializeNewBand(x, member, report));
            if(isNew)
                InitializeNewBand(detailBand, member, report);
            GroupField[] sortFields = member
                .SortExpressions
                .Where(x => usedSorts == null || !usedSorts.Contains(x))
                .Select(x => new GroupField(x.GetMemberOrGenerateCalculatedField(report, member.GroupName, converter), x.SortOrder))
                .ToArray();
            detailBand.SortFields.AddRange(sortFields);
            ConvertTable(member, detailBand, TableSource.CellContents, ref leftOffset);
            detailBandExists = true;
            groupLevel = 0;
            return detailBand;
        }
        void ConvertTable(TablixMember member, Band band, TableSource tableSource, ref float leftOffset) {
            if(tableSource == TableSource.None)
                return;
            if(tableSource.HasFlag(TableSource.Header) && member.Header == null)
                System.Diagnostics.Debug.Fail("Expected Header");
            int membersCount = member.CountMembers();
            List<RowModel> rowModels = model.Rows
                .Skip(modelRowIndex)
                .Take(membersCount)
                .ToList();
            //System.Diagnostics.Debug.Assert(rowModels.Count == membersCount);
            if(tableSource.HasFlag(TableSource.CellContents))
                modelRowIndex += rowModels.Count;
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
                case TableSource.CellContents | TableSource.Header:
                    tableBounds = new RectangleF(model.Bounds.X + leftOffset, 0, model.Bounds.Width - leftOffset, rowsHeight);
                    break;
                case TableSource.CellContents:
                    tableBounds = new RectangleF(model.Bounds.X + leftOffset, 0, model.Bounds.Width - leftOffset, rowsHeight);
                    break;
                case TableSource.Header:
                    tableBounds = new RectangleF(model.Bounds.X + leftOffset, 0, header.Size, rowsHeight);
                    break;
                default:
                    throw new NotSupportedException();
            }
            float? appendTableWidth = null;
            if(xrTableExist) {
                xrTable = (XRTable)band.Controls[0];
                appendTableWidth = tableBounds.Width;
            } else {
                xrTable = new XRTable { BoundsF = tableBounds };
                xrTable.Dpi = band.Dpi;
                band.Controls.Add(xrTable);
            }
            xrTable.BeginInit();
            if(appendTableWidth.HasValue)
                xrTable.WidthF += appendTableWidth.Value;
            try {
                IList<float> columns = tableSource.HasFlag(TableSource.CellContents)
                    ? (IList<float>)model.Columns
                    : new float[0];
                HeaderModel actualHeader = tableSource.HasFlag(TableSource.Header)
                    ? header
                    : null;
                for(int i = 0; i < rowModels.Count; i++) {
                    XRTableRow xrTableRow;
                    if(xrTableExist) {
                        xrTableRow = xrTable.Rows[i];
                    } else {
                        xrTableRow = new XRTableRow();
                        xrTable.Rows.Add(xrTableRow);
                    }
                    tableConverter.ConvertTableRow(rowModels[i], columns, xrTableRow, actualHeader);
                }
            } finally {
                xrTable.EndInit();
            }
            leftOffset = xrTable.RightF;
        }
        void UpdateGroupLevels(XtraReportBase currentReport) {
            BandCollection bands = currentReport.Bands;
            UpdateGroupLevels(bands.OfType<GroupHeaderBand>().Cast<GroupBand>().ToList());
            UpdateGroupLevels(bands.OfType<GroupFooterBand>().Cast<GroupBand>().ToList());
        }
        void UpdateGroupLevels(IList<GroupBand> bands) {
            List<GroupBand> orderedBands = bands
                .OrderBy(x => x.Level)
                .ToList();
            for(int i = 0; i < orderedBands.Count; i++)
                orderedBands[i].Level = i;
        }
        void InitializeNewBand(Band band, TablixMember member, XtraReportBase report) {
            string middle = string.IsNullOrEmpty(member.GroupName)
                ? ""
                : "_" + member.GroupName;
            converter.SetControlName(band, $"{model.Name}{middle}_{band.GetType().Name}");
            band.HeightF = 0;
            band.Dpi = report.Dpi;
            report.Bands.Add(band);
        }

        public void ConvertVBands(XRControl container) {
            throw new NotImplementedException();
        }
    }
}
