using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DevExpress.Data.Browsing;
using DevExpress.Data.Filtering;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Native;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.UI.CrossTab;

namespace DevExpress.XtraReports.Import.ReportingServices.Tablix {
    class TablixToCrossTabConverter {
        #region classes
        class EqualityComparer : IEqualityComparer<CrossTabDataField> {
            public bool Equals(CrossTabDataField x, CrossTabDataField y) {
                return x.FieldName == y.FieldName
                    && x.SummaryType == y.SummaryType;
            }
            public int GetHashCode(CrossTabDataField obj) {
                return obj.GetHashCode();
            }
        }
        class ColumnSpan {
            public int Start { get; }
            public int Count { get; }
            public ColumnSpan(int start, int count) {
                Start = start;
                Count = count;
            }
        }
        #endregion
        readonly IReportingServicesConverter rootConverter;
        readonly ITableConverter tableConverter;
        readonly Model model;
        readonly XtraReportBase report;
        public TablixToCrossTabConverter(IReportingServicesConverter rootConverter, ITableConverter tableConverter, Model model, XtraReportBase report) {
            this.rootConverter = rootConverter;
            this.tableConverter = tableConverter;
            this.model = model;
            this.report = report;
        }
        public void Convert(XRControl container, float yBodyOffset) {
            var xtab = new XRCrossTab {
                BoundsF = model.Bounds,
                Dpi = container.Dpi
            };
            rootConverter.SetComponentName(xtab, model.Element);
            ApplyDataSource(model, xtab);
            xtab.FilterString = model.GetFilterString(TablixMemberHierarchy.Columns | TablixMemberHierarchy.Rows);
            ReportingServicesConverter.IterateElements(
                model.Element,
                (x, _) => rootConverter.ProcessCommonControlProperties(x, xtab, yBodyOffset, false));

            XtraReportBase report = container.Report;
            ColumnSpan columnSpan = ConvertMembers(
                model.ColumnHierarchy.Members,
                model.Columns,
                xtab.ColumnFields,
                report,
                new TablixMemberVConductor(),
                (c, i) => new ColumnSpan(c.Index, i.Count));
            CrossTabDataField[] dataFields = ConvertMembers(
                model.RowHierarchy.Members,
                model.Rows,
                xtab.RowFields,
                report,
                new TablixMemberConductor(),
                (_, i) => ConvertData(i, columnSpan));
            xtab.DataFields.AddRange(dataFields);
            xtab.GenerateLayout();
            container.Controls.Add(xtab);
            new GenericStylesGenerator(xtab, InitializeNamedStyle).CreateDefaultStyles(true);
        }
        void InitializeNamedStyle(XRControlStyle style, string baseName) {
            rootConverter.SetComponentName(style, baseName);
        }
        void ApplyDataSource(Model model, XRCrossTab xtab) {
            DataPair dataPair;
            if(rootConverter.TryGetDataPair(model.DataSetName, out dataPair)) {
                xtab.DataSource = dataPair.Source;
                xtab.DataMember = dataPair.Member;
            } else {
                xtab.DataMember = model.DataSetName;
            }
        }
        TResult ConvertMembers<TField, TModelItem, TResult>(IEnumerable<TablixMember> members, IEnumerable<TModelItem> modelItems, CrossTabFieldCollection<TField> collection, XtraReportBase report, TablixMemberConductorBase conductor, Func<TablixMemberConductorBase, List<TModelItem>, TResult> onLeaf)
            where TField : CrossTabGroupFieldBase, new() {
            var result = default(TResult);
            foreach(TablixMember member in members) {
                if(member.HasGroup()) {
                    Tuple<string, XRColumnSortOrder> dataSourceField = GetOrCreateField(member, report);
                    var xtabField = new TField {
                        FieldName = dataSourceField.Item1,
                        SortOrder = dataSourceField.Item2
                    };
                    collection.Add(xtabField);
                }
                if(conductor.CanConvertGroupBand(member))
                    conductor.DoWithSpanModelItemsAndUpdateIndex(member, modelItems, conductor.GetGroupTableSource(member));
                if(conductor.CanConvertDetailBand(member)) {
                    result = conductor.DoWithSpanModelItemsAndUpdateIndex(
                        member,
                        modelItems,
                        conductor.GetDetailTableSource(member),
                        items => onLeaf(conductor, items));
                }
                if(conductor.CanRecursiveIterate(member)) {
                    var innerResult = ConvertMembers(member.Members, modelItems, collection, report, conductor, onLeaf);
                    if(Equals(result, default(TResult)))
                        result = innerResult;
                }
            }
            return result;
        }
        Tuple<string, XRColumnSortOrder> GetOrCreateField(TablixMember member, XtraReportBase report) {
            if(member.GroupExpressions.Count == 0)
                return Tuple.Create("", XRColumnSortOrder.None);
            if(member.GroupExpressions.Count > 1)
                Tracer.TraceWarning(NativeSR.TraceSource, "not supported.");
            string fieldName;
            var sortOrder = XRColumnSortOrder.None;
            ExpressionMember firstGroupExpression = member.GroupExpressions[0];
            fieldName = firstGroupExpression.GetMemberOrGenerateCalculatedField(report, member.GroupName, rootConverter);
            SortExpressionMember sortExpression;
            if(member.TryGetSortExpressionMember(firstGroupExpression.Expression, out sortExpression))
                sortOrder = sortExpression.SortOrder;
            return Tuple.Create(fieldName, sortOrder);
        }
        CrossTabDataField[] ConvertData(List<RowModel> rows, ColumnSpan columnSpan) {
            CrossTabDataField[] result = rows
                .Select(x => ConvertCells(x, columnSpan))
                .SelectMany(x => x)
                .Where(x => x != null)
                .ToArray();
            return result;
        }
        IEnumerable<CrossTabDataField> ConvertCells(RowModel rowModel, ColumnSpan columnSpan) {
            IEnumerable<CrossTabDataField> result = rowModel.Cells
                .Skip(columnSpan.Start)
                .Take(columnSpan.Count)
                .Select(ConvertCell);
            return result;
        }
        CrossTabDataField ConvertCell(XElement cell) {
            using(var xrTableCell = new XRTableCell()) {
                tableConverter.ProcessTablixCell(cell, xrTableCell);
                ExpressionBinding textExpression = xrTableCell.ExpressionBindings[XRControl.EventNames.BeforePrint, nameof(XRControl.Text)];
                if(textExpression == null)
                    return null;
                Tuple<SummaryType, CriteriaOperator> summaryAndCriteria = GetSummaryAndCriteria(textExpression.Expression);
                if(summaryAndCriteria == null)
                    return null;
                ExpressionMember expressionMember = new ExpressionMember(summaryAndCriteria.Item2);
                string member = expressionMember.GetMemberOrGenerateCalculatedField(report, null, rootConverter);
                return new CrossTabDataField {
                    FieldName = member,
                    SummaryType = summaryAndCriteria.Item1
                };
            }
        }
        Tuple<SummaryType, CriteriaOperator> GetSummaryAndCriteria(string expression) {
            var criteria = CriteriaOperator.TryParse(expression);
            if(ReferenceEquals(criteria, null))
                return null;
            var functionOperator = criteria as FunctionOperator;
            if(ReferenceEquals(functionOperator, null))
                return null;
            if(functionOperator.OperatorType == FunctionOperatorType.Custom) {
                var ops = functionOperator.Operands;
                if(ops.Count == 2) {
                    var methodName = (ops[0] as ConstantValue)?.Value as string;
                    const string sumPrefix = "sum";
                    if(methodName.StartsWith(sumPrefix)) {
                        SummaryFunc summaryFunc;
                        if(Enum.TryParse(methodName.Substring(sumPrefix.Length), out summaryFunc))
                            return Tuple.Create(Map(summaryFunc), ops[1]);
                    }
                }
            }
            return new Tuple<SummaryType, CriteriaOperator>(SummaryType.Average, criteria);
        }
        static SummaryType Map(SummaryFunc summaryFunc) {
            switch(summaryFunc) {
                case SummaryFunc.Max:
                    return SummaryType.Max;
                case SummaryFunc.Min:
                    return SummaryType.Min;
                case SummaryFunc.Sum:
                    return SummaryType.Sum;
                case SummaryFunc.Avg:
                    return SummaryType.Average;
                case SummaryFunc.Count:
                    return SummaryType.Count;
                default:
                    Tracer.TraceWarning(NativeSR.TraceSource, string.Format("Summary function '{0}' is not supported.", summaryFunc));
                    return SummaryType.Sum;
            }
        }
    }
}
