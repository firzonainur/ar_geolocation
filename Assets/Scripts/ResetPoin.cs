using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPoin : MonoBehaviour
{
    private string playerPrefsKey = "TotalPoin";
    public void ResetPoinCoin()
    {
            PlayerPrefs.DeleteKey(playerPrefsKey);
    }
}
