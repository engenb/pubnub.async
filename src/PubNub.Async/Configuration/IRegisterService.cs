using System;

namespace PubNub.Async.Configuration
{
	public interface IRegisterService
	{
		void Register<TService>(Func<IPubNubClient, TService> resolver);
	}
}