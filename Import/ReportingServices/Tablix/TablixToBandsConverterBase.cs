using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Data.Browsing;
using DevExpress.XtraReports.UI;

namespace DevExpress.XtraReports.Import.ReportingServices.Tablix {
    abstract class TablixToBandsConverterBase<TModelItem> {
        protected readonly TablixMemberConductorBase conductor;
        protected readonly IReportingServicesConverter rootConverter;
        protected readonly ITableConverter tableConverter;
        protected readonly Model model;
        readonly bool shouldUpdateOffsetOnDetailBand;
        protected abstract bool UseColumnsHierarchy { get; }
        Hierarchy Hierarchy {
            get { return UseColumnsHierarchy ? model.ColumnHierarchy : model.RowHierarchy; }
        }
        protected bool DetailBandExists { get; private set; } = false;
        public TablixToBandsConverterBase(TablixMemberConductorBase conductor, IReportingServicesConverter converter, ITableConverter tableConverter, Model model, bool shouldUpdateOffsetOnDetailBand) {
            this.conductor = conductor;
            this.rootConverter = converter;
            this.tableConverter = tableConverter;
            this.model = model;
            this.shouldUpdateOffsetOnDetailBand = shouldUpdateOffsetOnDetailBand;
        }
        public bool Convert(XRControl container) {
            if(container is XRPanel)
                container = container.Band;
            if(!(container is Band))
                throw new NotSupportedException(string.Format(Messages.NestedMatrix_NotSupported_Format, container.Name, container.GetType().Name));
            Tuple<bool, XtraReportBase> result = BeforeConvert((Band)container);
            bool shouldStartNewBand = result.Item1;
            XtraReportBase currentReport = result.Item2;
            ApplyDataSource(currentReport);
            currentReport.FilterString = model.GetFilterString(UseColumnsHierarchy ? TablixMemberHierarchy.Columns : TablixMemberHierarchy.Rows);

            XtraReport rootReport = currentReport.RootReport;
            rootReport.BeginUpdate();
            try {
                Convert(Hierarchy.Members, currentReport);
                AfterConvert(currentReport);
            } finally {
                rootReport.EndUpdate();
            }
            return shouldStartNewBand;
        }
        Tuple<bool, XtraReportBase> BeforeConvert(Band container) {
            XtraReportBase currentReport;
            bool shouldStartNewBand;
            if(BeforeConvert_ShouldFillExistContainer(container)) {
                currentReport = container.Report;
                rootConverter.SetComponentName(container, model.Name);
                shouldStartNewBand = false;
            } else if(BeforeConvert_ShouldCreateDetailReportBand(container)) {
                var detailReport = new DetailReportBand();
                rootConverter.SetComponentName(detailReport, model.Name);
                // todo: set upper container Height for space before Matrix
                container.Report.Bands.Add(detailReport);
                currentReport = detailReport;
                shouldStartNewBand = true;
            } else {
                currentReport = container.Report;
                shouldStartNewBand = BeforeConvert_ShouldStartNewBand(container);
            }
            return Tuple.Create(shouldStartNewBand, currentReport);
        }
        protected abstract bool BeforeConvert_ShouldFillExistContainer(Band container);
        protected abstract bool BeforeConvert_ShouldCreateDetailReportBand(Band container);
        protected virtual bool BeforeConvert_ShouldStartNewBand(Band container) {
            return false;
        }
        void ApplyDataSource(XtraReportBase currentReport) {
            if(currentReport.DataSource == null && string.IsNullOrEmpty(currentReport.DataMember)) {
                DataPair dataPair;
                if(rootConverter.TryGetDataPair(model.DataSetName, out dataPair)) {
                    currentReport.DataSource = dataPair.Source;
                    currentReport.DataMember = dataPair.Member;
                } else {
                    currentReport.DataMember = model.DataSetName;
                }
            }
        }
        void Convert(IEnumerable<TablixMember> members, XtraReportBase currentReport, IEnumerable<Band> parentGeneratedBands = null, float offset = 0) {
            parentGeneratedBands = parentGeneratedBands ?? Enumerable.Empty<Band>();
            foreach(TablixMember member in members) {
                float currentOffset = offset;
                IEnumerable<Band> currentlyGeneratedBands = Convert(member, currentReport, parentGeneratedBands, ref currentOffset);
                if(conductor.CanRecursiveIterate(member)) {
                    IEnumerable<Band> actualGeneratedBands = CanReuseGeneratedBands(member)
                        ? currentlyGeneratedBands
                        : Enumerable.Empty<Band>();
                    Convert(member.Members, currentReport, actualGeneratedBands, currentOffset);
                }
            }
        }
        protected abstract bool CanReuseGeneratedBands(TablixMember member);
        HashSet<Band> Convert(TablixMember member, XtraReportBase report, IEnumerable<Band> parentGeneratedBands, ref float offset) {
            HashSet<SortExpressionMember> usedSorts = null;
            var result = new HashSet<Band>(parentGeneratedBands);
            if(conductor.CanConvertGroupBand(member))
                result.Add(ConvertGroupBand(member, report, parentGeneratedBands, ref offset, out usedSorts));
            if(conductor.CanConvertDetailBand(member))
                result.Add(ConvertDetailBand(member, report, usedSorts, parentGeneratedBands, ref offset));
            return result;
        }
        Band ConvertGroupBand(TablixMember member, XtraReportBase report, IEnumerable<Band> parentGeneratedBands, ref float offset, out HashSet<SortExpressionMember> usedSorts) {
            Band groupBand = BeforeTableConvertGroupBand(member, report, parentGeneratedBands, out usedSorts);
            TableSource tableSource = conductor.GetGroupTableSource(member);
            offset = ConvertTable(member, groupBand, tableSource, offset);
            return groupBand;
        }
        protected abstract Band BeforeTableConvertGroupBand(TablixMember member, XtraReportBase report, IEnumerable<Band> parentGeneratedBands, out HashSet<SortExpressionMember> usedSorts);
        Band ConvertDetailBand(TablixMember member, XtraReportBase report, ICollection<SortExpressionMember> usedSorts, IEnumerable<Band> parentGeneratedBands, ref float offset) {
            Band detailBand = BeforeTableConvertDetailBand(member, report, usedSorts, parentGeneratedBands);
            TableSource tableSource = conductor.GetDetailTableSource(member);
            float newOffset = ConvertTable(member, detailBand, tableSource, offset);
            if(shouldUpdateOffsetOnDetailBand)
                offset = newOffset;
            AfterTableConvertDetailBand(member, tableSource);
            return detailBand;
        }
        protected abstract Band BeforeTableConvertDetailBand(TablixMember member, XtraReportBase report, ICollection<SortExpressionMember> usedSorts, IEnumerable<Band> parentGeneratedBands);
        float ConvertTable(TablixMember member, Band band, TableSource tableSource, float offset) {
            if(tableSource == TableSource.None)
                return offset;
            System.Diagnostics.Debug.Assert(!tableSource.HasFlag(TableSource.Header) || member.HasHeader(), "Expected Header");
            float newOffset = conductor.DoWithSpanModelItemsAndUpdateIndex(
                member, ModelItems, tableSource,
                spanModelItems => ConvertTableCore(member, band, tableSource, offset, spanModelItems));
            return newOffset;
        }
        protected abstract IEnumerable<TModelItem> ModelItems { get; }
        protected abstract float ConvertTableCore(TablixMember member, Band band, TableSource tableSource, float offset, List<TModelItem> modelItems);
        protected virtual void AfterTableConvertDetailBand(TablixMember member, TableSource tableSource) {
            if(tableSource != TableSource.None)
                DetailBandExists = true;
        }
        protected virtual void AfterConvert(XtraReportBase currentReport) { }
        protected void InitializeNewBand(Band band, TablixMember member, XtraReportBase currentReport) {
            string middle = string.IsNullOrEmpty(member.GroupName)
                ? ""
                : "_" + member.GroupName;
            rootConverter.SetComponentName(band, $"{model.Name}{middle}_{band.GetType().Name}");
            band.HeightF = 0;
            if(band is VerticalBand)
                band.WidthF = 0;
            band.Dpi = currentReport.Dpi;
            currentReport.Bands.Add(band);
        }
    }
}
