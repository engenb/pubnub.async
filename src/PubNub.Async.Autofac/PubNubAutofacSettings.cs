using System;
using PubNub.Async.Configuration;
using PubNub.Async.Services.Access;
using PubNub.Async.Services.Crypto;
using PubNub.Async.Services.History;
using PubNub.Async.Services.Publish;

namespace PubNub.Async.Autofac
{
	public class PubNubAutofacSettings : AbstractPubNubSettings
	{
		public PubNubAutofacSettings(
			Func<ICryptoService> cryptoFactory,
			Func<IPubNubClient, IAccessManager> accessFactory,
			Func<IPubNubClient, IHistoryService> historyFactory,
			Func<IPubNubClient, IPublishService> publishFactory)
		{
			CryptoFactory = cryptoFactory;
			AccessFactory = accessFactory;
			HistoryFactory = historyFactory;
			PublishFactory = publishFactory;
		}

		public override Func<ICryptoService> CryptoFactory { get; }
		public override Func<IPubNubClient, IAccessManager> AccessFactory { get; }
		public override Func<IPubNubClient, IHistoryService> HistoryFactory { get; }
		public override Func<IPubNubClient, IPublishService> PublishFactory { get; }
	}
}