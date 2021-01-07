using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using DevExpress.Data.Filtering;
using DevExpress.XtraReports.UI;

namespace DevExpress.XtraReports.Import.ReportingServices.Tablix {
    class TablixMember {
        public static List<TablixMember> ParseContainer(XElement containerElement, string componentName, IReportingServicesConverter converter) {
            XNamespace ns = containerElement.GetDefaultNamespace();
            XElement tablixMembersElement = containerElement.Element(ns + "TablixMembers");
            if(tablixMembersElement == null)
                return new List<TablixMember>();
            List<TablixMember> tablixMembers = tablixMembersElement
                .Elements(ns + "TablixMember")
                .Select(x => Parse(x, componentName, converter))
                .ToList();
            return tablixMembers;
        }
        static TablixMember Parse(XElement tablixMemberElement, string componentName, IReportingServicesConverter converter) {
            XNamespace ns = tablixMemberElement.GetDefaultNamespace();
            XElement group = tablixMemberElement.Element(ns + "Group");
            var repeatOnNewPage = bool.Parse(tablixMemberElement.Element(ns + "RepeatOnNewPage")?.Value ?? "false");
            string groupName = group?.Attribute("Name").Value;
            HeaderModel header = HeaderModel.Parse(tablixMemberElement.Element(ns + "TablixHeader"), converter.UnitConverter);
            CriteriaOperator filterCriteria = Filter.ParseFilters(group?.Element(ns + "Filters"), componentName, converter);
            List<ExpressionMember> groupExpressions = ExpressionMember.Parse(group?.Element(ns + "GroupExpressions"), componentName, converter);
            List<SortExpressionMember> sortExpressions = SortExpressionMember.Parse(tablixMemberElement.Element(ns + "SortExpressions"), componentName, converter);
            List<TablixMember> members = ParseContainer(tablixMemberElement, componentName, converter);
            return new TablixMember(groupName, header, filterCriteria, groupExpressions, sortExpressions, members, repeatOnNewPage);
        }
        public string GroupName { get; }
        public HeaderModel Header { get; }
        public CriteriaOperator FilterCriteria { get; }
        public ReadOnlyCollection<ExpressionMember> GroupExpressions { get; }
        public ReadOnlyCollection<SortExpressionMember> SortExpressions { get; }
        public ReadOnlyCollection<TablixMember> Members { get; }
        public bool RepeatOnNewPage { get; }

        public TablixMember(string groupName, HeaderModel header, CriteriaOperator filterCriteria, IList<ExpressionMember> groupExpressions, IList<SortExpressionMember> sortExpressions, IList<TablixMember> members, bool repeatOnNewPage) {
            GroupName = groupName;
            Header = header;
            FilterCriteria = filterCriteria;
            GroupExpressions = new ReadOnlyCollection<ExpressionMember>(groupExpressions);
            SortExpressions = new ReadOnlyCollection<SortExpressionMember>(sortExpressions);
            Members = new ReadOnlyCollection<TablixMember>(members);
            RepeatOnNewPage = repeatOnNewPage;
        }

        public bool TryGetSortExpressionMember(CriteriaOperator criteria, out SortExpressionMember sortExpression) {
            sortExpression = SortExpressions.FirstOrDefault(x => Equals(x.Expression, criteria));
            return sortExpression != null;
        }
    }
    [Flags]
    enum TableSource {
        None = 0,
        CellContents = 1,
        Header = 2
    }
    static class TablixMemberExtensions {
        internal static bool IsEmpty(this TablixMember member) {
            return !member.HasGroup()
                && !member.HasHeader();
        }
        public static bool HasGroup(this TablixMember member) {
            return !string.IsNullOrEmpty(member.GroupName);
        }
        public static bool HasHeader(this TablixMember member) {
            return member.Header != null;
        }
        static IEnumerable<TablixMember> GetMembers(this TablixMember member, bool includeEmpty = true) {
            if(!includeEmpty && member.Members.All(x => x.IsEmpty()))
                return Enumerable.Empty<TablixMember>();
            return member.Members;
        }
        public static List<TablixMember> Flatten(this TablixMember member, bool includeEmpty = true) {
            var result = new List<TablixMember> { member };
            result.AddRange(Flatten(member.GetMembers(includeEmpty), x => x.GetMembers(includeEmpty)));
            return result;
        }
        static IEnumerable<T> Flatten<T>(IEnumerable<T> source, Func<T, IEnumerable<T>> getItems) {
            return source.SelectMany(item => new[] { item }.Concat(Flatten(getItems(item), getItems)));
        }
        public static bool HasGroupRecursive(this TablixMember member) {
            return member.Flatten().Any(x => x.HasGroup());
        }
        public static bool HasHeaderRecursive(this TablixMember member, bool includeEmpty = true) {
            return member.Flatten(includeEmpty).Any(x => x.HasHeader());
        }
        public static bool HasContentRecursive(this TablixMember member) {
            return member.HasGroupRecursive() || member.HasHeaderRecursive();
        }
        public static bool HasSubContentRecursive(this TablixMember member) {
            return member.Members.Any(x => x.HasContentRecursive());
        }

        public static bool CanConvertDetailBand(this TablixMember member) {
            return member.HasGroup()
                && !member.Members.Any(x => x.HasGroupRecursive());
        }
        static bool GetGroupCellContentOnNextDetail(TablixMember member) {
            bool hasHeaderAndNoGroup = member.HasHeader() && !member.HasGroupRecursive();
            bool cellContentOnNextDetail = hasHeaderAndNoGroup && !member.HasSubContentRecursive();
            return cellContentOnNextDetail;
        }
        public static bool GetRowGroupPrintAcrossBands(this TablixMember member) {
            bool cellContentOnNextDetail = GetGroupCellContentOnNextDetail(member);
            bool printAcrossBands = !cellContentOnNextDetail
                && member.HasGroupRecursive();
            return printAcrossBands;
        }
        public static TableSource GetGroupTableSource(this TablixMember member) {
            var tableSource = TableSource.None;
            if(member.HasHeader())
                tableSource |= TableSource.Header;
            bool cellContentOnNextDetail = GetGroupCellContentOnNextDetail(member);
            if(cellContentOnNextDetail)
                tableSource |= TableSource.CellContents;
            else if(!member.HasGroupRecursive() && member.IsEmpty())
                tableSource |= TableSource.CellContents;
            return tableSource;
        }
        public static bool HasSubGroups(this TablixMember member) {
            return member.Members
                .SelectMany(x => x.Flatten())
                .Any(x => x.HasGroup());
        }
        public static int CountMembers(this TablixMember member) {
            return !member.HasSubGroups() && member.Members.Count > 0
                ? member.Members.Count
                : 1;
        }
    }
}
