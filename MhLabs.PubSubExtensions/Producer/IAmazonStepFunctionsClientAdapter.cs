using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService.Model;

namespace MhLabs.PubSubExtensions.Producer
{
    public interface IAmazonStepFunctionsClientAdapter
    {
        Task<PublishResponse> PublishAsync(PublishRequest request, string stateMachineArn, CancellationToken cancellationToken);
    }
}