using System.Security.Cryptography;

namespace ArtEFundAPIServices.DataAccess.Encryption;

public class AesEncryptionService : IEncryptionService
{
    private readonly IConfiguration _configuration;

    public AesEncryptionService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string Encrypt(string plainText)
    {
        var key = Convert.FromBase64String(_configuration["Encryption:Key"]);
        var iv = Convert.FromBase64String(_configuration["Encryption:IV"]);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using var sw = new StreamWriter(cs);

        sw.Write(plainText);
        sw.Flush();
        cs.FlushFinalBlock();

        return Convert.ToBase64String(ms.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        var key = Convert.FromBase64String(_configuration["Encryption:Key"]);
        var iv = Convert.FromBase64String(_configuration["Encryption:IV"]);
        var cipherBytes = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(cipherBytes);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);

        return sr.ReadToEnd();
    }
}