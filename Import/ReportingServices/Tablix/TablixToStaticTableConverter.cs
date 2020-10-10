using DevExpress.XtraReports.UI;

namespace DevExpress.XtraReports.Import.ReportingServices.Tablix {
    class TablixToStaticTableConverter {
        readonly IReportingServicesConverter rootConverter;
        readonly ITableConverter tableConverter;
        public TablixToStaticTableConverter(IReportingServicesConverter rootConverter, ITableConverter tableConverter) {
            this.rootConverter = rootConverter;
            this.tableConverter = tableConverter;
        }
        public void ConvertStaticTable(Model model, XRControl container, float yBodyOffset) {
            var table = new XRTable {
                BoundsF = model.Bounds,
                Dpi = container.Dpi
            };
            rootConverter.SetComponentName(table, model.Element);
            table.BeginInit();
            try {
                ReportingServicesConverter.IterateElements(
                    model.Element,
                    (x, _) => rootConverter.ProcessCommonControlProperties(x, table, yBodyOffset, false));
                for(int i = 0; i < model.Rows.Count; i++) {
                    RowModel modelRow = model.Rows[i];
                    XRTableRow xrRow = new XRTableRow();
                    table.Rows.Add(xrRow);
                    tableConverter.ConvertTableRow(modelRow, model.Columns, xrRow);
                }
            } finally {
                table.EndInit();
            }
            container.Controls.Add(table);
        }
    }
}
