using System;
using DevExpress.XtraReports.UI;

namespace DevExpress.XtraReports.Import {
    public class CrystalConverterSubreportGeneratedEventArgs : EventArgs {
        public string OriginalSubreportName { get; }
        public XRSubreport SubreportControl { get; }
        public XtraReport SubReport { get; }
        public CrystalConverterSubreportGeneratedEventArgs(string originalSubreportName, XRSubreport subreportControl, XtraReport subReport) {
            OriginalSubreportName = originalSubreportName;
            SubreportControl = subreportControl;
            SubReport = subReport;
        }
    }
    public delegate void CrystalConverterSubreportGeneratedHandler(object sender, CrystalConverterSubreportGeneratedEventArgs e);
}
