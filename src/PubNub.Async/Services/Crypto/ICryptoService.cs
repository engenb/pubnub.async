using PCLCrypto;

namespace PubNub.Async.Services.Crypto
{
	public interface ICryptoService
	{
		string Decrypt(string cipher, string source);
		string Encrypt(string cipher, string source);
		string Hash(string source, HashAlgorithm algo);
	}
}