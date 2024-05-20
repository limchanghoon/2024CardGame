using UnityEngine;

[CreateAssetMenu(fileName = "Card Data", menuName = "Scriptable Object/Card Data")]
public class CardSO : ScriptableObject
{
    [Header("카드 정보")]
    public int cardID;
    public string cardName;
    public CardType cardType;
    public CardGrade grade;
    public int cost;
    public int power;
    public int health;
    [TextArea] public string infomation;

    [Header("특수 능력")]
    public SpecialAbilityEnum speicalAbilityEnum;
    public GameObject battleCry;
    public TargetType battleCryTarget;
    public GameObject deathRattle;

    [Header("마법")]
    public TargetType magicTarget;
    public GameObject magic;

    public byte GetLimitCount()
    {
        if (grade == CardGrade.Lengend) return 1;
        return 2;
    }

    public bool IsTargetExist(CommandType commandType, GameManager gameManager)
    {
        TargetType curTargetType;
        switch (commandType)
        {
            case CommandType.BattleCry:
                if (battleCry == null) return false;
                curTargetType = battleCryTarget;
                break;
            case CommandType.DeathRattle:
                return false;
            case CommandType.Magic:
                if (magic == null) return false;
                curTargetType = magicTarget;
                break;
            default:
                return false;
        }
        if (curTargetType == 0) return false;

        if ((int)curTargetType == -1)
            curTargetType = TargetType.All;

        switch (curTargetType)
        {
            case TargetType.All:
                if (gameManager.heroMonos[0].CanBeTarget() || gameManager.heroMonos[1].CanBeTarget()) return true;
                Player OpponentPlayer = gameManager.GetOppenetPlayer();
                for (int i = 0; i < OpponentPlayer.field.Count; ++i)
                {
                    if (((CardMono_Minion)gameManager.GetCard(OpponentPlayer.field[i])).CanBeTarget()) return true;
                }
                Player MyPlayer = gameManager.GetMyPlayer();
                for (int i = 0; i < MyPlayer.field.Count; ++i)
                {
                    if (((CardMono_Minion)gameManager.GetCard(MyPlayer.field[i])).CanBeTarget()) return true;
                }
                return false;

            case TargetType.MyMinion:
                MyPlayer = gameManager.GetMyPlayer();
                for (int i = 0; i < MyPlayer.field.Count; ++i)
                {
                    if (((CardMono_Minion)gameManager.GetCard(MyPlayer.field[i])).CanBeTarget()) return true;
                }
                return false;

            case TargetType.OpponentMinion:
                OpponentPlayer = gameManager.GetOppenetPlayer();
                for (int i = 0; i < OpponentPlayer.field.Count; ++i)
                {
                    if (((CardMono_Minion)gameManager.GetCard(OpponentPlayer.field[i])).CanBeTarget()) return true;
                }
                return false;

            case TargetType.AllMinion:
                OpponentPlayer = gameManager.GetOppenetPlayer();
                for (int i = 0; i < OpponentPlayer.field.Count; ++i)
                {
                    if (((CardMono_Minion)gameManager.GetCard(OpponentPlayer.field[i])).CanBeTarget()) return true;
                }
                MyPlayer = gameManager.GetMyPlayer();
                for (int i = 0; i < MyPlayer.field.Count; ++i)
                {
                    if (((CardMono_Minion)gameManager.GetCard(MyPlayer.field[i])).CanBeTarget()) return true;
                }
                return false;

            case TargetType.TauntMinion://나중에 도발 찾아보자!
                OpponentPlayer = gameManager.GetOppenetPlayer();
                for (int i = 0; i < OpponentPlayer.field.Count; ++i)
                {
                    if (((CardMono_Minion)gameManager.GetCard(OpponentPlayer.field[i])).CanBeTarget()) return true;
                }
                MyPlayer = gameManager.GetMyPlayer();
                for (int i = 0; i < MyPlayer.field.Count; ++i)
                {
                    if (((CardMono_Minion)gameManager.GetCard(MyPlayer.field[i])).CanBeTarget()) return true;
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
                    if (((CardMono_Minion)gameManager.GetCard(MyPlayer.field[i])).CanBeTarget()) return true;
                }
                return false;

            case TargetType.Opponent:
                if (gameManager.GetOpponentHereMono().CanBeTarget()) return true;
                OpponentPlayer = gameManager.GetOppenetPlayer();
                for (int i = 0; i < OpponentPlayer.field.Count; ++i)
                {
                    if (((CardMono_Minion)gameManager.GetCard(OpponentPlayer.field[i])).CanBeTarget()) return true;
                }
                return false;

            default:
                return false;
        }
    }
}
