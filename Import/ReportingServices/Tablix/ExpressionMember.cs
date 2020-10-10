using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DevExpress.Data.Filtering;
using DevExpress.XtraReports.UI;

namespace DevExpress.XtraReports.Import.ReportingServices.Tablix {
    class ExpressionMember {
        public static List<ExpressionMember> Parse(XElement groupExpressionsElement, string componentName, IReportingServicesConverter converter) {
            if(groupExpressionsElement == null)
                return new List<ExpressionMember>();
            XNamespace ns = groupExpressionsElement.GetDefaultNamespace();
            IEnumerable<ExpressionMember> members = groupExpressionsElement
                .Elements(ns + "GroupExpression")
                .Select(x => ParseSingle(x, componentName, converter));
            var compactedMembers = new HashSet<ExpressionMember>();
            foreach(var member in members) {
                if(!member.IsEmpty)
                    compactedMembers.Add(member);
            }
            return compactedMembers.ToList();
        }
        static ExpressionMember ParseSingle(XElement groupExpressionElement, string componentName, IReportingServicesConverter converter) {
            CriteriaOperator criteria = converter.ParseExpression(groupExpressionElement.Value, componentName);
            return new ExpressionMember(criteria);
        }
        public CriteriaOperator Expression { get; }
        public ExpressionMember(CriteriaOperator expression) {
            Expression = expression;
        }
        public string GetMemberOrGenerateCalculatedField(XtraReportBase report, string groupName, IReportingServicesConverter rootConverter) {
            var operandProperty = Expression as OperandProperty;
            if(!ReferenceEquals(operandProperty, null))
                return operandProperty.PropertyName;
            DevExpress.Data.IDataContainerBase2 dataContainer = report;
            var calculatedField = new CalculatedField(dataContainer.GetEffectiveDataSource(), dataContainer.GetEffectiveDataMember()) {
                Expression = Expression?.ToString()
            };
            rootConverter.SetComponentName(calculatedField, groupName);
            report.RootReport.CalculatedFields.Add(calculatedField);
            return calculatedField.Name;
        }
        public virtual bool IsEmpty {
            get { return ReferenceEquals(Expression, null); }
        }
        public override int GetHashCode() {
            return Expression?.GetHashCode() ?? 0;
        }
        public override bool Equals(object obj) {
            return Equals(Expression, obj);
        }
    }
}
