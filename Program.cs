using System;
using System.Collections.Generic;
using System.IO;

namespace DevExpress.XtraReports.Import {
    internal class ResFinder {
    }
}
namespace DevExpress.XtraReports.Import {
    class Program {
        static void Main(string[] args) {
            try {
                Dictionary<string, string> argDictionary = CreateArgDictionary(args);
                string inputFile;
                if(!argDictionary.TryGetValue("/in", out inputFile))
                    throw new ArgumentException();
                string outputFile;
                if(!argDictionary.TryGetValue("/out", out outputFile))
                    throw new ArgumentException();

                string path = Path.GetFullPath(inputFile);
                if(!File.Exists(path))
                    throw new Exception("File \"" + path + "\" doesn't exist.");
                ConfigureTracer();

                ConverterBase converter = CreateConverter(Path.GetExtension(path));
                ConversionResult conversionResult = converter.Convert(path);
                conversionResult.TargetReport.SaveLayoutToXml(outputFile);
            } catch(Exception ex) {
                if(ex is ArgumentException) {
                    WriteInfo();
                } else {
                    Console.WriteLine(ex.Message);
                }
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
#endif
#if Active
                    "                    *.rpx file matches ActiveReports.",
#endif
#if Crystal
                    "                    *.rpt file matches Crystal Reports.",
#endif
                    "",
                    "              path2 Specifies the output file's location.\r\n",
                    @"For more information, see https://github.com/DevExpress/Reporting.Import"
                };
            foreach(string s in infos)
                Console.WriteLine(s);
        }
        static ConverterBase CreateConverter(string extension) {
#if Access
            if(extension == ".mdb" || extension == ".mde")
                return new AccessConverter();
#endif
#if Active
            if(extension == ".rpx")
                return new ActiveReportsConverter();
#endif
#if Crystal
            if(extension == ".rpt")
                return new CrystalConverter();
#endif
            throw new ArgumentException();
        }
        static Dictionary<string, string> CreateArgDictionary(string[] args) {
            if(args.Length != 2)
                throw new ArgumentException();
            Dictionary<string, string> argDictionary = new Dictionary<string, string>();
            foreach(string arg in args) {
                string[] items = arg.Split(new char[] { ':' }, 2);
                if(items.Length < 2)
                    throw new ArgumentException();
                argDictionary.Add(items[0], items[1]);
            }
            return argDictionary;
        }

        static void ConfigureTracer() {
            var traceSource = XtraPrinting.Tracer.GetSource("DXperience.Reporting", System.Diagnostics.SourceLevels.Error | System.Diagnostics.SourceLevels.Warning);
            var listener = new System.Diagnostics.ConsoleTraceListener();
            traceSource.Listeners.Add(listener);
        }
    }
}
