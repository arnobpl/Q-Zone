using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Q_Zone.Models.Utility
{
    public static class Encryption
    {
        public static string Encrypt(string plainText, string key) {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException(nameof(plainText));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));

            byte[] encrypted;
            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create()) {
                byte[] keyBytes = new MD5CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes(key));
                // ReSharper disable once PossibleNullReferenceException
                aesAlg.Key = keyBytes;
                aesAlg.IV = keyBytes;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream()) {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)) {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt)) {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted string from the memory stream.
            return Convert.ToBase64String(encrypted);
        }

        public static string Decrypt(string cipherText, string key) {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException(nameof(cipherText));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));

            // Declare the string used to hold the decrypted text.
            string plaintext;

            // Create an Aes object with the specified key and IV.
            using (Aes aesAlg = Aes.Create()) {
                byte[] keyBytes = new MD5CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes(key));
                // ReSharper disable once PossibleNullReferenceException
                aesAlg.Key = keyBytes;
                aesAlg.IV = keyBytes;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText))) {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)) {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt)) {
                            // Read the decrypted bytes from the decrypting stream and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
    }
}