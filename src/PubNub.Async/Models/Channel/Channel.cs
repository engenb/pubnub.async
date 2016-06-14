using System;

namespace PubNub.Async.Models.Channel
{
	public class Channel
	{
		public Channel(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException($"{nameof(name)} must have a non-null, non-whitespace value", nameof(name));
			}
			Name = name;
		}

		public string Name { get; }

		public bool Encrypted { get; set; }
		public string Cipher { get; set; }

		public bool Secured { get; set; }
	}
}