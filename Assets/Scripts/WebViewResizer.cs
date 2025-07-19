using System.Collections;
using UnityEngine;
using Vuplex.WebView;

public class WebViewResizer : MonoBehaviour
{

    public CanvasWebViewPrefab webViewPrefab;

    IEnumerator Start()
    {
        // Tunggu sampai WebView selesai diinisialisasi
        yield return webViewPrefab.WaitUntilInitialized();

        // Debug log untuk mengecek ukuran sebelum resize
        Debug.Log("Original Size: " + webViewPrefab.WebView.Size);

        // Ubah ukuran WebView ke 1080x1920
        webViewPrefab.WebView.Resize(1080, 1920);

        // Debug log untuk mengecek ukuran setelah resize
        Debug.Log("Resized to: " + webViewPrefab.WebView.Size);
    }
}
