using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.Lambda.KinesisEvents;
using MhLabs.PubSubExtensions.Model;
using Newtonsoft.Json;

namespace MhLabs.PubSubExtensions.Consumer.Extractors
{
    public class DynamoDBMutationExtractor : IMessageExtractor
    {
        public Type ExtractorForType => typeof(DynamoDBEvent);
        private DynamoDBContext _context;

        public DynamoDBMutationExtractor()
        {
            _context = new DynamoDBContext(new AmazonDynamoDBClient());
        }
        public async Task<IEnumerable<TMessageType>> ExtractEventBody<TEventType, TMessageType>(TEventType ev) where TMessageType : class, new()
        {
            var dynamoEvent = ev as DynamoDBEvent;
            await Task.CompletedTask;
            return dynamoEvent.Records.Select(p =>
            {
                var newDoc = Document.FromAttributeMap(p.Dynamodb.NewImage);
                var oldDoc = Document.FromAttributeMap(p.Dynamodb.OldImage);
                var update = new MutationModel<TMessageType>();
                update.OldImage = _context.FromDocument<TMessageType>(oldDoc);
                update.NewImage = _context.FromDocument<TMessageType>(newDoc);
                return update as TMessageType;
            });
        }
    }
}