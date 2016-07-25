using System.Threading.Tasks;
using PubNub.Async.Models.Publish;

namespace PubNub.Async.Services.Publish
{
	public interface IPublishService
	{
		Task<PublishResponse> Publish<TMessage>(TMessage message, bool recordHistory = true);
	}
}