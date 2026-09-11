using NUnit.Framework;
using System.Net;

namespace DnsChangerLite.Tests
{
    [TestFixture]
    public class DnsServiceTests
    {
        private DnsService _service;

        [SetUp]
        public void SetUp()
        {
            _service = new DnsService();
        }

        [Test]
        public void GetAdapters_ReturnsList()
        {
            // Should not throw; may be empty on Linux CI
            var adapters = _service.GetAdapters();
            Assert.IsNotNull(adapters);
        }

        [Test]
        public void SetDns_NullAdapter_ReturnsFailure()
        {
            var result = _service.SetDns(null, "1.1.1.1", "1.0.0.1");
            Assert.IsFalse(result.Success);
            Assert.That(result.Message, Does.Contain("adapter"));
        }

        [Test]
        public void SetDns_EmptyAdapter_ReturnsFailure()
        {
            var result = _service.SetDns("", "1.1.1.1", "1.0.0.1");
            Assert.IsFalse(result.Success);
        }

        [Test]
        public void ResetDns_NullAdapter_ReturnsFailure()
        {
            var result = _service.ResetDns(null);
            Assert.IsFalse(result.Success);
        }

        [Test]
        public void FlushDns_ReturnsResult()
        {
            // On Windows with admin, should succeed; on Linux, returns error
            var result = _service.FlushDns();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Message);
        }
    }

    [TestFixture]
    public class NetworkAdapterTests
    {
        [Test]
        public void DisplayName_WithDescription_ReturnsCombined()
        {
            var adapter = new NetworkAdapter { Name = "Wi-Fi", Description = "Intel AX201" };
            Assert.AreEqual("Wi-Fi — Intel AX201", adapter.DisplayName);
        }

        [Test]
        public void DisplayName_EmptyDescription_ReturnsName()
        {
            var adapter = new NetworkAdapter { Name = "Ethernet", Description = "" };
            Assert.AreEqual("Ethernet", adapter.DisplayName);
        }

        [Test]
        public void DisplayName_NullDescription_ReturnsName()
        {
            var adapter = new NetworkAdapter { Name = "Wi-Fi", Description = null };
            Assert.AreEqual("Wi-Fi", adapter.DisplayName);
        }
    }

    [TestFixture]
    public class DnsResultTests
    {
        [Test]
        public void SuccessResult_HasCorrectState()
        {
            var result = new DnsResult { Success = true, Message = "OK" };
            Assert.IsTrue(result.Success);
            Assert.AreEqual("OK", result.Message);
        }

        [Test]
        public void FailureResult_HasCorrectState()
        {
            var result = new DnsResult { Success = false, Message = "Error" };
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Error", result.Message);
        }
    }
}
