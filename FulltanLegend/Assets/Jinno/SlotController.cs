using UnityEngine;

public class SlotController : MonoBehaviour
{
    private int placedCardCount = 0; // スロットに重ねられたカードの枚数

    private void OnMouseDown()
    {
        if (CardController.SelectedCard != null)
        {
            if (CardController.SelectedCard.CurrentCardType == CardController.CardType.Study)
            {
                CardController placedCard = CardController.SelectedCard;

                // 枚数に応じて少しずつ上にずらし、手前に重ねる計算
                float heightOffset = 0.02f * placedCardCount;   
                float forwardOffset = -0.01f * placedCardCount; 
                
                Vector3 targetPosition = transform.position + new Vector3(0f, 0.05f + heightOffset, forwardOffset);
                placedCard.transform.position = targetPosition;
                placedCard.transform.rotation = transform.rotation;

                placedCardCount++;

                // カード側に「このスロットに置かれたよ」と通知して確定モードへ
                placedCard.TriggerUnitConfirmation(gameObject.name);
                CardController.ClearSelection();
            }
        }
    }

    // 単位を獲得したときなどに、重ねた枚数のカウントをゼロに戻す処理
    public void ResetPlacedCount()
    {
        placedCardCount = 0;
    }
}