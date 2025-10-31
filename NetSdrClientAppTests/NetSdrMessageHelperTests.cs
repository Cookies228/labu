using NetSdrClientApp.Messages;
using NetSdrClientAppTests;

namespace NetSdrClientAppTests
{
    public class NetSdrMessageHelperTests
    {


        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void GetControlItemMessageTest()
        {
            //Arrange
            var type = NetSdrMessageHelper.MsgTypes.Ack;
            var code = NetSdrMessageHelper.ControlItemCodes.ReceiverState;
            int parametersLength = 7500;

            //Act
            byte[] msg = NetSdrMessageHelper.GetControlItemMessage(type, code, new byte[parametersLength]);

            var headerBytes = msg.Take(2);
            var codeBytes = msg.Skip(2).Take(2);
            var parametersBytes = msg.Skip(4);

            var num = BitConverter.ToUInt16(headerBytes.ToArray());
            var actualType = (NetSdrMessageHelper.MsgTypes)(num >> 13);
            var actualLength = num - ((int)actualType << 13);
            var actualCode = BitConverter.ToInt16(codeBytes.ToArray());

            //Assert
            Assert.That(headerBytes.Count(), Is.EqualTo(2));
            Assert.That(msg.Length, Is.EqualTo(actualLength));
            Assert.That(type, Is.EqualTo(actualType));

            Assert.That(actualCode, Is.EqualTo((short)code));

            Assert.That(parametersBytes.Count(), Is.EqualTo(parametersLength));
        }

        [Test]
        public void GetDataItemMessageTest()
        {
            //Arrange
            var type = NetSdrMessageHelper.MsgTypes.DataItem2;
            int parametersLength = 7500;

            //Act
            byte[] msg = NetSdrMessageHelper.GetDataItemMessage(type, new byte[parametersLength]);

            var headerBytes = msg.Take(2);
            var parametersBytes = msg.Skip(2);

            var num = BitConverter.ToUInt16(headerBytes.ToArray());
            var actualType = (NetSdrMessageHelper.MsgTypes)(num >> 13);
            var actualLength = num - ((int)actualType << 13);

            //Assert
            Assert.That(headerBytes.Count(), Is.EqualTo(2));
            Assert.That(msg.Length, Is.EqualTo(actualLength));
            Assert.That(type, Is.EqualTo(actualType));

            Assert.That(parametersBytes.Count(), Is.EqualTo(parametersLength));
        }

        //TODO: add more NetSdrMessageHelper tests
            [Test]
        public void GetControlItemMessage_WithEmptyParameters_ReturnsHeaderOnly()
        {
            // Arrange
            var type = NetSdrMessageHelper.MsgTypes.Ack;
            var code = NetSdrMessageHelper.ControlItemCodes.ReceiverState;

            // Act
            byte[] msg = NetSdrMessageHelper.GetControlItemMessage(type, code, Array.Empty<byte>());

            var headerBytes = msg.Take(2);
            var codeBytes = msg.Skip(2).Take(2);
            var parametersBytes = msg.Skip(4);

            var num = BitConverter.ToUInt16(headerBytes.ToArray());
            var actualType = (NetSdrMessageHelper.MsgTypes)(num >> 13);
            var actualLength = num - ((int)actualType << 13);
            var actualCode = BitConverter.ToInt16(codeBytes.ToArray());

            // Assert
            Assert.That(actualType, Is.EqualTo(type));
            Assert.That(actualCode, Is.EqualTo((short)code));
            Assert.That(parametersBytes.Count(), Is.EqualTo(0));
            Assert.That(msg.Length, Is.EqualTo(actualLength));
        }

        [Test]
        public void GetDataItemMessage_WithZeroLengthParameters_ReturnsHeaderOnly()
        {
            // Arrange
            var type = NetSdrMessageHelper.MsgTypes.DataItem1;

            // Act
            byte[] msg = NetSdrMessageHelper.GetDataItemMessage(type, Array.Empty<byte>());

            var num = BitConverter.ToUInt16(msg.Take(2).ToArray());
            var actualType = (NetSdrMessageHelper.MsgTypes)(num >> 13);
            var actualLength = num - ((int)actualType << 13);

            // Assert
            Assert.That(actualType, Is.EqualTo(type));
            Assert.That(actualLength, Is.EqualTo(2)); // лише хедер
            Assert.That(msg.Length, Is.EqualTo(actualLength));
        }

     

        [Test]
        public void GetDataItemMessage_ShouldContainExpectedLength()
        {
            // Arrange
            var payload = new byte[128];
            var type = NetSdrMessageHelper.MsgTypes.DataItem2;

            // Act
            var msg = NetSdrMessageHelper.GetDataItemMessage(type, payload);

            var num = BitConverter.ToUInt16(msg.Take(2).ToArray());
            var actualLength = num - ((int)type << 13);

            // Assert
            Assert.That(actualLength, Is.EqualTo(msg.Length));
            Assert.That(msg.Length, Is.EqualTo(payload.Length + 2));
        }

        [Test]
        public void GetControlItemMessage_ShouldThrow_WhenParametersTooLarge()
        {
            // Arrange
            var type = NetSdrMessageHelper.MsgTypes.Ack;
            var code = NetSdrMessageHelper.ControlItemCodes.ReceiverState;

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
            {
                NetSdrMessageHelper.GetControlItemMessage(type, code, new byte[ushort.MaxValue]);
            });
        }

        
    }
}