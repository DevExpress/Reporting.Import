using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using DevExpress.Data.Filtering;

namespace DevExpress.XtraReports.Import.ReportingServices.Tablix {
    class Model {
        public static Model Parse(XElement element, UnitConverter unitConverter, IReportingServicesConverter converter) {
            XNamespace ns = element.GetDefaultNamespace();
            string name = element.Attribute("Name").Value;
            var bounds = new RectangleF(
                unitConverter.ToFloat(element.Element(ns + "Left")?.Value),
                unitConverter.ToFloat(element.Element(ns + "Top")?.Value),
                unitConverter.ToFloat(element.Element(ns + "Width")?.Value),
                unitConverter.ToFloat(element.Element(ns + "Height")?.Value));
            XElement body = element.Element(ns + "TablixBody");
            List<float> columns = body
                .Element(ns + "TablixColumns")
                .Elements(ns + "TablixColumn")
                .Select(x => unitConverter.ToFloat(x.Element(ns + "Width").Value))
                .ToList();
            List<RowModel> rows = body
                .Element(ns + "TablixRows")
                .Elements(ns + "TablixRow")
                .Select(x => RowModel.Parse(x, unitConverter))
                .ToList();
            Hierarchy rowHierarchy = Hierarchy.Parse(element.Element(ns + "TablixRowHierarchy"), name, converter);
            Hierarchy columnHierarchy = Hierarchy.Parse(element.Element(ns + "TablixColumnHierarchy"), name, converter);
            string dataSetName = element.Element(ns + "DataSetName")?.Value;
            CriteriaOperator filter = Filter.ParseFilters(element.Element(ns + "Filters"), name, converter);
            List<SortExpressionMember> sortExpressions = SortExpressionMember.Parse(element.Element(ns + "SortExpressions"), name, converter);
            return new Model(name, bounds, element, columns, rows, rowHierarchy, columnHierarchy, dataSetName, filter, sortExpressions);
        }
        public string Name { get; }
        public RectangleF Bounds { get; }
        public XElement Element { get; }
        public ReadOnlyCollection<float> Columns { get; }
        public ReadOnlyCollection<RowModel> Rows { get; }
        public Hierarchy RowHierarchy { get; }
        public Hierarchy ColumnHierarchy { get; }
        public string DataSetName { get; }
        public CriteriaOperator FilterExpression { get; }
        public List<SortExpressionMember> SortExpressions { get; }
        public Model(string name, RectangleF bounds, XElement element, IList<float> columns, IList<RowModel> rows, Hierarchy rowHierarchy, Hierarchy columnHierarchy, string dataSetName, CriteriaOperator filterCriteria, List<SortExpressionMember> sortExpressions) {
            Name = name;
            Bounds = bounds;
            Element = element;
            Columns = new ReadOnlyCollection<float>(columns);
            Rows = new ReadOnlyCollection<RowModel>(rows);
            RowHierarchy = rowHierarchy;
            ColumnHierarchy = columnHierarchy;
            DataSetName = dataSetName;
            FilterExpression = filterCriteria;
            SortExpressions = sortExpressions;
        }
    }
}
