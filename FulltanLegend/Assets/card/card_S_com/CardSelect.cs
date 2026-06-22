using UnityEngine;

public class CardSelect : MonoBehaviour
{
    //選択されたカードを一時的に保存
    public static GameObject selectedCard;
    //色変え
    private Renderer rend;
    private Color originalColor;

    //開始時に実行
    void Start()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
    }

    void OnMouseDown()
    {
        if (selectedCard != null)
        {
            Renderer oldRenderer = selectedCard.GetComponent<Renderer>();
            oldRenderer.material.color = originalColor;
        }

        selectedCard = gameObject;
        rend.material.color = Color.yellow;
    }
}