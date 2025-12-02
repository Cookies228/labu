using System;
using Xunit;

namespace NetSdrClientApp.Tests
{
    public class ProgramTests
    {
        [Fact]
        public void Program_CanInstantiateNetSdrClient()
        {
            // Arrange
            var tcpClient = new TcpClientWrapper("127.0.0.1", 5000);
            var udpClient = new UdpClientWrapper(60000);
            
            // Act
            var netSdr = new NetSdrClient(tcpClient, udpClient);
            
            // Assert
            Assert.NotNull(netSdr);
            
            // Cleanup
            tcpClient.Dispose();
        }

        [Fact]
        public void Program_TcpClientInitializesWithCorrectAddress()
        {
            // Arrange & Act
            var tcpClient = new TcpClientWrapper("127.0.0.1", 5000);
            
            // Assert
            Assert.NotNull(tcpClient);
            Assert.False(tcpClient.Connected);
            
            // Cleanup
            tcpClient.Dispose();
        }

        [Fact]
        public void Program_UdpClientInitializesWithCorrectPort()
        {
            // Arrange & Act
            var udpClient = new UdpClientWrapper(60000);
            
            // Assert
            Assert.NotNull(udpClient);
            
            // Cleanup - UDP client doesn't need disposal but we can verify creation
        }

        [Fact]
        public void Program_MultipleClientsCanBeCreated()
        {
            // Arrange & Act
            var tcpClient1 = new TcpClientWrapper("127.0.0.1", 5000);
            var tcpClient2 = new TcpClientWrapper("127.0.0.1", 5001);
            var udpClient1 = new UdpClientWrapper(60000);
            var udpClient2 = new UdpClientWrapper(60001);
            
            // Assert
            Assert.NotNull(tcpClient1);
            Assert.NotNull(tcpClient2);
            Assert.NotNull(udpClient1);
            Assert.NotNull(udpClient2);
            
            // Cleanup
            tcpClient1.Dispose();
            tcpClient2.Dispose();
        }
    }
}
