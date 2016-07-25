using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PubNub.Async.Configuration;
using PubNub.Async.Models;
using PubNub.Async.Models.Subscribe;

namespace PubNub.Async.Services.Subscribe
{
	public class SubscriptionRegistry : ISubscriptionRegistry
	{
		private IResolveSubscription ResolveSubscription { get; }

		private IDictionary<string, ISet<Subscription>> Subscriptions { get; }

		public SubscriptionRegistry(IResolveSubscription resolveSubscription)
		{
			ResolveSubscription = resolveSubscription;

			Subscriptions = new ConcurrentDictionary<string, ISet<Subscription>>();
		}

		public Subscription[] Get(string subscribeKey)
		{
			return Subscriptions.ContainsKey(subscribeKey)
				? Subscriptions[subscribeKey].ToArray()
				: new Subscription[0];
		}

		public void Register<TMessage>(IPubNubEnvironment environment, Channel channel, MessageReceivedHandler<TMessage> handler)
		{
			var subscribeKey = environment.SubscribeKey;
			if (!Subscriptions.ContainsKey(subscribeKey))
			{
				Subscriptions[subscribeKey] = new HashSet<Subscription>();
			}

			var sub = Subscriptions[subscribeKey]
				.SingleOrDefault(x =>
					x.Environment.SubscribeKey == subscribeKey &&
					x.Environment.AuthenticationKey == environment.AuthenticationKey &&
					x.Channel.Name == channel.Name) as Subscription<TMessage>;

			if (sub == null)
			{
				sub = ResolveSubscription.Resolve<TMessage>(environment, channel);
				Subscriptions[subscribeKey].Add(sub);
			}

			sub.MessageReceived += handler;
		}

		public void Unregister<TMessage>(IPubNubEnvironment environment, Channel channel, MessageReceivedHandler<TMessage> handler)
		{
			var subscribeKey = environment.SubscribeKey;
			if (Subscriptions.ContainsKey(subscribeKey))
			{
				var subs = Subscriptions[subscribeKey];
				var sub = subs.SingleOrDefault(x =>
					x.Environment.SubscribeKey == subscribeKey
					&& x.Channel.Name == channel.Name) as Subscription<TMessage>;

				if (sub != null)
				{
					sub.MessageReceived -= handler;
				}
			}
		}

		public void Unregister(IPubNubEnvironment environment, Channel channel)
		{
			var subscribeKey = environment.SubscribeKey;
			if (Subscriptions.ContainsKey(subscribeKey))
			{
				var subs = Subscriptions[subscribeKey];
				var sub = subs.SingleOrDefault(x =>
					x.Environment.SubscribeKey == subscribeKey
					&& x.Channel.Name == channel.Name);

				if (sub != null)
				{
					subs.Remove(sub);
				}
			}
		}

		public void MessageReceived(PubNubSubscribeResponseMessage message)
		{
			var subscribeKey = message.SubscribeKey;
			if (Subscriptions.ContainsKey(subscribeKey))
			{
				var subs = Subscriptions[subscribeKey];
				var channelSubs = subs
					.Where(x => x.Channel.Name == message.Channel)
					.ToArray();

				foreach (var channelSub in channelSubs)
				{
					channelSub.OnMessageReceived(message);
				}
			}
		}
	}
}
