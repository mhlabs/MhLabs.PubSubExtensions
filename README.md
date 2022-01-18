# MhLabs.PubSubExtensions

Extended functionality of `AmazonSimpleNotificationServiceClient` and `AmazonSQSClient` that handles messages larger than 256KB up to 2GB.

## Known issues/limitations
* Only supports publishing to SNS. SQS will be a later feature
* Only supports SNS/SQS consumption through AWS Lambda using SNS or SQS as event source.

## Usage
The package consists of two namespaces - `Producer` and `Consumer`.

Nuget: `dotnet add package MhLabs.PubSubExtensions`

### Producer
To publish to an SNS topic, register the extended client to `Startup.cs`:
`services.AddSingleton<IAmazonSimpleNotificationService, ExtendedSimpleNotificationServiceClient>();`

To publish a message:
```
public ProducingService(IAmazonSimpleNotificationService snsClient)
{
    _snsClient = snsClient;
}

public async Task Publish(Model model)
{            
    var response = await _snsClient.PublishAsync(_topic, JsonConvert.SerializeObject(model));
}
```

In the above example `model` could be on any size between 1 byte up to 2GB. The underlying logic will calculate the size of the stream and, if over 256KB, upload to S3 and setting `MessageAttributes` of the S3 bucket and key. The consuming end will automatically download from S3 when appropriate.

### Consumer
This is primarily designed to be consumed in an AWS Lambda function. Generally you want to publish to an SQS topic, subscribe an SQS queue to it and consume the queue in a Lambda. This allows you to consume the queue at the parallellism of your choice and it also provides built in retry logic.

#### Creating the subscription
In the `Resources` section of `serverless.template`:
```
"Topic": {
  "Type": "AWS::SNS::Topic"
},
"Queue": {
  "Type": "AWS::SQS::Queue"
},
"QueuePolicy": {
  "Type": "AWS::SQS::QueuePolicy",
  "Properties": {
    "PolicyDocument": {
      "Version": "2012-10-17",
      "Id": "QueuePolicy",
      "Statement": [
        {
          "Sid": "Allow-SendMessage-To-Both-Queues-From-SNS-Topic",
          "Effect": "Allow",
          "Principal": "*",
          "Action": ["sqs:SendMessage"],
          "Resource": "*",
          "Condition": {
            "ArnEquals": {
              "aws:SourceArn": { "Ref": "Topic" }
            }
          }
        }
      ]
    },
    "Queues": [{ "Ref": "Queue" }]
  }
},
"Subscription": {
  "Type": "AWS::SNS::Subscription",
  "Properties": {
    "TopicArn": {
      "Ref": "Topic"
    },
    "Endpoint": {
      "Fn::GetAtt": ["Queue", "Arn"]
    },
    "Protocol": "sqs",
    "RawMessageDelivery": true
  }
}
``` 

Also add a consumer Lambda resource to consume the queue:
```
"SQSConsumer": {
  "Type": "AWS::Serverless::Function",
  "Properties": {
    "Handler": "example::example.Lambdas.SQSConsumer::Process",
    "Runtime": "dotnetcore2.1",
    "CodeUri": "bin/publish",
    "MemorySize": 256,
    "Timeout": 30,
    "Role": null,
    "Policies": ["AWSLambdaFullAccess", "AWSXrayWriteOnlyAccess"],
    "Tracing": "Active",
    "Environment": {
      "Variables": {
        "PubSub_S3Bucket": { "Ref": "Bucket" }
      }
    },
    "Events": {
      "SQS": {
        "Type": "SQS",
        "Properties": {
          "Queue": { "Fn::GetAtt": ["Queue", "Arn"] },
          "BatchSize": 5
        }
      }
    }
  }
}
```

#### Creating the consumer. 

For SNS, SQS and Kinesis consumers, the message extraction and deserialization is performed on the base class. This to avoid boiletplate in your lambda handler. 
```
public class SNSConsumer : SNSMessageMessageProcessorBase<Model>
{
    protected override async Task HandleEvent(IEnumerable<Model> items, ILambdaContext context)
    {
        // Iterate through items
    }
}
```
For SQS you have the option of using partial batch failures. If you do not want to use them there is no need to return any data. See example below.
```
public class SQSConsumerWithoutPartialBatchFailure : SQSMessageMessageProcessorBase<Model>
{
    protected override async Task<SQSResponse> HandleEvent(IEnumerable<Model> items, ILambdaContext context)
    {
        // Iterate through items
        
        // No need to return anything 
        return default;
    }
}
```

When using partial batch failures you will need to return the message ids of the failed message in the response from the lambda.
```
public class SQSConsumerWithPartialBatchFailure : SQSMessageProcessorBase<Model>
{
    // This tells the base class to use partial batch failure on exceptions as well
    protected override Task<HandleErrorResult> HandleError(SQSEvent ev, ILambdaContext context, Exception exception)
    {
        return Task.FromResult(HandleErrorResult.ErrorHandledByConsumer);
    }

    protected override async Task<SQSResponse> HandleEvent(IEnumerable<SQSMessageEnvelope<Model>> items, ILambdaContext context)
    {
        var failures = new List<BatchItemFailure>();
        foreach (var orderEvent in items)
        {
            try
            {
                // Do something with event
            }
            catch (System.Exception e)
            {
                // Add any failed messages to the batch failures
                failures.Add(new BatchItemFailure
                {
                    ItemIdentifier = orderEvent.MessageId
                });
            }
        }
        return new SQSResponse
        {
            BatchItemFailures = failures
        };
    }
}
```

#### Message extraction
If you want to perform more advanced message extraction, such at populate your model with values from MessageAttributes or so, you will have to create your own message extractor.
```
public class MyExtractor : IMessageExtractor
{
    public Type ExtractorForType => typeof(SQSEvent);

    public async Task<IEnumerable<TMessageType>> ExtractEventBody<TEventType, TMessageType>(TEventType ev)
    {
        var sqsEvent = ev as SQSEvent;
        return await Task.Run(()=>sqsEvent.Records.Select(p => JsonConvert.DeserializeObject<TMessageType>(p.MessageAttributes["SomeAttributeWithJsonBody"].StringValue)));
    }
}
```

and register it with the base:

```
public class SQSConsumer : MessageProcessorBase<SQSEvent, Model>
{
    public SQSConsumer() {
        base.RegisterExtractor(new MyExtractor());
    }
    
    protected override async Task HandleEvent(IEnumerable<Model> items, ILambdaContext context)
    {
        // Iterate through items
    }
}
```
