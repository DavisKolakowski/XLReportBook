# XLReportBook

## Description
XLReportBook is a .NET library designed to facilitate the creation, management, and manipulation of report data. It provides a flexible and extensible framework for defining report schemas, refreshing data, and performing various operations on report data.

## Installation
To install XLReportBook, follow these steps:
1. Clone the repository: `git clone https://github.com/DavisKolakowski/XLReportBook.git`
2. Open the solution file `Packages.sln` in Visual Studio.
3. Build the solution to restore the necessary NuGet packages and compile the projects.

## Usage
Here are some examples of how to use XLReportBook:

### Defining a Report Schema
```csharp
using ReportBook.Attributes;
using ReportBook.Models;

public class MyReport : Report
{
    [ColumnCaption("Name")]
    public string Name { get; set; }

    [DateTimeMode(DataSetDateTime.Utc)]
    public DateTime CreatedDate { get; set; }

    public int Value { get; set; }
}
```

### Creating a Report Context
```csharp
using ReportBook.Context;

public class MyReportContext : ReportBookContext
{
    public ReportStore<MyReport> MyReports { get; set; }
}
```

### Refreshing Data
```csharp
var context = new MyReportContext();
context.Refresh(new[]
{
    new MyReport { Name = "Alice", CreatedDate = DateTime.UtcNow, Value = 10 },
    new MyReport { Name = "Bob", CreatedDate = DateTime.UtcNow.AddDays(-1), Value = 20 }
});
```

### Accessing Data
```csharp
var reports = context.MyReports.ToList();
foreach (var report in reports)
{
    Console.WriteLine($"Name: {report.Name}, CreatedDate: {report.CreatedDate}, Value: {report.Value}");
}
```

## Testing
To run the tests for XLReportBook, follow these steps:
1. Open the solution file `Packages.sln` in Visual Studio.
2. Build the solution to restore the necessary NuGet packages and compile the projects.
3. Open the Test Explorer in Visual Studio (Test > Test Explorer).
4. Run all tests to ensure that everything is working correctly.

## Contributing
We welcome contributions to XLReportBook! If you would like to contribute, please follow these guidelines:
1. Fork the repository and create a new branch for your feature or bugfix.
2. Write tests to cover your changes.
3. Ensure that all tests pass.
4. Submit a pull request with a clear description of your changes.

## License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more information.
