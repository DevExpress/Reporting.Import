# Overview

This repository contains a console application project designed to convert third-party reports into DevExpress Report Definition (.REPX) files. You can use these .REPX files to [load report layouts](https://docs.devexpress.com/XtraReports/2666/detailed-guide-to-devexpress-reporting/store-and-distribute-reports/store-report-layouts-and-documents/load-report-layouts) within the DevExpress Visual Studio Report Designer, DevExpress End-User Report Designer, or display the report at runtime.

# Project Specifics

You can modify the following options in project properties:

 *	The **Build** Tab’s **Conditional compilation symbols** specifies the list of all supported third-party suppliers (all are enabled by default);
 *	The **Debug** tab allows you to specify **Command line arguments**. These arguments determine input and output files (the **in** and **out** parameters). If the source or destination file is stored within the application’s root folder, specify the file's name. Otherwise, specify the full path to the file.

# Examples of use

You can launch the application from Visual Studio or from the command line and specify the **in** and **out** parameters.
Use the following command to convert multiple reports at a time:

```
FOR /R Reports %R IN (*.rpt) DO ReportsImport "/in:%R" "/out:%R.repx"
```

The following command converts an individual report: 

```
ReportsImport /in:c:\0\crystal\file.rpt /out:c:\0\converted\testreport.repx
```

# RDL/RDLC and Crystal Reports Conversion Specifics

If an RDL/RDLC or Crystal Reports function cannot be converted, it is replaced with the **"NOT_SUPPORTED"** message, as in the following [expression](https://docs.devexpress.com/XtraReports/120091/detailed-guide-to-devexpress-reporting/use-expressions) example:

| RDL/RDLC | Crystal | DevExpress |
| --- | --- | --- |
| =IsDate(Fields!Column.Value) | IsDate({report.Column}) | Iif(True, '#NOT_SUPPORTED#', 'IsDate([Column])') |

Set the **UnrecognizedFunctionBehavior** option to **Ignore** to leave unrecognized functions in expressions.

    RDL/RDLC Reports:
    ```
    ReportsImport /in:c:\0\rdlc\file.rdlc /out:c:\0\converted\testreport.repx /ssrs:UnrecognizedFunctionBehavior=Ignore
    ```

    Crystal Reports:
    ```
    ReportsImport /in:c:\0\crystal\file.rpt /out:c:\0\converted\testreport.repx /crystal:UnrecognizedFunctionBehavior=Ignore
    ```

The command listed above produces a .REPX file with the unrecognized *IsDate* function:

| RDL/RDLC | Crystal | DevExpress |
| --- | --- | --- |
| =IsDate(Fields!Column.Value) | IsDate({report.Column}) | IsDate([Column]) | 

You can implement [custom functions](https://docs.devexpress.com/XtraReports/DevExpress.XtraReports.Expressions.CustomFunctions) to support unrecognized functions in DevExpress reports (the *IsDate* custom function in the sample above).

# Limitations

This report conversion tool is limited in scope (due to differences between DevExpress Reports and other reporting tools). Review the [requirements and limitations](https://docs.devexpress.com/XtraReports/1468/get-started-with-devexpress-reporting/add-a-report-to-your-.net-application/convert-third-party-reports-to-devexpress-reports) related to this product before you convert reports.

**_Note: This project intentionally does not contain third-party libraries. To compile the application, add references to required assemblies._**
