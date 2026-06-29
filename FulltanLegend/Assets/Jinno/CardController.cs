using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CardController : MonoBehaviour
{
    // --- カードの種類を定義 ---
    public enum CardType { Study, Effect, Credit } 
    public CardType CurrentCardType { get; private set; } 

    [Header("---- 画面に表示するコンポーネント ----")]
    [SerializeField] private TextMeshPro textMeshPro;       // カードに表示する文字の枠（非表示にします）
    [SerializeField] private Renderer cardRenderer;         // カードの土台（フチの色用に使用します）
    [SerializeField] private SpriteRenderer imageRenderer;   // イラストを表示する枠

    [Header("---- カードのイラスト（種類ごとに分けて登録します） ----")]
    [SerializeField] private List<Sprite> studyCardImages;   // 🌟【改善】学習カード用のイラストリスト
    [SerializeField] private List<Sprite> effectCardImages;  // 🌟【改善】効果カード用のイラストリスト

    // --- 全員で共有する枚数カウンター ---
    private static int cardCount = 0;
    private static int fieldEffectCardCount = 0;
    private static int creditCardCount = 0; 

    // --- Y/Nキーの確認状態を管理する変数 ---
    private static CardController confirmingStudyCard = null;
    private static string currentTargetSlotName = "";

    // --- マウスで選択中の勉強カードをキープする変数 ---
    public static CardController SelectedCard { get; private set; }

    // --- 内部の判別用データ（テキストは非表示ですが、システム管理用に残します） ---
    private string[] studyCards = new string[] { "出席", "レポート提出", "テスト勉強" };
    private string[] effectCards = new string[] { "勉強会", "先輩のアドバイス", "レポート未提出", "インフルエンザ", "徹夜", "過去問ゲット" };

    private bool isPlacedOnField = false; // すでに場に出ているかどうかの目印

    // --- マウスホバー演出（拡大）用の変数 ---
    private Vector3 originalScale; // 元の大きさを一時的に記憶しておく
    private bool isHovered = false;  // マウスが乗っているかどうかの目印

    // 🌟 カードが生まれた瞬間に1回だけ実行される処理
    void Start()
    {
        // 手札が8枚以上の場合は、画面外（見えない場所）へ移動させて処理を終了
        if (cardCount >= 8) 
        {
            transform.position = new Vector3(0f, -100f, 0f);
            return; 
        }
        
        // 枚数に応じてカードを左に綺麗に並べる
        transform.position += new Vector3(cardCount * -0.3f, 0, 0);
        cardCount++;

        if (textMeshPro == null || cardRenderer == null || imageRenderer == null) return;

        // 🌟【テキストの消去】
        // イラストを見て判別するため、文字情報は空っぽにして完全に消去します
        textMeshPro.text = "";

        // 70%の確率で「勉強カード」、30%の確率で「効果カード」として内部の種類を決定
        bool isStudyCard = Random.Range(0, 10) >= 3; 

        if (isStudyCard)
        {
            // ----------------------------------------------------
            // 🌟 学習（勉強）カードとして生まれる処理
            // ----------------------------------------------------
            CurrentCardType = CardType.Study; 
            int randomIndex = Random.Range(0, studyCards.Length);
            
            // カードのベース（フチ）を「落ち着いた青色」にします
            cardRenderer.material.color = new Color(0.0f, 0.4f, 1.0f); 

            // 🌟 学習カード専用のイラストリストから、ランダムに1枚選んで表示します
            if (studyCardImages != null && studyCardImages.Count > 0)
            {
                int randomImageIndex = Random.Range(0, studyCardImages.Count);
                imageRenderer.sprite = studyCardImages[randomImageIndex];
            }
        }
        else
        {
            // ----------------------------------------------------
            // 🌟 効果カードとして生まれる処理
            // ----------------------------------------------------
            CurrentCardType = CardType.Effect; 
            int randomIndex = Random.Range(0, effectCards.Length);
            
            // カードのベース（フチ）を「落ち着いた黄色」にします
            cardRenderer.material.color = new Color(1.0f, 0.8f, 0.0f); 

            // 🌟 効果カード専用のイラストリストから、ランダムに1枚選んで表示します
            if (effectCardImages != null && effectCardImages.Count > 0)
            {
                int randomImageIndex = Random.Range(0, effectCardImages.Count);
                imageRenderer.sprite = effectCardImages[randomImageIndex];
            }
        }
    }

    // 🌟 マウスがカードの上に乗ったとき（ホバー開始）
    private void OnMouseEnter()
    {
        if (isPlacedOnField || confirmingStudyCard != null) return;

        if (!isHovered)
        {
            isHovered = true;
            originalScale = transform.localScale;            
            transform.localScale = originalScale * 1.5f;     
            transform.position += new Vector3(0f, 0.1f, 0f); 
        }
    }

    // 🌟 マウスがカードから離れたとき（ホバー終了）
    private void OnMouseExit()
    {
        if (isHovered)
        {
            isHovered = false;
            transform.localScale = originalScale;            
            transform.position -= new Vector3(0f, 0.1f, 0f); 
        }
    }

    // 🌟 勉強カードがスロットに置かれた時に、確認状態（Y/Nキー入力待ち）を開始する処理
    public void TriggerUnitConfirmation(string slotName)
    {
        if (isHovered) OnMouseExit(); 

        confirmingStudyCard = this;
        currentTargetSlotName = slotName;
        Debug.LogWarning($"★『{currentTargetSlotName}』の上へ置かれました！【 Y 】か【 N 】を押してください！");
    }

    // 🌟 毎フレーム常にキーボードの入力を監視する処理
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

    // 🌟 カードがマウスでクリックされたときの処理
    private void OnMouseDown()
    {
        if (isPlacedOnField)
        {
            Debug.Log("場に出ていた効果カードを画面外へ除外しました。");
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
            transform.position = new Vector3(0f, -1000f, 0f);
            gameObject.SetActive(false); 
            return;
        }

        // 効果カードだった場合：指定されたエフェクトゾーンへ自動で移動させる
        if (CurrentCardType == CardType.Effect)
        {
            GameObject zone = GameObject.FindWithTag("EffectZone");
            if (zone != null)
            {
                if (isHovered) OnMouseExit(); 

                Vector3 targetPosition = zone.transform.position + new Vector3(fieldEffectCardCount * 0.3f, 0f, -0.1f);
                transform.position = targetPosition;
                transform.rotation = zone.transform.rotation; 
                
                fieldEffectCardCount++;
                isPlacedOnField = true; 
                Debug.Log("効果カードを場に出しました。");
            }
            return;
        }

        // 勉強カードだった場合：このカードを現在選択中のカード（SelectedCard）としてキープ
        if (CurrentCardType == CardType.Study)
        {
            SelectedCard = this;
            return;
        }
    }

    // 🌟【単位カード（平べったい赤いキューブ）を生成する処理】
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