/*
 * This code written by me, SlushyRH (https://github.com/SlushyRH), and all copyright goes to me.
 * The license for this code is at https://github.com/SlushyRH/Advanced-Save-System/blob/master/LICENSE.
 */

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

using SRH.External.OdinSerializer;

namespace SRH
{
    public static class SPPUtility
    {
        private static int blockSize = 256;
        private static int keySize = 256;
        private static int bufferSize = 32;

        internal static bool IsEncrypted(string key)
        {
            // Check if there is a key with _Encrypted on the end meaning that the original key is encrypted
            string content = PlayerPrefs.GetString(key);

            if (content.StartsWith("SPP-[SRH]"))
                return true;

            return false;
        }

        internal static Type GetPrefType(string key)
        {
            // Extract type from string
            string[] valueSplit = key.Split("__");
            var objType = Type.GetType(valueSplit[1]);

            // check it isnt null
            if (objType != null)
                return objType;

            return null;
        }

        internal static string SerializeToBinary(object value)
        {
            // Serialize the value to binary format then convert it to string
            byte[] bytes = SerializationUtility.SerializeValue(value, DataFormat.Binary);
            return Convert.ToBase64String(bytes);
        }

        internal static T DeserializeToBinary<T>(string str)
        {
            // Convert string to byte array and deserialize into binary and return as type defined by T
            byte[] bytes = Convert.FromBase64String(str);
            return SerializationUtility.DeserializeValue<T>(bytes, DataFormat.Binary);
        }

        internal static byte[] Encrypt(byte[] data)
        {
            // setup the rijndael encryption
            RijndaelManaged rj = new RijndaelManaged();
            rj.BlockSize = blockSize;
            rj.KeySize = keySize;
            rj.Mode = CipherMode.CBC;
            rj.Padding = PaddingMode.PKCS7;

            // generate the salt and get buffer key
            Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes("", bufferSize);
            byte[] salt = deriveBytes.Salt;
            byte[] bufferKey = deriveBytes.GetBytes(bufferSize);

            // set the buffer key
            rj.Key = bufferKey;
            rj.GenerateIV();

            using (ICryptoTransform encrypt = rj.CreateEncryptor(rj.Key, rj.IV))
            {
                // encrypt the data
                byte[] dest = encrypt.TransformFinalBlock(data, 0, data.Length);

                // compile the encrypted data
                List<byte> compile = new List<byte>(salt);
                compile.AddRange(rj.IV);
                compile.AddRange(dest);
                return compile.ToArray();
            }
        }

        internal static byte[] Decrypt(byte[] data)
        {
            // setup the rijndael encryption
            RijndaelManaged rj = new RijndaelManaged();
            rj.BlockSize = blockSize;
            rj.KeySize = keySize;
            rj.Mode = CipherMode.CBC;
            rj.Padding = PaddingMode.PKCS7;

            List<byte> compile = new List<byte>(data);

            // extract the salt and iv
            List<byte> salt = compile.GetRange(0, bufferSize);
            List<byte> iv = compile.GetRange(bufferSize, bufferSize);
            rj.IV = iv.ToArray();

            // setup the rfc and get buffer key
            Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes("", salt.ToArray());
            byte[] bufferKey = deriveBytes.GetBytes(bufferSize);
            rj.Key = bufferKey;

            // extract the encrypted bytes
            byte[] plainBytes = compile.GetRange(bufferSize * 2, compile.Count - (bufferSize * 2)).ToArray();

            // decrypt the bytes and return them as an array
            using (ICryptoTransform decrypt = rj.CreateDecryptor(rj.Key, rj.IV))
            {
                byte[] dest = decrypt.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                return dest;
            }
        }
    }
}