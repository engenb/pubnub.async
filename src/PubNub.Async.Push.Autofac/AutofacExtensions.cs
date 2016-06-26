using Autofac;

namespace PubNub.Async.Push.Autofac
{
	public static class AutofacExtensions
	{
		public static void RegisterPubNubPush(this ContainerBuilder builder)
		{
			builder.RegisterModule<PubNubAsyncPushModule>();
		}
	}
}