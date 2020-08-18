using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using DevExpress.Data.Filtering;

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
            string groupName = group?.Attribute("Name").Value;
            HeaderModel header = HeaderModel.Parse(tablixMemberElement.Element(ns + "TablixHeader"), converter.UnitConverter);
            CriteriaOperator filterCriteria = Filter.ParseFilters(group?.Element(ns + "Filters"), componentName, converter);
            List<ExpressionMember> groupExpressions = ExpressionMember.Parse(group?.Element(ns + "GroupExpressions"), componentName, converter);
            List<SortExpressionMember> sortExpressions = SortExpressionMember.Parse(tablixMemberElement.Element(ns + "SortExpressions"), componentName, converter);
            List<TablixMember> members = ParseContainer(tablixMemberElement, componentName, converter);
            return new TablixMember(groupName, header, filterCriteria, groupExpressions, sortExpressions, members);
        }
        public string GroupName { get; }
        public HeaderModel Header { get; }
        public CriteriaOperator FilterCriteria { get; }
        public ReadOnlyCollection<ExpressionMember> GroupExpressions { get; }
        public ReadOnlyCollection<SortExpressionMember> SortExpressions { get; }
        public ReadOnlyCollection<TablixMember> Members { get; }
        public TablixMember(string groupName, HeaderModel header, CriteriaOperator filterCriteria, IList<ExpressionMember> groupExpressions, IList<SortExpressionMember> sortExpressions, IList<TablixMember> members) {
            GroupName = groupName;
            Header = header;
            FilterCriteria = filterCriteria;
            GroupExpressions = new ReadOnlyCollection<ExpressionMember>(groupExpressions);
            SortExpressions = new ReadOnlyCollection<SortExpressionMember>(sortExpressions);
            Members = new ReadOnlyCollection<TablixMember>(members);
        }
    }
    [Flags]
    enum TableSource {
        None = 0,
        CellContents = 1,
        Header = 2
    }
    struct TablixMemberGroupInfo {
        public TableSource TableSource { get; }
        public bool PrintAcrossBands { get; }
        public TablixMemberGroupInfo(TableSource tableSource, bool printAcrossBands) {
            TableSource = tableSource;
            PrintAcrossBands = printAcrossBands;
        }
    }
    static class TablixMemberExtensions {
        public static bool IsEmpty(this TablixMember member) {
            return string.IsNullOrEmpty(member.GroupName)
                && member.Header == null;
        }
        public static IEnumerable<TablixMember> GetMembers(this TablixMember member, bool includeEmpty = true) {
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
        public static bool HasGroup(this TablixMember member) {
            return !string.IsNullOrEmpty(member.GroupName);
        }
        public static bool HasGroupRecursive(this TablixMember member) {
            return member.Flatten().Any(x => x.HasGroup());
        }
        public static bool HasHeaderRecursive(this TablixMember member) {
            return member.Flatten().Any(x => x.Header != null);
        }
        public static bool HasContentRecursive(this TablixMember member) {
            return member.HasGroupRecursive() || member.HasHeaderRecursive();
        }
        public static bool HasSubContentRecursive(this TablixMember member) {
            return member.Members.Any(x => x.HasContentRecursive());
        }

        public static bool CanRecursiveIterate(this TablixMember member) {
            return member.HasSubContentRecursive();
        }
        public static bool CanConvertGroupBand(this TablixMember member) {
            if(member.Header != null || !member.HasContentRecursive())
                return true;
            if(member.HasGroupRecursive() && member.Members.Count == 0) {
                TablixMemberGroupInfo info = member.GetGroupInfo();
                return info.TableSource != TableSource.None || member.GroupExpressions.Count > 0;
            }
            return false;
        }
        public static bool CanConvertDetailBand(this TablixMember member) {
            return member.HasGroupRecursive()
                && !member.Members.Any(x => x.HasGroupRecursive());
        }
        public static TablixMemberGroupInfo GetGroupInfo(this TablixMember member) {
            var tableSource = TableSource.None;
            bool printAcrossBands = false;
            if(member.Header != null)
                tableSource |= TableSource.Header;
            bool hasHeaderOrNoGroup = member.Header != null && !member.HasGroupRecursive();
            bool cellContentOnNextDetail = hasHeaderOrNoGroup && !member.HasSubContentRecursive();
            if(cellContentOnNextDetail)
                tableSource |= TableSource.CellContents;
            else if(member.HasGroupRecursive())
                printAcrossBands = true;
            else if(member.IsEmpty())
                tableSource |= TableSource.CellContents;
            return new TablixMemberGroupInfo(tableSource, printAcrossBands);
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
        public static bool CanReuseGeneratedBands(this TablixMember member) {
            return !member.HasGroupRecursive()
                && member.Flatten(false).All(x => x.Header != null);
        }
    }
}
