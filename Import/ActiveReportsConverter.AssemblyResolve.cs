#region DEMO_REMOVE
#if Active
using System;
using System.Linq;
using System.Reflection;

namespace DevExpress.XtraReports.Import {
    partial class ActiveReportsConverter : IDisposable {
        const string activePrefix = "GrapeCity.ActiveReports.";
        string activeVersion;

        partial void BeforeConvert() {
            const string activePrefixVersion = activePrefix + "v";
            string activeReportsAssemblyName = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(x => x.GetName().Name)
                .FirstOrDefault(x => x.StartsWith(activePrefixVersion, StringComparison.Ordinal));
            if(activeReportsAssemblyName != null) {
                activeVersion = activeReportsAssemblyName.Substring(activePrefixVersion.Length);
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            }
        }

        static bool isInLoadingWithPartialName;
        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            if(isInLoadingWithPartialName)
                return null;
            if(activeVersion == null)
                return null;
            var assemblyName = new AssemblyName(args.Name);
            string name = assemblyName.Name;
            if(!name.StartsWith(activePrefix, StringComparison.Ordinal))
                return null;
            const string versionPostfix = ".v";
            int index = name.LastIndexOf(versionPostfix);
            if(index == -1)
                return null;
            string baseName = name.Substring(0, index + versionPostfix.Length);
            string fixedName = baseName + activeVersion;
            isInLoadingWithPartialName = true;
            try {
#pragma warning disable 618
                Assembly result = Assembly.LoadWithPartialName(fixedName);
#pragma warning restore 618
                return result;
            } finally {
                isInLoadingWithPartialName = false;
            }
        }

        public void Dispose() {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
        }
    }
}
#endif
#endregion
