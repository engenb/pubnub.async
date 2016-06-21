using Autofac;
using PubNub.Async.Push.Services;
using PubNub.Async.Services.Publish;
using System;

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
                        throw new InvalidOperationException($"{typeof(IPubNubClient).Name} is required to resolve ${typeof(IPushService).Name}");
                    }

                    var context = c.Resolve<IComponentContext>();
                    var publishFn = context.Resolve<Func<IPubNubClient, IPublishService>>();
                    return new PushService(client, publishFn(client));
                });
        }
    }
}