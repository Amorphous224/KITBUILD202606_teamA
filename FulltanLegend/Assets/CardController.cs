using UnityEngine;
using TMPro; // カードの文字（TextMeshPro）を操作するために必要な拡張パックです

public class CardController : MonoBehaviour
{
    // 【設定部屋】カードにくっついている「文字コンポーネント」を登録する場所です
    [SerializeField] private TextMeshPro textMeshPro;

    // 【魔法の命令】外部から「この文字に書き換えて！」と頼まれたときに実行する窓口です
    public void SetCardText(string newText)
    {
        if (textMeshPro != null)
        {
            textMeshPro.text = newText;
        }
    }
}