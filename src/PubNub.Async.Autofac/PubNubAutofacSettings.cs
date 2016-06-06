using System;
using PubNub.Async.Configuration;
using PubNub.Async.Services.Crypto;
using PubNub.Async.Services.History;

namespace PubNub.Async.Autofac
{
	public class PubNubAutofacSettings : AbstractPubNubSettings
	{
		public PubNubAutofacSettings(
			Func<ICryptoService> cryptoFactory,
			Func<IPubNubClient, IHistoryService> historyFactory)
		{
			CryptoFactory = cryptoFactory;
			HistoryFactory = historyFactory;
		}

		public override Func<ICryptoService> CryptoFactory { get; }
		public override Func<IPubNubClient, IHistoryService> HistoryFactory { get; }
	}
}