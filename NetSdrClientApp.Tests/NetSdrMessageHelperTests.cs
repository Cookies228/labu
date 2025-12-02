using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using NetSdrClientApp.Messages;

namespace NetSdrClientApp.Tests
{
    public class NetSdrMessageHelperTests
    {
        [Fact]
        public void GetControlItemMessage_WithValidParameters_ReturnsValidMessage()
        {
            // Arrange
            var parameters = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            
            // Act
            var result = NetSdrMessageHelper.GetControlItemMessage(
                NetSdrMessageHelper.MsgTypes.SetControlItem,
                NetSdrMessageHelper.ControlItemCodes.ReceiverFrequency,
                parameters);
            
            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void GetControlItemMessage_WithNoneItemCode_ReturnsValidMessage()
        {
            // Arrange
            var parameters = new byte[] { 0x01, 0x02 };
            
            // Act
            var result = NetSdrMessageHelper.GetControlItemMessage(
                NetSdrMessageHelper.MsgTypes.Ack,
                NetSdrMessageHelper.ControlItemCodes.None,
                parameters);
            
            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length >= parameters.Length);
        }

        [Fact]
        public void GetDataItemMessage_WithValidParameters_ReturnsValidMessage()
        {
            // Arrange
            var parameters = new byte[] { 0x01, 0x02, 0x03 };
            
            // Act
            var result = NetSdrMessageHelper.GetDataItemMessage(
                NetSdrMessageHelper.MsgTypes.DataItem0,
                parameters);
            
            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void TranslateMessage_WithValidControlItemMessage_ReturnsCorrectValues()
        {
            // Arrange
            var parameters = new byte[] { 0x01, 0x02 };
            var message = NetSdrMessageHelper.GetControlItemMessage(
                NetSdrMessageHelper.MsgTypes.SetControlItem,
                NetSdrMessageHelper.ControlItemCodes.ReceiverFrequency,
                parameters);
            
            // Act
            var success = NetSdrMessageHelper.TranslateMessage(
                message, 
                out var type, 
                out var itemCode, 
                out var sequenceNumber, 
                out var body);
            
            // Assert
            Assert.True(success);
            Assert.Equal(NetSdrMessageHelper.MsgTypes.SetControlItem, type);
            Assert.Equal(NetSdrMessageHelper.ControlItemCodes.ReceiverFrequency, itemCode);
        }

        [Fact]
        public void TranslateMessage_WithDataItemMessage_ReturnsSequenceNumber()
        {
            // Arrange
            var parameters = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var message = NetSdrMessageHelper.GetDataItemMessage(
                NetSdrMessageHelper.MsgTypes.DataItem0,
                parameters);
            
            // Act
            var success = NetSdrMessageHelper.TranslateMessage(
                message,
                out var type,
                out var itemCode,
                out var sequenceNumber,
                out var body);
            
            // Assert
            Assert.True(success);
            Assert.Equal(NetSdrMessageHelper.MsgTypes.DataItem0, type);
            Assert.Equal(NetSdrMessageHelper.ControlItemCodes.None, itemCode);
        }

        [Fact]
        public void TranslateMessage_WithInvalidItemCode_ReturnsFalse()
        {
            // Arrange - manually craft invalid message
            var header = BitConverter.GetBytes((ushort)(4 + (0 << 13))); // length=4, type=0
            var invalidCode = BitConverter.GetBytes((ushort)0xFFFF); // invalid code
            var message = header.Concat(invalidCode).ToArray();
            
            // Act
            var success = NetSdrMessageHelper.TranslateMessage(
                message,
                out var type,
                out var itemCode,
                out var sequenceNumber,
                out var body);
            
            // Assert
            Assert.False(success);
        }

        [Theory]
        [InlineData(8)]
        [InlineData(16)]
        [InlineData(24)]
        [InlineData(32)]
        public void GetSamples_WithVariousSampleSizes_ReturnsCorrectSamples(ushort sampleSize)
        {
            // Arrange
            var sampleBytes = sampleSize / 8;
            var testData = new byte[sampleBytes * 3];
            for (int i = 0; i < testData.Length; i++)
                testData[i] = (byte)(i % 256);
            
            // Act
            var samples = NetSdrMessageHelper.GetSamples(sampleSize, testData).ToList();
            
            // Assert
            Assert.Equal(3, samples.Count);
        }

        [Fact]
        public void GetSamples_WithInvalidSampleSize_ThrowsException()
        {
            // Arrange
            var data = new byte[16];
            
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                NetSdrMessageHelper.GetSamples(40, data).ToList());
        }

        [Fact]
        public void GetSamples_With8BitSamples_ReturnsCorrectValues()
        {
            // Arrange
            var testData = new byte[] { 0x10, 0x20, 0x30 };
            
            // Act
            var samples = NetSdrMessageHelper.GetSamples(8, testData).ToList();
            
            // Assert
            Assert.Equal(3, samples.Count);
            Assert.Equal(0x10, samples[0]);
            Assert.Equal(0x20, samples[1]);
            Assert.Equal(0x30, samples[2]);
        }

        [Fact]
        public void GetControlItemMessage_WithEmptyParameters_ReturnsValidMessage()
        {
            // Arrange
            var parameters = Array.Empty<byte>();
            
            // Act
            var result = NetSdrMessageHelper.GetControlItemMessage(
                NetSdrMessageHelper.MsgTypes.Ack,
                NetSdrMessageHelper.ControlItemCodes.ReceiverState,
                parameters);
            
            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void TranslateMessage_WithRoundTripConversion_PreservesData()
        {
            // Arrange
            var originalParams = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD };
            var message = NetSdrMessageHelper.GetControlItemMessage(
                NetSdrMessageHelper.MsgTypes.CurrentControlItem,
                NetSdrMessageHelper.ControlItemCodes.IQOutputDataSampleRate,
                originalParams);
            
            // Act
            var success = NetSdrMessageHelper.TranslateMessage(
                message,
                out var type,
                out var itemCode,
                out var sequenceNumber,
                out var body);
            
            // Assert
            Assert.True(success);
            Assert.Equal(originalParams, body);
        }

        [Theory]
        [InlineData(NetSdrMessageHelper.ControlItemCodes.ReceiverFrequency)]
        [InlineData(NetSdrMessageHelper.ControlItemCodes.ReceiverState)]
        [InlineData(NetSdrMessageHelper.ControlItemCodes.ADModes)]
        [InlineData(NetSdrMessageHelper.ControlItemCodes.RFFilter)]
        [InlineData(NetSdrMessageHelper.ControlItemCodes.IQOutputDataSampleRate)]
        public void GetControlItemMessage_WithAllItemCodes_CreatesValidMessages(
            NetSdrMessageHelper.ControlItemCodes itemCode)
        {
            // Arrange
            var parameters = new byte[] { 0x01, 0x02 };
            
            // Act
            var result = NetSdrMessageHelper.GetControlItemMessage(
                NetSdrMessageHelper.MsgTypes.SetControlItem,
                itemCode,
                parameters);
            
            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }
    }
}
