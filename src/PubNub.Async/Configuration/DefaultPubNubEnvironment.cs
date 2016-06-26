using System;
using System.Collections.Concurrent;
using PubNub.Async.Services.Access;
using PubNub.Async.Services.Crypto;
using PubNub.Async.Services.History;
using PubNub.Async.Services.Publish;

namespace PubNub.Async.Configuration
{
	public class DefaultPubNubEnvironment : AbstractPubNubEnvironment, IRegisterService
	{
		private static readonly object RegistrySyncRoot = new object();
		private static IAccessRegistry _registry;
		public static IAccessRegistry RegistryInstance
		{
			get
			{
				if (_registry == null)
				{
					lock (RegistrySyncRoot)
					{
						if (_registry == null)
						{
							_registry = new AccessRegistry();
						}
					}
				}
				return _registry;
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
			Register<IAccessManager>(client => new AccessManager(client, RegistryInstance));
			Register<IHistoryService>(client => new HistoryService(client, Resolve<ICryptoService>(client), Resolve<IAccessManager>(client)));
			Register<IPublishService>(client => new PublishService(client, Resolve<ICryptoService>(client), Resolve<IAccessManager>(client)));
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