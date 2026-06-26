using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class UnitCardController : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMeshPro;
    [SerializeField] private Renderer cardRenderer;
    [SerializeField] private SpriteRenderer imageRenderer; 
    [SerializeField] private List<Sprite> unitImages;      

    // 大学の単位名の一覧
    private string[] creditNames = new string[] 
    {
        "線形代数", "微分積分学", "英語総合", "プログラミング"
    };

    void Start()
    {
        // 最初もランダムにセットアップする
        SetupRandomCard();
    }

    // 🌟 外部から呼び出して、カードの名前と画像をランダムに新しく変更する機能
    public void SetupRandomCard()
    {
        // ランダムで単位の画像を1枚選択して表示
        if (imageRenderer != null && unitImages != null && unitImages.Count > 0)
        {
            int randomImageIndex = Random.Range(0, unitImages.Count);
            imageRenderer.sprite = unitImages[randomImageIndex];
        }

        if (textMeshPro == null || cardRenderer == null) return;

        // ランダムで単位名を選択して表示
        int randomIndex = Random.Range(0, creditNames.Length);
        textMeshPro.text = creditNames[randomIndex];

        // 単位カードの色を赤色に設定
        cardRenderer.material.color = new Color(1.0f, 0.2f, 0.2f); 
    }
}