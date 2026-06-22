using UnityEngine;

public class SlotController : MonoBehaviour
{
    // 🖱️ スロット（置き場）をクリックしたときの処理
    public void OnInteract()
    {
        // 現在、選択されている手札カードがあるか確認する
        if (CardController.SelectedCard != null)
        {
            // 選択されている手札カードを、このスロットの少し上（Y軸に+0.02mずつ重ねるなど）に移動させる
            // 重なりが見えやすいように、少し高さをずらして配置する
            Vector3 targetPosition = transform.position + new Vector3(0, 0.05f, 0);
            CardController.SelectedCard.transform.position = targetPosition;

            Debug.Log("スロットにカードを配置した。");
        }
    }
}