using UnityEngine;

public class CardTrriger : MonoBehaviour
{
    void OnMouseDown()
{
    if (CardSelect.selectedCard == null)
        return;
    //選択したカードをcardに一時保存
    GameObject card = CardSelect.selectedCard;

    card.transform.position =
        transform.position + Vector3.up * 0.05f;

    card.transform.rotation =
        Quaternion.Euler(90f, 0f, 0f);

    CardSelect.selectedCard = null;
}
}