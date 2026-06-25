using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("---- 場のスロット管理 ----")]
    [SerializeField] private GameObject unitCardPrefab; 
    [SerializeField] private Transform[] slotPositions; 
    private GameObject[] currentCards = new GameObject[5]; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (slotPositions != null)
        {
            for (int i = 0; i < slotPositions.Length; i++)
            {
                ReplenishCard(i);
            }
        }
    }

    public void ReplenishCard(int slotIndex)
    {
        if (unitCardPrefab == null || slotPositions == null || slotIndex >= slotPositions.Length || slotPositions[slotIndex] == null) return;
        
        GameObject newCard = Instantiate(unitCardPrefab, slotPositions[slotIndex].position, slotPositions[slotIndex].rotation);
        currentCards[slotIndex] = newCard;
    }
}