using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CardController : MonoBehaviour
{
    public enum CardType { Study, Effect, Credit } 
    public CardType CurrentCardType { get; private set; } 

    [SerializeField] private TextMeshPro textMeshPro;
    [SerializeField] private Renderer cardRenderer;
    [SerializeField] private SpriteRenderer imageRenderer; 
    [SerializeField] private List<Sprite> cardImages; 

    private static int cardCount = 0;
    private static int fieldEffectCardCount = 0;
    private static int creditCardCount = 0; 

    private static CardController confirmingStudyCard = null;
    private static string currentTargetSlotName = "";

    public static CardController SelectedCard { get; private set; }

    private string[] studyCards = new string[] { "出席", "レポート提出", "テスト勉強" };
    private string[] effectCards = new string[] { "勉強会", "先輩のアドバイス", "レポート未提出", "インフルエンザ", "徹夜", "過去問ゲット" };

    private bool isPlacedOnField = false; 

    void Start()
    {
        if (cardCount >= 8) 
        {
            transform.position = new Vector3(0f, -100f, 0f);
            return; 
        }
        transform.position += new Vector3(cardCount * -0.3f, 0, 0);
        cardCount++;

        if (imageRenderer != null && cardImages != null && cardImages.Count > 0)
        {
            int randomImageIndex = Random.Range(0, cardImages.Count);
            imageRenderer.sprite = cardImages[randomImageIndex];
        }

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

    public void TriggerUnitConfirmation(string slotName)
    {
        confirmingStudyCard = this;
        currentTargetSlotName = slotName;
        Debug.LogWarning($"★『{currentTargetSlotName}』の上へ置かれました！【 Y 】か【 N 】を押してください！");
    }

    void Update()
    {
        if (confirmingStudyCard != this) return;

        if (Input.GetKeyDown(KeyCode.Y))
        {
            ExecuteGetCredit(); 
            ClearConfirmState();
        }
        else if (Input.GetKeyDown(KeyCode.N))
        {
            ClearConfirmState();
        }
    }

    private void OnMouseDown()
    {
        // 🌟【絶対に画面移行させない修正】
        if (isPlacedOnField)
        {
            Debug.Log($"場に出ていた効果カード『{textMeshPro.text}』を画面外へ除外しました。");
            
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
            
            transform.position = new Vector3(0f, -1000f, 0f);
            gameObject.SetActive(false); 
            return;
        }

        if (CurrentCardType == CardType.Effect)
        {
            GameObject zone = GameObject.FindWithTag("EffectZone");
            if (zone != null)
            {
                Vector3 targetPosition = zone.transform.position + new Vector3(fieldEffectCardCount * 0.3f, 0f, -0.1f);
                transform.position = targetPosition;
                transform.rotation = zone.transform.rotation; 
                
                fieldEffectCardCount++;
                isPlacedOnField = true; 
                Debug.Log($"効果カード『{textMeshPro.text}』を場に出しました。");
            }
            return;
        }

        if (CurrentCardType == CardType.Study)
        {
            SelectedCard = this;
            return;
        }
    }

private void ExecuteGetCredit()
    {
        GameObject creditZone = GameObject.FindWithTag("CreditZone");
        GameObject targetSlot = GameObject.Find(currentTargetSlotName);

        if (creditZone != null && targetSlot != null)
        {
            GameObject creditObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            creditObject.name = "CreditCard_Object";

            Collider col = creditObject.GetComponent<Collider>();
            if (col != null) Destroy(col);

            creditObject.transform.localScale = new Vector3(0.2f, 0.01f, 0.3f); 
            Vector3 spawnOffset = new Vector3(0f, 0.05f + (creditCardCount * 0.025f), 0f); 
            creditObject.transform.position = creditZone.transform.position + spawnOffset;
            creditObject.transform.rotation = Quaternion.identity; 

            Renderer objRenderer = creditObject.GetComponent<Renderer>();
            if (objRenderer != null) objRenderer.material.color = new Color(1.0f, 0.2f, 0.2f); 

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddCredit(2); 
            }

            creditCardCount++;
            
            // 使用済みの学習カードも同様に地下へワープさせて安全に消す
            Collider myCol = GetComponent<Collider>();
            if (myCol != null) myCol.enabled = false;
            transform.position = new Vector3(0f, -1000f, 0f);
            gameObject.SetActive(false);
        }
    }

    private void ClearConfirmState()
    {
        confirmingStudyCard = null;
        currentTargetSlotName = "";
    }

    public static void ClearSelection() { SelectedCard = null; }

    void OnDisable()
    {
        CardController[] allCards = GameObject.FindObjectsByType<CardController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        bool anyActive = false;
        foreach (var c in allCards)
        {
            if (c.gameObject.activeSelf) anyActive = true;
        }
        if (!anyActive)
        {
            cardCount = 0;
            fieldEffectCardCount = 0;
            creditCardCount = 0;
            ClearConfirmState();
        }
    }
}