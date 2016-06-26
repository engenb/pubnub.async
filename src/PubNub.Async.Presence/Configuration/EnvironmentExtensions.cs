using System;
using PubNub.Async.Configuration;
using PubNub.Async.Presence.Services;

namespace PubNub.Async.Presence.Configuration
{
	public static class EnvironmentExtensions
	{
		public static void UsePresence(this IPubNubEnvironment environment)
		{
			var registrar = environment as IRegisterService;
			if (registrar == null)
			{
				throw new InvalidOperationException(
					$"Incompatible Environment: {nameof(environment)} must implement ${typeof (IRegisterService).Name}");
			}

			registrar.Register<IPresenceService>(client => new PresenceService(client));
		}
	}
}