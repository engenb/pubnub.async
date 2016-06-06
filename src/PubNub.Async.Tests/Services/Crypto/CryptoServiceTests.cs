using PubNub.Async.Services.Crypto;
using Xunit;

namespace PubNub.Async.Tests.Services.Crypto
{
	public class CryptoServiceTests
	{
		[Fact]
		public void Decrypt__Given_PubNubEncryptedString__When_CipherCorrect__Then_Decrypt()
		{
			var expectedResult = "{\"text\":\"Hello World!\"}";

			var cipher = "TEST";

			var encryptedMsg = "fhqbfIebqFs1rIzlMGNanS03azaP5nqBa16PbfnXkm8=";

			var subject = new CryptoService();

			var result = subject.Decrypt(cipher, encryptedMsg);

			Assert.Equal(expectedResult, result);
		}
	}
}