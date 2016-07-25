using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using PubNub.Async.Configuration;
using PubNub.Async.Extensions;
using PubNub.Async.Models;
using PubNub.Async.Models.Access;
using PubNub.Async.Models.Subscribe;
using PubNub.Async.Services.Access;

namespace PubNub.Async.Services.Subscribe
{
	public class SubscribeService : ISubscribeService
	{
		private IAccessManager Access { get; }
		private ISubscriptionMonitor Monitor { get; }
		private ISubscriptionRegistry Subscriptions { get; }

		private IPubNubEnvironment Environment { get; }
		private Channel Channel { get; }

		public SubscribeService(
			IPubNubClient client,
			IAccessManager access,
			ISubscriptionMonitor monitor,
			ISubscriptionRegistry subscriptions)
		{
			Environment = client.Environment;
			Channel = client.Channel;

			Access = access;
			Monitor = monitor;
			Subscriptions = subscriptions;
		}

		public async Task<SubscribeResponse> Subscribe<TMessage>(MessageReceivedHandler<TMessage> handler)
		{
			//stop the monitor for reconfiguration
			await Monitor.Stop(Environment);

			// attempt to subscribe before registering the channel
			var channels = Subscriptions
				.Get(Environment.SubscribeKey)
				.Where(x => x.Environment.AuthenticationKey == Environment.AuthenticationKey)
				.Select(x => x.Channel.Name)
				.ToList();
			channels.Add(Channel.Name);

			var channelsCsv = string.Join(",", channels);

			var requestUrl = Environment.Host
				.AppendPathSegments("v2", "subscribe")
				.AppendPathSegment(Environment.SubscribeKey)
				.AppendPathSegment(channelsCsv)
				.AppendPathSegment("0")
				.SetQueryParam("uuid", Environment.SessionUuid);

			if (Channel.Secured)
			{
				if (Environment.GrantCapable())
				{
					var grantResponse = await Access.Establish(AccessType.Read);
					if (!grantResponse.Success)
					{
						return new SubscribeResponse
						{
							Success = false,
							Message = grantResponse.Message
						};
					}
				}

				if (!string.IsNullOrWhiteSpace(Environment.AuthenticationKey))
				{
					requestUrl.SetQueryParam("auth", Environment.AuthenticationKey);
				}
			}

			var httpResponse = await requestUrl
				.AllowHttpStatus("403")
				.GetAsync()
				.ProcessResponse();

			var subResponse = await HandleResponse(httpResponse);

			//successfully subscribed, so register the channel for monitoring
			if (subResponse.Success)
			{
				Monitor.Register(Environment, subResponse.SubscribeTime);
				Subscriptions.Register(Environment, Channel, handler);
			}
			await StartMonitor(Environment);
			return subResponse;
		}

		public async Task Unsubscribe<TMessage>(MessageReceivedHandler<TMessage> handler)
		{
			await Monitor.Stop(Environment);

			Subscriptions.Unregister(Environment, Channel, handler);

			await StartMonitor(Environment);
		}

		public async Task Unsubscribe()
		{
			await Monitor.Stop(Environment);

			Subscriptions.Unregister(Environment, Channel);

			await StartMonitor(Environment);
		}

		private async Task StartMonitor(IPubNubEnvironment environment)
		{
			if (Subscriptions.Get(environment.SubscribeKey).Any())
			{
				await Monitor.Start(environment);
			}
		}

		private static async Task<SubscribeResponse> HandleResponse(HttpResponseMessage response)
		{
			if (response.IsSuccessStatusCode)
			{
				var responseJson = await Task.FromResult(response)
					.ReceiveJson<PubNubSubscribeResponse>();

				return new SubscribeResponse
				{
					Success = true,
					SubscribeTime = responseJson.SubscribeTime.TimeToken
				};
			}
			else
			{
				var responseJson = await Task.FromResult(response)
					.ReceiveJson<PubNubForbiddenResponse>();
				return new SubscribeResponse
				{
					Success = false,
					Message = responseJson.Message
				};
			}
		}
	}
}
