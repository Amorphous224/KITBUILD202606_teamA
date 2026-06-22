using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // お題のリスト
    [SerializeField] private List<string> odaiList = new List<string>();

    void Start()
    {
        // カードへの命令は不要になったため、中身を空っぽにします
    }
}