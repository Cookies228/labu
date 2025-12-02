using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Xunit;
using Moq;
using NetSdrClientApp.Networking;

namespace NetSdrClientApp.Tests
{
    public class TcpClientWrapperTests : IDisposable
    {
        [Fact]
        public void Constructor_WithValidParameters_InitializesCorrectly()
        {
            // Arrange & Act
            var wrapper = new TcpClientWrapper("127.0.0.1", 5000);
            
            // Assert
            Assert.NotNull(wrapper);
            Assert.False(wrapper.Connected);
            
            // Cleanup
            wrapper.Dispose();
        }

        [Fact]
        public void Connected_WhenNotConnected_ReturnsFalse()
        {
            // Arrange
            var wrapper = new TcpClientWrapper("127.0.0.1", 5000);
            
            // Act & Assert
            Assert.False(wrapper.Connected);
            
            // Cleanup
            wrapper.Dispose();
        }

        [Fact]
        public async Task SendMessageAsync_WhenNotConnected_ThrowsException()
        {
            // Arrange
            var wrapper = new TcpClientWrapper("127.0.0.1", 9999);
            var data = new byte[] { 0x01, 0x02 };
            
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                wrapper.SendMessageAsync(data));
            
            // Cleanup
            wrapper.Dispose();
        }

        [Fact]
        public async Task SendMessageAsync_WithStringParameter_ThrowsWhenNotConnected()
        {
            // Arrange
            var wrapper = new TcpClientWrapper("127.0.0.1", 9999);
            
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                wrapper.SendMessageAsync("test"));
            
            // Cleanup
            wrapper.Dispose();
        }

        [Fact]
        public void Disconnect_WhenNotConnected_DoesNotThrow()
        {
            // Arrange
            var wrapper = new TcpClientWrapper("127.0.0.1", 5000);
            
            // Act & Assert - should not throw
            wrapper.Disconnect();
            
            // Cleanup
            wrapper.Dispose();
        }

        [Fact]
        public void Dispose_CanBeCalledMultipleTimes()
        {
            // Arrange
            var wrapper = new TcpClientWrapper("127.0.0.1", 5000);
            
            // Act & Assert
            wrapper.Dispose();
            wrapper.Dispose(); // Should not throw
        }

        [Fact]
        public void ThrowIfDisposed_AfterDispose_ThrowsObjectDisposedException()
        {
            // Arrange
            var wrapper = new TcpClientWrapper("127.0.0.1", 5000);
            wrapper.Dispose();
            
            // Act & Assert
            Assert.Throws<ObjectDisposedException>(() => wrapper.Connect());
        }

        [Fact]
        public void Constructor_WithDifferentPorts_InitializesCorrectly()
        {
            // Arrange & Act
            var wrapper1 = new TcpClientWrapper("localhost", 5000);
            var wrapper2 = new TcpClientWrapper("localhost", 5001);
            
            // Assert
            Assert.NotNull(wrapper1);
            Assert.NotNull(wrapper2);
            
            // Cleanup
            wrapper1.Dispose();
            wrapper2.Dispose();
        }

        [Fact]
        public void MessageReceived_EventCanBeSubscribed()
        {
            // Arrange
            var wrapper = new TcpClientWrapper("127.0.0.1", 5000);
            var eventFired = false;
            
            // Act
            wrapper.MessageReceived += (sender, data) => { eventFired = true; };
            
            // Assert
            Assert.NotNull(wrapper.MessageReceived);
            
            // Cleanup
            wrapper.Dispose();
        }

        [Fact]
        public void Disconnect_MultipleTimes_DoesNotThrow()
        {
            // Arrange
            var wrapper = new TcpClientWrapper("127.0.0.1", 5000);
            
            // Act & Assert
            wrapper.Disconnect();
            wrapper.Disconnect();
            wrapper.Disconnect();
            
            // Cleanup
            wrapper.Dispose();
        }

        public void Dispose()
        {
            // Cleanup if needed
        }
    }
}
