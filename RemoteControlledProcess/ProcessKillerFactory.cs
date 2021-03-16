using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Xunit.Abstractions;

namespace RemoteControlledProcess
{
    public sealed class ProcessKillerFactory
    {
        public ProcessKillerFactory(ITestOutputHelper testOutputHelper)
        {
            TestOutputHelper = testOutputHelper;
        }

        public ITestOutputHelper TestOutputHelper { get; set; }

        public Func<int?, Process> CreateUnixStragey()
        {
            return (pid) =>
            {
                var killCommand = "kill";
                // ReSharper disable once PossibleInvalidOperationException
                var killArguments = $"-s TERM {pid.Value}";
                var signalName = "TERM";
                TestOutputHelper?.WriteLine($"Sending {signalName} signal to process ...");
                TestOutputHelper?.WriteLine($"Invoking system call: {killCommand} {killArguments}");
                var killProcess = Process.Start(killCommand, killArguments);
                return killProcess;
            };
        }


        public Func<int?, Process> CreateWindowsStrategy()
        {
            return (pid) =>
            {
                var killCommand = "taskkill";
                // ReSharper disable once PossibleInvalidOperationException
                var killArguments = $"/f /pid {pid.Value}";

                // Under Windows, SIGINT doesn't work. Thus we use the KILL signal.
                //
                // To try this out you can place a breakpoint here and check on the
                // command line yourself.
                //
                // This can be tolerated for our case here, because the application
                // is intended to run in a linux docker container and because the
                // build pipeline uses linux containers for testing.
                var signalName = "KILL";
                TestOutputHelper?.WriteLine($"Sending {signalName} signal to process ...");
                TestOutputHelper?.WriteLine($"Invoking system call: {killCommand} {killArguments}");
                var killProcess = Process.Start(killCommand, killArguments);
                return killProcess;
            };
        }

        public Func<int?, Process> CreateProcessKillingMethod() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? CreateWindowsStrategy() : CreateUnixStragey();
    }
}