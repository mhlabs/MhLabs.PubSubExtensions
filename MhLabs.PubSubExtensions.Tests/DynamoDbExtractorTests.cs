using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.DynamoDBEvents;
using MhLabs.PubSubExtensions.Consumer.Extractors;
using MhLabs.PubSubExtensions.Model;
using Moq;
using Xunit;

namespace MhLabs.PubSubExtensions.Tests
{
    public class EntityToTest
    {
        public string Name { get; set; }
    }

    public class DynamoDbExtractorTests
    {
        private readonly Mock<IAmazonDynamoDB> _dynamoDbClient;
        private readonly DynamoDBExtractor _dynamoDbExtractor;

        public DynamoDbExtractorTests()
        {
            _dynamoDbClient = new Mock<IAmazonDynamoDB>();
            _dynamoDbExtractor = new DynamoDBExtractor(_dynamoDbClient.Object);
        }

        public class ExtractEventBody : DynamoDbExtractorTests
        {
            [Fact]
            public async Task should()
            {
                var ev = new DynamoDBEvent
                {
                    Records = new List<DynamoDBEvent.DynamodbStreamRecord>()
                    {
                        new DynamoDBEvent.DynamodbStreamRecord
                        {
                            Dynamodb = new StreamRecord
                            {NewImage =  new Dictionary<string, AttributeValue>
                            {
                                {"Name", new AttributeValue("stefan") }
                            } }
                        }
                    }
                };

               var result = await _dynamoDbExtractor.ExtractEventBody<DynamoDBEvent, MutationModel<EntityToTest>>(ev);

               Assert.True(true);
            }
        }
    }
}
