using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Data.Filtering;
using DevExpress.DataAccess;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Native;
using DevExpress.XtraReports.Expressions;
using DevExpress.XtraReports.UI;

namespace DevExpress.XtraReports.Import.ReportingServices.Expressions {
    public class ExpressionParserResult {
        public CriteriaOperator Criteria { get; }
        public bool AccessToFields { get; }
        public bool AccessToPageArguments { get; }
        public bool HasSummary { get; }
        public IList<string> UsedScopes { get; }
        public string Expression {
            get { return Criteria?.ToString() ?? string.Empty; }
        }
        public ExpressionParserResult(CriteriaOperator criteria, bool accessToFields = false, bool accessToPageArguments = false, bool hasSummary = false, IList<string> usedScopes = null) {
            Criteria = criteria;
            AccessToFields = accessToFields;
            AccessToPageArguments = accessToPageArguments;
            HasSummary = hasSummary;
            UsedScopes = usedScopes ?? new string[0];
        }
        public ExpressionBinding ToExpressionBinding(string propertyName, Func<CriteriaOperator, CriteriaOperator> processExpression = null) {
            string eventName = AccessToPageArguments
                ? XRControl.EventNames.PrintOnPage
                : XRControl.EventNames.BeforePrint;
            if(AccessToFields && AccessToPageArguments) {
                Tracer.TraceInformation(NativeSR.TraceSource, Messages.ExpressionParser_AccessToFieldsAndPageArguments_NotSupported);
                eventName = XRControl.EventNames.BeforePrint;
            }
            string actualExpression = processExpression?.Invoke(Criteria).ToString() ?? Expression;
            return new ExpressionBinding(eventName, propertyName, actualExpression);
        }
        public BasicExpressionBinding ToBasicExpressionBinding() {
            return new BasicExpressionBinding("Value", Expression);
        }
        public Expression ToDataAccessExpression() {
            return new Expression(Expression);
        }
    }
    public partial class ExpressionParser {
        const string exceptionDataMark = "ExpressionParserLoggerMark";
        public static ExpressionParserResult ParseSafe(string expression, string componentName, bool useReportSummary, bool allowUnrecognizedFunctions = false) {
            ExpressionParserResult result;
            try {
                result = Parse(expression, componentName, useReportSummary, allowUnrecognizedFunctions);
            } catch(Exception e) {
                result = new ExpressionParserResult(CreateStub(expression, componentName, e));
            }
            return result;
        }
        internal static ExpressionParserResult Parse(string expression, string componentName, bool useReportSummary, bool allowUnrecognizedFunctions = false) {
            var lexer = ExpressionLexer.Create(expression);
            return Parse(lexer, componentName, useReportSummary, allowUnrecognizedFunctions);
        }
        static ExpressionParserResult Parse(yyInput lexer, string componentName, bool useReportSummary, bool allowUnrecognizedFunctions = false) {
            var parser = new ExpressionParser(useReportSummary, componentName, allowUnrecognizedFunctions);
            var result = (CriteriaOperator)parser.yyparse(lexer);
            return new ExpressionParserResult(
                result,
                parser.accessToFields,
                parser.accessToPageArguments,
                parser.summaryFunctionsProvider.IsCalled,
                parser.summaryFunctionsProvider.UsedScopes.ToList());
        }
        FunctionOperator CreateStub(string value) {
            return CreateStub(value, componentName);
        }
        static FunctionOperator CreateStub(string value, string componentName, Exception exception = null) {
            var formattableMessage = new FormattableString(Messages.ExpressionParser_NotSupportedComponentExpression, componentName);
            object messageInstance = CreateMessageInstance(formattableMessage, exception);
            Tracer.TraceInformation(NativeSR.TraceSource, messageInstance);
            const string NotSupportedStub = "#NOT_SUPPORTED#";
            return new FunctionOperator(FunctionOperatorType.Iif, new ConstantValue(true), new ConstantValue(NotSupportedStub), new ConstantValue(value));
        }
        static object CreateMessageInstance(FormattableString message, Exception exception = null) {
            if(exception == null)
                return message;
            if(exception is InvalidOperationException && exception.Data.Contains(exceptionDataMark)) {
                var exceptionMessage = exception.Data[exceptionDataMark] as FormattableString;
                if(exceptionMessage != null)
                    return message.Append(exceptionMessage);
            }
            var exceptionWrapper = new InvalidOperationException(message.ToString(), exception);
            exceptionWrapper.Data[exceptionDataMark] = message;
            return exceptionWrapper;
        }

        readonly string componentName;
        readonly SummaryFunctionsProviderBase summaryFunctionsProvider;
        readonly bool allowUnrecognizedFunctions = false;
        bool accessToFields;
        bool accessToPageArguments;
        ExpressionParser(bool useReportSummary, string componentName, bool allowUnrecognizedFunctions = false) {
            this.componentName = componentName;
            summaryFunctionsProvider = useReportSummary
                ? (SummaryFunctionsProviderBase)new ReportSummaryFunctionsProvider()
                : new GenericSummaryFunctionsProvider();
            this.allowUnrecognizedFunctions = allowUnrecognizedFunctions;
        }
        static void yyerror(string message, FormattableString exceptionData = null) {
            var exception = new InvalidOperationException(message);
            if(exceptionData != null)
                exception.Data[exceptionDataMark] = exceptionData;
            throw exception;
        }
        static void Fail(FormattableString formattableString) {
            yyerror(formattableString.ToString(), formattableString);
        }
        static void Assert(bool condition, FormattableString formattableString) {
            if(!condition)
                Fail(formattableString);
        }
        static void Assert(bool condition, string message = "syntax error") {
            if(!condition)
                yyerror(message);
        }
        CriteriaOperator GetFunctionOperator(CriteriaOperator method, IList<CriteriaOperator> parameters) {
            Utils.Guard.ArgumentMatch(method, nameof(method), x => x is FunctionOperator || x is OperandProperty);
            var userFunctionOperator = method as FunctionOperator;
            if(!ReferenceEquals(userFunctionOperator, null)) {
                if(userFunctionOperator.OperatorType == FunctionOperatorType.Iif)
                    AppendStubToLastOperator(parameters, userFunctionOperator);
                else
                    userFunctionOperator.Operands.AddRange(parameters);
                return userFunctionOperator;
            }
            var functionNameOperand = (OperandProperty)method;
            string functionName = functionNameOperand.PropertyName;
            switch(functionName.ToLowerInvariant()) {
                case "iif":
                    Assert(parameters.Count % 2 == 1 && parameters.Count >= 3, Messages.ExpressionParser_IifOddArguments_Format);
                    return new FunctionOperator(FunctionOperatorType.Iif, parameters);
                case "sum":
                    Assert(parameters.Count == 1, new FormattableString(Messages.ExpressionParser_FunctionSingleArgument_Format, "Sum"));
                    return summaryFunctionsProvider.Create(parameters, Aggregate.Sum);
                case "count":
                    Assert(parameters.Count == 1, new FormattableString(Messages.ExpressionParser_FunctionSingleArgument_Format, "Count"));
                    return summaryFunctionsProvider.Create(parameters, Aggregate.Count);
                case "countdistinct":
                    Assert(parameters.Count == 1, new FormattableString(Messages.ExpressionParser_FunctionSingleArgument_Format, "DCount"));
                    return summaryFunctionsProvider.Create(parameters, "sumDCount");
                case "avg":
                    Assert(parameters.Count == 1, new FormattableString(Messages.ExpressionParser_FunctionSingleArgument_Format, "Avg"));
                    return summaryFunctionsProvider.Create(parameters, Aggregate.Avg);
                case "max":
                    Assert(parameters.Count == 1, new FormattableString(Messages.ExpressionParser_FunctionSingleArgument_Format, "Max"));
                    return summaryFunctionsProvider.Create(parameters, Aggregate.Max);
                case "min":
                    Assert(parameters.Count == 1, new FormattableString(Messages.ExpressionParser_FunctionSingleArgument_Format, "Min"));
                    return summaryFunctionsProvider.Create(parameters, Aggregate.Min);
                case "first":
                    summaryFunctionsProvider.AddScope(parameters);
                    return parameters[0];
                case "formatcurrency":
                    Assert(parameters.Count == 1, new FormattableString(Messages.ExpressionParser_FunctionSingleArgument_Format, "FormatCurrency"));
                    return new FunctionOperator("FormatString", new ConstantValue("{0:C}"), parameters[0]);
                case "formatdatetime":
                    Assert(parameters.Count == 2, new FormattableString(Messages.ExpressionParser_FunctionSingleArgument_Format, "FormatDateTime"));
                    return new FunctionOperator("FormatString", parameters[1], parameters[0]);
                case "countrows":
                    Assert(parameters.Count == 0, "CountRows");
                    return new OperandProperty("DataSource.RowCount");
                case "rownumber":
                    return new OperandProperty("DataSource.CurrentRowIndex");
                case "cdec":
                    Assert(parameters.Count == 1, "CDec");
                    return new FunctionOperator(FunctionOperatorType.ToDecimal, parameters[0]);
                case "cdbl":
                    Assert(parameters.Count == 1, "CDbl");
                    return new FunctionOperator(FunctionOperatorType.ToDouble, parameters[0]);
                case "cint":
                    Assert(parameters.Count == 1, "CInt");
                    return new FunctionOperator(FunctionOperatorType.ToInt, parameters[0]);
                case "trim":
                    Assert(parameters.Count == 1, "Trim");
                    return new FunctionOperator(FunctionOperatorType.Trim, parameters[0]);
                case "ucase":
                    Assert(parameters.Count == 1, "UCase");
                    return new FunctionOperator(FunctionOperatorType.Upper, parameters[0]);
                case "lcase":
                    Assert(parameters.Count == 1, "LCase");
                    return new FunctionOperator(FunctionOperatorType.Lower, parameters[0]);
                case "instr":
                    Assert(parameters.Count == 2, "InStr");
                    return new BinaryOperator(new FunctionOperator(FunctionOperatorType.CharIndex, parameters[1], parameters[0]), new ConstantValue(1), BinaryOperatorType.Plus); // CharIndex([1], [0]) + 1
                case "left":
                    Assert(parameters.Count == 2, "Left");
                    return new FunctionOperator(FunctionOperatorType.Substring, parameters[0], new ConstantValue(0), parameters[1]);
                case "right":
                    Assert(parameters.Count == 2, "Right");
                    var rightLen = new FunctionOperator(FunctionOperatorType.Len, parameters[0]);
                    return new FunctionOperator(
                        FunctionOperatorType.Substring,
                        parameters[0],
                        new BinaryOperator(rightLen, parameters[1], BinaryOperatorType.Minus),
                        parameters[1]);
                case "len":
                    Assert(parameters.Count == 1, "Len");
                    return new FunctionOperator(FunctionOperatorType.Len, parameters[0]);
            }
            if(allowUnrecognizedFunctions)
                return new FunctionOperator(functionName, parameters);
            return CreateStub(new FunctionOperator(functionName, parameters).ToString());
        }
        static void AppendStubToLastOperator(IList<CriteriaOperator> parameters, FunctionOperator userFunctionOperator) {
            int lastIndex = userFunctionOperator.Operands.Count - 1;
            CriteriaOperator lastOperatpr = userFunctionOperator.Operands[lastIndex];
            string value = (lastOperatpr as ConstantValue)?.Value.ToString() ?? lastOperatpr.ToString();
            userFunctionOperator.Operands[lastIndex] = new ConstantValue(new FunctionOperator(value, parameters).ToString());
        }

        CriteriaOperator GetOperandPropertyExclamation(string left, string right) {
            switch(left.ToLowerInvariant()) {
                case "fields":
                    accessToFields = true;
                    return new OperandProperty(right);
                case "parameters":
                    return new OperandParameter(right);
                case "reportitems":
                    return new OperandProperty("ReportItems." + right);
                case "globals":
                    return ProcessGlobals(right);
            }
            Fail(new FormattableString(Messages.ExpressionParser_BuiltInCollection_NotSupported_Format, left));
            return null;
        }
        CriteriaOperator GetOperandPropertyDot(CriteriaOperator criteria, string property) {
            Utils.Guard.ArgumentMatch(criteria, nameof(criteria), x => x is FunctionOperator || x is OperandProperty || x is OperandParameter);
            var operandProperty = criteria as OperandProperty;
            switch(operandProperty?.PropertyName) {
                case "Code":
                    Tracer.TraceWarning(NativeSR.TraceSource, string.Format(Messages.ExpressionParser_Code_NotSupported, property));
                    if(allowUnrecognizedFunctions)
                        return new FunctionOperator(property);
                    else
                        return CreateStub(property);
                case "Globals":
                    return ProcessGlobals(property);
                case "DateFormat":
                    return ProcessDateFormat(property);
            }
            if(string.Equals(property, "Value", StringComparison.CurrentCultureIgnoreCase))
                return criteria;
            if(string.Equals(property, "ToString", StringComparison.CurrentCultureIgnoreCase))
                return new FunctionOperator(FunctionOperatorType.ToStr, criteria);
            Fail(new FormattableString(Messages.ExpressionParser_Field_NotSupported_Format, property));
            return null;
        }
        CriteriaOperator ProcessGlobals(string right) {
            switch(right) {
                case "OverallPageNumber":
                case "PageNumber":
                    accessToPageArguments = true;
                    return new OperandProperty("Arguments.PageIndex");
                case "OverallTotalPages":
                case "TotalPages":
                    accessToPageArguments = true;
                    return new OperandProperty("Arguments.PageCount");
                case "ExecutionTime":
                    return new FunctionOperator(FunctionOperatorType.Now);
            }
            Fail(new FormattableString(Messages.ExpressionParser_GlobalField_NotSupported_Format, right));
            return null;
        }
        CriteriaOperator ProcessDateFormat(string right) {
            switch(right) {
                case "ShortDate":
                    return new ConstantValue("{0:MM/dd/yyyy}");
            }
            Fail(new FormattableString(Messages.ExpressionParser_DateFormat_NotSupported_Format, right));
            return null;
        }
    }
    abstract class SummaryFunctionsProviderBase {
        public HashSet<string> UsedScopes { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public bool IsCalled { get; private set; }
        public CriteriaOperator Create(IList<CriteriaOperator> criteria, Aggregate aggregate) {
            IsCalled = true;
            AddScope(criteria);
            return CreateCore(criteria[0], aggregate);
        }
        public CriteriaOperator Create(IList<CriteriaOperator> criteria, string summaryFunction) {
            IsCalled = true;
            AddScope(criteria);
            return CreateCore(criteria[0], summaryFunction);
        }
        public void AddScope(IList<CriteriaOperator> criteria) {
            if(criteria.Count > 1) {
                var constantValue = criteria.Last() as ConstantValue;
                var stringValue = constantValue?.Value as string;
                if(!string.IsNullOrEmpty(stringValue))
                    UsedScopes.Add(stringValue);
            }
        }
        protected abstract CriteriaOperator CreateCore(CriteriaOperator criteria, Aggregate aggregate);
        protected abstract CriteriaOperator CreateCore(CriteriaOperator criteria, string summaryFunction);
    }
    class GenericSummaryFunctionsProvider : SummaryFunctionsProviderBase {
        protected override CriteriaOperator CreateCore(CriteriaOperator criteria, Aggregate aggregate) {
            return new AggregateOperand(null, criteria, aggregate, null);
        }
        protected override CriteriaOperator CreateCore(CriteriaOperator criteria, string summaryFunction) { 
            throw new NotSupportedException();
        }
    }
    class ReportSummaryFunctionsProvider : SummaryFunctionsProviderBase {
        readonly Dictionary<Aggregate, SummaryFunc> map = new Dictionary<Aggregate, SummaryFunc> {
            { Aggregate.Sum, SummaryFunc.Sum },
            { Aggregate.Avg, SummaryFunc.Avg },
            { Aggregate.Count, SummaryFunc.Count },
            { Aggregate.Max, SummaryFunc.Max },
            { Aggregate.Min, SummaryFunc.Min }
        };
        protected override CriteriaOperator CreateCore(CriteriaOperator criteria, Aggregate aggregate) {
            SummaryFunc summaryFunc;
            if(!map.TryGetValue(aggregate, out summaryFunc)) {
                throw new ArgumentOutOfRangeException(nameof(aggregate), string.Format(Messages.ExpressionParser_NotSupportedAggregate_Format, aggregate));
            }
            return new FunctionOperator("sum" + summaryFunc, criteria);
        }

        protected override CriteriaOperator CreateCore(CriteriaOperator criteria, string summaryFunction) {
            return new FunctionOperator(summaryFunction, criteria);
        }
    }
}
