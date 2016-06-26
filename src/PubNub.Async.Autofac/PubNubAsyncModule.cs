using System;
using Autofac;
using PubNub.Async.Configuration;
using PubNub.Async.Services.Access;
using PubNub.Async.Services.Crypto;
using PubNub.Async.Services.History;
using PubNub.Async.Services.Publish;

namespace PubNub.Async.Autofac
{
	public class PubNubAsyncModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder
				.RegisterType<PubNubAutofacBootstrapper>()
				.As<IStartable>()
				.SingleInstance();

			builder
				.RegisterType<PubNubAutofacEnvironment>()
				.As<IPubNubEnvironment>()
				.SingleInstance();

			builder
				.RegisterType<CryptoService>()
				.As<ICryptoService>()
				.SingleInstance();

			builder
				.RegisterType<AccessRegistry>()
				.As<IAccessRegistry>()
				.SingleInstance();

			builder
				.RegisterType<AccessManager>()
				.As<IAccessManager>();

			// ensure that all dependent services have the same client instance
			builder
				.Register<IHistoryService>((c, p) =>
				{
					var context = c.Resolve<IComponentContext>();

					var client = p.TypedAs<IPubNubClient>();
					if (client == null)
					{
						throw new InvalidOperationException(
							$"{typeof (IPubNubClient).Name} is required to resolve ${typeof (IHistoryService).Name}");
					}

					var access = context.Resolve<Func<IPubNubClient, IAccessManager>>();

					return new HistoryService(client, c.Resolve<ICryptoService>(), access(client));
				});

			builder
				.Register<IPublishService>((c, p) =>
				{
					var context = c.Resolve<IComponentContext>();

					var client = p.TypedAs<IPubNubClient>();
					if (client == null)
					{
						throw new InvalidOperationException(
							$"{typeof (IPubNubClient).Name} is required to resolve ${typeof (IPublishService).Name}");
					}

					var access = context.Resolve<Func<IPubNubClient, IAccessManager>>();

					return new PublishService(client, c.Resolve<ICryptoService>(), access(client));
				});
		}
	}
}