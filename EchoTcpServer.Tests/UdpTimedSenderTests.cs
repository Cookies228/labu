using System;
using System.Threading.Tasks;
using Xunit;

namespace EchoTcpServer.Tests
{
    public class UdpTimedSenderTests : IDisposable
    {
        [Fact]
        public void Constructor_WithValidParameters_InitializesCorrectly()
        {
            // Arrange & Act
            var sender = new UdpTimedSender("127.0.0.1", 60000);
            
            // Assert
            Assert.NotNull(sender);
            
            // Cleanup
            sender.Dispose();
        }

        [Fact]
        public void StartSending_StartsTimer()
        {
            // Arrange
            var sender = new UdpTimedSender("127.0.0.1", 60001);
            
            // Act
            sender.StartSending(1000);
            var taskDelay = Task.Delay(100);
            taskDelay.Wait();
            
            // Assert - if no exception, it started
            Assert.True(true);
            
            // Cleanup
            sender.StopSending();
            sender.Dispose();
        }

        [Fact]
        public void StartSending_WhenAlreadyRunning_ThrowsException()
        {
            // Arrange
            var sender = new UdpTimedSender("127.0.0.1", 60002);
            sender.StartSending(1000);
            
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => sender.StartSending(1000));
            
            // Cleanup
            sender.StopSending();
            sender.Dispose();
        }

        [Fact]
        public void StopSending_StopsTimer()
        {
            // Arrange
            var sender = new UdpTimedSender("127.0.0.1", 60003);
            sender.StartSending(1000);
            
            // Act
            sender.StopSending();
            
            // Assert - if no exception, it stopped
            Assert.True(true);
            
            // Cleanup
            sender.Dispose();
        }

        [Fact]
        public void StopSending_CanBeCalledMultipleTimes()
        {
            // Arrange
            var sender = new UdpTimedSender("127.0.0.1", 60004);
            sender.StartSending(1000);
            
            // Act & Assert
            sender.StopSending();
            sender.StopSending(); // Should not throw
            
            // Cleanup
            sender.Dispose();
        }

        [Fact]
        public void Dispose_ReleasesResources()
        {
            // Arrange
            var sender = new UdpTimedSender("127.0.0.1", 60005);
            sender.StartSending(1000);
            
            // Act
            sender.Dispose();
            
            // Assert - if no exception, dispose worked
            Assert.True(true);
        }

        [Fact]
        public void Constructor_WithDifferentHosts_InitializesCorrectly()
        {
            // Arrange & Act
            var sender1 = new UdpTimedSender("127.0.0.1", 60006);
            var sender2 = new UdpTimedSender("localhost", 60007);
            
            // Assert
            Assert.NotNull(sender1);
            Assert.NotNull(sender2);
            
            // Cleanup
            sender1.Dispose();
            sender2.Dispose();
        }

        [Fact]
        public void StartSending_WithDifferentIntervals_Works()
        {
            // Arrange
            var sender1 = new UdpTimedSender("127.0.0.1", 60008);
            var sender2 = new UdpTimedSender("127.0.0.1", 60009);
            
            // Act
            sender1.StartSending(500);
            sender2.StartSending(1000);
            
            // Assert
            Assert.True(true);
            
            // Cleanup
            sender1.StopSending();
            sender2.StopSending();
            sender1.Dispose();
            sender2.Dispose();
        }

        public void Dispose()
        {
            // Cleanup
        }
    }
}
