using UnityEngine;
using System.Collections.Generic;

public class UnitCardController : MonoBehaviour
{
    [Header("表示用コンポーネント")]
    [SerializeField] private Renderer cardRenderer;         // カードの土台の見た目
    [SerializeField] private SpriteRenderer imageRenderer;   // カードのイラスト枠

    [Header("カードの素材データ")]
    [SerializeField] private List<Sprite> unitImages;        // ランダム用の画像リスト

    void Start()
    {
        // 生まれた瞬間にランダムなイラストに変身する
        SetupRandomCard();
    }

    // 🌟 カードの画像をランダムに新しく変更する機能
    public void SetupRandomCard()
    {
        // 1. 画像のランダム決定
        if (imageRenderer != null && unitImages != null && unitImages.Count > 0)
        {
            int randomImageIndex = Random.Range(0, unitImages.Count);
            imageRenderer.sprite = unitImages[randomImageIndex];
        }

        // 安全チェック：カードの土台（Renderer）がなければここで終了
        if (cardRenderer == null) return;

        // 2. 単位カードのベース色を赤色に染める
        cardRenderer.material.color = new Color(1.0f, 0.2f, 0.2f); 
    }
}