using System;

namespace PubNub.Async.Models
{
	public class Channel
    {
        public string Name { get; }

        public bool Encrypted { get; set; }
        public string Cipher { get; set; }

        public bool Secured { get; set; }

        public Channel(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException($"{nameof(name)} must have a non-null, non-whitespace value", nameof(name));
			}
			Name = name;
		}

	    public Channel Clone()
	    {
	        return (Channel)MemberwiseClone();
	    }

	    public override bool Equals(object obj)
	    {
	        return Name.Equals((obj as Channel)?.Name);
	    }

	    public override int GetHashCode()
	    {
	        return Name.GetHashCode();
	    }
    }
}