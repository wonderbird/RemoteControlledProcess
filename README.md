# Test Helper: RemoteControlledProcess

[![Build Status Badge](https://github.com/wonderbird/kata-rabbitmq/workflows/.NET%20Core/badge.svg)](https://github.com/wonderbird/kata-rabbitmq/actions?query=workflow%3A%22.NET+Core%22)
[![Test Coverage (coveralls)](https://img.shields.io/coveralls/github/wonderbird/RemoteControlledProcess)](https://coveralls.io/github/wonderbird/RemoteControlledProcess)
[![Test Coverage (codeclimate)](https://img.shields.io/codeclimate/coverage-letter/wonderbird/RemoteControlledProcess)](https://codeclimate.com/github/wonderbird/RemoteControlledProcess/trends/test_coverage_total)
[![Code Maintainability](https://img.shields.io/codeclimate/maintainability-percentage/wonderbird/RemoteControlledProcess)](https://codeclimate.com/github/wonderbird/RemoteControlledProcess)
[![Issues in Code](https://img.shields.io/codeclimate/issues/wonderbird/RemoteControlledProcess)](https://codeclimate.com/github/wonderbird/RemoteControlledProcess/issues)
[![Technical Debt](https://img.shields.io/codeclimate/tech-debt/wonderbird/RemoteControlledProcess)](https://codeclimate.com/github/wonderbird/RemoteControlledProcess)
[![CodeScene Code Health](https://codescene.io/projects/12257/status-badges/code-health)](https://codescene.io/projects/12257/jobs/latest-successful/results)
[![CodeScene System Mastery](https://codescene.io/projects/12257/status-badges/system-mastery)](https://codescene.io/projects/12257/jobs/latest-successful/results)

Launch and control `dotnet` processes wrapped into the [coverlet](https://github.com/coverlet-coverage/coverlet) code
coverage analyzer.

The class `ProcessWrapper` is intended to launch one ore more `dotnet` processes for performing acceptance tests. The
class captures the messages written to the Console and to Console.Error. It allows to terminate the process gracefully
and forcefully. One of the processes can be wrapped by the [coverlet](https://github.com/coverlet-coverage/coverlet)
command line tool in order to calculate code coverage.

## Attention

You can use the `coverlet` wrapper only once per `dotnet` application, because `coverlet` instruments the `dotnet` DLL.
If you use `coverlet` with two or more instances of the same application, `coverlet` will report an exception after
(or during) the application termination and the reported coverage will be 0.

## Development and Support Standard

This project is incomplete and not production ready.

I am developing during my spare time and use this project for learning purposes. Please assume that I will need some
days to answer your questions. Please keep this in mind when using this project in a production environment.

## Thanks

Many thanks to [JetBrains](https://www.jetbrains.com/?from=RemoteControlledProcess) who provide
an [Open Source License](https://www.jetbrains.com/community/opensource/) for this project ❤️.

# Development

### Prerequisites

To compile, test and run this project the latest [.NET Core SDK](https://dotnet.microsoft.com/download) is required on
your machine. For calculating code metrics I recommend [metrix++](https://github.com/metrixplusplus/metrixplusplus).
This requires python.

To use the `RemoteControlledProcess` library and to run the unit tests you need the following tools installed:

```shell
dotnet tool install --global coverlet.console --configfile NuGet-OfficialOnly.config
dotnet tool install --global dotnet-reportgenerator-globaltool --configfile NuGet-OfficialOnly.config
```

## Build, Test, Run

Run the following commands from the folder containing the `RemoteControlledProcess.sln` file in order to build, test and
run the application:

### Build the Solution and Run the Acceptance Tests

```sh
dotnet build
dotnet test
```

### Known Issue

When you run the tests on a mac, then the tests using *two* `RemoteControlledProcess`es issue a crash report regarding
"dotnet".

At the moment I cannot explain that behavior.

### Before Creating a Pull Request ...

... apply code formatting rules

```shell
dotnet format
```

... and check code metrics using [metrix++](https://github.com/metrixplusplus/metrixplusplus)

```shell
# Collect metrics
metrix++ collect --std.code.complexity.cyclomatic --std.code.lines.code --std.code.todo.comments --std.code.maintindex.simple -- .

# Get an overview
metrix++ view --db-file=./metrixpp.db

# Apply thresholds
metrix++ limit --db-file=./metrixpp.db --max-limit=std.code.complexity:cyclomatic:5 --max-limit=std.code.lines:code:25:function --max-limit=std.code.todo:comments:0 --max-limit=std.code.mi:simple:1
```

At the time of writing, I want to stay below the following thresholds:

```shell
--max-limit=std.code.complexity:cyclomatic:5
--max-limit=std.code.lines:code:25:function
--max-limit=std.code.todo:comments:0
--max-limit=std.code.mi:simple:1
```

I allow generated files named `*.feature.cs` to exceed these thresholds.

Finally, remove all code duplication. The next section describes how to detect code duplication.

## Identify Code Duplication

The `tools\dupfinder.bat` or `tools/dupfinder.sh` file calls
the [JetBrains dupfinder](https://www.jetbrains.com/help/resharper/dupFinder.html) tool and creates an HTML report of
duplicated code blocks in the solution directory.

In order to use the `dupfinder` you need to globally install
the [JetBrains ReSharper Command Line Tools](https://www.jetbrains.com/help/resharper/ReSharper_Command_Line_Tools.html)
On Unix like operating systems you also need [xsltproc](http://xmlsoft.org/XSLT/xsltproc2.html), which is pre-installed
on macOS.

From the folder containing the `.sln` file run

```sh
tools\dupfinder.bat
```

or

```sh
tools/dupfinder.sh
```

respectively.

The report will be created as `dupfinder-report.html` in the current directory.

# References

## .NET Core

*
GitHub: [aspnet / Hosting / samples / GenericHostSample](https://github.com/aspnet/Hosting/tree/2.2.0/samples/GenericHostSample)

## Behavior Driven Development (BDD)

* Tricentis: [SpecFlow - Getting Started](https://specflow.org/getting-started/)
* The SpecFlow
  Team: [SpecFlow.xUnit — documentation](https://docs.specflow.org/projects/specflow/en/latest/Integrations/xUnit.html)
* The SpecFlow
  Team: [SpecFlow - Getting Started with a new project](https://docs.specflow.org/projects/specflow/en/latest/Getting-Started/Getting-Started-With-A-New-Project.html?utm_source=website&utm_medium=newproject&utm_campaign=getting_started)
* [Testcontainers](https://www.testcontainers.org/)

## Code Analysis

* JetBrains s.r.o.: [dupFinder Command-Line Tool](https://www.jetbrains.com/help/resharper/dupFinder.html)
* GitHub: [coverlet-coverage / coverlet](https://github.com/coverlet-coverage/coverlet)
* GitHub: [danielpalme / ReportGenerator](https://github.com/danielpalme/ReportGenerator)
* Scott
  Hanselman: [EditorConfig code formatting from the command line with .NET Core's dotnet format global tool](https://www.hanselman.com/blog/editorconfig-code-formatting-from-the-command-line-with-net-cores-dotnet-format-global-tool)
* [EditorConfig.org](https://editorconfig.org)
* GitHub: [dotnet / roslyn - .editorconfig](https://github.com/dotnet/roslyn/blob/master/.editorconfig)
* Check all the badges on top of this README
