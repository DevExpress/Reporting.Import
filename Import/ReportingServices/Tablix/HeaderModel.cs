using System.Xml.Linq;

namespace DevExpress.XtraReports.Import.ReportingServices.Tablix {
    class HeaderModel {
        public static HeaderModel Parse(XElement element, UnitConverter unitConverter) {
            if(element == null)
                return null;
            XNamespace ns = element.GetDefaultNamespace();
            float size = unitConverter.ToFloat(element.Element(ns + "Size").Value);
            XElement cell = element.Element(ns + "CellContents");
            return new HeaderModel(size, cell);
        }

        public float Size { get; }
        public XElement Cell { get; }
        public HeaderModel(float size, XElement cell) {
            Size = size;
            Cell = cell;
        }
    }
}
