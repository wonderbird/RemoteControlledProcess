using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace RemoteControlledProcess.Unit.Tests
{
    public class ExceptionReporterExtensionTest
    {
        private const string ExceptionMessage = "Exception generated for test purpose";

        [Theory]
        [InlineData(0,
            "Unhandled exception in .*RemoteControlledProcess.Unit.Tests/ExceptionReporterExtensionTest.cs:[0-9]+")]
        [InlineData(1, ExceptionMessage)]
        public void Write_CalledInCatchBlock_WrittenMessagesMatch(int invocationIndex, string expectedMessageRegex)
        {
            // Arrange
            var writerMock = new Mock<TextWriter>();

            try
            {
                throw new UnitTestException(ExceptionMessage);
            }
            catch (UnitTestException e)
            {
                // Act
                e.Write(writerMock.Object);
            }

            // Assert
            var actualMessage = (string)writerMock.Invocations[invocationIndex].Arguments[0];
            Assert.Matches(expectedMessageRegex, actualMessage);
        }

        [Fact]
        public void Log_CalledInCatchBlock_LogsCriticalMessageWithExpectedRegex()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            try
            {
                throw new UnitTestException(ExceptionMessage);
            }
            catch (UnitTestException e)
            {
                // Act
                e.Log(loggerMock.Object);
            }

            // Assert
            var expectedMessageRegex =
                "Unhandled exception in .*RemoteControlledProcess.Unit.Tests/ExceptionReporterExtensionTest.cs:[0-9]+";
            var actualLogLevel = (LogLevel)loggerMock.Invocations[0].Arguments[0];
            var actualMessage = loggerMock.Invocations[0].Arguments[2].ToString();
            Assert.Equal(LogLevel.Critical, actualLogLevel);
            Assert.Matches(expectedMessageRegex, actualMessage);
        }
    }
}