using System;
using Autofac;
using PubNub.Async.Push.Services;
using PubNub.Async.Services.Access;
using PubNub.Async.Services.Publish;

namespace PubNub.Async.Push.Autofac
{
	public class PubNubAsyncPushModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder
				.Register<IPushService>((c, p) =>
				{
					var client = p.TypedAs<IPubNubClient>();
					if (client == null)
					{
						throw new InvalidOperationException(
							$"{typeof (IPubNubClient).Name} is required to resolve ${typeof (IPushService).Name}");
					}

					var context = c.Resolve<IComponentContext>();
				    var access = context.Resolve<Func<IPubNubClient, IAccessManager>>();
					var publish = context.Resolve<Func<IPubNubClient, IPublishService>>();
					return new PushService(client, access(client), publish(client));
				});
		}
	}
}