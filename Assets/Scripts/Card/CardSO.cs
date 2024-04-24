using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Card Data", menuName = "Scriptable Object/Card Data")]
public class CardSO : ScriptableObject
{
    [Header("카드 정보")]
    public int cardID;
    public string cardName;
    public CardGrade grade;
    public int cost;
    public int power;
    public int health;

    public byte GetLimitCount()
    {
        if (grade == CardGrade.Lengend) return 1;
        return 2;
    }
}
