using System;

namespace PubNub.Async.Tests.Services.Publish
{
	public static class MessageLargerThan32K
	{
		static MessageLargerThan32K()
		{
			var val = string.Empty;
			var rand = new Random();
			var charSource = "abcdefghijklmnopqrstuvwxyz";
			for (var i = 0; i < 32000; i++)
			{
				val += charSource[rand.Next(0, 26)];
			}
			Value = val;
		}

		public static string Value { get; }
	}
}
