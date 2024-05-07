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

    [Header("특수 능력")]
    public SpecialAbilityEnum speicalAbilityEnum;
    [TextArea] public string infomation;
    public GameObject battleCry;
    public GameObject deathRattle;

    public TargetType battleCryTarget;

    public byte GetLimitCount()
    {
        if (grade == CardGrade.Lengend) return 1;
        return 2;
    }

    public bool IsTargetExist(GameManager gameManager)
    {
        if (battleCry == null || battleCryTarget == 0) return false;

        if ((int)battleCryTarget == -1)
        {
            if (gameManager.heroMonos[0].CanBeTarget() || gameManager.heroMonos[1].CanBeTarget()) return true;
            Player OpponentPlayer = gameManager.GetOppenetPlayer();
            for (int i = 0; i < OpponentPlayer.field.Count; ++i)
            {
                if (gameManager.GetCard(OpponentPlayer.field[i]).CanBeTarget()) return true;
            }
            Player MyPlayer = gameManager.GetMyPlayer();
            for (int i = 0; i < MyPlayer.field.Count; ++i)
            {
                if (gameManager.GetCard(MyPlayer.field[i]).CanBeTarget()) return true;
            }
            return false;
        }

        switch (battleCryTarget)
        {
            case TargetType.MyMinion:
                Player MyPlayer = gameManager.GetMyPlayer();
                for (int i = 0; i < MyPlayer.field.Count; ++i)
                {
                    if (gameManager.GetCard(MyPlayer.field[i]).CanBeTarget()) return true;
                }
                return false;

            case TargetType.OpponentMinion:
                Player OpponentPlayer = gameManager.GetOppenetPlayer();
                for (int i = 0; i < OpponentPlayer.field.Count; ++i)
                {
                    if (gameManager.GetCard(OpponentPlayer.field[i]).CanBeTarget()) return true;
                }
                return false;

            case TargetType.AllMinion:
                OpponentPlayer = gameManager.GetOppenetPlayer();
                for (int i = 0; i < OpponentPlayer.field.Count; ++i)
                {
                    if (gameManager.GetCard(OpponentPlayer.field[i]).CanBeTarget()) return true;
                }
                MyPlayer = gameManager.GetMyPlayer();
                for (int i = 0; i < MyPlayer.field.Count; ++i)
                {
                    if (gameManager.GetCard(MyPlayer.field[i]).CanBeTarget()) return true;
                }
                return false;

            case TargetType.TauntMinion://나중에 도발 찾아보자!
                OpponentPlayer = gameManager.GetOppenetPlayer();
                for (int i = 0; i < OpponentPlayer.field.Count; ++i)
                {
                    if (gameManager.GetCard(OpponentPlayer.field[i]).CanBeTarget()) return true;
                }
                MyPlayer = gameManager.GetMyPlayer();
                for (int i = 0; i < MyPlayer.field.Count; ++i)
                {
                    if (gameManager.GetCard(MyPlayer.field[i]).CanBeTarget()) return true;
                }
                return false;

            case TargetType.Hero:
                if (gameManager.heroMonos[0].CanBeTarget() || gameManager.heroMonos[1].CanBeTarget()) return true;
                return false;

            case TargetType.Mine:
                if (gameManager.GetMyHereMono().CanBeTarget()) return true;
                MyPlayer = gameManager.GetMyPlayer();
                for (int i = 0; i < MyPlayer.field.Count; ++i)
                {
                    if (gameManager.GetCard(MyPlayer.field[i]).CanBeTarget()) return true;
                }
                return false;

            case TargetType.Opponent:
                if (gameManager.GetOpponentHereMono().CanBeTarget()) return true;
                OpponentPlayer = gameManager.GetOppenetPlayer();
                for (int i = 0; i < OpponentPlayer.field.Count; ++i)
                {
                    if (gameManager.GetCard(OpponentPlayer.field[i]).CanBeTarget()) return true;
                }
                return false;

            default:
                return false;
        }
    }
}
