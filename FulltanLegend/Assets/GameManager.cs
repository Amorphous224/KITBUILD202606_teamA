using UnityEngine;
using System.Collections.Generic; // リスト機能を使うための拡張パック

public class GameManager : MonoBehaviour
{
    // 【お題部屋】Unityの画面から、好きなだけお題を追加できるリストです
    [SerializeField] private List<string> odaiList = new List<string>();

    // 【カード部屋】先ほど作った CardController をここに登録します
    [SerializeField] private CardController cardController;

    // ゲームが始まった瞬間に自動で実行される命令
    void Start()
    {
        if (odaiList.Count > 0 && cardController != null)
        {
            // リストの「0番目（最初）」にあるお題をカードに送る
            cardController.SetCardText(odaiList[0]);
        }
    }
}
