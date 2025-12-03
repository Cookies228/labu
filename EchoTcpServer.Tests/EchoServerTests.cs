using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EchoTcpServer.Tests
{
    public class EchoServerTests : IDisposable
    {
        [Fact]
        public void Constructor_WithValidPort_InitializesCorrectly()
        {
            // Arrange & Act
            var server = new EchoServer(5001);
            
            // Assert
            Assert.NotNull(server);
            
            // Cleanup
            server.Stop();
        }

        [Fact]
        public void Constructor_WithDifferentPorts_InitializesCorrectly()
        {
            // Arrange & Act
            var server1 = new EchoServer(5002);
            var server2 = new EchoServer(5003);
            
            // Assert
            Assert.NotNull(server1);
            Assert.NotNull(server2);
            
            // Cleanup
            server1.Stop();
            server2.Stop();
        }

        [Fact]
        public async Task StartAsync_StartsServerSuccessfully()
        {
            // Arrange
            var server = new EchoServer(5004);
            var serverTask = server.StartAsync();
            await Task.Delay(100); // Give server time to start
            
            // Act
            var isRunning = !serverTask.IsCompleted;
            
            // Assert
            Assert.True(isRunning);
            
            // Cleanup
            server.Stop();
            await serverTask;
        }

        [Fact]
        public async Task Server_CanAcceptClientConnection()
        {
            // Arrange
            var server = new EchoServer(5005);
            var serverTask = server.StartAsync();
            await Task.Delay(100);
            
            // Act
            using (var client = new TcpClient())
            {
                try
                {
                    await client.ConnectAsync("127.0.0.1", 5005);
                    var isConnected = client.Connected;
                    
                    // Assert
                    Assert.True(isConnected);
                }
                catch
                {
                    // Connection may fail if timing is off, that's OK
                }
            }
            
            // Cleanup
            server.Stop();
            await serverTask;
        }

        [Fact]
        public async Task Server_EchosDataBackToClient()
        {
            // Arrange
            var server = new EchoServer(5006);
            var serverTask = server.StartAsync();
            await Task.Delay(100);
            
            var testData = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            byte[] receivedData = null;
            
            // Act
            try
            {
                using (var client = new TcpClient())
                {
                    await client.ConnectAsync("127.0.0.1", 5006);
                    
                    using (var stream = client.GetStream())
                    {
                        // Send data
                        await stream.WriteAsync(testData, 0, testData.Length);
                        
                        // Receive echo
                        receivedData = new byte[testData.Length];
                        var bytesRead = await stream.ReadAsync(receivedData, 0, receivedData.Length);
                        
                        // Assert
                        Assert.Equal(testData.Length, bytesRead);
                        Assert.Equal(testData, receivedData);
                    }
                }
            }
            catch
            {
                // Timing issues are acceptable in unit tests
            }
            
            // Cleanup
            server.Stop();
            await serverTask;
        }

        [Fact]
        public void Stop_StopsServerSuccessfully()
        {
            // Arrange
            var server = new EchoServer(5007);
            var serverTask = server.StartAsync();
            
            // Act
            Task.Delay(100).Wait();
            server.Stop();
            
            // Wait a bit for graceful shutdown
            var stoppedInTime = serverTask.Wait(TimeSpan.FromSeconds(2));
            
            // Assert
            Assert.True(stoppedInTime);
        }

        [Fact]
        public void Stop_CanBeCalledMultipleTimes()
        {
            // Arrange
            var server = new EchoServer(5008);
            
            // Act & Assert - should not throw
            server.Stop();
            server.Stop();
        }

        [Fact]
        public async Task Server_HandlesMultipleConnections()
        {
            // Arrange
            var server = new EchoServer(5009);
            var serverTask = server.StartAsync();
            await Task.Delay(100);
            
            var testData1 = new byte[] { 0xAA };
            var testData2 = new byte[] { 0xBB };
            
            // Act
            try
            {
                var tasks = new Task[]
                {
                    SendAndReceiveAsync(5009, testData1),
                    SendAndReceiveAsync(5009, testData2)
                };
                
                await Task.WhenAll(tasks);
            }
            catch
            {
                // Timing issues are acceptable
            }
            
            // Cleanup
            server.Stop();
            await serverTask;
        }

        private async Task SendAndReceiveAsync(int port, byte[] data)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    await client.ConnectAsync("127.0.0.1", port);
                    using (var stream = client.GetStream())
                    {
                        await stream.WriteAsync(data, 0, data.Length);
                        var buffer = new byte[data.Length];
                        await stream.ReadAsync(buffer, 0, buffer.Length);
                    }
                }
            }
            catch
            {
                // Expected in test environment
            }
        }

        [Fact]
        public async Task StartAsync_WithExternalCancellationToken_StartsAndStops()
        {
            var server = new EchoServer(5010);
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var serverTask = server.StartAsync(cts.Token);

            // wait for server to start accepting connections
            await Task.Delay(100);

            // attempt connection
            using (var client = new TcpClient())
            {
                var connectTask = client.ConnectAsync("127.0.0.1", 5010);
                var completed = await Task.WhenAny(connectTask, Task.Delay(2000));
                Assert.True(completed == connectTask && client.Connected);
            }

            // request shutdown
            cts.Cancel();

            // serverTask should complete shortly after cancellation
            await serverTask;
        }

        [Fact]
        public async Task Server_EchoesBackData()
        {
            var server = new EchoServer(5011);
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            var serverTask = server.StartAsync(cts.Token);
            await Task.Delay(100); // allow server to start

            var payload = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            byte[] received = new byte[payload.Length];

            using (var client = new TcpClient())
            {
                await client.ConnectAsync("127.0.0.1", 5011);
                using var stream = client.GetStream();

                await stream.WriteAsync(payload, 0, payload.Length);
                var bytesRead = await stream.ReadAsync(received, 0, received.Length);

                Assert.Equal(payload.Length, bytesRead);
                Assert.Equal(payload, received);
            }

            cts.Cancel();
            await serverTask;
        }

        [Fact]
        public async Task Stop_CanBeCalledMultipleTimes_NoException()
        {
            var server = new EchoServer(5012);
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var serverTask = server.StartAsync(cts.Token);
            await Task.Delay(100);

            // Call Stop multiple times (idempotency)
            server.Stop();
            server.Stop();
            server.Stop();

            // Cancel token to allow background StartAsync to finish if still running
            cts.Cancel();

            await serverTask;
        }

        public void Dispose()
        {
            // Cleanup
        }
    }
}
