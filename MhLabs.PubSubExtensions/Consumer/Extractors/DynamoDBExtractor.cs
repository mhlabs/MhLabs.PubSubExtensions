using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IDynamoDBContext _context;

        public DynamoDBExtractor(IAmazonDynamoDB dynamoDbService)
        {
            _context = new DynamoDBContext(dynamoDbService);
        }

        public Type ExtractorForType => typeof(DynamoDBEvent);
        public async Task<IEnumerable<TMessageType>> ExtractEventBody<TEventType, TMessageType>(TEventType ev) where TMessageType : class, new()
        {
            var dynamoEvent = ev as DynamoDBEvent;
            await Task.CompletedTask;
            return dynamoEvent.Records.Select(p =>
            {
                var newDoc = Document.FromAttributeMap(p.Dynamodb.NewImage);
                var oldDoc = Document.FromAttributeMap(p.Dynamodb.OldImage);

                var update = new MutationModel<TMessageType>
                {
                    OldImage = _context.FromDocument<TMessageType>(oldDoc),
                    NewImage = _context.FromDocument<TMessageType>(newDoc)
                };

                return update as TMessageType;
            });
        }
    }
}
