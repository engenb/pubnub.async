using Autofac;
using PubNub.Async.Configuration;
using PubNub.Async.Services.Crypto;
using PubNub.Async.Services.History;

namespace PubNub.Async.Autofac
{
	public class PubNubAsyncModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder
				.RegisterType<PubNubSettingsBootstrapper>()
				.AsImplementedInterfaces()
				.SingleInstance();

			builder
				.RegisterType<PubNubAutofacSettings>()
				.As<IPubNubSettings>();

			builder
				.RegisterType<CryptoService>()
				.As<ICryptoService>()
				.SingleInstance();

			builder
				.RegisterType<HistoryService>()
				.As<IHistoryService>();
		}
	}
}