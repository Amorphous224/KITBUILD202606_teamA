using UnityEngine;
using TMPro;

public class CardController : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMeshPro;
    [SerializeField] private Renderer cardRenderer; // 🎨 カードの見た目（色）を変えるための部屋

    // 💡 静的（static）な変数を使うことで、これまで何枚カードが作られたかをプログラムが記憶します
    private static int cardCount = 0;

    // 📘 学習カードのリスト
    private string[] studyCards = new string[]
    {
        "出席", "レポート提出", "テスト勉強"
    };

    // ⚡ 効果カードのリスト
    private string[] effectCards = new string[]
    {
        "勉強会", "先輩のアドバイス", "レポート未提出", "インフルエンザ",
        "徹夜", "過去問ゲット"
    };

    void Start()
    {
        // 🔄 生まれた瞬間に、自分の位置を枚数に応じて右にずらす（1枚ごとに0.3メートル）
        transform.position += new Vector3(cardCount * -0.3f, 0, 0);
        cardCount++; // 次に生まれるカードのためにカウントを1増やす

        if (textMeshPro == null || cardRenderer == null) return;

        // 🎲 確率50%で「学習」か「効果」か決める
        bool isStudyCard = Random.Range(0, 2) == 0;

        if (isStudyCard)
        {
            // 📘 学習カードの設定
            int randomIndex = Random.Range(0, studyCards.Length);
            textMeshPro.text = studyCards[randomIndex];
            
            // 🎨 色を「青」にする
            cardRenderer.material.color = new Color(0.0f, 0.4f, 1.0f);
        }
        else
        {
            // ⚡ 効果カードの設定
            int randomIndex = Random.Range(0, effectCards.Length);
            textMeshPro.text = effectCards[randomIndex];
            
            // 🎨 色を「黄色」にする
            cardRenderer.material.color = new Color(1.0f, 0.8f, 0.0f);
        }
    }

    // ゲームを終了（再生を停止）したときや、カードがリセットされたときのための処理
    void OnDestroy()
    {
        // 画面内からカードが1枚もなくなったら、カウントを0にリセットします
        if (GameObject.FindObjectsByType<CardController>(FindObjectsSortMode.None).Length == 0)
        {
            cardCount = 0;
        }
    }
}