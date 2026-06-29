using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; } //

    [SerializeField] private TextMeshProUGUI scoreText; // Canvas内のTextMeshPro(GUI)を登録する枠
    private int creditCount = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateScoreText();
    }

    // 単位を獲得した時に、CardControllerから呼び出される関数
    public void AddCredit(int amount)
    {
        creditCount += amount;
        UpdateScoreText();
    }

    // 画面のテキスト表示を更新する関数
    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = $"獲得単位数 : {creditCount}";
        }
    }
}
