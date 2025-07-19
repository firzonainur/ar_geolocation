using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vuplex.WebView;

public class LoadURLManager : MonoBehaviour
{
    public CanvasWebViewPrefab webViewPrefab;

    async void Start()
    {
        await webViewPrefab.WaitUntilInitialized();

        string url = PlayerPrefs.GetString("WebUrl", "https://default.com"); // URL default jika belum diset
        webViewPrefab.WebView.LoadUrl(url);
    }

    void Update()
    {
        // Tangani tombol back (Android) atau Escape (PC)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("QR-Code"); // Ganti dengan nama scene sebelumnya
        }
    }
}
