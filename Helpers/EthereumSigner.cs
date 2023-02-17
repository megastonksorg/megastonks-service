using System.Globalization;
using Nethereum.Signer;
using Nethereum.Util;

namespace Megastonks.Helpers
{
	public static class EthereumSigner
	{
		public static bool IsSignatureValid(string messsagePublicKey, string walletAddress, string signature)
        {
            if (IsAddressValid(walletAddress))
            {
                var sha3Keccak = new Sha3Keccack();
                string hashedMessage = sha3Keccak.CalculateHash(messsagePublicKey);
                byte[] hashedMessageToBytes = ConvertHexStringToByteArray(hashedMessage);

                var signer = new MessageSigner();
                var addressFromSignature = signer.EcRecover(hashedMessageToBytes, signature);

                return addressFromSignature == walletAddress;
            }
            else
            {
                throw new AppException("Invalid address used for validation");
            }
        }

        private static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] data = new byte[hexString.Length / 2];
            for (int index = 0; index < data.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return data;
        }

        public static bool IsAddressValid(string address)
        {
            var addressUtil = new AddressUtil();
            return addressUtil.IsValidAddressLength(address) &&
                addressUtil.IsValidEthereumAddressHexFormat(address) &&
                addressUtil.IsChecksumAddress(address);
        }

        public static string HashMessage(string message)
        {
            var sha3Keccak = new Sha3Keccack();
            return sha3Keccak.CalculateHash(message);
        }
    }
}