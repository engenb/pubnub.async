using Autofac;
using PubNub.Async.Configuration;

namespace PubNub.Async.Autofac
{
	public class PubNubAutofacEnvironment : AbstractPubNubEnvironment
	{
		private IComponentContext Context { get; }

		public PubNubAutofacEnvironment(IComponentContext context)
		{
			Context = context;
		}

		public override TService Resolve<TService>(IPubNubClient client)
		{
			return Context.Resolve<TService>(new TypedParameter(typeof(IPubNubClient), client));
		}
	}
}