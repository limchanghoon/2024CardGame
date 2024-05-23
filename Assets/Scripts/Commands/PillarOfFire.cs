using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarOfFire : NetworkBehaviour, ICommand
{
    [SerializeField] GameObject effect;
    [SerializeField] int damage;

    public void Execute(CardMono mine, NetworkId target, CommandType _commandType)
    {
        if (!mine.owner.IsMyTurn()) return;
        RPC_Execute(mine.owner.gameManager.GetOppenetPlayer().networkObject);

        // ав╦ч!
        mine.owner.gameManager.DoDeathRattleOneLayer();
        mine.owner.gameManager.RPC_EnqueueChangeField();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_Execute(NetworkObject _target)
    {
        var _opponentPlayer = _target.GetComponent<Player>();
        List<int> damageList = new List<int>();
        List<CardMono_Minion> cardList = new List<CardMono_Minion>();
        for (int i = _opponentPlayer.field.Count - 1; i >= 0; --i)
        {
            var fieldCard = (CardMono_Minion)_opponentPlayer.GetMyCard(_opponentPlayer.field[i]);
            int _damage = fieldCard.PredictHit(damage);

            fieldCard.Hit(damage);
            fieldCard.CheckIsFirstDie();

            damageList.Add(_damage);
            cardList.Add(fieldCard);
        }
        GameManager.actionQueue.Enqueue(() => HitUpdate(damageList, cardList));
    }

    void HitUpdate(List<int> damageList, List<CardMono_Minion> cardList)
    {
        Instantiate(effect, new Vector3(0f, 1.5f, -10f), Quaternion.identity);
        for (int i =0;i<damageList.Count;i++)
        {
            cardList[i].UpdateHit(damageList[i]);
        }
        GameManager.isAction = false;
    }

    public bool IsNeedTarget()
    {
        return false;
    }
}
