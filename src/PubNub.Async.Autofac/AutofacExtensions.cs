using Autofac;

namespace PubNub.Async.Autofac
{
	public static class AutofacExtensions
	{
		public static void RegisterPubNub(this ContainerBuilder builder)
		{
			builder.RegisterModule<PubNubAsyncModule>();
		}
	}
}
