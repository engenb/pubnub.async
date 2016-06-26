using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PubNub.Async.Models.Channel;
using PubNub.Async.Presence.Models;
using PubNub.Async.Presence.Services;

namespace PubNub.Async.Presence.Extensions
{
	public static class PresenceExtensions
	{
		public static Task<SessionStateResponse<TState>> GetState<TState>(this string channel)
			where TState : class
		{
			return new PubNubClient(channel)
				.GetState<TState>();
		}

		public static Task<SessionStateResponse<TState>> GetState<TState>(this Channel channel)
			where TState : class
		{
			return new PubNubClient(channel)
				.GetState<TState>();
		}

		public static async Task<SessionStateResponse<TState>> GetState<TState>(this IPubNubClient client)
			where TState : class
		{
			return await PubNub.Environment
				.Resolve<IPresenceService>(client)
				.SessionState<TState>()
				.ConfigureAwait(false);
		}

		public static Task<SessionStateResponse<TState>> SetState<TState>(this string channel, TState state)
			where TState : class
		{
			return new PubNubClient(channel)
				.SetState(state);
		}

		public static Task<SessionStateResponse<TState>> SetState<TState>(this Channel channel, TState state)
			where TState : class
		{
			return new PubNubClient(channel)
				.SetState(state);
		}

		public static async Task<SessionStateResponse<TState>> SetState<TState>(this IPubNubClient client, TState state)
			where TState : class
		{
			return await PubNub.Environment
				.Resolve<IPresenceService>(client)
				.SessionState(state)
				.ConfigureAwait(false);
		}

		public static Task<SubscribersResponse<JObject>> Subscribers(this string channel)
		{
			return new PubNubClient(channel)
				.Subscribers();
		}

		public static Task<SubscribersResponse<JObject>> Subscribers(this Channel channel)
		{
			return new PubNubClient(channel)
				.Subscribers();
		}

		public static async Task<SubscribersResponse<JObject>> Subscribers(this IPubNubClient client)
		{
			return await PubNub.Environment
				.Resolve<IPresenceService>(client)
				.Subscribers<JObject>()
				.ConfigureAwait(false);
		}

		public static Task<SubscribersResponse<TState>> Subscribers<TState>(this string channel)
			where TState : class
		{
			return new PubNubClient(channel)
				.Subscribers<TState>();
		}

		public static Task<SubscribersResponse<TState>> Subscribers<TState>(this Channel channel)
			where TState : class
		{
			return new PubNubClient(channel)
				.Subscribers<TState>();
		}

		public static async Task<SubscribersResponse<TState>> Subscribers<TState>(this IPubNubClient client)
			where TState : class
		{
			return await PubNub.Environment
				.Resolve<IPresenceService>(client)
				.Subscribers<TState>()
				.ConfigureAwait(false);
		}
	}
}