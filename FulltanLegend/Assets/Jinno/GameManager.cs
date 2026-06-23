using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject unitCardPrefab; // 生成する単位カードのプレハブ
    [SerializeField] private Transform[] slotPositions; // 5つの場の位置（配列）

    // 各スロットに現在置かれているカードを記録する配列
    private GameObject[] currentCards = new GameObject[5];

    void Start()
    {
        // ゲーム開始時にすべてのスロットに単位カードを補充
        for (int i = 0; i < slotPositions.Length; i++)
        {
            ReplenishCard(i);
        }
    }

    void Update()
    {
        // アナログで獲得（カードが削除）されたら、空いた場所に自動補充する
        for (int i = 0; i < slotPositions.Length; i++)
        {
            if (currentCards[i] == null)
            {
                ReplenishCard(i);
            }
        }
    }

    // 指定された番号のスロットに新しく単位カードを生成する
    private void ReplenishCard(int slotIndex)
    {
        if (unitCardPrefab == null || slotPositions[slotIndex] == null) return;

        // 指定されたスロットの位置と向きでカードを生成
        GameObject newCard = Instantiate(unitCardPrefab, slotPositions[slotIndex].position, slotPositions[slotIndex].rotation);
        currentCards[slotIndex] = newCard;
    }
}