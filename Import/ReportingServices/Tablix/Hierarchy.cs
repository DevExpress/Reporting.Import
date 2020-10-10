using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace DevExpress.XtraReports.Import.ReportingServices.Tablix {
    class Hierarchy {
        public static Hierarchy Parse(XElement element, string componentName, IReportingServicesConverter converter) {
            List<TablixMember> members = TablixMember.ParseContainer(element, componentName, converter);
            return new Hierarchy(members);
        }
        public ReadOnlyCollection<TablixMember> Members { get; }
        public Hierarchy(IList<TablixMember> members) {
            Members = new ReadOnlyCollection<TablixMember>(members);
        }
        public bool HasAnyGroup() {
            return Members
                .SelectMany(x => x.Flatten(false))
                .Any(x => x.HasGroupRecursive());
        }
        public bool HasSingleGroup() {
            return Members
                .SelectMany(x => x.Flatten(false))
                .Count(x => x.HasGroup()) == 1;
        }
    }
}
