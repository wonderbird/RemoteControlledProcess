namespace RemoteControlledProcess
{
    public class ProcessStreamBufferFactory : IProcessStreamBufferFactory
    {
        public IProcessStreamBuffer CreateProcessStreamBuffer() => new ProcessStreamBuffer();
    }
}