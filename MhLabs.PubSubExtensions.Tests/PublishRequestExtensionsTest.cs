using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.SimpleNotificationService.Model;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using MhLabs.PubSubExtensions.Model;
using MhLabs.PubSubExtensions.Producer;
using Moq;
using Newtonsoft.Json;
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
            Assert.Equal(HttpStatusCode.OK, response.HttpStatusCode);
        }

        [Fact]
        public async Task AddMutation_Adds_Diff_MessageAttribute_Identical_Objects()
        {
            await Task.CompletedTask;
            // Arrange
            var obj1 = new TestItem();
            var obj2 = new TestItem();

            var request = new PublishRequest();

            // Act
            request.AddMutation(obj1, obj2);

            // Assert
            Assert.Empty(JsonConvert.DeserializeObject<List<string>>(request.MessageAttributes[Constants.UpdatedProperties].StringValue));
        }

        [Fact]
        public async Task AddMutation_Adds_Diff_MessageAttribute_Different_Objects()
        {
            await Task.CompletedTask;
            // Arrange
            var obj1 = new TestItem();
            var obj2 = new TestItem { Name = "Test" };

            var request = new PublishRequest();

            // Act
            request.AddMutation(obj1, obj2);

            // Assert
            var diff = JsonConvert.DeserializeObject<List<string>>(request.MessageAttributes[Constants.UpdatedProperties].StringValue);
            Assert.Single(diff);
            Assert.Equal("Name", diff[0]);
        }

        [Fact]
        public async Task AddMutation_Adds_Diff_MessageAttribute_Null_OldImage()
        {
            await Task.CompletedTask;
            // Arrange
            TestItem obj1 = null;
            var obj2 = new TestItem { Name = "Test", Age = 1, CreationDate = DateTime.Now };

            var request = new PublishRequest();

            // Act
            request.AddMutation(obj1, obj2);

            // Assert
            var diff = JsonConvert.DeserializeObject<List<string>>(request.MessageAttributes[Constants.UpdatedProperties].StringValue);
            Assert.Equal(4, diff.Count);
        }

        [Fact]
        public async Task AddMutation_Adds_Diff_MessageAttribute_Null_NewImage()
        {
            await Task.CompletedTask;
            // Arrange
            var obj1 = new TestItem { Name = "Test", Age = 1, CreationDate = DateTime.Now };
            TestItem obj2 = null;

            var request = new PublishRequest();

            // Act
            request.AddMutation(obj1, obj2);

            // Assert
            var diff = JsonConvert.DeserializeObject<List<string>>(request.MessageAttributes[Constants.UpdatedProperties].StringValue);
            Assert.Equal(4, diff.Count);
        }

        [Fact]
        public void AddMutation_Adds_EventType()
        {
            // Arrange
            var obj1 = new TestItem { Name = "Test", Age = 1, CreationDate = DateTime.Now };
            var obj2 = new TestItem { Name = "Test", Age = 2, CreationDate = DateTime.Now };
            var eventType = "order-service.order";

            var request = new PublishRequest();

            // Act
            request.AddMutation(obj1, obj2, eventType);

            // Assert
            var model = JsonConvert.DeserializeObject<MutationModel<TestItem>>(request.Message);
            Assert.Equal(eventType, model.EventType);
        }

        [Fact]
        public void AddMutation_Adds_EventId()
        {
            // Arrange
            var obj1 = new TestItem { Name = "Test", Age = 1, CreationDate = DateTime.Now };
            var obj2 = new TestItem { Name = "Test", Age = 2, CreationDate = DateTime.Now };

            var request = new PublishRequest();

            // Act
            request.AddMutation(obj1, obj2);

            // Assert
            var model = JsonConvert.DeserializeObject<MutationModel<TestItem>>(request.Message);
            Assert.True(Guid.Parse(model.EventId) != default(Guid));
        }

        [Fact]
        public void AddMutation_Adds_Published()
        {
            // Arrange
            var obj1 = new TestItem { Name = "Test", Age = 1, CreationDate = DateTime.Now };
            var obj2 = new TestItem { Name = "Test", Age = 2, CreationDate = DateTime.Now };

            var request = new PublishRequest();

            // Act
            request.AddMutation(obj1, obj2);

            // Assert
            var model = JsonConvert.DeserializeObject<MutationModel<TestItem>>(request.Message);
            Assert.True(DateTime.UtcNow >= model.Published);
            Assert.True(model.Published != default(DateTime));
        }
    }
}