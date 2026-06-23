using UnityEngine;

public class SlotController : MonoBehaviour
{
    // このスロットに今何枚カードが重なっているかを数えるカウンター
    private int placedCardCount = 0;

    // 🖱️ スロット（置き場）をマウスで直接クリックしたときの処理
    private void OnMouseDown()
    {
        // 現在、選択されている手札カードがあるか確認する
        if (CardController.SelectedCard != null)
        {
            // 1. 位置の計算：枚数に応じて、少しずつ上に浮かせ、手前（Z軸マイナス方向）にずらす
            float heightOffset = 0.01f * placedCardCount; // 1枚ごとに1cm上に浮く
            float forwardOffset = -0.02f * placedCardCount; // 1枚ごとに2cm手前にずれる
            
            Vector3 targetPosition = transform.position + new Vector3(0, 0.01f + heightOffset, forwardOffset);
            CardController.SelectedCard.transform.position = targetPosition;

            // 2. 角度の計算：スロットと同じ角度（ペタッと寝た状態）にする
            CardController.SelectedCard.transform.rotation = transform.rotation;

            // 枚数を1増やす
            placedCardCount++;

            Debug.Log($"スロットにカードを配置した。（合計: {placedCardCount} 枚）");
        }
    }
}