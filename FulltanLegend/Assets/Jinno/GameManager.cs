using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } 

    [Header("---- 場のスロット管理 ----")]
    [SerializeField] private GameObject unitCardPrefab; // 単位カードのプレハブをInspector上で設定するための変数
    [SerializeField] private Transform[] slotPositions; // 単位カードを配置するスロットの位置をInspector上で設定するための配列
    private GameObject[] currentCards = new GameObject[5]; // 現在のスロットに配置されている単位カードを管理する配列

    private void Awake() //ゲームが始まった瞬間呼び出される
    {
        if (Instance == null) Instance = this; // GameManagerのインスタンスがまだ存在しない場合は、現在のインスタンスを設定する
        else Destroy(gameObject); 
    }

    private void Start() //
    {
        if (slotPositions != null) // slotPositionsがnullでない場合にのみ処理を実行する
        {
            for (int i = 0; i < slotPositions.Length; i++) // slotPositionsの長さ分ループする
            {
                ReplenishCard(i); // 各スロットに単位カードを生成する
            }
        }
    }

    public void ReplenishCard(int slotIndex) // 指定されたスロットに単位カードを生成する処理
    {
        if (unitCardPrefab == null || slotPositions == null || slotIndex >= slotPositions.Length || slotPositions[slotIndex] == null) return;
        
        GameObject newCard = Instantiate(unitCardPrefab, slotPositions[slotIndex].position, slotPositions[slotIndex].rotation);
        currentCards[slotIndex] = newCard;
    }
}