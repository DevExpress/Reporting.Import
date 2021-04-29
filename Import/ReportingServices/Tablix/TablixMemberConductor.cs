using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Native;

namespace DevExpress.XtraReports.Import.ReportingServices.Tablix {
    abstract class TablixMemberConductorBase {
        public int Index { get; protected set; } = 0;
        protected readonly bool useEmptyGroups;
        public TablixMemberConductorBase(bool useEmptyGroups) {
            this.useEmptyGroups = useEmptyGroups;
        }
        public virtual bool CanRecursiveIterate(TablixMember member) {
            return member.HasSubContentRecursive();
        }
        public virtual bool CanConvertGroupBand(TablixMember member) {
            if(member.HasHeader() || !member.HasContentRecursive())
                return true;
            if(member.HasGroup() && member.Members.Count == 0) {
                TableSource tableSource = member.GetGroupTableSource(useEmptyGroups);
                return tableSource != TableSource.None || member.GroupExpressions.Count > 0;
            }
            return false;
        }
        public TableSource GetGroupTableSource(TablixMember member) {
            return member.GetGroupTableSource(useEmptyGroups);
        }
        public bool CanConvertDetailBand(TablixMember member) {
            return member.CanConvertDetailBand(useEmptyGroups);
        }
        public abstract TableSource GetDetailTableSource(TablixMember member);
        public TResult DoWithSpanModelItemsAndUpdateIndex<TModelItem, TResult>(TablixMember member, IEnumerable<TModelItem> items, TableSource tableSource, Func<List<TModelItem>, TResult> func) {
            int membersCount = member.CountMembers();
            List<TModelItem> spanModelItems = items
                .Skip(Index)
                .Take(membersCount)
                .ToList();
            if(membersCount > 0 && spanModelItems.Count == 0)
                Tracer.TraceWarning(NativeSR.TraceSource, "Tablix Member Conductor has invalid members count.");
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
        readonly bool saveGroups;
        public TablixMemberConductor(bool useEmptyGroups = false, bool saveGroups = false)
            : base(useEmptyGroups) {
            this.saveGroups = saveGroups;
        }
        public override bool CanRecursiveIterate(TablixMember member) {
            return base.CanRecursiveIterate(member)
                || (useEmptyGroups && member.Members.Any(x => x.IsEmpty()));
        }
        public override bool CanConvertGroupBand(TablixMember member) {
            if(useEmptyGroups && member.IsEmpty())
                return false;
            if(saveGroups && member.HasGroup() && member.Members.Count > 0)
                return true;
            return base.CanConvertGroupBand(member);
        }
        public override TableSource GetDetailTableSource(TablixMember member) {
            return useEmptyGroups && member.HasGroup() && member.Members.Count > 0 && !member.Members.Any(x => x.HasGroupRecursive())
                ? TableSource.None
                : TableSource.CellContents;
        }
        protected override bool AfterTableConvertedPredicate(TablixMember member, TableSource tableSource) {
            return tableSource.HasFlag(TableSource.CellContents);
        }
    }
    class TablixMemberVConductor : TablixMemberConductorBase {
        public TablixMemberVConductor()
            : base(false) {
        }
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
