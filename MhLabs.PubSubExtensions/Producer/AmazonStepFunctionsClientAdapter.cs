using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService.Model;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using Newtonsoft.Json;

namespace MhLabs.PubSubExtensions.Producer
{
    public class AmazonStepFunctionsClientAdapter : IAmazonStepFunctionsClientAdapter
    {
        private readonly IAmazonStepFunctions _stepFunctions;

        public AmazonStepFunctionsClientAdapter(IAmazonStepFunctions stepFunctions)
        {
            _stepFunctions = stepFunctions;
        }

        public async Task<PublishResponse> PublishAsync(PublishRequest request, string stateMachineArn, CancellationToken cancellationToken = default(CancellationToken))
        {
            var executionRequest = new StartExecutionRequest
            {
                Input = JsonConvert.SerializeObject(request),
                Name = request.GetExecutionName(),
                StateMachineArn = stateMachineArn
            };

            await StartExecutionAsync(request, executionRequest, cancellationToken);

            return new PublishResponse
            {
                HttpStatusCode = HttpStatusCode.OK
            };
        }

        private async Task StartExecutionAsync(PublishRequest request, StartExecutionRequest executionRequest, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request.ShouldSuppressExecutionAlreadyExistsException())
            {
                try
                {
                    await _stepFunctions.StartExecutionAsync(executionRequest, cancellationToken);
                }
                catch (ExecutionAlreadyExistsException ex)
                {
                    Console.WriteLine($"ExecutionAlreadyExistsException error for {request.GetExecutionName()}. Exception was: {ex.Message}");
                }
            }
            else
            {
                await _stepFunctions.StartExecutionAsync(executionRequest, cancellationToken);
            }
        }
    }
}