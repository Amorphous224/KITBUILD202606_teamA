using UnityEngine;
using TMPro;

public class UnitCardController : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMeshPro;
    [SerializeField] private Renderer cardRenderer;

    // 大学の単位名の一覧
    private string[] creditNames = new string[]
    {
        "線形代数", "微分積分学", "英語総合", "プログラミング基礎"
    };

    void Start()
    {
        if (textMeshPro == null || cardRenderer == null) return;

        // ランダムで単位名を選択して表示
        int randomIndex = Random.Range(0, creditNames.Length);
        textMeshPro.text = creditNames[randomIndex];

        // 単位カードの色を赤色に設定
        cardRenderer.material.color = new Color(0.9f, 0.1f, 0.1f);
    }
}