using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    [Header("UI Components")]
    public Slider loadingSlider;
    public TextMeshProUGUI loadingText;
    public Image circleLoadingImage; // <- Tambahan: Circle bar Image (type Fill)

    [Header("Scene Settings")]
    public string sceneToLoad = "NextScene";

    [Header("Loading Time Settings")]
    public float minimumLoadingTime = 5f; // Durasi loading minimal

    [Header("Circle Rotation Settings")]
    public float rotationSpeed = 200f; // Kecepatan rotasi (degree per second)

    private void Start()
    {
        StartCoroutine(LoadSceneWithFakeProgress());
    }

    IEnumerator LoadSceneWithFakeProgress()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;

        float elapsedTime = 0f;

        while (elapsedTime < minimumLoadingTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / minimumLoadingTime);

            // Update Slider dan Text Persentase
            loadingSlider.value = progress;
            loadingText.text = Mathf.RoundToInt(progress * 100f) + "%";

            // Update Circle Fill (jika ada)
            if (circleLoadingImage != null)
            {
                circleLoadingImage.fillAmount = progress;
                circleLoadingImage.transform.Rotate(Vector3.forward, -rotationSpeed * Time.deltaTime);
            }

            yield return null;
        }

        // Tunggu sampai proses asli loading sudah siap
        while (operation.progress < 0.9f)
        {
            yield return null;
        }

        // Finalize
        loadingSlider.value = 1f;
        loadingText.text = "100%";
        if (circleLoadingImage != null)
        {
            circleLoadingImage.fillAmount = 1f;
        }

        operation.allowSceneActivation = true;
    }
}
