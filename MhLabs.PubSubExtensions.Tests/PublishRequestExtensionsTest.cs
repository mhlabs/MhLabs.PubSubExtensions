using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.SimpleNotificationService.Model;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using MhLabs.PubSubExtensions.Producer;
using Moq;
using Xunit;

namespace MhLabs.PubSubExtensions.Tests
{
    public class AmazonStepFunctionsClientAdapterTests
    {
        private readonly AmazonStepFunctionsClientAdapter _client;
        private readonly PublishRequest _request;
        private readonly Mock<IAmazonStepFunctions> _stepFunctions;
        private const string StateMachineTestArn = "test-machine-arn";

        public AmazonStepFunctionsClientAdapterTests()
        {
            _stepFunctions = new Mock<IAmazonStepFunctions>();
            _request = new PublishRequest();
            _client = new AmazonStepFunctionsClientAdapter(_stepFunctions.Object);
        }

        [Fact]
        public async Task Should_Throw_If_Execution_Name_Already_Exists_And_SuppressExecutionAlreadyExistsException_Is_Not_Set()
        {
            // Arrange
            _stepFunctions.Setup(x => x.StartExecutionAsync(It.IsAny<StartExecutionRequest>(), CancellationToken.None))
                .ThrowsAsync(new ExecutionAlreadyExistsException("..."));

            // Act && Assert
            await Assert.ThrowsAsync<ExecutionAlreadyExistsException>(async () => await _client.PublishAsync(_request, StateMachineTestArn));
        }

        [Fact]
        public async Task Should_Throw_If_Execution_Name_Already_Exists_And_SuppressExecutionAlreadyExistsException_Is_Set_To_False()
        {
            // Arrange
            _request.SuppressExecutionAlreadyExistsException(false);
            _stepFunctions.Setup(x => x.StartExecutionAsync(It.IsAny<StartExecutionRequest>(), CancellationToken.None))
                .ThrowsAsync(new ExecutionAlreadyExistsException("..."));

            // Act && Assert
            await Assert.ThrowsAsync<ExecutionAlreadyExistsException>(async () => await _client.PublishAsync(_request, StateMachineTestArn));
        }

        [Fact]
        public async Task Should_Not_Throw_If_Execution_Name_Already_Exists_And_SuppressExecutionAlreadyExistsException_Is_True()
        {
            // Arrange
            _request.SuppressExecutionAlreadyExistsException(true);
            _stepFunctions.Setup(x => x.StartExecutionAsync(It.IsAny<StartExecutionRequest>(), CancellationToken.None))
                .ThrowsAsync(new ExecutionAlreadyExistsException("..."));

            // Act
            var response = await _client.PublishAsync(_request, StateMachineTestArn);

            // Assert
            Assert.Equal(response.HttpStatusCode, HttpStatusCode.OK);
        }
    }
}