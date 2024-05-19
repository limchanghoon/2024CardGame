using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitNonTarget : NetworkBehaviour, ICommand
{
    [SerializeField] CommandType commandType;
    [SerializeField] int damage;
    [SerializeField] int randomCount;
    [SerializeField] GameObject bobm;
    Vector3 start;

    public void Execute(CardMono _mine, NetworkId target)
    {
        CardMono_Minion mine = (CardMono_Minion)_mine;
        start = mine.transform.position;
        start.z = -200;
        if (!mine.owner.IsMyTurn()) return;
        List<ITargetable> targets = new List<ITargetable>();
        foreach (var hero in mine.owner.gameManager.heroMonos)
        {
            if (hero.CanBeTarget())
                targets.Add(hero);
        }

        foreach (var _uniqueId in mine.owner.gameManager.GetMyPlayer().field)
        {
            CardMono_Minion _card = (CardMono_Minion)mine.owner.gameManager.GetMyPlayer().GetMyCard(_uniqueId);
            if(_card.CanBeTarget() && mine != _card)
                targets.Add(_card);
        }

        foreach (var _uniqueId in mine.owner.gameManager.GetOppenetPlayer().field)
        {
            CardMono_Minion _card = (CardMono_Minion)mine.owner.gameManager.GetOppenetPlayer().GetMyCard(_uniqueId);
            if (_card.CanBeTarget() && mine != _card)
                targets.Add(_card);
        }

        List<NetworkId> needToCheck = new List<NetworkId>();
        for (int n = 0; n < randomCount; ++n)
        {
            if (targets.Count == 0) break;
            int rnd = Random.Range(0, targets.Count);

            RPC_Execute(targets[rnd].GetTargetGameObject().GetComponent<NetworkObject>());
            if (!needToCheck.Exists(x => x == targets[rnd].GetNetworkId()))
                needToCheck.Add(targets[rnd].GetNetworkId());

            for (int i = targets.Count - 1; i >= 0; --i)
            {
                if (!targets[i].CanBeTarget())
                    targets.RemoveAt(i);
            }
        }
        //mine.owner.gameManager.RPC_CheckAllDie(needToCheck.ToArray());

        // ав╦ч!
        mine.owner.gameManager.DoDeathRattleOneLayer();
        mine.owner.gameManager.RPC_EnqueueChangeField();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_Execute(NetworkObject _target)
    {
        var targetHit = _target.GetComponent<ITargetable>();
        int _damage = targetHit.PredictHit(damage);

        GameManager.actionQueue.Enqueue(() => StartCoroutine(HitCoroutine(_damage, targetHit)));
        targetHit.Hit(damage);
        targetHit.CheckIsFirstDie();
    }

    IEnumerator HitCoroutine(int _damage, ITargetable targetHit)
    {
        Vector3 end = targetHit.GetTargetGameObject().transform.position;
        end.z = -200;

        GameObject _BOBM = Instantiate(bobm,start,Quaternion.identity);
        float t = 0f;
        while (t < 1f)
        {
            yield return null;
            t += Time.deltaTime;
            _BOBM.transform.position = Vector3.Lerp(start, end, t);
        }
        Destroy(_BOBM);
        targetHit.UpdateHit(_damage);
        GameManager.isAction = false;
    }

    public bool IsNeedTarget()
    {
        return false;
    }
}
