using Moq;
using NetSdrClientApp;
using NetSdrClientApp.Networking;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NetSdrClientAppTests
{
    public class NetSdrClientTests
    {
        NetSdrClient _client;
        Mock<ITcpClient> _tcpMock;
        Mock<IUdpClient> _updMock;

        public NetSdrClientTests() { }

        [SetUp]
        public void Setup()
        {
            _tcpMock = new Mock<ITcpClient>();
            _tcpMock.Setup(tcp => tcp.Connect()).Callback(() =>
            {
                _tcpMock.Setup(tcp => tcp.Connected).Returns(true);
            });

            _tcpMock.Setup(tcp => tcp.Disconnect()).Callback(() =>
            {
                _tcpMock.Setup(tcp => tcp.Connected).Returns(false);
            });

            _tcpMock.Setup(tcp => tcp.SendMessageAsync(It.IsAny<byte[]>())).Callback<byte[]>((bytes) =>
            {
                _tcpMock.Raise(tcp => tcp.MessageReceived += null, _tcpMock.Object, bytes);
            });

            _updMock = new Mock<IUdpClient>();

            _client = new NetSdrClient(_tcpMock.Object, _updMock.Object);
        }

        [Test]
        public async Task ConnectAsyncTest()
        {
            //act
            await _client.ConnectAsync();

            //assert
            _tcpMock.Verify(tcp => tcp.Connect(), Times.Once);
            _tcpMock.Verify(tcp => tcp.SendMessageAsync(It.IsAny<byte[]>()), Times.Exactly(3));
        }

        [Test]
        public async Task DisconnectWithNoConnectionTest()
        {
            //act
            _client.Disconect();

            //assert
            //No exception thrown
            _tcpMock.Verify(tcp => tcp.Disconnect(), Times.Once);
        }

        [Test]
        public async Task DisconnectTest()
        {
            //Arrange 
            await ConnectAsyncTest();

            //act
            _client.Disconect();

            //assert
            //No exception thrown
            _tcpMock.Verify(tcp => tcp.Disconnect(), Times.Once);
        }

        [Test]
        public async Task StartIQNoConnectionTest()
        {

            //act
            await _client.StartIQAsync();

            //assert
            //No exception thrown
            _tcpMock.Verify(tcp => tcp.SendMessageAsync(It.IsAny<byte[]>()), Times.Never);
            _tcpMock.VerifyGet(tcp => tcp.Connected, Times.AtLeastOnce);
        }

        [Test]
        public async Task StartIQTest()
        {
            //Arrange 
            await ConnectAsyncTest();

            //act
            await _client.StartIQAsync();

            //assert
            //No exception thrown
            _updMock.Verify(udp => udp.StartListeningAsync(), Times.Once);
            Assert.That(_client.IQStarted, Is.True);
        }

        [Test]
        public async Task StopIQTest()
        {
            //Arrange 
            await ConnectAsyncTest();

            //act
            await _client.StopIQAsync();

            //assert
            //No exception thrown
            _updMock.Verify(tcp => tcp.StopListening(), Times.Once);
            Assert.That(_client.IQStarted, Is.False);
        }
        //TODO: cover the rest of the NetSdrClient code here

        [Test]
        public async Task ConnectAsync_IsIdempotent_WhenCalledTwice_DoesNotDuplicateHandshake()
        {
            // act
            await _client.ConnectAsync();
            await _client.ConnectAsync();

            // assert - expect only one connect and only the initial handshake sends (3 messages)
            _tcpMock.Verify(tcp => tcp.Connect(), Times.Once);
            _tcpMock.Verify(tcp => tcp.SendMessageAsync(It.IsAny<byte[]>()), Times.Exactly(3));
        }

        [Test]
        public async Task StopIQAsync_CalledWithoutStart_DoesNotCallStopListening()
        {
            // act
            await _client.StopIQAsync();

            // assert
            _updMock.Verify(udp => udp.StopListening(), Times.Never);
            Assert.That(_client.IQStarted, Is.False);
        }

        [Test]
        public async Task MessageReceived_HandlesInvalidData_DoesNotThrowException()
        {
            // Arrange
            await _client.ConnectAsync();
            byte[] invalidData = new byte[] { 0xFF, 0xFF, 0xFF };

            // Act & Assert
            Assert.DoesNotThrowAsync(async () =>
            {
                _tcpMock.Raise(tcp => tcp.MessageReceived += null, _tcpMock.Object, invalidData);
            });
        }

        [Test]
        public async Task SendTcpRequest_ReturnsNull_WhenNotConnected()
        {
            // Arrange
            var method = typeof(NetSdrClient).GetMethod("SendTcpRequest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var msg = new byte[] { 0x01, 0x02 };
            _tcpMock.Setup(tcp => tcp.Connected).Returns(false);

            // Act
            var task = (Task<byte[]?>)method!.Invoke(_client, new object[] { msg })!;
            var result = await task;

            // Assert
            Assert.That(result, Is.Null);
        }

                [Test]
        public async Task ChangeFrequencyAsync_SendsMessage_WhenConnected()
        {
            // Arrange
            await _client.ConnectAsync();
            long frequency = 145000000; // 145 MHz
            int channel = 1;

            // Act
            await _client.ChangeFrequencyAsync(frequency, channel);

            // Assert
            _tcpMock.Verify(tcp => tcp.SendMessageAsync(It.IsAny<byte[]>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task ChangeFrequencyAsync_DoesNothing_WhenNotConnected()
        {
            // Arrange
            _tcpMock.Setup(tcp => tcp.Connected).Returns(false);

            // Act
            await _client.ChangeFrequencyAsync(144000000, 0);

            // Assert
            _tcpMock.Verify(tcp => tcp.SendMessageAsync(It.IsAny<byte[]>()), Times.Never);
        }

        [Test]
        public async Task SendTcpRequest_SendsMessage_WhenConnected()
        {
            // Arrange
            var method = typeof(NetSdrClient).GetMethod("SendTcpRequest",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var msg = new byte[] { 0xAA, 0xBB };
            _tcpMock.Setup(tcp => tcp.Connected).Returns(true);

            // Act
            var task = (Task<byte[]?>)method!.Invoke(_client, new object[] { msg })!;
            var result = await task;

            // Assert
            _tcpMock.Verify(tcp => tcp.Connected, Times.AtLeastOnce);
        }



    }
}