using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Microsoft.Win32;
using System.Text;
using System;

namespace SRH
{
    public static class SPPEditorUtility
    {
        private static readonly string STR_ERROR_CODE = "<SPP_xXxThisShitDontWork69xXx>";
        private static readonly int INT_ERROR_CODE = int.MinValue;

        [MenuItem("SRH/SPP/Convert All Existing Prefs To SPP", false, 100)]
        private static void ConvertAllPrefsToSPP()
        {
            bool option = EditorUtility.DisplayDialog("Convert Player Pref",
                    "Are you sure you want to convert all existing prefs to SPP format.\n\nThis means you have to update your code to call SPP.Get and SPP.Set instead of the normal PlayerPrefs methods.",
                    "Convert",
                    "Cancel");

            if (option)
            {
                List<SPPEditor.PrefStruct> allPrefs = GetAllPrefs(true);
                foreach (var item in allPrefs)
                {
                    string str = PlayerPrefs.GetString(item.Key, "<SPP_xXxThisShitDontWork69xXx>");
                    if (str != STR_ERROR_CODE)
                        SPP.Set(item.Key, str);

                    float flt = PlayerPrefs.GetFloat(item.Key, float.NaN);
                    if (!float.IsNaN(flt))
                        SPP.Set(item.Key, flt);

                    int num = PlayerPrefs.GetInt(item.Key, INT_ERROR_CODE);
                    if (num != INT_ERROR_CODE)
                        SPP.Set(item.Key, num);
                }
            }
        }

        [MenuItem("SRH/SPP/Documentation", false, 100)]
        private static void Documenation()
        {
            Application.OpenURL($"");
        }

        internal static List<SPPEditor.PrefStruct> GetAllPrefs(bool isForEditor = false)
        {
            Dictionary<string, string> prefsDict = GetPrefsFromOs();
            List<SPPEditor.PrefStruct> prefs = new List<SPPEditor.PrefStruct>();

            // get key values
            foreach (var pref in prefsDict)
            {
                string key = pref.Key;
               
                // skip the unity system ones
                if (!key.StartsWith("unity") && !key.StartsWith("Unity"))
                {
                    if (!isForEditor)
                    {
                        // get deserialized data and add to pref struct
                        string value = EditorGetPrefs(pref.Value);
                        bool encrypted = SPPUtility.IsEncrypted(key);

                        prefs.Add(new SPPEditor.PrefStruct(key, key, value, encrypted));
                    }
                    else
                    {
                        // get raw data and add to pref struct if not SPP
                        string value = EditorGetRawPrefs(pref.Value);
                        bool encrypted = SPPUtility.IsEncrypted(key);

                        if (!value.StartsWith("SPP-"))
                            prefs.Add(new SPPEditor.PrefStruct(key, key, value, encrypted));
                    }
                }
            }

            return prefs;
        }

        private static Dictionary<string, string> GetPrefsFromOs()
        {
#if UNITY_EDITOR_WIN
            // get pref location
            string prefLocation = @$"SOFTWARE\Unity\UnityEditor\{Application.companyName}\{Application.productName}";
            RegistryKey reg = Registry.CurrentUser.OpenSubKey(prefLocation);

            // get key names
            string[] allKeys = reg.GetValueNames();
            Dictionary<string, string> prefs = new Dictionary<string, string>();

            // setup encoding shit
            Encoding utf8 = Encoding.UTF8;
            Encoding ansi = Encoding.GetEncoding(1252);

            // get bytes from keys
            for (int i = 0; i < allKeys.Length; i++)
            {
                allKeys[i] = allKeys[i].Substring(0, allKeys[i].LastIndexOf("_h"));
                prefs.Add(allKeys[i], utf8.GetString(ansi.GetBytes(allKeys[i])));
            }

            return prefs;
#elif UNITY_EDITOR_OSX
            
#elif UNITY_EDITOR_LINX
            
#endif
        }

        internal static string EditorGetPrefs(string key)
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

                // return value as string
                return value.ToString();
            }
            catch (Exception e)
            {
                try
                {
                    // try get string pref
                    string str = PlayerPrefs.GetString(key, "<SPP_xXxThisShitDontWork69xXx>");
                    if (str != STR_ERROR_CODE) // check if not error code
                        return str;

                    // try get float pref
                    float flt = PlayerPrefs.GetFloat(key, float.NaN);
                    if (!float.IsNaN(flt)) // check if not error code
                        return flt.ToString();

                    // try get int pref
                    int num = PlayerPrefs.GetInt(key, INT_ERROR_CODE);
                    if (num != INT_ERROR_CODE) // check if not error code
                        return num.ToString();
                }
                catch (Exception e2)
                {
                    Debug.LogError(e2.Message);
                    throw;
                }

                Debug.LogError(e.Message);
                return null;
            }
        }

        internal static string EditorGetRawPrefs(string key)
        {
            try
            {
                // try get string pref
                string str = PlayerPrefs.GetString(key, "<SPP_xXxThisShitDontWork69xXx>");
                if (str != STR_ERROR_CODE) // check if not error code
                    return str;

                // try get float pref
                float flt = PlayerPrefs.GetFloat(key, float.NaN);
                if (!float.IsNaN(flt)) // check if not error code
                    return flt.ToString();

                // try get int pref
                int num = PlayerPrefs.GetInt(key, INT_ERROR_CODE);
                if (num != INT_ERROR_CODE) // check if not error code
                    return num.ToString();

                return str;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return null;
            }
        }
    }
}