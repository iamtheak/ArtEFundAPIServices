using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ArtEFundAPIServices.Helper;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 32; 
    private const int HashSize = 32; 
    private const int Iterations = 100000; 
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

    public string HashPassword(string password)
    {
        if (password.IsNullOrEmpty())
        {
            throw new Exception("Password cannot be null or empty. ");
        }
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize); 
        byte[] hash = Rfc2898DeriveBytes. Pbkdf2(password, salt, Iterations, Algorithm, HashSize); 
        return $"{Convert.ToHexString (hash)}-{Convert.ToHexString(salt)}";
    } 
    
    public bool VerifyPassword(string password, string passwordHash)
    {

        if (password.IsNullOrEmpty())
        {
            throw new Exception("Password cannot be null or empty. ");
        }
        
        if(passwordHash.IsNullOrEmpty())
        {
            throw new Exception("Password hash cannot be null or empty. ");
        }
        
        string[] parts = passwordHash.Split('-');
        byte[] hash = Convert.FromHexString(parts[0]);
        byte[] salt = Convert.FromHexString(parts[1]);
        byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);
        return hash.SequenceEqual(inputHash);
    }
}


