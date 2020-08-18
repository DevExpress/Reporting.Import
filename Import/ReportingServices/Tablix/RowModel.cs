using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace DevExpress.XtraReports.Import.ReportingServices.Tablix {
    class RowModel {
        public static RowModel Parse(XElement tablixRowElement, UnitConverter unitConverter) {
            XNamespace ns = tablixRowElement.GetDefaultNamespace();
            float height = unitConverter.ToFloat(tablixRowElement.Element(ns + "Height").Value);
            List<XElement> cells = tablixRowElement
                .Element(ns + "TablixCells")
                .Elements(ns + "TablixCell")
                .Select(x => x.Element(ns + "CellContents"))
                .ToList();
            return new RowModel(height, cells);
        }
        public float Height { get; }
        public List<XElement> Cells { get; }
        public RowModel(float height, List<XElement> cells) {
            Height = height;
            Cells = cells;
        }
    }
}
