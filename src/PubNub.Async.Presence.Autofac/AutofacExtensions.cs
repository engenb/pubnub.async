using Autofac;

namespace PubNub.Async.Presence.Autofac
{
	public static class AutofacExtensions
	{
		public static void RegisterPubNubPush(this ContainerBuilder builder)
		{
			builder.RegisterModule<PresenceModule>();
		}
	}
}