namespace PubNub.Async.Models.Access
{
	public enum AccessType
	{
		None = 0,
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
