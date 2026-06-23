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
                // 枚数に応じて少しずつずらして重ねる
                float heightOffset = 0.02f * placedCardCount;   
                float forwardOffset = -0.01f * placedCardCount; 
                
                Vector3 targetPosition = transform.position + new Vector3(0f, 0.05f + heightOffset, forwardOffset);
                CardController.SelectedCard.transform.position = targetPosition;
                CardController.SelectedCard.transform.rotation = transform.rotation;

                placedCardCount++;

                Debug.Log($"スロット『{gameObject.name}』にカード配置完了 / 合計: {placedCardCount}枚");

                CardController.ClearSelection();
            }
            else
            {
                Debug.LogWarning("単位カードの上には学習カードしか置けません");
            }
        }
    }
}