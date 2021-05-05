using Bit.Core.Enums;

namespace Bit.Core.Abstractions
{
    public interface ICryptoPrimitiveService
    {
        byte[] Pbkdf2(byte[] password, byte[] salt, CryptoHashAlgorithm algorithm, int iterations);
        byte[] AesGcmEncrypt(byte[] data, byte[] iv, byte[] key);
        byte[] AesGcmDecrypt(byte[] data, byte[] iv, byte[] key);
    }
}
