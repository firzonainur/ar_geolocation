using UnityEngine;
using UnityEngine.UI;

public class PointButton : MonoBehaviour
{
    public int poinYangDitambahkan = 10;
    private Button btn;
    public GameObject poin;

    void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnButtonClicked);
    }

    void OnButtonClicked()
    {
        if (PointManager.Instance != null)
        {
            PointManager.Instance.TambahPoin(poinYangDitambahkan);
        }
        poin.SetActive(false);

        Destroy(gameObject); // Hilangkan button setelah diklik
    }
}
