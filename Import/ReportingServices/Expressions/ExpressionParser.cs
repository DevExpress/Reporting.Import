using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public string Expression {
            get { return Criteria?.ToString() ?? string.Empty; }
        }
        public ExpressionParserResult(CriteriaOperator criteria, bool accessToFields = false, bool accessToPageArguments = false, bool hasSummary = false) {
            Criteria = criteria;
            AccessToFields = accessToFields;
            AccessToPageArguments = accessToPageArguments;
            HasSummary = hasSummary;
        }
        public ExpressionBinding ToExpressionBinding(string propertyName, Func<CriteriaOperator, CriteriaOperator> processExpression = null) {
            string eventName = AccessToPageArguments
                ? XRControl.EventNames.PrintOnPage
                : XRControl.EventNames.BeforePrint;
            if(AccessToFields && AccessToPageArguments) {
                ReportingServicesConverter.TraceInfo(Messages.ExpressionParser_AccessToFieldsAndPageArguments_NotSupported);
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
                parser.summaryFunctionsProvider.IsCalled);
        }
        FunctionOperator CreateStub(string value) {
            return CreateStub(value, componentName);
        }
        static FunctionOperator CreateStub(string value, string componentName, Exception exception = null) {
            string message = string.Format(Messages.ExpressionParser_NotSupportedComponentExpression, componentName);
            var messageInstance = exception != null
                ? (object)new InvalidOperationException(message, exception)
                : message;
            Tracer.TraceInformation(NativeSR.TraceSource, messageInstance);
            const string NotSupportedStub = "#NOT_SUPPORTED#";
            return new FunctionOperator(FunctionOperatorType.Iif, new ConstantValue(true), new ConstantValue(NotSupportedStub), new ConstantValue(value));
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
        static void yyerror(string message) {
            throw new InvalidOperationException(message);
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
            switch(functionName.ToLower()) {
                case "iif":
                    Assert(parameters.Count % 2 == 1 && parameters.Count >= 3, Messages.ExpressionParser_IifOddArguments_Format);
                    return new FunctionOperator(FunctionOperatorType.Iif, parameters);
                case "sum":
                    Assert(parameters.Count == 1, string.Format(Messages.ExpressionParser_FunctionSingleArgument_Format, "Sum"));
                    return summaryFunctionsProvider.Create(parameters[0], Aggregate.Sum);
                case "count":
                    Assert(parameters.Count == 1, string.Format(Messages.ExpressionParser_FunctionSingleArgument_Format, "Count"));
                    return summaryFunctionsProvider.Create(parameters[0], Aggregate.Count);
                case "avg":
                    Assert(parameters.Count == 1, string.Format(Messages.ExpressionParser_FunctionSingleArgument_Format, "Avg"));
                    return summaryFunctionsProvider.Create(parameters[0], Aggregate.Avg);
                case "max":
                    Assert(parameters.Count == 1, string.Format(Messages.ExpressionParser_FunctionSingleArgument_Format, "Max"));
                    return summaryFunctionsProvider.Create(parameters[0], Aggregate.Max);
                case "min":
                    Assert(parameters.Count == 1, string.Format(Messages.ExpressionParser_FunctionSingleArgument_Format, "Min"));
                    return summaryFunctionsProvider.Create(parameters[0], Aggregate.Min);
                case "first":
                    //Assert(parameters.Count == 1, string.Format(Messages.ExpressionParser_FunctionSingleArgument_Format, "First"));
                    return parameters[0];
                case "formatcurrency":
                    Assert(parameters.Count == 1, string.Format(Messages.ExpressionParser_FunctionSingleArgument_Format, "FormatCurrency"));
                    return new FunctionOperator("FormatString", new ConstantValue("{0:C}"), parameters[0]);
                case "countrows":
                    Assert(parameters.Count == 0);
                    return new OperandProperty("DataSource.RowCount");
            }
            if(allowUnrecognizedFunctions)
                return new FunctionOperator(functionName, parameters);
            return CreateStub(new FunctionOperator(functionName, parameters).ToString(), componentName);
        }
        static void AppendStubToLastOperator(IList<CriteriaOperator> parameters, FunctionOperator userFunctionOperator) {
            int lastIndex = userFunctionOperator.Operands.Count - 1;
            CriteriaOperator lastOperatpr = userFunctionOperator.Operands[lastIndex];
            string value = (lastOperatpr as ConstantValue)?.Value.ToString() ?? lastOperatpr.ToString();
            userFunctionOperator.Operands[lastIndex] = new ConstantValue(new FunctionOperator(value, parameters).ToString());
        }

        CriteriaOperator GetOperandPropertyExclamation(string left, string right) {
            switch(left) {
                case "Fields":
                    accessToFields = true;
                    return new OperandProperty(right);
                case "Parameters":
                    return new OperandParameter(right);
                case "ReportItems":
                    return new OperandProperty("ReportItems." + right);
                case "Globals":
                    return ProcessGlobals(right);
            }
            throw new InvalidOperationException($"Built-in Collection '{left}!' is not supported.");
        }
        CriteriaOperator GetOperandPropertyDot(CriteriaOperator criteria, string property) {
            Debug.Assert(criteria is OperandProperty || criteria is OperandParameter, $"{criteria.GetType().Name} is not supported.");
            var operandProperty = criteria as OperandProperty;
            switch(operandProperty?.PropertyName) {
                case "Code":
                    Tracer.TraceWarning(NativeSR.TraceSource, $"User defined function '{property}' should be implemeted manualy. Please read the documentation https://docs.devexpress.com/XtraReports/DevExpress.XtraReports.Expressions.CustomFunctions.Register(DevExpress.Data.Filtering.ICustomFunctionOperator--) .");
                    if(allowUnrecognizedFunctions)
                        return new FunctionOperator(property);
                    else
                        return CreateStub(property);
                case "Globals":
                    return ProcessGlobals(property);
            }
            if(property == "Value")
                return criteria;
            //todo: "Name":
            throw new InvalidOperationException($"Property '.{property}' is not supported.");
        }
        CriteriaOperator ProcessGlobals(string right) {
            switch(right) {
                case "OverallPageNumber":
                case "PageNumber":
                    accessToPageArguments = true;
                    return new OperandProperty("Arguments.PageIndex"); // todo: AfterPrint & "Arguments.PageIndex + 1"
                case "OverallTotalPages":
                case "TotalPages":
                    accessToPageArguments = true;
                    return new OperandProperty("Arguments.PageCount"); // todo: AfterPrint
                case "ExecutionTime":
                    return new FunctionOperator(FunctionOperatorType.Now);
                default:
                    throw new InvalidOperationException($"'Global!{right}' is not supported.");
            }
        }
    }
    abstract class SummaryFunctionsProviderBase {
        public bool IsCalled { get; private set; }
        public CriteriaOperator Create(CriteriaOperator criteria, Aggregate aggregate) {
            IsCalled = true;
            return CreateCore(criteria, aggregate);
        }
        protected abstract CriteriaOperator CreateCore(CriteriaOperator criteria, Aggregate aggregate);
    }
    class GenericSummaryFunctionsProvider : SummaryFunctionsProviderBase {
        protected override CriteriaOperator CreateCore(CriteriaOperator criteria, Aggregate aggregate) {
            return new AggregateOperand(null, criteria, aggregate, null);
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
    }
}
