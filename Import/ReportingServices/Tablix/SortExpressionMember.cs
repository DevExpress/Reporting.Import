using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DevExpress.Data.Filtering;
using DevExpress.XtraReports.UI;

namespace DevExpress.XtraReports.Import.ReportingServices.Tablix {
    class SortExpressionMember : ExpressionMember {
        public static new List<SortExpressionMember> Parse(XElement sortExpressionsElements, string componentName, IReportingServicesConverter converter) {
            if(sortExpressionsElements == null)
                return new List<SortExpressionMember>();
            XNamespace ns = sortExpressionsElements.GetDefaultNamespace();
            IEnumerable<SortExpressionMember> members = sortExpressionsElements
                .Elements(ns + "SortExpression")
                .Select(x => ParseSingle(x, componentName, converter));
            var compactedMembers = new HashSet<SortExpressionMember>();
            foreach(var member in members) {
                if(!member.IsEmpty)
                    compactedMembers.Add(member);
            }
            return compactedMembers.ToList();
        }
        static SortExpressionMember ParseSingle(XElement sortExpressionElement, string componentName, IReportingServicesConverter converter) {
            XNamespace ns = sortExpressionElement.GetDefaultNamespace();
            CriteriaOperator criteria = converter.ParseExpression(sortExpressionElement.Element(ns + "Value").Value, componentName);
            string directionString = sortExpressionElement.Element(ns + "Direction")?.Value;
            XRColumnSortOrder direction = directionString == "Descending" ? XRColumnSortOrder.Descending : XRColumnSortOrder.Ascending;
            return new SortExpressionMember(criteria, direction);
        }
        public XRColumnSortOrder SortOrder { get; }
        public SortExpressionMember(CriteriaOperator expression, XRColumnSortOrder sortOrder)
            : base(expression) {
            SortOrder = sortOrder;
        }
        public override bool IsEmpty {
            get { return base.IsEmpty && SortOrder == XRColumnSortOrder.None; }
        }
        public override int GetHashCode() {
            return SortOrder.GetHashCode() ^ base.GetHashCode();
        }
        public override bool Equals(object obj) {
            var sortExpressionMember = obj as SortExpressionMember;
            return sortExpressionMember != null
                && SortOrder == sortExpressionMember.SortOrder
                && base.Equals(obj);
        }
    }
}
