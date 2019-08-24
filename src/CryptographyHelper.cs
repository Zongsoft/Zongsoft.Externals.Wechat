using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Zongsoft.Externals.Wechat
{
	internal static class CryptographyHelper
	{
		public static byte[] Encrypt(string identity, string password, byte[] data)
		{
			var key = Convert.FromBase64String(password + "=");
			var iv = new byte[16];
			Array.Copy(key, iv, 16);

			string nonce = Common.Randomizer.GenerateString(16);
			byte[] nonceArray = Encoding.UTF8.GetBytes(nonce);
			byte[] identityArray = Encoding.UTF8.GetBytes(identity);
			byte[] lengthArray = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(data.Length));
			byte[] buffer = new byte[nonceArray.Length + lengthArray.Length + identityArray.Length + data.Length];

			Array.Copy(nonceArray, buffer, nonceArray.Length);
			Array.Copy(lengthArray, 0, buffer, nonceArray.Length, lengthArray.Length);
			Array.Copy(data, 0, buffer, nonceArray.Length + lengthArray.Length, data.Length);
			Array.Copy(identityArray, 0, buffer, nonceArray.Length + lengthArray.Length + data.Length, identityArray.Length);

			var algorithm = new RijndaelManaged()
			{
				IV = iv,
				Key = key,
				KeySize = 256,
				BlockSize = 128,
				Mode = CipherMode.CBC,
				Padding = PaddingMode.None, //PaddingMode.PKCS7
			};

			var encryptor = algorithm.CreateEncryptor(algorithm.Key, algorithm.IV);
			byte[] xBuff = null;

			byte[] msg = new byte[buffer.Length + 32 - buffer.Length % 32];
			Array.Copy(buffer, msg, buffer.Length);
			byte[] padding = KCS7Encoder(buffer.Length);
			Array.Copy(padding, 0, msg, buffer.Length, padding.Length);

			using(var ms = new MemoryStream())
			{
				using(var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
				{
					cs.Write(msg, 0, msg.Length);
				}

				xBuff = ms.ToArray();
			}

			return xBuff;
		}

		public static byte[] Decrypt(string identity, string password, byte[] data)
		{
			throw new NotImplementedException();
		}

		#region 私有方法
		private static byte[] KCS7Encoder(int text_length)
		{
			var block_size = 32;

			// 计算需要填充的位数
			var amount_to_pad = block_size - (text_length % block_size);

			if(amount_to_pad == 0)
				amount_to_pad = block_size;

			// 获得补位所用的字符
			var pad_chr = (char)(byte)(amount_to_pad & 0xFF);
			var tmp = "";

			for(int index = 0; index < amount_to_pad; index++)
			{
				tmp += pad_chr;
			}

			return Encoding.UTF8.GetBytes(tmp);
		}
		#endregion
	}
}
