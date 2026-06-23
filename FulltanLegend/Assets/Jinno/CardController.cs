using UnityEngine;
using TMPro;

public class CardController : MonoBehaviour
{
    public enum CardType { Study, Effect }
    public CardType CurrentCardType { get; private set; } 

    [SerializeField] private TextMeshPro textMeshPro;
    [SerializeField] private Renderer cardRenderer;

    private static int cardCount = 0;
    private static int fieldEffectCardCount = 0;

    public static CardController SelectedCard { get; private set; }

    private string[] studyCards = new string[] { "出席", "レポート提出", "テスト勉強" };
    private string[] effectCards = new string[] { "勉強会", "先輩のアドバイス", "レポート未提出", "インフルエンザ", "徹夜", "過去問ゲット" };

    void Start()
    {
        // もともとの左にずれていく配置
        transform.position += new Vector3(cardCount * -0.3f, 0, 0);
        cardCount++;

        if (textMeshPro == null || cardRenderer == null) return;

        bool isStudyCard = Random.Range(0, 10) >= 3; 
        if (isStudyCard)
        {
            CurrentCardType = CardType.Study; 
            int randomIndex = Random.Range(0, studyCards.Length);
            textMeshPro.text = studyCards[randomIndex];
            cardRenderer.material.color = new Color(0.0f, 0.4f, 1.0f);
        }
        else
        {
            CurrentCardType = CardType.Effect; 
            int randomIndex = Random.Range(0, effectCards.Length);
            textMeshPro.text = effectCards[randomIndex];
            cardRenderer.material.color = new Color(1.0f, 0.8f, 0.0f);
        }
    }

    private void OnMouseDown()
    {
        if (CurrentCardType == CardType.Effect)
        {
            GameObject zone = GameObject.FindWithTag("EffectZone");

            if (zone != null)
            {
                Vector3 targetPosition = zone.transform.position + new Vector3(fieldEffectCardCount * 0.3f, 0f, -0.1f);
                transform.position = targetPosition;
                transform.rotation = zone.transform.rotation; 
                
                fieldEffectCardCount++;
                Debug.Log($"効果カード『{textMeshPro.text}』を場に出した");
            }
            return;
        }

        SelectedCard = this;
        Debug.Log($"学習カード『{textMeshPro.text}』を選択中");
    }

    public static void ClearSelection()
    {
        SelectedCard = null;
    }

    void OnDestroy()
    {
        if (GameObject.FindObjectsByType<CardController>(FindObjectsSortMode.None).Length == 0)
        {
            cardCount = 0;
            fieldEffectCardCount = 0;
        }
    }
}