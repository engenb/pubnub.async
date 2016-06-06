using System;
using PubNub.Async.Services.Crypto;
using PubNub.Async.Services.History;

namespace PubNub.Async.Configuration
{
	public class DefaultPubNubSettings : AbstractPubNubSettings
	{
		public override Func<ICryptoService> CryptoFactory => () => new CryptoService();
		public override Func<IPubNubClient, IHistoryService> HistoryFactory => pn => new HistoryService(pn, CryptoFactory());
	}
}