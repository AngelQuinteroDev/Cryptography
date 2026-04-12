using System;
using System.Security.Cryptography;
using System.Text;

// Adaptador que usa las librerías estándar de .NET
// Hash: SHA-256  |  Firma/Verificación: RSA 2048 + PKCS#1
public class LibraryCryptoAdapter : ICryptoAdapter
{
    private readonly RSACryptoServiceProvider _rsa;

    public LibraryCryptoAdapter()
    {
        _rsa = new RSACryptoServiceProvider(2048);
    }

    // SHA-256 → string hexadecimal (64 chars)
    public string Hash(string mensaje)
    {
        using SHA256 sha = SHA256.Create();
        byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(mensaje));
        return BitConverter.ToString(bytes).Replace("-", "");
    }

    // Cifra el hash con la clave PRIVADA → Base64
    public string Sign(string hashHex)
    {
        byte[] hashBytes = HexABytes(hashHex);
        byte[] firma = _rsa.SignHash(hashBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return Convert.ToBase64String(firma);
    }

    // Descifra la firma con la clave PÚBLICA y compara con el hash
    public bool Verify(string hashHex, string firmaBase64, string llavePublicaXml)
    {
        using var rsaVerif = new RSACryptoServiceProvider();
        rsaVerif.FromXmlString(llavePublicaXml);

        byte[] hashBytes  = HexABytes(hashHex);
        byte[] firmaBytes = Convert.FromBase64String(firmaBase64);

        return rsaVerif.VerifyHash(hashBytes, firmaBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }

    // Solo exporta la parte pública (false = sin clave privada)
    public string ExportarLlavePublica() => _rsa.ToXmlString(false);

    private static byte[] HexABytes(string hex)
    {
        byte[] result = new byte[hex.Length / 2];
        for (int i = 0; i < result.Length; i++)
            result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        return result;
    }
}
