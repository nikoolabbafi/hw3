
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(TextMeshProUGUI))]
public class BombCountHUD : MonoBehaviour
{
    [Header("Display Settings")]

    [SerializeField]
    private string textPrefix = "BOMBS: ";


    private TMP_Text bombCountText;


    private void Awake()
    {
        bombCountText = GetComponent<TMP_Text>();
    }


    private void OnEnable()
    {
        GameEvents.BombCountChanged += UpdateBombCount;

        int initialCount = GameManager.Instance != null
            ? GameManager.Instance.BombCount
            : 2;

        UpdateBombCount(initialCount);
    }


    private void OnDisable()
    {
        GameEvents.BombCountChanged -= UpdateBombCount;
    }


    private void UpdateBombCount(int count)
    {
        if (bombCountText == null)
            return;

        bombCountText.text =
            $"{textPrefix}{Mathf.Max(0, count)}";
    }
}
