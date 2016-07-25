using System;
using Autofac;
using PubNub.Async.Configuration;
using PubNub.Async.Models;
using PubNub.Async.Models.Subscribe;
using PubNub.Async.Services.Access;
using PubNub.Async.Services.Crypto;
using PubNub.Async.Services.History;
using PubNub.Async.Services.Publish;
using PubNub.Async.Services.Subscribe;

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
            
		    builder
		        .RegisterType<SubscriptionMonitor>()
		        .As<ISubscriptionMonitor>()
                .SingleInstance();

		    builder
		        .RegisterType<SubscriptionRegistry>()
		        .As<ISubscriptionRegistry>()
		        .SingleInstance();

		    builder
		        .RegisterGeneric(typeof(Subscription<>));

		    builder
		        .RegisterType<ResolveSubscription>()
		        .As<IResolveSubscription>()
		        .SingleInstance();

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

					var access = context.Resolve<Func<IPubNubEnvironment, Channel, IAccessManager>>();

					return new HistoryService(
                        client,
                        context.Resolve<ICryptoService>(),
                        access(client.Environment, client.Channel));
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

                    var access = context.Resolve<Func<IPubNubEnvironment, Channel, IAccessManager>>();

                    return new PublishService(
                        client,
                        context.Resolve<ICryptoService>(),
                        access(client.Environment, client.Channel));
				});

            builder
                .Register<ISubscribeService>((c, p) =>
                {
                    var context = c.Resolve<IComponentContext>();

                    var client = p.TypedAs<IPubNubClient>();
                    if (client == null)
                    {
                        throw new InvalidOperationException(
                            $"{typeof(IPubNubClient).Name} is required to resolve ${typeof(IPublishService).Name}");
                    }
                    
                    var access = context.Resolve<Func<IPubNubEnvironment, Channel, IAccessManager>>();

                    return new SubscribeService(
                        client,
                        access(client.Environment, client.Channel),
                        context.Resolve<ISubscriptionMonitor>(),
                        context.Resolve<ISubscriptionRegistry>());
                });
        }
	}
}