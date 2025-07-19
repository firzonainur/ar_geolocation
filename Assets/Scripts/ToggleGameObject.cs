using UnityEngine;
using UnityEngine.UI;

public class ToggleGameObject : MonoBehaviour
{
    public Toggle toggle;
    public GameObject targetObject;

    private const string WebViewStatusKey = "WebViewStatus";

    void Start()
    {
        if (toggle != null && targetObject != null)
        {
            // Ambil status dari PlayerPrefs (default nonaktif)
            string status = PlayerPrefs.GetString(WebViewStatusKey, "nonaktif");
            bool isActive = status == "aktif";

            targetObject.SetActive(isActive);
            toggle.isOn = isActive;

            toggle.onValueChanged.AddListener(OnToggleChanged);
        }
    }

    void OnToggleChanged(bool isOn)
    {
        if (targetObject != null)
        {
            targetObject.SetActive(isOn);

            if (isOn)
            {
                AktifkanWebView();
            }
            else
            {
                NonaktifkanWebView();
            }
        }
    }

    public void AktifkanWebView()
    {
        PlayerPrefs.SetString(WebViewStatusKey, "aktif");
        PlayerPrefs.Save();
    }

    public void NonaktifkanWebView()
    {
        PlayerPrefs.SetString(WebViewStatusKey, "nonaktif");
        PlayerPrefs.Save();
    }
}
