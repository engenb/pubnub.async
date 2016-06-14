using System;
using System.Text;
using PCLCrypto;
using static PCLCrypto.WinRTCrypto;

namespace PubNub.Async.Services.Crypto
{
	public class CryptoService : ICryptoService
	{
		private static byte[] IV { get; } = Encoding.UTF8.GetBytes("0123456789012345"); //IV hardcoded in PubNub

		public string Decrypt(string cipher, string source)
		{
			var provider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithm.AesCbcPkcs7);

			//decode the content
			var encryptedBytes = Convert.FromBase64CharArray(source.ToCharArray(), 0, source.Length);
			//decrypt the content
			var key = provider.CreateSymmetricKey(BuildCipher(cipher));
			var decryptedBytes = CryptographicEngine.Decrypt(key, encryptedBytes, IV);
			//convert to string
			return Encoding.UTF8.GetString(decryptedBytes, 0, decryptedBytes.Length);
		}

		public string Encrypt(string cipher, string source)
		{
			var provider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithm.AesCbcPkcs7);

			//convert to bytes
			var sourceBytes = Encoding.UTF8.GetBytes(source);
			//encrypt the content
			var key = provider.CreateSymmetricKey(BuildCipher(cipher));
			var encryptedBytes = CryptographicEngine.Encrypt(key, sourceBytes, IV);
			//encode the content
			return Convert.ToBase64String(encryptedBytes);
		}

		public string Hash(string source, HashAlgorithm algo)
		{
			var sourceBytes = Encoding.UTF8.GetBytes(source);
			var hasher = HashAlgorithmProvider.OpenAlgorithm(algo);
			var hashedBytes = hasher.HashData(sourceBytes);
			return BitConverter.ToString(hashedBytes)
				.Replace("-", string.Empty)
				.ToLower();
		}

		private byte[] BuildCipher(string cipherSrc)
		{
			var hashedCipher = Hash(cipherSrc, HashAlgorithm.Sha256);
			//get the first 32 bytes
			return Encoding.UTF8.GetBytes(hashedCipher.Substring(0, 32));
		}
	}
}