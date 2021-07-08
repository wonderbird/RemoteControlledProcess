using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace RemoteControlledProcess
{
    /// <summary>
    ///     Asynchronously read from StandardOutput or StandardError of a process.
    /// </summary>
    /// <remarks>
    ///     This class implements an asynchronous method to read a process stream reliably.
    ///     For this purpose the class declares a stream buffer and appends the stream data to it
    ///     when the corresponding DataReceivedEvent has been published. The class is thread safe.
    ///     <see href="https://stackoverflow.com/questions/7160187/standardoutput-readtoend-hangs">
    ///         Stackoverflow:
    ///         StandardOutput.ReadToEnd() hangs [duplicate]
    ///     </see>
    /// </remarks>
    public sealed class ProcessStreamBuffer : IProcessStreamBuffer
    {
        private readonly object _lock = new();
        private StringBuilder _buffer;
        private bool _isDisposed;
        private Action<DataReceivedEventHandler> _unsubscribeFromDataReceivedEvent;
        private AutoResetEvent _waitHandle;

        public string StreamContent
        {
            get
            {
                lock (_lock)
                {
                    return _buffer.ToString();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void BeginCapturing(Action beginReadLine, Action<DataReceivedEventHandler> subscribeToDataReceivedEvent,
            Action<DataReceivedEventHandler> unsubscribeFromDataReceivedEvent)
        {
            _unsubscribeFromDataReceivedEvent = unsubscribeFromDataReceivedEvent;
            _waitHandle = new AutoResetEvent(false);

            lock (_lock)
            {
                _buffer = new StringBuilder();
            }

            subscribeToDataReceivedEvent(appendEventDataToOutputBuffer);
            beginReadLine();
        }

        ~ProcessStreamBuffer()
        {
            Dispose(false);
        }

        private void appendEventDataToOutputBuffer(object sender, DataReceivedEventArgs eventArg)
        {
            if (eventArg.Data == null)
            {
                _waitHandle.Set();
            }
            else
            {
                lock (_lock)
                {
                    _buffer.AppendLine(eventArg.Data);
                }
            }
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                _unsubscribeFromDataReceivedEvent(appendEventDataToOutputBuffer);
                _waitHandle?.Dispose();
            }

            _isDisposed = true;
        }
    }
}