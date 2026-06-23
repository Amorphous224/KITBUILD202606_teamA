using UnityEngine;

public class EffectZoneController : MonoBehaviour
{
    // ゾーンに置かれた効果カードの枚数
    private static int effectCardCount = 0;

    private void OnMouseDown()
    {
        if (CardController.SelectedCard != null)
        {
            if (CardController.SelectedCard.CurrentCardType == CardController.CardType.Effect)
            {
                // 🌟 置かれた枚数に応じて、右側に「0.4s」ずつズラして並べる
                Vector3 targetPosition = transform.position + new Vector3(effectCardCount * 0.4f, 0, 0);
                
                CardController.SelectedCard.transform.position = targetPosition;
                
                // 角度はゾーン（あるいは手札と同じ向き）に合わせる
                CardController.SelectedCard.transform.rotation = transform.rotation;

                effectCardCount++;

                // 🌟 配置が終わったら選択をクリアする
                CardController.ClearSelection();
                Debug.Log("効果カードを場に並べた！");
            }
            else
            {
                Debug.LogWarning("ここには効果カードしか置けません！");
            }
        }
    }

    // ゲームリセット時などのために枚数をリセットする関数
    public static void ResetCount()
    {
        effectCardCount = 0;
    }
}