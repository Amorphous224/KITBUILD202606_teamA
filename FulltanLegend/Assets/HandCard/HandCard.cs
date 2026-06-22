using UnityEngine;

// 手札置き場
public class HandCard : MonoBehaviour
{
    // カード同士の間隔
    [SerializeField] private float cardSpacing = 0.1f;

    // 板から手前に出す距離
    [SerializeField] private float forwardOffset = 0.15f;

    void OnMouseDown()
    {
        // カードが選択されていないなら何もしない
        if (CardSelect.selectedCard == null)
            return;

        GameObject card = CardSelect.selectedCard;

        // HandCardの子にする
        card.transform.SetParent(transform);

        // 今何枚あるか取得
        int index = transform.childCount - 1;

        // 左から順番に並べる
        float xPos = index * cardSpacing;

       // HandCardの面の手前に配置
        card.transform.position =
        transform.position
        + transform.right * xPos
        + transform.forward * forwardOffset;

        // HandCardと同じ向きにする
    card.transform.rotation = transform.rotation;

        // 選択解除
        CardSelect.selectedCard = null;
    }
}