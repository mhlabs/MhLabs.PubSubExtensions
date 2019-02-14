using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.DynamoDBEvents;
using MhLabs.PubSubExtensions.Model;

namespace MhLabs.PubSubExtensions.Consumer.Extractors
{
    public class DynamoDBExtractor : IMessageExtractor
    {
        private readonly IAmazonDynamoDB _dynamoDb;

        public DynamoDBExtractor(IAmazonDynamoDB dynamoDb)
        {
            _dynamoDb = dynamoDb;
        }

        public Type ExtractorForType => typeof(DynamoDBEvent);
        public async Task<IEnumerable<TMessageType>> ExtractEventBody<TEventType, TMessageType>(TEventType ev) where TMessageType : class, new()
        {
            var dynamoEvent = ev as DynamoDBEvent;
            await Task.CompletedTask;

            using (var context = new DynamoDBContext(_dynamoDb))
            {
                return dynamoEvent.Records.Select(record =>
                {
                    var newDoc = Document.FromAttributeMap(record.Dynamodb.NewImage);
                    var oldDoc = Document.FromAttributeMap(record.Dynamodb.OldImage);

                    var update = new MutationModel<TMessageType>
                    {
                        OldImage = context.FromDocument<TMessageType>(oldDoc),
                        NewImage = context.FromDocument<TMessageType>(newDoc)
                    };

                    return update as TMessageType;
                });
            }
        }
    }
}
