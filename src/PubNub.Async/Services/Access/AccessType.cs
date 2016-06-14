namespace PubNub.Async.Services.Access
{
	public enum AccessType
	{
		Read = 1,
		Write = 2,
		ReadWrite = 3
	}

	public static class AccessTypeExtensions
	{
		public static bool GrantsRead(this AccessType access)
		{
			return access == AccessType.Read || access == AccessType.ReadWrite;
		}

		public static bool GrantsWrite(this AccessType access)
		{
			return access == AccessType.Write || access == AccessType.ReadWrite;
		}
	}
}
