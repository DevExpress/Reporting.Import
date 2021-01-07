using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Security;
using DevExpress.Utils;
using DevExpress.XtraReports.Serialization;
using DevExpress.XtraReports.UI;

namespace DevExpress.XtraReports.Import {
    public abstract class ExternalConverterBase : ConverterBase {
        protected class ComponentNamingMapper {
            readonly HashSet<string> xrnames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            readonly object rootComponent;

            public ComponentNamingMapper(object rootComponent) {
                this.rootComponent = rootComponent;
            }

            public T GenerateAndAssignXRControlName<T>(T component, string originalName = null) {
                string newName = GenerateSafeNameCore(typeof(T), originalName, rootComponent, xrnames.Contains);
                SetName(component, newName);
                xrnames.Add(newName);
                return component;
            }

            public string GenerateSafeName<T>(string originalName = null, Predicate<string> isNameExists = null) {
                return GenerateSafeNameCore(typeof(T), originalName, rootComponent, isNameExists);
            }

            public string GenerateSafeName(Type type, string originalName = null, Predicate<string> isNameExists = null) {
                return GenerateSafeNameCore(type, originalName, rootComponent, isNameExists);
            }

            static string GenerateSafeNameCore(Type type, string originalName = null, object rootComponent = null, Predicate<string> isNameExists = null) {
                bool hasOriginalName = !string.IsNullOrEmpty(originalName);
                if(hasOriginalName) {
                    originalName = originalName
                        .Replace(' ', '_')
                        .Replace(',', '_')
                        .Replace('/', '_')
                        .Replace(';', '_')
                        .Replace('<', '_')
                        .Replace('>', '_');
                    for(int i = 0; i < 0xffff; i++) {
                        string suffix = i == 0 ? "" : "_" + i.ToString();
                        string newName = originalName + suffix;
                        if(IsValidName(newName, rootComponent) && (isNameExists == null || !isNameExists(newName)))
                            return newName;
                    }
                }
                string baseName = XRNameCreationService.GetDefaultBaseName(type);
                int number = 1;
                while(isNameExists != null && isNameExists(baseName + number.ToString()))
                    number++;
                return baseName + number;
            }

            static bool IsValidName(string name, object rootComponent = null) {
                var isInvalid = string.IsNullOrEmpty(name)
                    || XRNameCreationService.HasWrongCharacters(name)
                    || (rootComponent != null && XRNameCreationService.RootComponentHasMember(rootComponent, name));
                return !isInvalid;
            }

            static void SetName(object component, string name) {
                PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(component)["Name"];
                propertyDescriptor.SetValue(component, name);
            }
        }

        sealed class AssemblyResolver : IDisposable {
            readonly string assemblyPrefix;
            public AssemblyResolver(string assemblyPrefix) {
                Guard.ArgumentNotNull(assemblyPrefix, "assemblyPrefix");
                this.assemblyPrefix = assemblyPrefix;
                AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolveEventHandler;
            }

            public void Dispose() {
                AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolveEventHandler;
            }

            bool inAssemblyResolve;
            Assembly AssemblyResolveEventHandler(object sender, ResolveEventArgs args) {
                if(inAssemblyResolve)
                    return null;
                if(!args.Name.StartsWith(assemblyPrefix, StringComparison.Ordinal)) {
                    return null;
                }
                inAssemblyResolve = true;
                Assembly asm = null;
                try {
                    for(int i = 0; i < 10; i++) {
                        try {
#pragma warning disable 618
                            asm = Assembly.LoadWithPartialName(XRSerializer.GetShortAssemblyName(args.Name));
#pragma warning restore 618
                            break;
                        } catch {
                        }
                    }
                } finally {
                    inAssemblyResolve = false;
                }
                return asm;
            }
        }

        [SecuritySafeCritical]
        protected static IDisposable SubscribeAssemblyResolveEventStatic(string assemblyPrefix) {
            return new AssemblyResolver(assemblyPrefix);
        }
        [SecuritySafeCritical]
        protected virtual IDisposable SubscribeAssemblyResolveEvent(string assemblyPrefix) {
            return SubscribeAssemblyResolveEventStatic(assemblyPrefix);
        }

        protected ComponentNamingMapper NamingMapper { get; private set; }

        protected ExternalConverterBase() {
        }

        protected sealed override void BeforeConvertInternal() {
            base.BeforeConvertInternal();
            NamingMapper = new ComponentNamingMapper(TargetReport);
        }

        protected virtual void BindDataToControl(XRControl control, string property, string dataMember, string formatString = "") {
            try {
                if(!string.IsNullOrEmpty(dataMember))
                    control.DataBindings.Add(property, TargetReport.DataSource, dataMember, formatString ?? string.Empty);
            } catch(Exception e) {
                XtraPrinting.Tracer.TraceWarning(XtraPrinting.Native.NativeSR.TraceSource, e);
            }
        }

        protected void SetParentStyleUsing(XRControl tgt, bool val) {
            var parentStyleUsing = tgt.ParentStyleUsing;
            parentStyleUsing.UseBackColor = val;
            parentStyleUsing.UseBorderColor = val;
            parentStyleUsing.UseBorders = val;
            parentStyleUsing.UseBorderWidth = val;
            parentStyleUsing.UseFont = val;
            parentStyleUsing.UseForeColor = val;
        }

        protected T CreateXRControl<T>(Band parentBand, string name, Action<T> configureAction = null)
            where T : XRControl, new() {
            Guard.ArgumentNotNull(parentBand, "parentBand");
            T result = (T)CreateXRControl(typeof(T));
            NamingMapper.GenerateAndAssignXRControlName(result, name);
            result.Dpi = parentBand.Dpi;
            if(configureAction != null) {
                configureAction(result);
            }
            var subBand = result as SubBand;
            if(subBand != null) {
                parentBand.SubBands.Add(subBand);
            } else {
                var crossBandControl = result as XRCrossBandControl;
                if(crossBandControl != null) {
                    parentBand.RootReport.CrossBandControls.Add(crossBandControl);
                    crossBandControl.StartBand = parentBand;
                } else {
                    parentBand.Controls.Add(result);
                }
            }
            return result;
        }
    }
    public enum UnrecognizedFunctionBehavior {
        InsertWarning,
        Ignore
    }
    public enum MultipleTextRunBehavior
    {
        RichText,
        CombinedExpression
    }
}
