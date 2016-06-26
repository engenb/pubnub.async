using System;

namespace PubNub.Async.Models.Access
{
	public class AccessRegistration
	{
		public long? ReadExpires { get; set; }
		public long? WriteExpires { get; set; }

		public bool AccessValid(AccessType access)
		{
			var utcNow = DateTime.UtcNow.Ticks;
			switch (access)
			{
				case AccessType.ReadWrite:
				{
					return utcNow < ReadExpires && utcNow < WriteExpires;
				}
				case AccessType.Read:
				{
					return utcNow < ReadExpires;
				}
				case AccessType.Write:
				{
					return utcNow < WriteExpires;
				}
				default:
				{
					return false;
				}
			}
		}
	}
}