using System;
using Ploeh.AutoFixture;
using PubNub.Async.Models.Access;
using PubNub.Async.Tests.Common;
using Xunit;

namespace PubNub.Async.Tests.Models.Access
{
	public class AccessRegistrationTests : AbstractTest
	{
		#region Read/Write

		[Fact]
		public void AccessValid__Given_ReadWrite__When_ReadNull__Then_ReturnFalse()
		{
			var subject = new AccessRegistration
			{
				ReadExpires = null,
				WriteExpires = Fixture.Create<long>()
			};

			var result = subject.AccessValid(AccessType.ReadWrite);

			Assert.False(result);
		}

		[Fact]
		public void AccessValid__Given_ReadWrite__When_WriteNull__Then_ReturnFalse()
		{
			var subject = new AccessRegistration
			{
				ReadExpires = Fixture.Create<long>(),
				WriteExpires = null
			};

			var result = subject.AccessValid(AccessType.ReadWrite);

			Assert.False(result);
		}

		[Fact]
		public void AccessValid__Given_ReadWrite__When_ReadAndWriteExpired__Then_ReturnFalse()
		{
			var utcAnHourAgo = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)).Ticks;
			var subject = new AccessRegistration
			{
				ReadExpires = utcAnHourAgo,
				WriteExpires = utcAnHourAgo
			};

			var result = subject.AccessValid(AccessType.ReadWrite);

			Assert.False(result);
		}

		[Fact]
		public void AccessValid__Given_ReadWrite__When_ReadValidAndWriteExpired__Then_ReturnFalse()
		{
			var utcAnHourAgo = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)).Ticks;
			var utcAnHourFwd = DateTime.UtcNow.Add(TimeSpan.FromHours(1)).Ticks;
			var subject = new AccessRegistration
			{
				ReadExpires = utcAnHourFwd,
				WriteExpires = utcAnHourAgo
			};

			var result = subject.AccessValid(AccessType.ReadWrite);

			Assert.False(result);
		}

		[Fact]
		public void AccessValid__Given_ReadWrite__When_ReadExpiredAndWriteValid__Then_ReturnTrue()
		{
			var utcAnHourAgo = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)).Ticks;
			var utcAnHourFwd = DateTime.UtcNow.Add(TimeSpan.FromHours(1)).Ticks;
			var subject = new AccessRegistration
			{
				ReadExpires = utcAnHourAgo,
				WriteExpires = utcAnHourFwd
			};

			var result = subject.AccessValid(AccessType.ReadWrite);

			Assert.False(result);
		}

		[Fact]
		public void AccessValid__Given_ReadWrite__When_ReadAndWriteValid__Then_ReturnTrue()
		{
			var utcAnHourFwd = DateTime.UtcNow.Add(TimeSpan.FromHours(1)).Ticks;
			var subject = new AccessRegistration
			{
				ReadExpires = utcAnHourFwd,
				WriteExpires = utcAnHourFwd
			};

			var result = subject.AccessValid(AccessType.ReadWrite);

			Assert.True(result);
		}

		#endregion

		#region Read

		[Fact]
		public void AccessValid__Given_Read__When_ReadNull__Then_ReturnFalse()
		{
			var subject = new AccessRegistration
			{
				ReadExpires = null
			};

			var result = subject.AccessValid(AccessType.Read);

			Assert.False(result);
		}

		[Fact]
		public void AccessValid__Given_Read__When_ReadExpired__Then_ReturnFalse()
		{
			var utcAnHourAgo = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)).Ticks;
			var subject = new AccessRegistration
			{
				ReadExpires = utcAnHourAgo,
			};

			var result = subject.AccessValid(AccessType.Read);

			Assert.False(result);
		}

		[Fact]
		public void AccessValid__Given_Read__When_ReadValid__Then_ReturnTrue()
		{
			var utcAnHourFwd = DateTime.UtcNow.Add(TimeSpan.FromHours(1)).Ticks;
			var subject = new AccessRegistration
			{
				ReadExpires = utcAnHourFwd,
			};

			var result = subject.AccessValid(AccessType.Read);

			Assert.True(result);
		}

		#endregion

		#region Write

		[Fact]
		public void AccessValid__Given_Write__When_WriteNull__Then_ReturnFalse()
		{
			var subject = new AccessRegistration
			{
				WriteExpires = null
			};

			var result = subject.AccessValid(AccessType.Write);

			Assert.False(result);
		}

		[Fact]
		public void AccessValid__Given_Write__When_WriteExpired__Then_ReturnFalse()
		{
			var utcAnHourAgo = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)).Ticks;
			var subject = new AccessRegistration
			{
				WriteExpires = utcAnHourAgo
			};

			var result = subject.AccessValid(AccessType.Write);

			Assert.False(result);
		}

		[Fact]
		public void AccessValid__Given_Write__When_WriteValid__Then_ReturnTrue()
		{
			var utcAnHourFwd = DateTime.UtcNow.Add(TimeSpan.FromHours(1)).Ticks;
			var subject = new AccessRegistration
			{
				WriteExpires = utcAnHourFwd
			};

			var result = subject.AccessValid(AccessType.Write);

			Assert.True(result);
		}

		#endregion
	}
}
