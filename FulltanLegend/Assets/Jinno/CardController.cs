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
            cardRenderer.material.color = new Color(0.0f, 0.4f, 1.0f); // 青色
        }
        else
        {
            CurrentCardType = CardType.Effect; 
            int randomIndex = Random.Range(0, effectCards.Length);
            textMeshPro.text = effectCards[randomIndex];
            cardRenderer.material.color = new Color(1.0f, 0.8f, 0.0f); // 黄色
        }
    }

    public void TriggerUnitConfirmation(string slotName)
    {
        confirmingStudyCard = this;
        currentTargetSlotName = slotName;

        Debug.LogWarning($"★『{currentTargetSlotName}』の上に学習カード『{textMeshPro.text}』が置かれました！この単位を獲得エリア（CreditZone）に移動しますか？ 【 Y 】か【 N 】を押してください！");
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
            Debug.Log($"『{textMeshPro.text}』による単位の獲得を見送りました。");
            ClearConfirmState();
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

        if (CurrentCardType == CardType.Study)
        {
            SelectedCard = this;
            Debug.Log($"手札の学習カード『{textMeshPro.text}』を選択中...（テーブルのSlot（単位）をクリックしてください）");
            return;
        }
    }

    private void ExecuteGetCredit()
    {
        GameObject creditZone = GameObject.FindWithTag("CreditZone");
        GameObject targetSlot = GameObject.Find(currentTargetSlotName);

        if (creditZone != null && targetSlot != null)
        {
            // 💡 1. 【エラー対策の要】Clusterスクリプトを100%排除した、純粋なUnityのCubeを動的生成！
            GameObject creditObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            creditObject.name = "CreditCard_Object";

            // 物理バグを防ぐため、コライダーを即座に削除
            Collider col = creditObject.GetComponent<Collider>();
            if (col != null) Destroy(col);

            // 💡 2. 形状を「カードと同じサイズ（薄い直方体）」にプログラムで変形
            // インスペクターのSlot1のサイズ(0.2, 0.1, 0.3)を参考に、カードっぽい薄さに調整
            creditObject.transform.localScale = new Vector3(0.2f, 0.01f, 0.3f); 

            // 💡 3. 位置と回転をテーブル左下のCreditZoneに合わせる
            // 獲得数に応じて左から右（X軸マイナス方向）へ綺麗に並べる
            // 🔄 修正後：獲得数に応じて「上（Y軸）」へ少しずつ積み重ねる
            Vector3 spawnOffset = new Vector3(0f, 0.05f + (creditCardCount * 0.025f), 0f); 
            creditObject.transform.position = creditZone.transform.position + spawnOffset;
            creditObject.transform.rotation = Quaternion.identity; // 正面を向かせる

            // 💡 4. 色を綺麗な赤色（獲得済み単位）にする
            Renderer objRenderer = creditObject.GetComponent<Renderer>();
            if (objRenderer != null)
            {
                objRenderer.material.color = new Color(1.0f, 0.2f, 0.2f); 
            }

            // 💡 5. 【補充の処理】
            // 元のSlot（台座）を「非表示（SetActive(false)）」にしないことで、
            // 「無くなった場所に即座に新しい単位カードが補充され、常に5枚ある状態」を完全再現します！
            Debug.Log($"『{currentTargetSlotName}』を獲得！その場に新しい単位が即座に補充されました。");

            creditCardCount++;

            // コストとして使った手札の青いカードは奈落へ退避
            transform.position = new Vector3(0f, -100f, 0f);
        }
    }

    private void ClearConfirmState()
    {
        confirmingStudyCard = null;
        currentTargetSlotName = "";
    }

    public static void ClearSelection() { SelectedCard = null; }

    void OnDestroy()
    {
        if (GameObject.FindObjectsByType<CardController>(FindObjectsSortMode.None).Length == 0)
        {
            cardCount = 0;
            fieldEffectCardCount = 0;
            creditCardCount = 0;
            ClearConfirmState();
        }
    }
}