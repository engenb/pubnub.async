using PubNub.Async.Configuration;
using PubNub.Async.Services.Publish;
using PubNub.Async.Push.Services;
using System;

namespace PubNub.Async.Push.Configuration
{
    public static class EnvironmentExtensions
    {
        public static void UsePush(this IPubNubEnvironment environment)
        {
            var registrar = environment as IRegisterService;
            if (registrar == null)
            {
                throw new InvalidOperationException($"Incompatible Environment: {nameof(environment)} must implement ${typeof(IRegisterService).Name}");
            }

            registrar.Register<IPushService>(client => new PushService(client, environment.Resolve<IPublishService>(client)));
        }
    }
}