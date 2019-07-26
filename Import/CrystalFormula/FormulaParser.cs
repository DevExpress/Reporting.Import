using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Data.Filtering;

namespace DevExpress.XtraReports.Design.Import.CrystalFormula {
    public partial class FormulaParser {
        public const string NotSupportedStub = "#NOT_SUPPORTED#";
        CriteriaOperator result;
        FormulaCalculationDirective directive;
        List<OperandParameter> resultParameters = new List<OperandParameter>();
        List<OperandProperty> resultFormulae = new List<OperandProperty>();
        bool allowUnrecognizedFunctions;

        public event Action<string> GotUnrecognizedFunctions;
        FormulaParser() { }
        void yyerror(string message) {
            throw new FormulaParserException(message);
        }
        internal static Formula Parse(yyInput lexer, bool allowUnrecognizedFunctions = false, Action<string> gotUnrecognizedFunctions = null) {
            var parser = new FormulaParser();
            parser.allowUnrecognizedFunctions = allowUnrecognizedFunctions;
            if(gotUnrecognizedFunctions != null)
                parser.GotUnrecognizedFunctions += gotUnrecognizedFunctions;
            parser.yyparse(lexer);
            return new Formula {
                Statement = parser.result,
                Directive = parser.directive,
                Parameters = parser.resultParameters,
                Formulae = parser.resultFormulae,
            };
        }
        static FormulaCalculationDirective GetDirective(object value) {
            return (FormulaCalculationDirective)value;
        }
        OperandParameter GetParameter(object value) {
            string paramName = (string)value;
            Assert(!string.IsNullOrEmpty(paramName));
            foreach(OperandParameter p in resultParameters) {
                if(ReferenceEquals(p, null))
                    continue;
                if(p.ParameterName != paramName)
                    continue;
                resultParameters.Add(p);
                return p;
            }
            OperandParameter param = new OperandParameter(paramName);
            resultParameters.Add(param);
            return param;
        }
        OperandProperty GetFormula(object value) {
            string formulaName = (string)value;
            Assert(!string.IsNullOrEmpty(formulaName));
            foreach(OperandProperty p in resultFormulae) {
                if(ReferenceEquals(p, null))
                    continue;
                if(p.PropertyName != formulaName)
                    continue;
                resultFormulae.Add(p);
                return p;
            }
            OperandProperty prop = new OperandProperty(formulaName);
            resultFormulae.Add(prop);
            return prop;
        }
        CriteriaOperator GetNegativeValue(object source) {
            try {
                if(source is OperandValue) {
                    OperandValue operand = (OperandValue)source;
                    if(operand.Value is Int32) {
                        operand.Value = -(Int32)operand.Value;
                        return operand;
                    } else if(operand.Value is Int64) {
                        operand.Value = -(Int64)operand.Value;
                        return operand;
                    } else if(operand.Value is Double) {
                        operand.Value = -(Double)operand.Value;
                        return operand;
                    } else if(operand.Value is Single) {
                        operand.Value = -(Single)operand.Value;
                        return operand;
                    } else if(operand.Value is Decimal) {
                        operand.Value = -(Decimal)operand.Value;
                        return operand;
                    } else if(operand.Value is Int16) {
                        operand.Value = -(Int16)operand.Value;
                        return operand;
                    } else if(operand.Value is SByte) {
                        operand.Value = -(SByte)operand.Value;
                        return operand;
                    }
                }
            } catch { }
            return new UnaryOperator(UnaryOperatorType.Minus, (CriteriaOperator)source);
        }
        CriteriaOperator GetFunctionOperator(object fnName, object fnParameters) {
            string name = (string)fnName;
            var parameters = (IList<CriteriaOperator>)fnParameters;
            string lcName = name.ToLower();
            switch(lcName) {
                case "isnull":
                    Assert(parameters.Count == 1);
                    return new UnaryOperator(UnaryOperatorType.IsNull, parameters[0]);
                case "lowercase":
                case "lcase":
                    Assert(parameters.Count == 1);
                    return new FunctionOperator(FunctionOperatorType.Lower, parameters);
                case "uppercase":
                case "ucase":
                    Assert(parameters.Count == 1);
                    return new FunctionOperator(FunctionOperatorType.Upper, parameters);
                case "trim":
                    Assert(parameters.Count == 1);
                    return new FunctionOperator(FunctionOperatorType.Trim, parameters);
                case "year":
                    Assert(parameters.Count == 1);
                    return new FunctionOperator(FunctionOperatorType.GetYear, parameters);
                case "month":
                    Assert(parameters.Count == 1);
                    return new FunctionOperator(FunctionOperatorType.GetMonth, parameters);
                case "day":
                    Assert(parameters.Count == 1);
                    return new FunctionOperator(FunctionOperatorType.GetDay, parameters);
                case "date":
                    return CreateDateExpression(parameters);
                case "dateadd":
                    return CreateDateAddExpression(parameters);
                case "space":
                    Assert(parameters.Count == 1);
                    return new FunctionOperator(FunctionOperatorType.PadLeft, new ConstantValue(""), parameters[0]);
                case "iif":
                    Assert(parameters.Count % 2 == 1 && parameters.Count >= 3);
                    return new FunctionOperator(FunctionOperatorType.Iif, parameters);
                case "totext":
                case "cstr":
                    Assert(parameters.Count == 1 || parameters.Count == 2);
                    return parameters.Count == 1
                        ? new FunctionOperator(FunctionOperatorType.ToStr, parameters[0])
                        : new FunctionOperator("FormatString", new OperandValue(GenerateFormatString(parameters[1])), parameters[0]);
                case "sum":
                    return CreateAggregate(parameters, Aggregate.Sum);
                case "count":
                    return CreateAggregate(parameters, Aggregate.Count);
                case "cdbl":
                    Assert(parameters.Count == 1);
                    return new FunctionOperator(FunctionOperatorType.ToDouble, parameters);
                case "chrw":
                    Assert(parameters.Count == 1);
                    return new FunctionOperator(FunctionOperatorType.Char, parameters);
            }
            GotUnrecognizedFunctions?.Invoke(name);
            if(allowUnrecognizedFunctions)
                return new FunctionOperator(name, parameters);
            return new FunctionOperator(FunctionOperatorType.Iif, new OperandValue(true), new OperandValue(NotSupportedStub), new OperandValue(new FunctionOperator(name, parameters).ToString()));
        }
        CriteriaOperator GetSpecialField(object fieldName) {
            throw new FormulaParserException("Special Fields are not supported.");
        }
        static string GenerateFormatString(CriteriaOperator criteriaOperator) {
            var operandValue = criteriaOperator as OperandValue;
            object value = !ReferenceEquals(operandValue, null) ? operandValue.Value : null;
            if(value is string)
                return "{0:" + value + "}";
            if(value is int) {
                int afterDot = (int)value;
                return afterDot == 0
                    ? "{0:0}"
                    : "{0:0." + new string('0', afterDot) + "}";
            }
            return "{0}";
        }
        CriteriaOperator CreateDateExpression(IList<CriteriaOperator> parameters) {
            Assert(parameters.Count == 3);
            if(parameters.OfType<FunctionOperator>()
                .Select(x => x.OperatorType)
                .SequenceEqual(new FunctionOperatorType[] { FunctionOperatorType.GetYear, FunctionOperatorType.GetMonth, FunctionOperatorType.GetDay })) {

                var operands = parameters.OfType<FunctionOperator>().Select(x => x.Operands[0]).ToArray();
                if(operands[0].Equals(operands[1]) && operands[0].Equals(operands[2]))
                    return new FunctionOperator(FunctionOperatorType.GetDate, operands[0]);
            }
            return new FunctionOperator(FunctionOperatorType.AddDays, new FunctionOperator(FunctionOperatorType.AddMonths, new FunctionOperator(FunctionOperatorType.AddYears, new ConstantValue(new DateTime(1, 1, 1)), parameters[0] - 1), parameters[1] - 1), parameters[2] - 1);
        }
        CriteriaOperator CreateDateAddExpression(IList<CriteriaOperator> parameters) {
            Assert(parameters.Count == 3 && parameters[0] is ConstantValue);
            var dateComponentConst = ((ConstantValue)parameters[0]).Value;
            Assert(dateComponentConst is string);
            string dateComponent = ((string)dateComponentConst).ToLower();

            FunctionOperatorType type = FunctionOperatorType.Cos;
            CriteriaOperator count = parameters[2];
            switch(dateComponent) {
                case "m":
                    type = FunctionOperatorType.AddMonths; break;
                case "d":
                case "y":
                case "w":
                    type = FunctionOperatorType.AddDays; break;
                case "yyyy":
                    type = FunctionOperatorType.AddYears; break;
                //case "q":
                //    type = FunctionOperatorType.AddQuarter; break;
                case "ww":
                    type = FunctionOperatorType.AddDays;
                    count = count * 7;
                    break;
                case "h":
                    type = FunctionOperatorType.AddHours; break;
                case "n":
                    type = FunctionOperatorType.AddMinutes; break;
                case "s":
                    type = FunctionOperatorType.AddSeconds; break;
            }
            Assert(type != FunctionOperatorType.Cos);
            return new FunctionOperator(type, count, parameters[1]);
        }
        CriteriaOperator CreateAggregate(IList<CriteriaOperator> parameters, Aggregate aggregate) {
            Assert(parameters.Count == 2 || parameters.Count == 1);

            CriteriaOperator condition = null;
            if(parameters.Count == 2) {
                Assert(parameters[1] is OperandProperty);
                var prop = parameters[1] as OperandProperty;
                condition = new BinaryOperator(prop, new OperandProperty("^." + prop.PropertyName), BinaryOperatorType.Equal);
            }
            CriteriaOperator aggregatedExpression = aggregate == Aggregate.Count
                ? null
                : parameters[0];
            return new AggregateOperand(new OperandProperty(""), aggregatedExpression, aggregate, condition);
        }
        CriteriaOperator GetPercentExpression(CriteriaOperator leftOperand, CriteriaOperator rightOperand) {
            return new ConstantValue(100.0) * leftOperand / rightOperand;
        }
        CriteriaOperator GetPowerExpression(CriteriaOperator leftOperand, CriteriaOperator rightOperand) {
            return new FunctionOperator(FunctionOperatorType.Power, leftOperand, rightOperand);
        }
        CriteriaOperator GetIifFormula(CriteriaOperator condition, CriteriaOperator thenOperand, CriteriaOperator elseOperand) {
            FunctionOperator tail = elseOperand as FunctionOperator;
            List<CriteriaOperator> operands = new List<CriteriaOperator>() {
                condition,
                thenOperand
            };
            if(!ReferenceEquals(tail, null) && tail.OperatorType == FunctionOperatorType.Iif) {
                operands.AddRange(tail.Operands);
            } else
                operands.Add(elseOperand ?? new ConstantValue(null));
            return new FunctionOperator(FunctionOperatorType.Iif, operands.ToArray());
        }
        void Assert(bool condition) {
            if(!condition)
                throw new FormulaParserException("syntax error");
        }
        CriteriaOperator GetSelectExpression(CriteriaOperator selectExpression, List<Tuple<List<CriteriaOperator>, CriteriaOperator>> caseList, CriteriaOperator defaultExpression) {
            List<CriteriaOperator> operands = new List<CriteriaOperator>();
            foreach(var caseCondition in caseList) {
                var conditions = caseCondition.Item1.Select(x => new BinaryOperator(selectExpression, x, BinaryOperatorType.Equal)).ToArray();
                CriteriaOperator condition = conditions.Length == 1 ? (CriteriaOperator)conditions[0] : new GroupOperator(GroupOperatorType.Or, conditions);
                operands.Add(condition);
                operands.Add(caseCondition.Item2);
            }
            operands.Add(defaultExpression);
            return new FunctionOperator(FunctionOperatorType.Iif, operands.ToArray());
        }
    }
}

