using System;
using System.Collections.Concurrent;
using PubNub.Async.Services.Access;
using PubNub.Async.Services.Crypto;
using PubNub.Async.Services.History;
using PubNub.Async.Services.Publish;
using PubNub.Async.Services.Subscribe;

namespace PubNub.Async.Configuration
{
	public class DefaultPubNubEnvironment : AbstractPubNubEnvironment, IRegisterService
	{
		private static readonly object AccessRegistrySyncRoot = new object();
		private static IAccessRegistry _accessRegistry;
		public static IAccessRegistry AccessRegistryInstance
		{
			get
			{
				if (_accessRegistry == null)
				{
					lock (AccessRegistrySyncRoot)
					{
						if (_accessRegistry == null)
						{
                            _accessRegistry = new AccessRegistry();
						}
					}
				}
				return _accessRegistry;
			}
		}

		private static readonly object CryptoSyncRoot = new object();
		private static ICryptoService _crypto;
		public static ICryptoService CryptoInstance
		{
			get
			{
				if (_crypto == null)
				{
					lock (CryptoSyncRoot)
					{
						if (_crypto == null)
						{
							_crypto = new CryptoService();
						}
					}
				}
				return _crypto;
			}
		}

	    private static readonly object SubscriptionRegistrySyncRoot = new object();
	    private static ISubscriptionRegistry _subscriptionRegistry;

	    public static ISubscriptionRegistry SubscriptionRegistryInstance
        {
	        get
	        {
	            if (_subscriptionRegistry == null)
	            {
	                lock (SubscriptionRegistrySyncRoot)
	                {
	                    if (_subscriptionRegistry == null)
	                    {
	                        _subscriptionRegistry = new SubscriptionRegistry(ResolveSubscriptionInstance);
	                    }
	                }
	            }
	            return _subscriptionRegistry;
	        }
        }

        private static readonly object SubscriptionMonitorSyncRoot = new object();
        private static ISubscriptionMonitor _subscriptionMonitor;

        public static ISubscriptionMonitor SubscriptionMonitorInstance
        {
            get
            {
                if (_subscriptionMonitor == null)
                {
                    lock (SubscriptionMonitorSyncRoot)
                    {
                        if (_subscriptionMonitor == null)
                        {
                            _subscriptionMonitor = new SubscriptionMonitor(
                                (env, channel) => new AccessManager(env, channel, AccessRegistryInstance),
                                SubscriptionRegistryInstance);
                        }
                    }
                }
                return _subscriptionMonitor;
            }
        }

        private static readonly object ResolveSubscriptionSyncRoot = new object();
        private static IResolveSubscription _resolveSubscription;

        public static IResolveSubscription ResolveSubscriptionInstance
        {
            get
            {
                if (_resolveSubscription == null)
                {
                    lock (ResolveSubscriptionSyncRoot)
                    {
                        if (_resolveSubscription == null)
                        {
                            _resolveSubscription = new DefaultResolveSubscription(CryptoInstance);
                        }
                    }
                }
                return _resolveSubscription;
            }
        }

        private ConcurrentDictionary<Type, Func<IPubNubClient, object>> Services { get; }

		/// <summary>
		/// DefaultPubNubEnvironment utilizes a flavor of the infamous service locator pattern.
		/// Since we all know this to be an anti-pattern, check out one of pubnub.async's IoC
		/// packages like pubnub.async.autofac.
		/// </summary>
		public DefaultPubNubEnvironment()
		{
			Services = new ConcurrentDictionary<Type, Func<IPubNubClient, object>>();

			Register<ICryptoService>(client => CryptoInstance);
            Register<IAccessRegistry>(client => AccessRegistryInstance);
            Register<ISubscriptionRegistry>(client => SubscriptionRegistryInstance);
            Register<ISubscriptionMonitor>(client => SubscriptionMonitorInstance);
            Register<IResolveSubscription>(client => ResolveSubscriptionInstance);

            Register<IAccessManager>(client => new AccessManager(client.Environment, client.Channel, Resolve<IAccessRegistry>(client)));
			Register<IHistoryService>(client => new HistoryService(client, Resolve<ICryptoService>(client), Resolve<IAccessManager>(client)));
			Register<IPublishService>(client => new PublishService(client, Resolve<ICryptoService>(client), Resolve<IAccessManager>(client)));

            Register<ISubscribeService>(client => new SubscribeService(
                client,
                Resolve<IAccessManager>(client),
                Resolve<ISubscriptionMonitor>(client),
                Resolve<ISubscriptionRegistry>(client)));
        }

        public void Register<TService>(Func<IPubNubClient, TService> resolver)
		{
			Services[typeof (TService)] = client => resolver(client);
		} 

		public override TService Resolve<TService>(IPubNubClient client)
		{
			return (TService) Services[typeof (TService)](client);
        }
    }
}