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

    public void Execute(CardMono mine, NetworkId target)
    {
        start = mine.transform.position;
        start.z = -400;
        if (!mine.owner.IsMyTurn()) return;
        mine.RPC_DoEffect(commandType);
        List<ITargetable> targets = new List<ITargetable>();
        foreach (var hero in mine.owner.gameManager.heroMonos)
        {
            if (hero.CanBeTarget())
                targets.Add(hero);
        }

        foreach (var _uniqueId in mine.owner.gameManager.GetMyPlayer().field)
        {
            CardMono _card = mine.owner.gameManager.GetMyPlayer().GetMyCard(_uniqueId);
            if(_card.CanBeTarget() && mine != _card)
                targets.Add(_card);
        }

        foreach (var _uniqueId in mine.owner.gameManager.GetOppenetPlayer().field)
        {
            CardMono _card = mine.owner.gameManager.GetOppenetPlayer().GetMyCard(_uniqueId);
            if (_card.CanBeTarget() && mine != _card)
                targets.Add(_card);
        }

        List<NetworkId> needToCheck = new List<NetworkId>();
        for (int n = 0; n < randomCount; ++n)
        {
            if (targets.Count == 0) break;
            int rnd = Random.Range(0, targets.Count);

            targets[rnd].RPC_Command(GetComponent<NetworkObject>());
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

    public void ExecuteInRPC(ITargetable targetHit)
    {
        int _damage = targetHit.PredictHit(damage);

        GameManager.actionQueue.Enqueue(() => StartCoroutine(HitCoroutine(_damage, targetHit)));
        targetHit.Hit(damage);
        targetHit.CheckIsFirstDie();
    }

    IEnumerator HitCoroutine(int _damage, ITargetable targetHit)
    {
        Vector3 end = targetHit.GetTargetGameObject().transform.position;
        end.z = -400;

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
