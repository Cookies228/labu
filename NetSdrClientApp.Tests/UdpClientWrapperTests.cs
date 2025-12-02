using System;
using System.Threading.Tasks;
using Xunit;

namespace NetSdrClientApp.Tests
{
    public class UdpClientWrapperTests : IDisposable
    {
        [Fact]
        public void Constructor_WithValidPort_InitializesCorrectly()
        {
            // Arrange & Act
            var wrapper = new UdpClientWrapper(60000);
            
            // Assert
            Assert.NotNull(wrapper);
        }

        [Fact]
        public void Constructor_WithDifferentPorts_InitializesCorrectly()
        {
            // Arrange & Act
            var wrapper1 = new UdpClientWrapper(60000);
            var wrapper2 = new UdpClientWrapper(60001);
            
            // Assert
            Assert.NotNull(wrapper1);
            Assert.NotNull(wrapper2);
        }

        [Fact]
        public void MessageReceived_EventCanBeSubscribed()
        {
            // Arrange
            var wrapper = new UdpClientWrapper(60000);
            var eventHandled = false;
            
            // Act
            wrapper.MessageReceived += (sender, data) => { eventHandled = true; };
            
            // Assert
            Assert.NotNull(wrapper.MessageReceived);
        }

        [Fact]
        public void GetHashCode_WithSamePort_ReturnsSameHash()
        {
            // Arrange
            var wrapper1 = new UdpClientWrapper(60000);
            var wrapper2 = new UdpClientWrapper(60000);
            
            // Act
            var hash1 = wrapper1.GetHashCode();
            var hash2 = wrapper2.GetHashCode();
            
            // Assert
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void GetHashCode_WithDifferentPorts_ReturnsDifferentHash()
        {
            // Arrange
            var wrapper1 = new UdpClientWrapper(60000);
            var wrapper2 = new UdpClientWrapper(60001);
            
            // Act
            var hash1 = wrapper1.GetHashCode();
            var hash2 = wrapper2.GetHashCode();
            
            // Assert
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void StopListening_WhenNotListening_DoesNotThrow()
        {
            // Arrange
            var wrapper = new UdpClientWrapper(60000);
            
            // Act & Assert
            wrapper.StopListening(); // Should not throw
        }

        [Fact]
        public void Exit_WhenNotListening_DoesNotThrow()
        {
            // Arrange
            var wrapper = new UdpClientWrapper(60000);
            
            // Act & Assert
            wrapper.Exit(); // Should not throw
        }

        [Fact]
        public async Task StartListeningAsync_CanBeStoppedImmediately()
        {
            // Arrange
            var wrapper = new UdpClientWrapper(61000);
            
            // Act
            var listeningTask = wrapper.StartListeningAsync();
            await Task.Delay(100); // Let it start
            wrapper.StopListening();
            
            // Wait for the listening task to complete
            await listeningTask;
            
            // Assert - if we get here, it stopped gracefully
            Assert.True(true);
        }

        [Fact]
        public void GetHashCode_IsConsistent()
        {
            // Arrange
            var wrapper = new UdpClientWrapper(60000);
            
            // Act
            var hash1 = wrapper.GetHashCode();
            var hash2 = wrapper.GetHashCode();
            
            // Assert
            Assert.Equal(hash1, hash2);
        }

        public void Dispose()
        {
            // Cleanup if needed
        }
    }
}
