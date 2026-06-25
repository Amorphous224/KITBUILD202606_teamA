using UnityEngine;

public class SlotController : MonoBehaviour
{
    private int placedCardCount = 0;

    private void OnMouseDown()
    {
        if (CardController.SelectedCard != null)
        {
            if (CardController.SelectedCard.CurrentCardType == CardController.CardType.Study)
            {
                // 置こうとしているカードを一時的にキープ
                CardController placedCard = CardController.SelectedCard;

                // 枚数に応じて少しずつずらして重ねる
                float heightOffset = 0.02f * placedCardCount;   
                float forwardOffset = -0.01f * placedCardCount; 
                
                Vector3 targetPosition = transform.position + new Vector3(0f, 0.05f + heightOffset, forwardOffset);
                placedCard.transform.position = targetPosition;
                placedCard.transform.rotation = transform.rotation;

                placedCardCount++;

                Debug.Log($"スロット『{gameObject.name}』にカード配置完了 / 合計: {placedCardCount}枚");

                // 💡 【新機能】置かれたカードに対して、選択肢を出すように直接命令する！
                placedCard.TriggerUnitConfirmation(gameObject.name);

                // 選択のクリアは命令の後に行う
                CardController.ClearSelection();
            }
            else
            {
                Debug.LogWarning("単位カードの上には学習カードしか置けません");
            }
        }
    }
}