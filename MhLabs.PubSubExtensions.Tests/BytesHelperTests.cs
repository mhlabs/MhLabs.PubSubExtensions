using System;
using Amazon.SimpleNotificationService.Model;
using Xunit;
using MhLabs.PubSubExtensions.Producer;
using Xunit.Abstractions;

namespace MhLabs.PubSubExtensions.Tests
{
    public class BytesHelperTests
    {
        private readonly ITestOutputHelper output;

        public BytesHelperTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void MessageSizeTestSNS_Empty()
        {
            var request = new PublishRequest();
            bool tooLarge = BytesHelper.TooLarge(request, 10);
            Assert.False(tooLarge);
        }

        [Fact]
        public void MessageSizeTestSNS_FewBytesInBody()
        {
            var request = new PublishRequest();
            request.Message = "1234567890";
            bool tooLarge = BytesHelper.TooLarge(request, 10);
            Assert.False(tooLarge);
        }

        [Fact]
        public void MessageSizeTestSNS_TooManyBytesInBody()
        {
            var request = new PublishRequest();
            request.Message = "1234567890a";
            bool tooLarge = BytesHelper.TooLarge(request, 10);
            Assert.True(tooLarge);
        }

        [Fact]
        public void MessageSizeTestSNS_ValidMessageAttributes()
        {
            var request = new PublishRequest();
            request.Message = null;
            request.MessageAttributes.Add("12345", new MessageAttributeValue { DataType = "String", StringValue = "67890" });
            bool tooLarge = BytesHelper.TooLarge(request, 10);
            Assert.False(tooLarge);
        }

        [Fact]
        public void MessageSizeTestSNS_TooLargeMessageAttributes()
        {
            var request = new PublishRequest();
            request.Message = null;
            request.MessageAttributes.Add("12345", new MessageAttributeValue { DataType = "String", StringValue = "67890a" });
            bool tooLarge = BytesHelper.TooLarge(request, 10);
            Assert.True(tooLarge);
        }
    }
}
