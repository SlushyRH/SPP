/*
 * This code written by me, SlushyRH (https://github.com/SlushyRH), and all copyright goes to me.
 * The license for this code is at https://github.com/SlushyRH/SPP/blob/master/LICENSE.
 */

using System;
using UnityEngine;

namespace SRH
{
    public static class SPP
    {
        #region Default Methods
        /// <summary>
        /// Check if there is already a player pref that exists with this key.
        /// </summary>
        /// <returns>True if key exists. False if key doesn't exists.</returns>
        public static bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        /// <summary>
        /// Writes all modifed preferences to the players disk.
        /// </summary>
        public static void Save()
        {
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Deletes the key from the players disk. Cannot reserve this.
        /// </summary>
        /// <param name="key"></param>
        public static void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }

        /// <summary>
        /// Deletes all keys and values from the players disk. Cannot reserve this.
        /// </summary>
        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }
        #endregion Default Methods

        #region Custom Methods
        /// <summary>
        /// Sets a value to be saved on the players disk identified by the given key. You can use PlayerPrefsPlus.Get to retrieve this value.
        /// </summary>
        /// <param name="key">The identifying string.</param>
        /// <param name="value">The value to be saved.</param>
        public static void Set(string key, object value)
        {
            // serialize data
            string content = SPPUtility.SerializeToBinary(value);

            // Sets the value (encrypted or not) to player prefs
            PlayerPrefs.SetString(key, "SPP-" + content);
        }

        /// <summary>
        /// Sets an encrypted value to be saved on the players disk identified by the given key. You can use PlayerPrefsPlus.Get to retrieve this value.
        /// </summary>
        /// <param name="key">The identifying string.</param>
        /// <param name="value">The value to be saved.</param>
        public static void SetEncrypted(string key, object value)
        {
            // encrypt data
            string encryptedContent = SPPUtility.SerializeToBinary(value);
            byte[] data = SPPUtility.Encrypt(Convert.FromBase64String(encryptedContent));

            // Convert encrypted data to string
            string content = Convert.ToBase64String(data);
            string encryptionKeyword = "[SRH]";

            // Sets the value (encrypted or not) to player prefs
            string finalContent = "SPP-" + encryptionKeyword + content;
            PlayerPrefs.SetString(key, finalContent);
        }

        /// <summary>
        /// Returns the value corresponding to key in the players disk if it exists.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="key">The identifier.</param>
        /// <returns>If it doesn't exist, it will return null.</returns>
        public static T Get<T>(string key)
        {
            if (SPP.HasKey(key))
            {
                try
                {
                    // Get content from player prefs
                    string content = PlayerPrefs.GetString(key);
                    object value = null;

                    // Check if the pref is encrypted
                    if (SPPUtility.IsEncrypted(key))
                    {
                        // Decrypt the string and convert it to a string
                        string rawContent = content.Remove(0, 9);
                        byte[] data = SPPUtility.Decrypt(Convert.FromBase64String(rawContent));
                        string encrypted = Convert.ToBase64String(data);

                        // Deserialize encrypted string to binary
                        value = SPPUtility.DeserializeToBinary<object>(encrypted);
                    }
                    else
                    {
                        // Deserialize string to binary
                        string rawContent = content.Remove(0, 4);
                        value = SPPUtility.DeserializeToBinary<object>(rawContent);
                    }

                    // return value as the type defined by T
                    return (T)value;
                }
                catch
                {
                    // this error is thrown if the player pref is not serialized
                    Debug.LogError($"This pref [{key}] is not a SPP Player Pref and cannot be returned. Please either convert this to a SPP Player Pref or use PlayerPrefs.GetString/Int/Float.");
                    throw;
                }
            }

            return default;
        }
        #endregion Custom Methods
    }
}