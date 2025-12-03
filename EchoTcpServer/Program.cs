using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// This program was designed for test purposes only
/// Not for a review
/// </summary>
public class EchoServer
{
    private readonly int _port;
    private TcpListener _listener;
    private CancellationTokenSource _cancellationTokenSource;


    public EchoServer(int port)
    {
        _port = port;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public async Task StartAsync()
    {
        // ensure we have a non-cancelled CTS for internal usage
        if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        await StartAsyncInternal(_cancellationTokenSource.Token).ConfigureAwait(false);
    }

    // New overload for tests: allows caller to control server lifetime via CancellationToken
    public async Task StartAsync(CancellationToken externalToken)
    {
        await StartAsyncInternal(externalToken).ConfigureAwait(false);
    }

    // Extracted internal implementation of the server loop — same behavior as before
    private async Task StartAsyncInternal(CancellationToken token)
    {
        _listener = new TcpListener(IPAddress.Any, _port);
        _listener.Start();
        Console.WriteLine($"Server started on port {_port}.");

        while (!token.IsCancellationRequested)
        {
            try
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                Console.WriteLine("Client connected.");

                _ = Task.Run(() => HandleClientAsync(client, token));
            }
            catch (ObjectDisposedException)
            {
                // Listener has been closed
                break;
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                Console.WriteLine($"Error in accept loop: {ex.Message}");
                break;
            }
        }

        Console.WriteLine("Server shutdown.");
    }

    private static async Task HandleClientAsync(TcpClient client, CancellationToken token)
    {
        using (NetworkStream stream = client.GetStream())
        {
            try
            {
                byte[] buffer = new byte[8192];
                int bytesRead;

                while (!token.IsCancellationRequested && (bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                {
                    // Echo back the received message
                    await stream.WriteAsync(buffer, 0, bytesRead, token);
                    Console.WriteLine($"Echoed {bytesRead} bytes to the client.");
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                client.Close();
                Console.WriteLine("Client disconnected.");
            }
        }
    }

    public void Stop()
    {
        try
        {
            // cancel any internal token source
            try
            {
                _cancellationTokenSource?.Cancel();
            }
            catch { /* swallow */ }

            try
            {
                _listener?.Stop();
            }
            catch { /* swallow */ }

            // dispose and nullify to allow future restarts
            try
            {
                _cancellationTokenSource?.Dispose();
            }
            catch { /* swallow */ }

            _cancellationTokenSource = new CancellationTokenSource();

            Console.WriteLine("Server stopped.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while stopping server: {ex.Message}");
        }
    }

    public static async Task Main(string[] args)
    {
        EchoServer server = new EchoServer(5000);

        // Start the server in a separate task
        _ = Task.Run(() => server.StartAsync());

        string host = "127.0.0.1"; // Target IP
        int port = 60000;          // Target Port
        int intervalMilliseconds = 5000; // Send every 3 seconds

        using (var sender = new UdpTimedSender(host, port))
        {
            Console.WriteLine("Press any key to stop sending...");
            sender.StartSending(intervalMilliseconds);

            Console.WriteLine("Press 'q' to quit...");
            while (Console.ReadKey(intercept: true).Key != ConsoleKey.Q)
            {
                // Just wait until 'q' is pressed
            }

            sender.StopSending();
            server.Stop();
            Console.WriteLine("Sender stopped.");
        }
    }
}


public class UdpTimedSender : IDisposable
{
    private readonly string _host;
    private readonly int _port;
    private readonly UdpClient _udpClient;
    private Timer _timer;

    public UdpTimedSender(string host, int port)
    {
        _host = host;
        _port = port;
        _udpClient = new UdpClient();
    }

    public void StartSending(int intervalMilliseconds)
    {
        if (_timer != null)
            throw new InvalidOperationException("Sender is already running.");

        _timer = new Timer(SendMessageCallback, null, 0, intervalMilliseconds);
    }

    ushort i = 0;

    private void SendMessageCallback(object state)
    {
        try
        {
            //dummy data
            Random rnd = new Random();
            byte[] samples = new byte[1024];
            rnd.NextBytes(samples);
            i++;

            byte[] msg = (new byte[] { 0x04, 0x84 }).Concat(BitConverter.GetBytes(i)).Concat(samples).ToArray();
            var endpoint = new IPEndPoint(IPAddress.Parse(_host), _port);

            _udpClient.Send(msg, msg.Length, endpoint);
            Console.WriteLine($"Message sent to {_host}:{_port} ");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
    }

    public void StopSending()
    {
        _timer?.Dispose();
        _timer = null;
    }

    public void Dispose()
    {
        StopSending();
        _udpClient.Dispose();
    }
}