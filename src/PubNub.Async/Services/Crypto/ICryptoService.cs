namespace PubNub.Async.Services.Crypto
{
	public interface ICryptoService
	{
		string Decrypt(string cipher, string source);
	}
}