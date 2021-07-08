using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit.Abstractions;

namespace RemoteControlledProcess
{
    public sealed class TestProcessWrapper : IDisposable
    {
        private readonly List<Func<bool>> _readinessChecks;
        private readonly TestProjectInfo _testProjectInfo;
        private int? _dotnetHostProcessId;
        private bool _isDisposed;
        private IProcess _process;
        private readonly IProcessFactory _processFactory = new ProcessFactory();
        private IProcessStreamBuffer _processStreamBuffer;
        private readonly IProcessStreamBufferFactory _processStreamBufferFactory = new ProcessStreamBufferFactory();

        public TestProcessWrapper(string appProjectName, bool isCoverletEnabled)
        {
            _readinessChecks = new List<Func<bool>>();
            IsCoverletEnabled = isCoverletEnabled;

            _testProjectInfo = new TestProjectInfo(appProjectName);
        }

        internal TestProcessWrapper(IProcessFactory processFactory, IProcessStreamBufferFactory streamBufferFactory)
            : this("fakeProjectName", false)
        {
            _processFactory = processFactory;
            _processStreamBufferFactory = streamBufferFactory;
        }

        public bool IsCoverletEnabled { get; }

        public bool HasExited => _process == null || _process.HasExited;

        public bool IsRunning => _process != null && !_process.HasExited;

        private static string BinFolder
        {
            get
            {
#if DEBUG
                var binFolder = Path.Combine("bin", "Debug", "net5.0");
#else
                var binFolder = Path.Combine("bin", "Release", "net5.0");
#endif
                return binFolder;
            }
        }

        public ITestOutputHelper TestOutputHelper { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Start()
        {
            _process = _processFactory.CreateProcess();
            _process.StartInfo = CreateProcessStartInfo();

            TestOutputHelper?.WriteLine(
                $"Starting process: {_process.StartInfo.FileName} {_process.StartInfo.Arguments} ...");
            _process.Start();

            _processStreamBuffer = _processStreamBufferFactory.CreateProcessStreamBuffer();
            _processStreamBuffer.BeginCapturing(_process.BeginOutputReadLine,
                handler => _process.OutputDataReceived += handler, handler => _process.OutputDataReceived -= handler);

            TestOutputHelper?.WriteLine($"Process ID: {_process.Id} has exited: {_process.HasExited} ...");

            WaitAndProcessRequiredStartupMessages();
        }

        private ProcessStartInfo CreateProcessStartInfo()
        {
            ProcessStartInfo processStartInfo;

            if (!IsCoverletEnabled)
            {
                processStartInfo = CreateProcessStartInfo("dotnet", _testProjectInfo.AppDllName);
            }
            else
            {
                processStartInfo = CreateProcessStartInfoWithCoverletWrapper();
            }

            return processStartInfo;
        }

        private ProcessStartInfo CreateProcessStartInfoWithCoverletWrapper()
        {
            var arguments =
                $"\".\" --target \"dotnet\" --targetargs \"{_testProjectInfo.AppDllName}\" --output {_testProjectInfo.CoverageReportPath} --format cobertura";

            return CreateProcessStartInfo("coverlet", arguments);
        }

        private ProcessStartInfo CreateProcessStartInfo(string processName, string processArguments)
        {
            var processStartInfo = new ProcessStartInfo(processName)
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = processArguments,
                WorkingDirectory = Path.Combine(_testProjectInfo.ProjectDir, _testProjectInfo.AppProjectName,
                    BinFolder)
            };

            TestOutputHelper?.WriteLine($".NET Application: {processStartInfo.Arguments}");
            TestOutputHelper?.WriteLine($"Application path: {processStartInfo.WorkingDirectory}");
            return processStartInfo;
        }

        private void WaitAndProcessRequiredStartupMessages()
        {
            bool isReady;

            do
            {
                // TODO: Checking for the correct startupMessage and waiting for the _dotnetHostProcessId is a readiness check. Move the corresponding logic into _readinessChecks.
                var startupMessage = ReadOutput();
                ParseStartupMessage(startupMessage);

                // TODO: Add test ensuring that the results of the readiness checks are considered correctly
                // TODO: Only re-execute failing readiness checks
                isReady = _readinessChecks.All(check => check());

                // TODO: Don't sleep if all _readinessChecks passed
                Thread.Sleep(100);
            }
            while (!_dotnetHostProcessId.HasValue || !isReady);
        }

        public string ReadOutput() => _processStreamBuffer.StreamContent;

        public void AddReadinessCheck(Func<bool> readinessCheck)
        {
            _readinessChecks.Add(readinessCheck);
        }

        private void ParseStartupMessage(string startupMessage)
        {
            if (_dotnetHostProcessId.HasValue || !startupMessage.Contains("Process ID"))
            {
                return;
            }

            var processIdStartIndex = startupMessage.IndexOf("Process ID", StringComparison.Ordinal);
            var newLineAfterProcessIdIndex =
                startupMessage.IndexOf("\n", processIdStartIndex, StringComparison.Ordinal);
            var processIdNumberOfDigits = newLineAfterProcessIdIndex - processIdStartIndex - 10;
            var processIdString = startupMessage.Substring(processIdStartIndex + 10, processIdNumberOfDigits);
            _dotnetHostProcessId = int.Parse(processIdString, NumberStyles.Integer, CultureInfo.InvariantCulture);
            TestOutputHelper?.WriteLine($"Process ID: {_dotnetHostProcessId.Value}");
        }

        public void ShutdownGracefully()
        {
            MurderTestProcess();
            WaitForProcessExit();
        }

        private void MurderTestProcess()
        {
            var murderFactory = new ProcessKillerFactory(TestOutputHelper);

            var murder = murderFactory.CreateProcessKillingMethod();
            var murderInProgress = murder(_dotnetHostProcessId);
            RemoveEvidenceForMurder(murderInProgress);
        }

        private void RemoveEvidenceForMurder(Process theMurder)
        {
            WaitSomeTimeForProcessToExit(theMurder);
            KillProcessIfItIsStillRunning(theMurder);
        }

        private void WaitSomeTimeForProcessToExit(Process theProcess)
        {
            if (theProcess != null)
            {
                TestOutputHelper?.WriteLine("Waiting for system call to complete.");
                theProcess.WaitForExit(2000);
            }
        }

        private void KillProcessIfItIsStillRunning(Process theProcess)
        {
            if (!theProcess.HasExited)
            {
                TestOutputHelper?.WriteLine("System call has " + (theProcess.HasExited ? "" : "NOT ") + "completed.");
                theProcess.Kill();
            }
        }

        private void WaitForProcessExit()
        {
            TestOutputHelper?.WriteLine("Waiting for process to shutdown ...");
            _process.WaitForExit(2000);
            TestOutputHelper?.WriteLine($"Process {_testProjectInfo.AppProjectName} has " +
                                        (_process.HasExited ? "" : "NOT ") +
                                        "completed.");
        }

        public void ForceTermination()
        {
            _process.Kill();
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                _process?.Dispose();
                _processStreamBuffer?.Dispose();
            }

            _isDisposed = true;
        }

        ~TestProcessWrapper()
        {
            Dispose(false);
        }
    }
}