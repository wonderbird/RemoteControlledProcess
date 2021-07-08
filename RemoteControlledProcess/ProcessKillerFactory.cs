using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Xunit.Abstractions;

namespace RemoteControlledProcess
{
    public sealed class ProcessKillerFactory
    {
        public ProcessKillerFactory(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

        private ITestOutputHelper TestOutputHelper { get; }

        private Func<int?, Process> CreateKillStrategy(string killCommand, string killArgumentsWithPidPlaceholder,
            string signalName)
        {
            return pid =>
            {
                // ReSharper disable once PossibleInvalidOperationException
                var killArguments = string.Format(CultureInfo.InvariantCulture, killArgumentsWithPidPlaceholder,
                    pid.Value);
                TestOutputHelper?.WriteLine($"Sending {signalName} signal to process ...");
                TestOutputHelper?.WriteLine($"Invoking system call: {killCommand} {killArguments}");
                var killProcess = Process.Start(killCommand, killArguments);
                return killProcess;
            };
        }

        private Func<int?, Process> CreateUnixStrategy() => CreateKillStrategy("kill", "-s TERM {0}", "TERM");


        // Under Windows, SIGINT doesn't work. Thus we use the KILL signal.
        //
        // To try this out you can place a breakpoint here and check on the
        // command line yourself.
        //
        // This can be tolerated for our case here, because the application
        // is intended to run in a linux docker container and because the
        // build pipeline uses linux containers for testing.
        private Func<int?, Process> CreateWindowsStrategy() => CreateKillStrategy("taskkill", "/f /pid {0}", "KILL");

        public Func<int?, Process> CreateProcessKillingMethod() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? CreateWindowsStrategy() : CreateUnixStrategy();
    }
}