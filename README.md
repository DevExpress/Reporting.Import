# Reporting.Import
In the project's properties, there are the following options, which you can adjust:

In the **Build** tab, the **Conditional compilation symbols** specify the suppliers (all three - **Crystal**, **Active**, and **Access** - are enabled, by default).

And, in the **Debug** tab, you can specify the **Command line arguments**, which determine the input and output files (the "in" and "out" parameters). So, simply specify the name of the file that should be converted, if it resides in the same folder where the application's EXE file is located. Or, if it resides in another folder, specify the path to it. Similarly, for the "out" parameter, specify the name and path to the resulting file.

Then, you can launch the application either from Visual Studio, or from the command line (with both parameters defined).

**_Note: This project intentionally does not contain third-party libraries. To compile a converter application, you will have to personally add the required assemblies._**

After running this tool, use the following command line to convert multiple reports at a time:
```
FOR /R Reports %R IN (*.rpt) DO ReportsImport "/in:%R" "/out:%R.repx"
```

The following command starts converting a specific report:
```
C:\0>reportsimport /in:c:\0\crystal\file.rpt /out c:\0\converted\testreport.repx
```
As a result of differences between DevExpress Reports and other reporting tools, the extent to which your converted report will match the original depends upon the layout of the initial report.
Please consider the [requirements and limitations](https://docs.devexpress.com/XtraReports/1468/get-started-with-devexpress-reporting/add-a-report-to-your-.net-application/convert-third-party-reports-to-devexpress-reports) applying to the import of third-party reports.


_All trademarks and registered trademarks are the property of their respective owners._
