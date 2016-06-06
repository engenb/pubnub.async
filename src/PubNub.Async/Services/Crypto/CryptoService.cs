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

		private static byte[] BuildCipher(string cipherSrc)
		{
			var inputBytes = Encoding.UTF8.GetBytes(cipherSrc);
			var hasher = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithm.Sha256);
			var hashedBytes = hasher.HashData(inputBytes);
			var hashedString = BitConverter.ToString(hashedBytes);
			//modified from PubNub client source
			return Encoding.UTF8.GetBytes(hashedString.Replace("-", string.Empty)
				.Substring(0, 32)
				.ToLower());
		}
	}
}