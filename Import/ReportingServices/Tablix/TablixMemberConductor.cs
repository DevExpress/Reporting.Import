using System;
using System.Collections.Generic;
using System.Linq;

namespace DevExpress.XtraReports.Import.ReportingServices.Tablix {
    abstract class TablixMemberConductorBase {
        public int Index { get; protected set; } = 0;
        public bool CanRecursiveIterate(TablixMember member) {
            return member.HasSubContentRecursive();
        }
        public virtual bool CanConvertGroupBand(TablixMember member) {
            if(member.HasHeader() || !member.HasContentRecursive())
                return true;
            if(member.HasGroupRecursive() && member.Members.Count == 0) {
                TableSource tableSource = member.GetGroupTableSource();
                return tableSource != TableSource.None || member.GroupExpressions.Count > 0;
            }
            return false;
        }
        public TableSource GetGroupTableSource(TablixMember member) {
            return member.GetGroupTableSource();
        }
        public bool CanConvertDetailBand(TablixMember member) {
            return member.CanConvertDetailBand();
        }
        public abstract TableSource GetDetailTableSource(TablixMember member);
        public TResult DoWithSpanModelItemsAndUpdateIndex<TModelItem, TResult>(TablixMember member, IEnumerable<TModelItem> items, TableSource tableSource, Func<List<TModelItem>, TResult> func) {
            int membersCount = member.CountMembers();
            List<TModelItem> spanModelItems = items
                .Skip(Index)
                .Take(membersCount)
                .ToList();
            TResult result = func(spanModelItems);
            if(AfterTableConvertedPredicate(member, tableSource))
                Index += spanModelItems.Count;
            return result;
        }
        protected abstract bool AfterTableConvertedPredicate(TablixMember member, TableSource tableSource);
    }
    static class TablixMemberConductorBaseExtensions {
        public static void DoWithSpanModelItemsAndUpdateIndex<TModelItem>(this TablixMemberConductorBase conductor, TablixMember member, IEnumerable<TModelItem> items, TableSource tableSource, Action<List<TModelItem>> action = null) {
            conductor.DoWithSpanModelItemsAndUpdateIndex<TModelItem, object>(member, items, tableSource, x => {
                action?.Invoke(x);
                return null;
            });
        }
    }
    class TablixMemberConductor : TablixMemberConductorBase {
        public override TableSource GetDetailTableSource(TablixMember member) {
            return TableSource.CellContents;
        }
        protected override bool AfterTableConvertedPredicate(TablixMember member, TableSource tableSource) {
            return tableSource.HasFlag(TableSource.CellContents);
        }
    }
    class TablixMemberVConductor : TablixMemberConductorBase {
        public override bool CanConvertGroupBand(TablixMember member) {
            return !member.HasGroup() && base.CanConvertGroupBand(member);
        }
        public override TableSource GetDetailTableSource(TablixMember member) {
            TableSource result = TableSource.None;
            if(member.HasHeader())
                result |= TableSource.Header;
            if(member.HasGroup() && !member.HasSubContentRecursive())
                result |= TableSource.CellContents;
            return result;
        }
        protected override bool AfterTableConvertedPredicate(TablixMember member, TableSource tableSource) {
            return tableSource.HasFlag(TableSource.CellContents) && !member.HasSubContentRecursive();
        }
    }
}
