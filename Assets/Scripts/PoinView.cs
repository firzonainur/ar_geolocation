using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PoinView : MonoBehaviour
{
    public Slider sliderPoin;
    public TMP_Text textPoin;
    private int currentPoin = 0;
    private string playerPrefsKey = "TotalPoin";

    void Start()
    {
        currentPoin = PlayerPrefs.GetInt(playerPrefsKey, 0);
    }

    void Update()
    {
        sliderPoin.value = currentPoin;
        textPoin.text = "Poin: " + currentPoin.ToString();
    }
}
