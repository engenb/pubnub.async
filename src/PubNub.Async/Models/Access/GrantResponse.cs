namespace PubNub.Async.Models.Access
{
	public class GrantResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; }
	    public string SubscribeKey { get; set; }
	    public string Channel { get; set; }
		public AccessType Access { get; set; }
		public int MinutesToExpire { get; set; }
	}
}