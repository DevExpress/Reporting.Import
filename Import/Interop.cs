#region DEMO_REMOVE

#if Access
using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DevExpress.XtraReports.Import.Interop.Access;

namespace DevExpress.XtraReports.Import {
    public abstract class AccessReportBase {
        public static AccessReportBase CreateInstance(object report) {
            if(report is Report)
                return new AccessReport((Report)report);
            if(report is _Report)
                return new _AccessReport((_Report)report);
            return null;
        }
        protected AccessReportBase() {
        }
        protected abstract object SourceReport { get; }
        protected abstract string Picture { get; }
        protected abstract object PictureData { get; }
        protected abstract string RecordSource { get; }
        protected abstract object PrtMip { get; }
        public abstract int LogicalPageWidth { get; }
        protected abstract byte PictureSizeMode { get; }
        protected abstract byte PictureType { get; }
        protected abstract _Section get_Section(object Index);
        protected abstract GroupLevel get_GroupLevel(int Index);


        public GroupLevel GetGroupLevel(int index) {
            try {
                return get_GroupLevel(index);
                //return SourceReport[index];
            } catch {
                try {
                    return (GroupLevel)GetProperty("GroupLevel", new object[] { index });
                } catch {
                    return null;
                }
            }
        }
        public _Section GetSection(object index) {
            try {
                return get_Section(index);
            } catch {
                try {
                    return (_Section)GetProperty("Section", new object[] { index });
                } catch {
                    return null;
                }
            }
        }
        public byte GetReportPictureType() {
            try {
                return PictureType;
            } catch {
                return (byte)GetProperty("PictureType");
            }
        }
        public object GetReportPictureData() {
            try {
                return PictureData;
            } catch {
                return GetProperty("PictureData");
            }
        }
        public string GetReportPicture() {
            try {
                return Picture;
            } catch {
                return (string)GetProperty("Picture");
            }
        }
        public string GetRecordSource() {
            try {
                return RecordSource;
            } catch {
                return (string)GetProperty("RecordSource");
            }
        }
        public byte GetReportPictureSizeMode() {
            try {
                return PictureSizeMode;
            } catch {
                return (byte)GetProperty("PictureSizeMode");
            }
        }
        public object GetPrintingSettings() {
            try {
                return PrtMip;
            } catch {
                return GetProperty("PrtMip");
            }
        }
        object GetProperty(string name) {
            return GetProperty(name, new object[] { });
        }
        object GetProperty(string name, object[] args) {
            return SourceReport.GetType().InvokeMember(name, BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public, null, SourceReport, args);
        }
    }
    public class AccessReport : AccessReportBase {
        Report report;
        public AccessReport(Report report) {
            this.report = report;
        }
        protected override object SourceReport { get { return report; } }
        protected override string Picture { get { return report.Picture; } }
        protected override object PictureData { get { return report.PictureData; } }
        protected override string RecordSource { get { return report.RecordSource; } }
        protected override object PrtMip { get { return report.PrtMip; } }
        public override int LogicalPageWidth { get { return report.LogicalPageWidth; } }
        protected override byte PictureSizeMode { get { return report.PictureSizeMode; } }
        protected override byte PictureType { get { return report.PictureType; } }
        protected override _Section get_Section(object Index) {
            return report.get_Section(Index);
        }
        protected override GroupLevel get_GroupLevel(int Index) {
            return report.get_GroupLevel(Index);
        }
    }
    public class _AccessReport : AccessReportBase {
        _Report report;
        public _AccessReport(_Report report) {
            this.report = report;
        }
        protected override object SourceReport { get { return report; } }
        protected override string Picture { get { return report.Picture; } }
        protected override object PictureData { get { return report.PictureData; } }
        protected override string RecordSource { get { return report.RecordSource; } }
        protected override object PrtMip { get { return report.PrtMip; } }
        public override int LogicalPageWidth { get { return report.LogicalPageWidth; } }
        protected override byte PictureSizeMode { get { return report.PictureSizeMode; } }
        protected override byte PictureType { get { return report.PictureType; } }
        protected override _Section get_Section(object Index) {
            return report.get_Section(Index);
        }
        protected override GroupLevel get_GroupLevel(int Index) {
            return report.get_GroupLevel(Index);
        }
    }
}

namespace DevExpress.XtraReports.Import.Interop.Access {
    public enum AcView {
        acViewNormal = 0,
        acViewDesign = 1,
        acViewPreview = 2,
        acViewPivotTable = 3,
        acViewPivotChart = 4
    }
    public enum AcWindowMode {
        acWindowNormal = 0,
        acHidden = 1,
        acIcon = 2,
        acDialog = 3
    }
    public enum AcQuitOption {
        acQuitPrompt = 0,
        acQuitSaveAll = 1,
        acQuitSaveNone = 2
    }
    public enum AcFormView {
        acNormal = 0,
        acDesign = 1,
        acPreview = 2,
        acFormDS = 3,
        acFormPivotTable = 4,
        acFormPivotChart = 5
    }
    public enum AcObjectType {
        // Fields
        acDataAccessPage = 6,
        acDefault = -1,
        acDiagram = 8,
        acForm = 2,
        acFunction = 10,
        acMacro = 4,
        acModule = 5,
        acQuery = 1,
        acReport = 3,
        acServerView = 7,
        acStoredProcedure = 9,
        acTable = 0
    }
    public enum AcCloseSave {
        acSavePrompt = 0,
        acSaveYes = 1,
        acSaveNo = 2
    }
    public enum AcFormOpenDataMode {
        // Fields
        acFormAdd = 0,
        acFormEdit = 1,
        acFormPropertySettings = -1,
        acFormReadOnly = 2
    }
    public enum AcControlType {
        // Fields
        acBoundObjectFrame = 0x6c,
        acCheckBox = 0x6a,
        acComboBox = 0x6f,
        acCommandButton = 0x68,
        acCustomControl = 0x77,
        acImage = 0x67,
        acLabel = 100,
        acLine = 0x66,
        acListBox = 110,
        acObjectFrame = 0x72,
        acOptionButton = 0x69,
        acOptionGroup = 0x6b,
        acPage = 0x7c,
        acPageBreak = 0x76,
        acRectangle = 0x65,
        acSubform = 0x70,
        acTabCtl = 0x7b,
        acTextBox = 0x6d,
        acToggleButton = 0x7a
    }
    public enum AcSection {
        acDetail = 0,
        acHeader = 1,
        acFooter = 2,
        acPageHeader = 3,
        acPageFooter = 4,
        acGroupLevel1Header = 5,
        acGroupLevel1Footer = 6,
        acGroupLevel2Header = 7,
        acGroupLevel2Footer = 8
    }

    [ComImport, Guid("73A4C9C1-D68D-11D0-98BF-00A0C90DC8D9")]
    public class ApplicationClass {
    }
    [ComImport, Guid("68CCE6C0-6129-101B-AF4E-00AA003F0F07"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface _Application {
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x7ef)]
        _Control CreateControl([In, MarshalAs(UnmanagedType.BStr)] string FormName, [In] AcControlType ControlType, [In, Optional] AcSection Section /* = 0 */, [In, Optional, MarshalAs(UnmanagedType.Struct)] object Parent, [In, Optional, MarshalAs(UnmanagedType.Struct)] object ColumnName, [In, Optional, MarshalAs(UnmanagedType.Struct)] object Left, [In, Optional, MarshalAs(UnmanagedType.Struct)] object Top, [In, Optional, MarshalAs(UnmanagedType.Struct)] object Width, [In, Optional, MarshalAs(UnmanagedType.Struct)] object Height);

        [DispId(0x7d3)]
        Reports Reports { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x7d3)] get; }

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x7e0)]
        void Quit([In, Optional] AcQuitOption Option /* = 1 */);

        [DispId(0x7e9)]
        DoCmd DoCmd { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x7e9)] get; }

        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x7ed)]
        Form CreateForm([In, Optional, MarshalAs(UnmanagedType.Struct)] object Database, [In, Optional, MarshalAs(UnmanagedType.Struct)] object FormTemplate);

        //[return: MarshalAs(UnmanagedType.Interface)]
        //[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x801)]
        //DaoInterop.Database CurrentDb();

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x85d)]
        void CloseCurrentDatabase();

        [DispId(0x864)]
        bool Visible { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x864)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x864)] set; }

        [DispId(0x8a7)]
        _CurrentProject CurrentProject { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x8a7)] get; }

        [DispId(0x8a8)]
        _CurrentData CurrentData { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x8a8)] get; }
    }
    [ComImport, Guid("9212BA73-3E79-11D1-98BD-006008197D41"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface _CurrentData {
        [DispId(0x8b1)]
        AllObjects AllTables { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x8b1)] get; }
        [DispId(0x8b2)]
        AllObjects AllQueries { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x8b2)] get; }
    }
    [ComImport, Guid("9212BA71-3E79-11D1-98BD-006008197D41"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface _CurrentProject {
        [DispId(0x8ac)]
        AllObjects AllReports { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x8ac)] get; }
    }
    [ComImport, Guid("DDBD4001-44D5-11D1-98C0-006008197D41"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface AllObjects {
        [DispId(0)]
        AccessObject this[object var] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0)] get; }

        [DispId(0x7d1)]
        int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x7d1)] get; }
    }
    [ComImport, Guid("ABE316B1-3FF6-11D1-98BD-006008197D41"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface AccessObject {
        [DispId(-2147418112)]
        string Name { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418112)] get; }
    }
    [ComImport, Guid("C547E760-9658-101B-81EE-00AA004750E2"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface DoCmd {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3ed)]
        void Close([In, Optional] AcObjectType ObjectType /* = -1 */, [In, Optional, MarshalAs(UnmanagedType.Struct)] object ObjectName, [In, Optional] AcCloseSave Save /* = 0 */);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3ef)]
        void DoMenuItem([In, MarshalAs(UnmanagedType.Struct)] object MenuBar, [In, MarshalAs(UnmanagedType.Struct)] object MenuName, [In, MarshalAs(UnmanagedType.Struct)] object Command, [In, Optional, MarshalAs(UnmanagedType.Struct)] object Subcommand, [In, Optional, MarshalAs(UnmanagedType.Struct)] object Version);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3fb)]
        void OpenForm([In, MarshalAs(UnmanagedType.Struct)] object FormName, [In, Optional] AcFormView View /* = 0 */, [In, Optional, MarshalAs(UnmanagedType.Struct)] object FilterName, [In, Optional, MarshalAs(UnmanagedType.Struct)] object WhereCondition, [In, Optional] AcFormOpenDataMode DataMode /* = -1 */, [In, Optional] AcWindowMode WindowMode /* = 0 */, [In, Optional, MarshalAs(UnmanagedType.Struct)] object OpenArgs);
    }
    [ComImport, Guid("D1523700-6128-101B-AF4E-00AA003F0F07"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface Reports {
        [DispId(0)]
        object this[object Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0)] get; }
    }
    [ComImport, Guid("3E8B6B00-91FF-101B-AF4E-00AA003F0F07"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface _Report {
        [DispId(7)]
        string Picture { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(7)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(7)] set; }

        [DispId(0xbc)]
        object PictureData { [return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)0x400), DispId(0xbc)] get; [param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0xbc), TypeLibFunc((short)0x400)] set; }

        [DispId(0x9c)]
        string RecordSource { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x9c)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x9c)] set; }

        [DispId(0xbd)]
        object PrtMip { [return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)0x400), DispId(0xbd)] get; [param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)0x400), DispId(0xbd)] set; }

        [DispId(0xc6)]
        int LogicalPageWidth { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)0x40), DispId(0xc6)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)0x40), DispId(0xc6)] set; }

        [DispId(0x138)]
        byte PictureSizeMode { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x138)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x138)] set; }

        [DispId(0x155)]
        byte PictureType { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x155)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x155)] set; }

        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x828)]
        _Section get_Section([In, MarshalAs(UnmanagedType.Struct)] object Index);

        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x832)]
        GroupLevel get_GroupLevel([In] int Index);
    }
    [ComImport, Guid("32A1C62A-D374-11D3-8D21-0050048383FB"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface Report {
        [DispId(7)]
        string Picture { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(7)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(7)] set; }

        [DispId(0xbc)]
        object PictureData { [return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)0x400), DispId(0xbc)] get; [param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0xbc), TypeLibFunc((short)0x400)] set; }

        [DispId(0x9c)]
        string RecordSource { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x9c)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x9c)] set; }

        [DispId(0xbd)]
        object PrtMip { [return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)0x400), DispId(0xbd)] get; [param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)0x400), DispId(0xbd)] set; }

        [DispId(0xc6)]
        int LogicalPageWidth { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)0x40), DispId(0xc6)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)0x40), DispId(0xc6)] set; }

        [DispId(0x138)]
        byte PictureSizeMode { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x138)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x138)] set; }

        [DispId(0x155)]
        byte PictureType { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x155)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x155)] set; }

        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x828)]
        _Section get_Section([In, MarshalAs(UnmanagedType.Struct)] object Index);

        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x832)]
        GroupLevel get_GroupLevel([In] int Index);
    }
    [ComImport, Guid("331FDCFC-CF31-11CD-8701-00AA003F0F07"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface _Section {
        [DispId(0x2c)]
        short Height { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x2c)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x2c)] set; }

        [DispId(0x88e)]
        Children Controls { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x88e)] get; }

        [DispId(0x94)]
        bool Visible { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x94)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x94)] set; }

        [DispId(-2147418112)]
        string Name { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418112)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418112)] set; }
    }
    [ComImport, Guid("3B06E977-E47C-11CD-8701-00AA003F0F07"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface Children : IEnumerable {
    }
    [ComImport, Guid("331FDD27-CF31-11CD-8701-00AA003F0F07"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface GroupLevel {
        [DispId(0x1b)]
        string ControlSource { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x1b)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x1b)] set; }

        [DispId(0xae)]
        bool SortOrder { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0xae)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0xae)] set; }
    }
    [ComImport, Guid("26B96540-8F8E-101B-AF4E-00AA003F0F07"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface Control {
    }
    [ComImport, Guid("3B06E946-E47C-11CD-8701-00AA003F0F07"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface TextBox {
        [DispId(8)]
        int BorderColor { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(8)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(8)] set; }

        [DispId(9)]
        byte BorderStyle { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(9)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(9)] set; }

        [DispId(10)]
        byte BorderWidth { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(10)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(10)] set; }

        [DispId(14)]
        bool CanGrow { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(14)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(14)] set; }

        [DispId(0x10)]
        bool CanShrink { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x10)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x10)] set; }

        [DispId(0x1b)]
        string ControlSource { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x1b)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x1b)] set; }

        [DispId(0x1c)]
        int BackColor { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x1c)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x1c)] set; }

        [DispId(0x1d)]
        byte BackStyle { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x1d)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x1d)] set; }

        [DispId(0x20)]
        short FontBold { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x20)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x20)] set; }

        [DispId(0x21)]
        bool FontItalic { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x21)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x21)] set; }

        [DispId(0x22)]
        string FontName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x22)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x22)] set; }

        [DispId(0x23)]
        short FontSize { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x23)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x23)] set; }

        [DispId(0x24)]
        bool FontUnderline { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x24)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x24)] set; }

        [DispId(0x26)]
        string Format { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x26)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x26)] set; }

        [DispId(0x2c)]
        short Height { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x2c)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x2c)] set; }

        [DispId(0x36)]
        short Left { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x36)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x36)] set; }

        [DispId(0x47)]
        byte DecimalPlaces { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x47)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x47)] set; }

        [DispId(0x88)]
        byte TextAlign { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x88)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x88)] set; }

        [DispId(0x8d)]
        short Top { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x8d)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x8d)] set; }

        [DispId(0x94)]
        bool Visible { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x94)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x94)] set; }

        [DispId(150)]
        short Width { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(150)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(150)] set; }

        [DispId(0x180)]
        short LeftMargin { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x180)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x180)] set; }

        [DispId(0x163)]
        bool Vertical { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x163)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x163)] set; }

        [DispId(0x181)]
        short TopMargin { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x181)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x181)] set; }

        [DispId(0x184)]
        short RightMargin { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x184)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x184)] set; }

        [DispId(0x185)]
        short BottomMargin { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x185)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x185)] set; }

        [DispId(0x885)]
        Hyperlink Hyperlink { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x885)] get; }

        [DispId(-2147418112)]
        string Name { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418112)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418112)] set; }
    }
    [ComImport, Guid("3B06E960-E47C-11CD-8701-00AA003F0F07"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface PageBreak {
        [DispId(0x36)]
        short Left { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x36)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x36)] set; }

        [DispId(0x8d)]
        short Top { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x8d)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x8d)] set; }

        [DispId(0x94)]
        bool Visible { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x94)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x94)] set; }

        [DispId(-2147418112)]
        string Name { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418112)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418112)] set; }
    }
    [ComImport, Guid("3B06E948-E47C-11CD-8701-00AA003F0F07"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface Label {
        [DispId(8)]
        int BorderColor { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(8)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(8)] set; }

        [DispId(0x1c)]
        int BackColor { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x1c)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x1c)] set; }

        [DispId(9)]
        byte BorderStyle { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(9)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(9)] set; }

        [DispId(10)]
        byte BorderWidth { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(10)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(10)] set; }

        [DispId(0x11)]
        string Caption { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x11)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x11)] set; }

        [DispId(0x1d)]
        byte BackStyle { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x1d)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x1d)] set; }

        [DispId(0x20)]
        short FontBold { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x20)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x20)] set; }

        [DispId(0x21)]
        bool FontItalic { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x21)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x21)] set; }

        [DispId(0x22)]
        string FontName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x22)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x22)] set; }

        [DispId(0x23)]
        short FontSize { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x23)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x23)] set; }

        [DispId(0x24)]
        bool FontUnderline { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x24)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x24)] set; }

        [DispId(0x2c)]
        short Height { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x2c)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x2c)] set; }

        [DispId(0x36)]
        short Left { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x36)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x36)] set; }

        [DispId(0x8d)]
        short Top { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x8d)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x8d)] set; }

        [DispId(0x88)]
        byte TextAlign { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x88)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x88)] set; }

        [DispId(0x94)]
        bool Visible { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x94)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x94)] set; }

        [DispId(0xcc)]
        int ForeColor { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0xcc)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0xcc)] set; }

        [DispId(150)]
        short Width { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(150)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(150)] set; }

        [DispId(0x180)]
        short LeftMargin { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x180)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x180)] set; }

        [DispId(0x181)]
        short TopMargin { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x181)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x181)] set; }

        [DispId(0x184)]
        short RightMargin { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x184)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x184)] set; }

        [DispId(0x185)]
        short BottomMargin { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x185)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x185)] set; }

        [DispId(0x885)]
        Hyperlink Hyperlink { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x885)] get; }

        [DispId(-2147418112)]
        string Name { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418112)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418112)] set; }
    }
    [ComImport, Guid("50D56611-60AC-11CF-82C9-00AA004B9FE6"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface Hyperlink {
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x887)]
        string get_Address();
    }
    [ComImport, Guid("3B06E94E-E47C-11CD-8701-00AA003F0F07"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface Image {
        [DispId(7)]
        string Picture { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(7)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(7)] set; }

        [DispId(8)]
        int BorderColor { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(8)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(8)] set; }

        [DispId(9)]
        byte BorderStyle { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(9)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(9)] set; }

        [DispId(10)]
        byte BorderWidth { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(10)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(10)] set; }

        [DispId(0x1c)]
        int BackColor { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x1c)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x1c)] set; }

        [DispId(0x1d)]
        byte BackStyle { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x1d)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x1d)] set; }

        [DispId(0x2c)]
        short Height { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x2c)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x2c)] set; }

        [DispId(0x36)]
        short Left { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x36)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x36)] set; }

        [DispId(0x59)]
        byte SizeMode { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x59)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x59)] set; }

        [DispId(0x8d)]
        short Top { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x8d)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x8d)] set; }

        [DispId(0x94)]
        bool Visible { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x94)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x94)] set; }

        [DispId(0xbc)]
        object PictureData { [return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)0x400), DispId(0xbc)] get; [param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)0x400), DispId(0xbc)] set; }

        [DispId(150)]
        short Width { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(150)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(150)] set; }

        [DispId(0x155)]
        byte PictureType { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x155)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x155)] set; }

        [DispId(0x885)]
        Hyperlink Hyperlink { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x885)] get; }

        [DispId(-2147418112)]
        string Name { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418112)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418112)] set; }
    }
    [ComImport, Guid("3B06E94C-E47C-11CD-8701-00AA003F0F07"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface Line {
        [DispId(8)]
        int BorderColor { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(8)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(8)] set; }

        [DispId(9)]
        byte BorderStyle { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(9)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(9)] set; }

        [DispId(10)]
        byte BorderWidth { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(10)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(10)] set; }

        [DispId(0x2c)]
        short Height { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x2c)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x2c)] set; }

        [DispId(0x36)]
        short Left { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x36)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x36)] set; }

        [DispId(0x37)]
        bool LineSlant { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x37)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x37)] set; }

        [DispId(0x8d)]
        short Top { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x8d)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x8d)] set; }

        [DispId(0x94)]
        bool Visible { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x94)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x94)] set; }

        [DispId(150)]
        short Width { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(150)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(150)] set; }

        [DispId(-2147418112)]
        string Name { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418112)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418112)] set; }
    }
    [ComImport, Guid("3B06E954-E47C-11CD-8701-00AA003F0F07"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface CheckBox {
        [DispId(8)]
        int BorderColor { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(8)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(8)] set; }

        [DispId(9)]
        byte BorderStyle { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(9)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(9)] set; }

        [DispId(10)]
        byte BorderWidth { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(10)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(10)] set; }

        [DispId(0x1b)]
        string ControlSource { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x1b)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x1b)] set; }

        [DispId(0x2c)]
        short Height { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x2c)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x2c)] set; }

        [DispId(0x36)]
        short Left { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x36)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x36)] set; }

        [DispId(0x8d)]
        short Top { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x8d)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x8d)] set; }

        [DispId(0x94)]
        bool Visible { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x94)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x94)] set; }

        [DispId(150)]
        short Width { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(150)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(150)] set; }

        [DispId(-2147418112)]
        string Name { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418112)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418112)] set; }
    }
    [ComImport, Guid("3B06E94A-E47C-11CD-8701-00AA003F0F07"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface Rectangle {
        [DispId(8)]
        int BorderColor { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(8)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(8)] set; }

        [DispId(10)]
        byte BorderWidth { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(10)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(10)] set; }

        [DispId(0x1c)]
        int BackColor { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x1c)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x1c)] set; }

        [DispId(0x1d)]
        byte BackStyle { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x1d)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x1d)] set; }

        [DispId(0x2c)]
        short Height { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x2c)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x2c)] set; }

        [DispId(0x36)]
        short Left { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x36)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x36)] set; }

        [DispId(0x8d)]
        short Top { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x8d)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x8d)] set; }

        [DispId(0x94)]
        bool Visible { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x94)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x94)] set; }

        [DispId(150)]
        short Width { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(150)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(150)] set; }

        [DispId(-2147418112)]
        string Name { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418112)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418112)] set; }
    }
    [ComImport, Guid("3F4A878E-C395-11D3-8D1F-0050048383FB"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface Form {
        [DispId(-2147418112)]
        string Name { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418112)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147418112)] set; }
    }
    [ComImport, Guid("26B96540-8F8E-101B-AF4E-00AA003F0F07"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface _Control {
    }
}
#endif

#endregion
