using System.Net.Sockets;
using System.Text;

namespace NetSdrClientApp.Networking
{
    public class TcpClientWrapper : ITcpClient, IDisposable
    {
        private string _host;
        private int _port;
        private TcpClient? _tcpClient;
        private NetworkStream? _stream;
        private CancellationTokenSource? _cts;
        private bool _disposed;

        public bool Connected => _tcpClient != null && _tcpClient.Connected && _stream != null;

        public event EventHandler<byte[]>? MessageReceived;

        public TcpClientWrapper(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public void Connect()
        {
            ThrowIfDisposed();

            if (Connected)
            {
                Console.WriteLine($"Already connected to {_host}:{_port}");
                return;
            }

            _tcpClient = new TcpClient();

            try
            {
                _cts = new CancellationTokenSource();
                _tcpClient.Connect(_host, _port);
                _stream = _tcpClient.GetStream();
                Console.WriteLine($"Connected to {_host}:{_port}");
                // fire-and-forget listening task (keeps original behavior)
                _ = StartListeningAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect: {ex.Message}");
                // cleanup partially created resources on failure
                Cleanup();
            }
        }

        public void Disconnect()
        {
            if (Connected)
            {
                _cts?.Cancel();

                try
                {
                    _stream?.Close();
                    _tcpClient?.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while closing connection: {ex.Message}");
                }

                _cts = null;
                _tcpClient = null;
                _stream = null;
                Console.WriteLine("Disconnected.");
            }
            else
            {
                Console.WriteLine("No active connection to disconnect.");
            }
        }

        public async Task SendMessageAsync(byte[] data)
        {
            await SendCoreAsync(data).ConfigureAwait(false);
        }

        public async Task SendMessageAsync(string str)
        {
            var data = Encoding.UTF8.GetBytes(str);
            await SendCoreAsync(data).ConfigureAwait(false);
        }

        private async Task SendCoreAsync(byte[] data)
        {
            ThrowIfDisposed();

            if (Connected && _stream != null && _stream.CanWrite)
            {
                try
                {
                    // keep the original hex logging format
                    Console.WriteLine("Message sent: " + string.Join(" ", data.Select(b => Convert.ToString(b, 16))));
                    await _stream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send message: {ex.Message}");
                    throw;
                }
            }
            else
            {
                throw new InvalidOperationException("Not connected to a server.");
            }
        }

        private async Task StartListeningAsync()
        {
            ThrowIfDisposed();

            if (Connected && _stream != null && _stream.CanRead && _cts != null)
            {
                try
                {
                    Console.WriteLine($"Starting listening for incomming messages.");

                    while (!_cts.Token.IsCancellationRequested)
                    {
                        byte[] buffer = new byte[8194];

                        int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, _cts.Token).ConfigureAwait(false);
                        if (bytesRead > 0)
                        {
                            MessageReceived?.Invoke(this, buffer.AsSpan(0, bytesRead).ToArray());
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // expected when cancelling
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in listening loop: {ex.Message}");
                }
                finally
                {
                    Console.WriteLine("Listener stopped.");
                }
            }
            else
            {
                throw new InvalidOperationException("Not connected to a server.");
            }
        }

        private void Cleanup()
        {
            try
            {
                _stream?.Dispose();
            }
            catch { /* swallow */ }
            _stream = null;

            try
            {
                _tcpClient?.Close();
                _tcpClient?.Dispose();
            }
            catch { /* swallow */ }
            _tcpClient = null;

            try
            {
                _cts?.Cancel();
            }
            catch { /* swallow */ }

            try
            {
                _cts?.Dispose();
            }
            catch { /* swallow */ }
            _cts = null;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(TcpClientWrapper));
        }

        public void Dispose()
        {
            if (_disposed) return;

            Cleanup();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
