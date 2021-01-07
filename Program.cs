using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DevExpress.XtraReports.Import {
    class Program {
        static void Main(string[] args) {
            try {
                Dictionary<string, string> argDictionary = CreateArgDictionary(args);
                string inputFile;
                string outputFile;
                if(!argDictionary.TryGetValue("/in", out inputFile) || !argDictionary.TryGetValue("/out", out outputFile)) {
                    WriteInfo();
                    return;
                }
                string path = Path.GetFullPath(inputFile);
                if(!File.Exists(path)) {
                    Console.WriteLine($"File '{path}' doesn't exist." + Environment.NewLine);
                    WriteInfo();
                    return;
                }
                ConfigureTracer();

                ConverterBase converter = CreateConverter(Path.GetExtension(path), argDictionary, outputFile);
                ConversionResult conversionResult = converter.Convert(path);
                conversionResult.TargetReport.SaveLayoutToXml(outputFile);
            } catch(Exception ex) {
                Console.WriteLine(ex.Message + Environment.NewLine);
                WriteInfo();
            }
        }
        static void WriteInfo() {
            string[] infos = new string[] {
                    "Imports report files of different types into an XtaReport class file.\r\n",
                    "Usage:",
                    "ReportsImport /in:path1 /out:path2\r\n",
                    "              path1 Specifies the input file's location and type.",
#if Access
                    "                    *.mdb or *.mde file matches MS Access reports.",
                    "              /access:ReportIndex=[Number]",
                    "              /access:ReportName=[String]",
#endif
#if Active
                    "                    *.rpx file matches ActiveReports.",
#endif
#if Crystal
                    "                    *.rpt file matches Crystal Reports.",
                    "              /crystal:UnrecognizedFunctionBehavior=Ignore",
#endif
                    "                    *.rdl or *.rdlc file matches MS SQL Server Reporting Services.",
                    "              /ssrs:UnrecognizedFunctionBehavior=Ignore",
                    "",
                    "              path2 Specifies the output file's location.\r\n",
                    @"For more information, see https://github.com/DevExpress/Reporting.Import"
                };
            foreach(string s in infos)
                Console.WriteLine(s);
        }
        static ConverterBase CreateConverter(string extension, Dictionary<string, string> argDictionary, string outputPath) {
#if Access
            if(extension == ".mdb" || extension == ".mde") {
                AccessReportSelectionForm.AccessIconResourceName = typeof(AccessConverter).Namespace + ".Import.AccessReport.bmp";
                Dictionary<string, string> accessProperties = CreateSubArg(argDictionary, "/access");
                string reportName;
                accessProperties.TryGetValue("ReportName", out reportName);
                string reportIndexStr;
                int? reportIndex = null;
                if(accessProperties.TryGetValue("ReportIndex", out reportIndexStr)) {
                    int reportIndexLocal;
                    if(int.TryParse(reportIndexStr, out reportIndexLocal))
                        reportIndex = reportIndexLocal;
                }
                return new AccessConverter(reportName, reportIndex);
            }
#endif
#if Active
            if(extension == ".rpx")
                return new ActiveReportsConverter();
#endif
#if Crystal
            if(extension == ".rpt") {
                Dictionary<string, string> crystalProperties = CreateSubArg(argDictionary, "/crystal");
                string unrecognizedFunctionBehavior;
                if(crystalProperties.TryGetValue("UnrecognizedFunctionBehavior", out unrecognizedFunctionBehavior)) {
                    CrystalConverter.UnrecognizedFunctionBehavior = string.Equals(unrecognizedFunctionBehavior, nameof(UnrecognizedFunctionBehavior.Ignore))
                        ? UnrecognizedFunctionBehavior.Ignore
                        : UnrecognizedFunctionBehavior.InsertWarning;
                }
                var crystalConverter = new CrystalConverter();
                crystalConverter.SubreportGenerated += (_, e) => Converter_SubreportGenerated(outputPath, e);
                return crystalConverter;
            }
#endif
            if(extension == ".rdl" || extension == ".rdlc") {
                Dictionary<string, string> ssrsProperties = CreateSubArg(argDictionary, "/ssrs");
                string unrecognizedFunctionBehavior;
                var reportingServicesConverter = new ReportingServicesConverter();
                if(ssrsProperties.TryGetValue("UnrecognizedFunctionBehavior", out unrecognizedFunctionBehavior)) {
                    reportingServicesConverter.UnrecognizedFunctionBehavior = string.Equals(unrecognizedFunctionBehavior, nameof(UnrecognizedFunctionBehavior.Ignore))
                        ? UnrecognizedFunctionBehavior.Ignore
                        : UnrecognizedFunctionBehavior.InsertWarning;
                } 
                if(ssrsProperties.TryGetValue("MultipleTextRunBehavior", out var multipleTextRunBehavior)) {
                    ssrsConverter.MultipleTextRunBehavior = string.Equals(multipleTextRunBehavior, nameof(MultipleTextRunBehavior.RichText))
                        ? MultipleTextRunBehavior.RichText
                        : MultipleTextRunBehavior.CombinedExpression;
                }
                string ignoreQueryValidation;
                if(ssrsProperties.TryGetValue("IgnoreQueryValidation", out ignoreQueryValidation))
                {
                    reportingServicesConverter.IgnoreQueryValidation = bool.Parse(ignoreQueryValidation);
                }
                return reportingServicesConverter;
            }
            throw new ArgumentException($"File extension '{extension}' is not supported.");
        }

        static Dictionary<string, string> CreateArgDictionary(string[] args) {
            if(args.Length < 2)
                throw new ArgumentException("Expected two or more aguments.");
            Dictionary<string, string> argDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach(string arg in args) {
                string[] items = arg.Split(new char[] { ':' }, 2);
                if(items.Length < 2)
                    throw new ArgumentException("A value should be specified after the colon.");
                argDictionary.Add(items[0], items[1]);
            }
            return argDictionary;
        }
        static Dictionary<string, string> CreateSubArg(Dictionary<string, string> argDictionary, string key) {
            string subArgumentsString;
            if(!argDictionary.TryGetValue(key, out subArgumentsString))
                return new Dictionary<string, string>();
            Dictionary<string, string> subArgDictionary = subArgumentsString
                .Split(';')
                .Select(x => x.Split(new[] { '=' }, 2))
                .ToDictionary(x => x[0], x => x.Length == 2 ? x[1] : null, StringComparer.OrdinalIgnoreCase);
            return subArgDictionary;
        }
        static void ConfigureTracer() {
            var traceSource = XtraPrinting.Tracer.GetSource("DXperience.Reporting", System.Diagnostics.SourceLevels.Error | System.Diagnostics.SourceLevels.Warning);
            var listener = new System.Diagnostics.ConsoleTraceListener();
            traceSource.Listeners.Add(listener);
        }
        static void Converter_SubreportGenerated(string outputFile, CrystalConverterSubreportGeneratedEventArgs e) {
            var subreportFile = Path.Combine(
                Path.GetDirectoryName(outputFile),
                Path.GetFileNameWithoutExtension(outputFile) + "_" + EscapeFileName(e.OriginalSubreportName) + Path.GetExtension(outputFile));
            e.SubReport.SaveLayoutToXml(subreportFile);
            e.SubreportControl.ReportSourceUrl = subreportFile;
        }
        static string EscapeFileName(string originalSubreportName) {
            foreach(char invalidChar in Path.GetInvalidFileNameChars())
                originalSubreportName = originalSubreportName.Replace(invalidChar, '_');
            return originalSubreportName;
        }
    }
}
