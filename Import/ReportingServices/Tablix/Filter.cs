using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using DevExpress.Data.Filtering;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Native;

namespace DevExpress.XtraReports.Import.ReportingServices.Tablix {
    class Filter {
        enum SsrsOperatorKind {
            Compare,
            Between,
            In
        }
        public static CriteriaOperator ParseFilters(XElement filterElement, string componentName, IReportingServicesConverter converter) {
            if(filterElement == null)
                return null;
            XNamespace ns = filterElement.GetDefaultNamespace();
            List<CriteriaOperator> filters = filterElement
                .Elements(ns + "Filter")
                .Select(x => ParseFilter(x, componentName, converter).ToCriteria())
                .ToList();
            return CombineFilters(filters);
        }
        static CriteriaOperator CombineFilters(IList<CriteriaOperator> filters) {
            if(filters.Count == 0)
                return string.Empty;
            if(filters.Count == 1)
                return filters[0];
            var compactedFilters = new HashSet<CriteriaOperator>();
            foreach(CriteriaOperator filter in filters) {
                if(!ReferenceEquals(filter, null))
                    compactedFilters.Add(filter);
            }
            if(compactedFilters.Count == 1)
                return compactedFilters.First();
            return new GroupOperator(GroupOperatorType.And, compactedFilters);
        }
        static Filter ParseFilter(XElement filterElement, string componentName, IReportingServicesConverter converter) {
            XNamespace ns = filterElement.GetDefaultNamespace();
            CriteriaOperator criteriaOperator = converter.ParseExpression(filterElement.Element(ns + "FilterExpression").Value, componentName);
            Tuple<SsrsOperatorKind, BinaryOperatorType?> operatorType = GetOperatorType(filterElement.Element(ns + "Operator").Value);
            List<object> values = filterElement
                .Element(ns + "FilterValues")
                .Elements(ns + "FilterValue")
                .Select(x => GetValue(x, componentName, converter))
                .ToList();
            return new Filter(criteriaOperator, operatorType.Item1, operatorType.Item2, values);
        }
        static Tuple<SsrsOperatorKind, BinaryOperatorType?> GetOperatorType(string @operator) {
            switch(@operator) {
                case "Equal":
                    return new Tuple<SsrsOperatorKind, BinaryOperatorType?>(SsrsOperatorKind.Compare, BinaryOperatorType.Equal);
                case "NotEqual":
                    return new Tuple<SsrsOperatorKind, BinaryOperatorType?>(SsrsOperatorKind.Compare, BinaryOperatorType.NotEqual);
                case "GreaterThan":
                    return new Tuple<SsrsOperatorKind, BinaryOperatorType?>(SsrsOperatorKind.Compare, BinaryOperatorType.Greater);
                case "GreaterThanOrEqual":
                    return new Tuple<SsrsOperatorKind, BinaryOperatorType?>(SsrsOperatorKind.Compare, BinaryOperatorType.GreaterOrEqual);
                case "LessThan":
                    return new Tuple<SsrsOperatorKind, BinaryOperatorType?>(SsrsOperatorKind.Compare, BinaryOperatorType.Less);
                case "LessThanOrEqual":
                    return new Tuple<SsrsOperatorKind, BinaryOperatorType?>(SsrsOperatorKind.Compare, BinaryOperatorType.LessOrEqual);
                case "Between":
                    return new Tuple<SsrsOperatorKind, BinaryOperatorType?>(SsrsOperatorKind.Between, null);
                case "In":
                    return new Tuple<SsrsOperatorKind, BinaryOperatorType?>(SsrsOperatorKind.In, null);
                default:
                    Tracer.TraceWarning(NativeSR.TraceSource, $"Filter operator '{@operator}' is not suported.");
                    return new Tuple<SsrsOperatorKind, BinaryOperatorType?>(SsrsOperatorKind.Compare, BinaryOperatorType.Equal);
            }
        }
        readonly CriteriaOperator expression;
        readonly SsrsOperatorKind operatorKind;
        readonly BinaryOperatorType? operatorType;
        readonly List<object> values;
        Filter(CriteriaOperator expression, SsrsOperatorKind operatorKind, BinaryOperatorType? operatorType, List<object> values) {
            this.expression = expression;
            this.operatorKind = operatorKind;
            this.operatorType = operatorType;
            this.values = values;
        }
        public CriteriaOperator ToCriteria() {
            switch(operatorKind) {
                case SsrsOperatorKind.Between:
                    CriteriaOperator fromValue = GetCriteriaValue(values[0]);
                    CriteriaOperator toValue = GetCriteriaValue(values[1]);
                    return new BetweenOperator(expression, fromValue, toValue);
                case SsrsOperatorKind.In:
                    IEnumerable<CriteriaOperator> constValues = values.Select(GetCriteriaValue);
                    return new InOperator(expression, constValues);
                case SsrsOperatorKind.Compare:
                default:
                    CriteriaOperator compareValue = GetCriteriaValue(values[0]);
                    return new BinaryOperator(expression, compareValue, operatorType.Value);
            }
        }
        static CriteriaOperator GetCriteriaValue(object value) {
            var criteria = value as CriteriaOperator;
            if(!ReferenceEquals(criteria, null))
                return criteria;
            return new ConstantValue(value);
        }
        static object GetValue(XElement valueElement, string componentName, IReportingServicesConverter converter) {
            CriteriaOperator criteria = converter.ParseExpression(valueElement.Value, componentName);
            if(!ReferenceEquals(criteria, null))
                return criteria;
            string stringValue = valueElement.Value;
            string dataType = valueElement.Attribute("DataType")?.Value;
            if(!string.IsNullOrEmpty(dataType)) {
                Type type = ReportingServicesConverter.GetTypeFromDataType(dataType);
                if(type != typeof(string))
                    return Convert.ChangeType(stringValue, type, CultureInfo.InvariantCulture);
            }
            return stringValue;
        }
    }
}
