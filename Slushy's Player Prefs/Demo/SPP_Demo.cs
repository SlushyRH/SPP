using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SRH; // reference SRH

public class SPP_Demo : MonoBehaviour
{
    // you can put this script on an object then press play and you will be able to see these values in the Pref Viewer.
    private void Start()
    {
        SPP.Set("keyName", "prefValue"); // then simply call this method to set an unencrypted player pref
        SPP.SetEncrypted("encryptedKeyName", "encryptedPrefValue"); // then simply call this method to set an encrypted player pref

        SPP.Get<string>("keyName"); // this will get the string saved
        SPP.Get<string>("encryptedKeyName"); // this will get the encrypted string saved

        // this will not work, if you set a pref as an SPP one, you can only get it via SPP.Get and vice versa
        // you can convert these values in the SRH tab in the toolbar, once they are converted you can access them via SPP.Get 
        // but will need to update your Set method.
        //
        //      PlayerPrefs.SetInt("defaultInt", 0);
        //      SPP.Get<int>("defaultInt");
    }
}