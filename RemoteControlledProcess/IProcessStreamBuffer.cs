using System;
using System.Diagnostics;

namespace RemoteControlledProcess
{
    public interface IProcessStreamBuffer : IDisposable
    {
        string StreamContent { get; }

        void BeginCapturing(Action beginReadLine, Action<DataReceivedEventHandler> subscribeToDataReceivedEvent,
            Action<DataReceivedEventHandler> unsubscribeFromDataReceivedEvent);
    }
}