using Autofac;
using PubNub.Async.Presence.Services;

namespace PubNub.Async.Presence.Autofac
{
	public class PresenceModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder
				.RegisterType<PresenceService>()
				.As<IPresenceService>();
		}
	}
}