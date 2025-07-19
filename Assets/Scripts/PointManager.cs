using UnityEngine;
using UnityEngine.UI;
using TMPro; // <-- Ini penting buat TMP

public class PointManager : MonoBehaviour
{
    public static PointManager Instance;

    public Slider sliderPoin, sliderPopUp;
    public TMP_Text textPoin, textPopUp; // <-- Ganti dari Text ke TMP_Text
    public GameObject rewardPanel;

    private int currentPoin = 0;
    private int poinMax = 1000;

    private string playerPrefsKey = "TotalPoin";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        currentPoin = PlayerPrefs.GetInt(playerPrefsKey, 0);
        UpdateUI();
        rewardPanel.SetActive(currentPoin >= poinMax);
    }

    public void TambahPoin(int jumlah)
    {
        if (currentPoin >= poinMax) return;

        currentPoin += jumlah;
        if (currentPoin >= poinMax)
        {
            currentPoin = poinMax;
            rewardPanel.SetActive(true);
        }

        PlayerPrefs.SetInt(playerPrefsKey, currentPoin);
        PlayerPrefs.Save();

        UpdateUI();
    }

    private void UpdateUI()
    {
        sliderPoin.value = currentPoin;
        sliderPopUp.value = currentPoin;
        textPoin.text = "Poin: " + currentPoin.ToString();
        textPopUp.text = "Poin: " + currentPoin.ToString();
    }

    // Optional reset method
    public void ResetPoin()
    {
        PlayerPrefs.DeleteKey(playerPrefsKey);
        currentPoin = 0;
        rewardPanel.SetActive(false);
        UpdateUI();
    }
}
