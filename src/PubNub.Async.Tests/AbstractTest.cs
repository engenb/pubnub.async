using System.Collections.Generic;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;

namespace PubNub.Async.Tests
{
	public abstract class AbstractTest
	{
		private IFixture _fixture;

		protected virtual IFixture Fixture => _fixture ?? CreateFixture();

		private IFixture CreateFixture()
		{
			_fixture = new Fixture();
			foreach (var c in FixtureCustomizations())
			{
				_fixture.Customize(c);
			}
			return _fixture;
		}

		protected virtual IEnumerable<ICustomization> FixtureCustomizations()
		{
			return new ICustomization[]
			{
				new AutoConfiguredMoqCustomization(),
				new TestFixtureCustomizations()
			};
		}
	}
}