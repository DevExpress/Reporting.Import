namespace DevExpress.XtraReports.Design.Import.CrystalFormula {
    static class Messages {
        internal static string
            Information_Started = "SAP Crystal Reports to XtraReports Converter - Conversion started.",
            Information_Completed = "SAP Crystal Reports to XtraReports Converter - Conversion finished.",
            Information_CompletedWithError = "SAP Crystal Reports to XtraReports Converter - Conversion finished with error.",
            Error_Generic_Format = "Cannot complete conversion because of the following exception: '{0}'.",
            Warning_Connection_OleDbProviderNotSupported_Format = "Connection - Cannot generate a report data source because the following OLE DB provider is not supported: '{0}'.",
            Warning_Connection_OleDbProviderNotSpecified = "Connection - Cannot generate a report data source because the OLE DB connection has no provider.",
            Warning_Connection_DatabaseDllNotSupported_Format = "Connection - Cannot generate a report data source because the following database DLL is not supported: '{0}'.",
            Warning_FieldObject_Kind_NotSupported_Format = "FieldObject named '{0}' with definition kind '{1}' is not currently supported.",
            Warning_FieldObject_SpecialVarType_NotSupported_Format = "FieldObject named '{0}' with special field '{1}' is not currently supported.",
            Warning_SummaryOperation_NotSupported_Format = "FieldObject named '{0}' with summary operation '{1}' is not currently supported.",
            Warning_PictureContent_NotSupported_Format = "PictureObject named '{0}' cannot be properly converted with the image content due to API limitations of SAP Crystal Reports.",
            Warning_Chart_NotSupported_Format = "ChartObject named '{0}' has not been converted.",
            Warning_CrossTab_NotSupported_Format = "CrossTabObject named '{0}' has not been converted.",
            Warning_ReportObjectKind_NotSupported_Format = "Report Object named '{0}' with kind '{1}' has not been converted.",
            Warning_ParameterType_NotSupported_Format = "Report Parameter named '{0}' with type '{1}' is not supported.",
            Warning_Binding_CanNotResolve_Format = "Report Object named '{0}' has unsupported binding '{1}'.",
            Control_Untranslated = "Untranslated",
            Control_CantResolveBinding = "Cannot resolve binding",
            Warning_CalculatedField_FormulaNotFound_Format = "Cannot find the '{0}' formula used in the '{1}' calculated field.",
            Warning_DataBinding_FormulaNotFound_Format = "Cannot find the '{0}' formula used in the '{1}' control binding.",
            Warning_CalculatedField_ParameterNotFound_Format = "Cannot find the '{0}' parameter used in the '{1}' calculated field.",
            Warning_DataSourceSP_ParameterNotFound_Format = "Cannot find the '{0}' parameter used in the '{1}' datasource stored procedure query.",
            Warning_DataSourceSP_Limitation = "The converted report may include issues because the original report contains multiple stored procedures.",
            Warning_DataSource_Limitation = "The converted report may include issues because the original report contains several connections.",
            Warning_CalculatedField_UncategorizedFunction_Format = "Cannot convert the '{0}' function used in the '{1}' calculated field.",
            Warning_ParameterLookups_CanNotFindTable_Format = "Cannot find the '{0}' query for dynamic values list used in the '{1}' parameter."
        ;
    }
}
