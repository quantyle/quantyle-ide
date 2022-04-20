using System;
using System.IO;
using System.Security.Cryptography;

namespace X1
{
    public class Security
    {

        private const int SaltSize = 8;

        public void Encrypt(string fileName, string password)
        {

            var targetFile = new System.IO.FileInfo(fileName);
            var keyGenerator = new Rfc2898DeriveBytes(password, SaltSize);
            var rijndael = Rijndael.Create();

            // BlockSize, KeySize in bit --> divide by 8
            rijndael.IV = keyGenerator.GetBytes(rijndael.BlockSize / 8);
            rijndael.Key = keyGenerator.GetBytes(rijndael.KeySize / 8);

            using (var fileStream = targetFile.Create())
            {
                // write random salt
                fileStream.Write(keyGenerator.Salt, 0, SaltSize);

                using (var cryptoStream = new CryptoStream(fileStream, rijndael.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    // write data
                    using (StreamReader encrypted = new StreamReader(cryptoStream))
                    {

                        // Read the decrypted bytes from the decrypting stream
                        // and place them in a string.
                        var plaintext = encrypted.ReadToEnd();
                        Console.WriteLine("ENCRYPTED: {0}", plaintext);
                    }
                }
            }
        }

        public void Decrypt(string fileName, string password)
        {
            // read salt
            var sourceFile = new System.IO.FileInfo(fileName);
            var fileStream = sourceFile.OpenRead();
            var salt = new byte[SaltSize];
            fileStream.Read(salt, 0, SaltSize);

            // initialize algorithm with salt
            var keyGenerator = new Rfc2898DeriveBytes(password, salt);
            var rijndael = Rijndael.Create();
            rijndael.IV = keyGenerator.GetBytes(rijndael.BlockSize / 8);
            rijndael.Key = keyGenerator.GetBytes(rijndael.KeySize / 8);

            // decrypt
            using (var cryptoStream = new CryptoStream(fileStream, rijndael.CreateDecryptor(), CryptoStreamMode.Read))
            {
                // read data
                using (StreamReader decrypted = new StreamReader(cryptoStream))
                {

                    // Read the decrypted bytes from the decrypting stream
                    // and place them in a string.
                    var plaintext = decrypted.ReadToEnd();
                    Console.WriteLine("ENCRYPTED: {0}", plaintext);
                }
            }
        }
    }
}
