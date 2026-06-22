using UnityEngine;
using TMPro;

public class CardController : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMeshPro;
    [SerializeField] private Renderer cardRenderer;

    private static int cardCount = 0;

    // 現在プレイヤーが選択して「動かそうとしている」カードを一時的に記録する変数
    public static CardController SelectedCard { get; private set; }

    private string[] studyCards = new string[] { "出席", "レポート提出", "テスト勉強" };
    private string[] effectCards = new string[] { "勉強会", "先輩のアドバイス", "レポート未提出", "インフルエンザ", "徹夜", "過去問ゲット" };

    void Start()
    {
        transform.position += new Vector3(cardCount * -0.3f, 0, 0);
        cardCount++;

        if (textMeshPro == null || cardRenderer == null) return;

        bool isStudyCard = Random.Range(0, 2) == 0;
        if (isStudyCard)
        {
            int randomIndex = Random.Range(0, studyCards.Length);
            textMeshPro.text = studyCards[randomIndex];
            cardRenderer.material.color = new Color(0.0f, 0.4f, 1.0f);
        }
        else
        {
            int randomIndex = Random.Range(0, effectCards.Length);
            textMeshPro.text = effectCards[randomIndex];
            cardRenderer.material.color = new Color(1.0f, 0.8f, 0.0f);
        }
    }

    // 🖱️ 手札カードをクリックしたときの処理
    public void OnInteract()
    {
        // このカードを「移動対象（選択状態）」として記録する
        SelectedCard = this;
        Debug.Log(textMeshPro.text + " が選択された。単位カードをクリック");
    }

    void OnDestroy()
    {
        if (GameObject.FindObjectsByType<CardController>(FindObjectsSortMode.None).Length == 0)
        {
            cardCount = 0;
        }
    }
}