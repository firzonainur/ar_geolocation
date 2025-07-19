using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TBEasyWebCam;

public class QRDecodeTest : MonoBehaviour
{
	public QRCodeDecodeController e_qrController;

	public Text UiText;

	public GameObject resetBtn;

	public GameObject scanLineObj;
    
	public Image torchImage;

    public Button openUrlButton;
    private string lastScannedText = "";
    /// <summary>
    /// when you set the var is true,if the result of the decode is web url,it will open with browser.
    /// </summary>
    public bool isOpenBrowserIfUrl;

	private void Start()
	{
        if (openUrlButton != null)
        {
            openUrlButton.gameObject.SetActive(false);
        }
    }

	private void Update()
	{
	}

	public void qrScanFinished(string dataText)
	{
        Debug.Log(dataText);
        lastScannedText = dataText;

        if (isOpenBrowserIfUrl && Utility.CheckIsUrlFormat(dataText))
        {
            if (!dataText.Contains("http://") && !dataText.Contains("https://"))
            {
                dataText = "http://" + dataText;
            }
            Application.OpenURL(dataText);
        }

        this.UiText.text = dataText;

        if (this.resetBtn != null)
            this.resetBtn.SetActive(true);

        if (this.scanLineObj != null)
            this.scanLineObj.SetActive(false);

        if (openUrlButton != null)
        {
            bool isValidUrl = !string.IsNullOrEmpty(dataText) && Utility.CheckIsUrlFormat(dataText);
            openUrlButton.gameObject.SetActive(isValidUrl);
        }

    }

    public void OnOpenUrlButtonClicked()
    {
        if (Utility.CheckIsUrlFormat(lastScannedText))
        {
            string url = lastScannedText;
            if (!url.Contains("http://") && !url.Contains("https://"))
            {
                url = "http://" + url;
            }
            Application.OpenURL(url);
        }
    }

    public void Reset()
	{
		if (this.e_qrController != null)
		{
			this.e_qrController.Reset();
		}

		if (this.UiText != null)
		{
			this.UiText.text = string.Empty;
		}
		if (this.resetBtn != null)
		{
			this.resetBtn.SetActive(false);
		}
		if (this.scanLineObj != null)
		{
			this.scanLineObj.SetActive(true);
		}
        if (openUrlButton != null)
        {
            openUrlButton.gameObject.SetActive(false);
        }
    }

	public void Play()
	{
		Reset ();
		if (this.e_qrController != null)
		{
			this.e_qrController.StartWork();
		}
	}

	public void Stop()
	{
		if (this.e_qrController != null)
		{
			this.e_qrController.StopWork();
		}

		if (this.resetBtn != null)
		{
			this.resetBtn.SetActive(false);
		}
		if (this.scanLineObj != null)
		{
			this.scanLineObj.SetActive(false);
		}
	}

    public void GotoNextScene(string targetSceneName)
    {
        if (this.e_qrController != null)
        {
            this.e_qrController.StopWork();
        }

        string url = lastScannedText;

        if (!string.IsNullOrEmpty(url))
        {
            if (!url.Contains("http://") && !url.Contains("https://"))
            {
                url = "http://" + url;
            }

            PlayerPrefs.SetString("WebUrl", url);
            PlayerPrefs.Save();

            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogWarning("URL kosong, tidak bisa pindah scene.");
        }
    }


}
