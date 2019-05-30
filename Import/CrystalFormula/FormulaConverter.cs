using System;
using System.Collections.Generic;
using DevExpress.Data.Filtering;
using DevExpress.XtraReports.Parameters;
using DevExpress.XtraReports.UI;

namespace DevExpress.XtraReports.Design.Import.CrystalFormula {
    class FormulaConverter : DevExpress.Data.Filtering.Helpers.ClientCriteriaVisitorBase {
        readonly CalculatedField currentCalculatedField;
        readonly List<OperandProperty> formulae;
        readonly Func<string, CalculatedField> getCalculatedField;
        readonly Dictionary<string, string> map;
        readonly string dataMember;
        readonly Dictionary<string, Parameter> parametersByOriginalNames;

        public FormulaConverter(CalculatedField currentCalculatedField, List<OperandProperty> formulae, Func<string, CalculatedField> getCalculatedField, Dictionary<string, string> map, Dictionary<string, Parameter> parametersByOriginalNames) {
            this.currentCalculatedField = currentCalculatedField;
            this.formulae = formulae;
            this.getCalculatedField = getCalculatedField;
            this.map = map;
            if(!string.IsNullOrEmpty(currentCalculatedField.DataMember))
                dataMember = currentCalculatedField.DataMember + ".";
            this.parametersByOriginalNames = parametersByOriginalNames;
        }
        protected override CriteriaOperator Visit(OperandProperty theOperand) {
            if(formulae.Contains(theOperand)) {
                CalculatedField calculatedField = getCalculatedField(theOperand.PropertyName);
                System.Diagnostics.Debug.Assert(calculatedField != null, string.Format(Messages.Warning_CalculatedField_FormulaNotFound_Format, theOperand.PropertyName, currentCalculatedField.Name));
                System.Diagnostics.Debug.Assert(calculatedField.DataMember.StartsWith(currentCalculatedField.DataMember), $"CalculatedField '{currentCalculatedField.Name}' can not use the '{calculatedField.Name}' with '{calculatedField.DataMember}' DataMember.");
                return new OperandProperty(calculatedField?.Name ?? theOperand.PropertyName);
            }
            string mapPropertyName;
            if(map.TryGetValue(theOperand.PropertyName, out mapPropertyName))
                return new OperandProperty(CleanDataMember(mapPropertyName));
            return base.Visit(theOperand);
        }
        protected override CriteriaOperator Visit(OperandValue theOperand) {
            var operandParameter = theOperand as OperandParameter;
            if(!ReferenceEquals(operandParameter, null)) {
                Parameter parameter;
                if(parametersByOriginalNames.TryGetValue(operandParameter.ParameterName, out parameter))
                    return new OperandParameter(parameter.Name);
                else
                    throw new InvalidOperationException(string.Format(Messages.Warning_CalculatedField_ParameterNotFound_Format, currentCalculatedField.Name, operandParameter.ParameterName));
            }
            return base.Visit(theOperand);
        }
        string CleanDataMember(string propertyName) {
            if(string.IsNullOrEmpty(dataMember))
                return propertyName;
            if(propertyName.StartsWith(dataMember, StringComparison.Ordinal))
                return propertyName.Substring(dataMember.Length);
            return propertyName;
        }
    }
}
