using NetArchTest.Rules;
using Xunit;
using NetSdrClientApp; // для доступу до типів у клієнті
using NetSdrClientApp.Networking;
using NetSdrClientApp.Messages;
using Assert = Xunit.Assert;

namespace NetSdrClientAppTests
{
    public class ArchitectureTests
    {
        [Fact]
        public void NetSdrClientApp_Should_Not_Depend_On_EchoTcpServer()
        {
            var result = Types.InAssembly(typeof(NetSdrClient).Assembly)
                .ShouldNot()
                .HaveDependencyOn("EchoTcpServer")
                .GetResult();

            Assert.True(result.IsSuccessful, "NetSdrClientApp should not depend directly on EchoTcpServer.");
        }

        [Fact]
        public void Messages_Should_Not_Depend_On_Networking()
        {
            var result = Types.InAssembly(typeof(NetSdrClient).Assembly)
                .ShouldNot()
                .HaveDependencyOn("NetSdrClientApp.Networking")
                .GetResult();

            Assert.True(result.IsSuccessful, "Messages should not depend on Networking layer.");
        }

        [Fact]
        public void Tests_Should_Not_Depend_On_EchoTcpServer()
        {
            var result = Types.InAssembly(typeof(ArchitectureTests).Assembly)
                .ShouldNot()
                .HaveDependencyOn("EchoTcpServer")
                .GetResult();

            Assert.True(result.IsSuccessful, "Test project should not depend on EchoTcpServer directly.");
        }
    }
}
