using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CardController : MonoBehaviour
{
    // --- カードの種類を定義 ---
    public enum CardType { Study, Effect, Credit }  // 列挙型
    public CardType CurrentCardType { get; private set; } 

    [Header("---- 画面に表示するコンポーネント ----")] //[Header(...)]でInspector上に見出しを表示できる
    /*
    *[SerializeField]の意味：privateの安全性を保ったまま、Unityのインスペクター画面に編集用の枠を出す設定
    */
    [SerializeField] private TextMeshPro textMeshPro;       // カードに表示する文字の枠（非表示にする）
    [SerializeField] private Renderer cardRenderer;         // カードの土台（フチの色用に使用する）
    [SerializeField] private SpriteRenderer imageRenderer;   // イラストを表示する枠

    [Header("---- カードのイラスト（種類ごとに分けて登録します） ----")]
    [SerializeField] private List<Sprite> studyCardImages;   // 学習カード用のイラストリスト
    [SerializeField] private List<Sprite> effectCardImages;  // 効果カード用のイラストリスト

    //獲得枚数カウンター
    private static int cardCount = 0;
    private static int fieldEffectCardCount = 0;
    private static int creditCardCount = 0; 

    // Y/Nキーの確認状態を管理する変数 
    private static CardController confirmingStudyCard = null;
    private static string currentTargetSlotName = "";

    // マウスで選択中の勉強カードをキープする変数 
    public static CardController SelectedCard { get; private set; } //get; データを読み取るのは自由、private set; データを書き換えるのはこのクラス内だけに制限する

    private bool isPlacedOnField = false; // すでに場に出ているかどうかの目印

    // マウスがカードに乗ったとき拡大される用の変数
    private Vector3 originalScale; // 元の大きさを一時的に記憶しておく
    private bool isHovered = false;  // マウスが乗っているかどうかの目印

 // カードが生まれた瞬間に1回だけ実行される処理
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

        // コンポーネントが正しくアタッチされていない場合は処理を中断
        if (textMeshPro == null || cardRenderer == null || imageRenderer == null) return;

        // イラストを見て判別するため、文字情報は空っぽにして完全に消去
        textMeshPro.text = "";

        // 70%の確率で「勉強カード」、30%の確率で「効果カード」として内部の種類を決定
        bool isStudyCard = Random.Range(0, 10) >= 3; 

        if (isStudyCard)
        {
            // 学習カードとして生まれる処理
            CurrentCardType = CardType.Study; 
            
            // カードのベース（フチ）を青色にします
            cardRenderer.material.color = new Color(0.0f, 0.4f, 1.0f); 

            // 学習カード専用のイラストリストから、ランダムに1枚選んで表示します
            if (studyCardImages != null && studyCardImages.Count > 0)
            {
                int randomImageIndex = Random.Range(0, studyCardImages.Count);
                imageRenderer.sprite = studyCardImages[randomImageIndex];
            }
        }
        else
        {
            // 効果カードとして生まれる処理
            CurrentCardType = CardType.Effect; 
            
            // カードのベース（フチ）を黄色にします
            cardRenderer.material.color = new Color(1.0f, 0.8f, 0.0f); 

            // 効果カード専用のイラストリストから、ランダムに1枚選んで表示します
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
        if (isPlacedOnField || confirmingStudyCard != null) return; // 場に出ているカードや確認状態のカードは拡大しない

        if (!isHovered) // まだ拡大されていない場合のみ拡大処理を行う
        {
            isHovered = true;
            originalScale = transform.localScale; // 元の大きさを記憶しておく        
            transform.localScale = originalScale * 1.5f; // カードを1.5倍に拡大する  
            transform.position += new Vector3(0f, 0.1f, 0f); // カードを少し上に持ち上げる
        }
    }

    // 🌟 マウスがカードから離れたとき（ホバー終了）
    private void OnMouseExit()
    {
        if (isHovered)
        {
            isHovered = false; 
            transform.localScale = originalScale;  // 元の大きさに戻す         
            transform.position -= new Vector3(0f, 0.1f, 0f); // カードを元の位置に戻す
        }
    }

    // 学習カードがスロットに置かれた時に、確認状態（Y/Nキー入力待ち）を開始する処理
    public void TriggerUnitConfirmation(string slotName)
    {
        if (isHovered) OnMouseExit(); // マウスが乗っている状態なら、強制的にホバー終了させる

        confirmingStudyCard = this; // このカードが確認状態の対象であることを記録する
        currentTargetSlotName = slotName; // どのスロットに置かれたかを記録する
    }

    //毎フレーム常にキーボードの入力を監視する処理
    void Update()
    {
        if (confirmingStudyCard != this) return; // このカードがYかNの確認状態の対象でない場合は何もしない

        if (Input.GetKeyDown(KeyCode.Y)) // Yキーが押された場合の処理
        {
            ExecuteGetCredit(); // 単位カードを生成する処理を実行
            ClearConfirmState(); // 確認状態をクリアする
        }
        else if (Input.GetKeyDown(KeyCode.N)) // Nキーが押された場合の処理
        {
            ClearConfirmState(); // 確認状態をクリアする
        }
    }

    // カードがマウスでクリックされたときの処理
    private void OnMouseDown()
    {
        if (isPlacedOnField)
        {
            Collider col = GetComponent<Collider>(); // 場に出ている効果カードはクリックされたら消えるようにする
            if (col != null) col.enabled = false; // クリックされたらコライダーを無効化して、再度クリックされないようにする 
            transform.position = new Vector3(0f, -1000f, 0f); // カードを画面外に移動させる
            gameObject.SetActive(false);  // カードを非表示にする
            return;
        }

        // 効果カードだった場合：指定されたエフェクトゾーンへ自動で移動させる
        if (CurrentCardType == CardType.Effect)
        {
            GameObject zone = GameObject.FindWithTag("EffectZone");
            if (zone != null)
            {
                if (isHovered) OnMouseExit();  // マウスが乗っている状態なら、強制的にホバー終了させる

                Vector3 targetPosition = zone.transform.position + new Vector3(fieldEffectCardCount * 0.3f, 0f, -0.1f); // 効果カードを横に並べるためのオフセットを追加
                transform.position = targetPosition; // 効果カードをエフェクトゾーンの位置に移動させる
                transform.rotation = zone.transform.rotation; // 効果カードの回転をエフェクトゾーンに合わせる
                
                fieldEffectCardCount++; // 効果カードの枚数をカウントアップ
                isPlacedOnField = true;  // 場に出ている状態にする
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

    // 単位カード（平べったい赤いキューブ）を生成する処理、単位カード獲得の処理
    private void ExecuteGetCredit()
    {
        GameObject creditZone = GameObject.FindWithTag("CreditZone"); // CreditZoneタグのオブジェクトを探す
        GameObject targetSlot = GameObject.Find(currentTargetSlotName);// どのスロットに置かれたかを名前で探す

        if (creditZone != null && targetSlot != null) // CreditZoneとスロットが両方存在する場合のみ単位カードを生成する
        {
            GameObject creditObject = GameObject.CreatePrimitive(PrimitiveType.Cube); // 単位カードとして立方体を生成する
            creditObject.name = "CreditCard_Object"; 

            Collider col = creditObject.GetComponent<Collider>(); // 生成した立方体のコライダーを取得
            if (col != null) Destroy(col);

            creditObject.transform.localScale = new Vector3(0.2f, 0.01f, 0.3f); // 単位カードの大きさを設定する
            Vector3 spawnOffset = new Vector3(0f, 0.05f + (creditCardCount * 0.025f), 0f); // 単位カードを少しずつ積み重ねるためのオフセットを設定する
            creditObject.transform.position = creditZone.transform.position + spawnOffset; // 単位カードをCreditZoneの位置に生成する
            creditObject.transform.rotation = Quaternion.identity; // 単位カードの回転をリセットする

            Renderer objRenderer = creditObject.GetComponent<Renderer>(); 
            if (objRenderer != null) objRenderer.material.color = new Color(1.0f, 0.2f, 0.2f); 

            if (ScoreManager.Instance != null) // ScoreManagerのインスタンスが存在する場合にのみ単位を加算する
            {
                ScoreManager.Instance.AddCredit(2); //スコアを２加算する
            }

            creditCardCount++; 
            
            Collider myCol = GetComponent<Collider>(); 
            if (myCol != null) myCol.enabled = false; 
            transform.position = new Vector3(0f, -1000f, 0f);
            gameObject.SetActive(false);
        }
    }

    private void ClearConfirmState() // Y/Nキーの確認状態をクリアする処理
    {
        confirmingStudyCard = null; // 確認状態の対象カードをリセット
        currentTargetSlotName = ""; // どのスロットに置かれたかの情報をリセット
    }

    public static void ClearSelection() { SelectedCard = null; } // 選択中のカードをクリアする処理

    void OnDisable() // カードが非表示になったときに呼ばれる処理
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