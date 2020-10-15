using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;

namespace DevExpress.XtraReports.Import.ReportingServices {
    class UnitConverter {
        class UnitDpiInfo {
            const float CmDpi = GraphicsDpi.Millimeter / 10;
            const float PicaDpi = GraphicsDpi.Inch * 6;

            public string Name { get; }
            public string ShortName { get; }
            public float Dpi { get; }
            public ReportUnit MappedUnit { get; }
            public float MappedDpi { get; }

            UnitDpiInfo(string name, string shortName, float dpi, ReportUnit mappedUnit, float mappedDpi) {
                Name = name;
                ShortName = shortName;
                Dpi = dpi;
                MappedUnit = mappedUnit;
                MappedDpi = mappedDpi;
            }

            public static UnitDpiInfo Create(string name) {
                if(name == "Cm")
                    return new UnitDpiInfo(name, "cm", CmDpi, ReportUnit.TenthsOfAMillimeter, GraphicsDpi.TenthsOfAMillimeter);
                else if(name == "Mm")
                    return new UnitDpiInfo(name, "mm", GraphicsDpi.Millimeter, ReportUnit.TenthsOfAMillimeter, GraphicsDpi.TenthsOfAMillimeter);
                else if(name == "Inch")
                    return new UnitDpiInfo(name, "in", GraphicsDpi.Inch, ReportUnit.HundredthsOfAnInch, GraphicsDpi.HundredthsOfAnInch);
                else if(name == "Point")
                    return new UnitDpiInfo(name, "pt", GraphicsDpi.Point, ReportUnit.HundredthsOfAnInch, GraphicsDpi.HundredthsOfAnInch);
                else if(name == "Pica")
                    return new UnitDpiInfo(name, "pc", PicaDpi, ReportUnit.HundredthsOfAnInch, GraphicsDpi.HundredthsOfAnInch);
                throw new NotSupportedException(string.Format(Messages.ReportMeasureUnit_NotSupported_Format, name));
            }
        }

        readonly static Dictionary<string, UnitDpiInfo> unitDpiInfos;
        readonly UnitDpiInfo targetUnitInfo;

        public ReportUnit ReportUnit => targetUnitInfo.MappedUnit;
        public float TargetDpi => targetUnitInfo.MappedDpi;

        static UnitConverter() {
            unitDpiInfos = new[] { "Cm", "Mm", "Inch", "Point", "Pica" }
                .ToDictionary(x => x, UnitDpiInfo.Create);
        }

        public UnitConverter(string reportUnit) {
            unitDpiInfos.TryGetValue(reportUnit, out targetUnitInfo);
            if(targetUnitInfo == null)
                throw new NotSupportedException(string.Format(Messages.ReportMeasureUnit_NotSupported_Format, reportUnit));
        }

        public float ToFloat(string value) {
            return ToFloat(value, TargetDpi);
        }

        public float ToFloat(string value, float toDpi) {
            if(string.IsNullOrEmpty(value))
                return 0f;
            value = value.Trim();
            var unitInfo = GetUnitInfo(value);
            if(value.EndsWith(unitInfo.ShortName))
                value = value.Substring(0, value.LastIndexOf(unitInfo.ShortName));
            var convertedValue = GraphicsUnitConverter.Convert(float.Parse(value, CultureInfo.InvariantCulture), unitInfo.Dpi, toDpi);
            return (float)Math.Round(convertedValue, 2);
        }

        public int ToInt(string value) {
            return (int)Math.Round(ToFloat(value));
        }

        public SizeF ToSizeF(string value) {
            var values = value.Split(',');
            return new SizeF(ToFloat(values[0]), ToFloat(values[0]));
        }

        public string CutUnits(string value) {
            value = value.Trim();
            var unitInfo = GetUnitInfo(value);
            if(value.EndsWith(unitInfo.ShortName)) {
                value = value.Substring(0, value.LastIndexOf(unitInfo.ShortName));
            }
            return value;
        }

        UnitDpiInfo GetUnitInfo(string value) {
            var shortUnit = value.Substring(value.Length - 2);
            return unitDpiInfos.Values.SingleOrDefault(x => x.ShortName == shortUnit) ?? targetUnitInfo;
        }
    }
}
