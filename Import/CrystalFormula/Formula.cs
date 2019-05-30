using DevExpress.Data.Filtering;
using System.Collections.Generic;

namespace DevExpress.XtraReports.Design.Import.CrystalFormula {
    public class Formula {
        public CriteriaOperator Statement { get; set; }
        public FormulaCalulationDirective Directive { get; set; }
        public List<OperandParameter> Parameters { get; set; }
        public List<OperandProperty> Formulae { get; set; }
    }
}
