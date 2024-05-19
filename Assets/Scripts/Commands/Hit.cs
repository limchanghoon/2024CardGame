using Fusion;
using System.Collections;
using UnityEngine;

public class Hit : NetworkBehaviour, ICommand, IPredict
{
    [SerializeField] int damage;
    ITargetable target;
    [SerializeField] GameObject bobm;
    CardMono myCard;

    public void Execute(CardMono mine, NetworkId target)
    {
        myCard = mine;
        if (!mine.owner.IsMyTurn()) return;
        if (target == default) return;
        ITargetable targetHit = mine.owner.gameManager.GetNetworkObject(target).GetComponent<ITargetable>();
        if (targetHit == null) return;
        RPC_Execute(mine.owner.gameManager.GetNetworkObject(target).GetComponent<NetworkObject>());

        // ав╦ч!
        myCard.owner.gameManager.DoDeathRattleOneLayer();
        mine.owner.gameManager.RPC_EnqueueChangeField();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_Execute(NetworkObject _target)
    {
        ITargetable targetHit = _target.GetComponent<ITargetable>();
        int _damage = targetHit.PredictHit(damage);
        GameManager.actionQueue.Enqueue(() => StartCoroutine(HitCoroutine(_damage, targetHit)));
        targetHit.Hit(damage);
        targetHit.CheckIsFirstDie();
    }

    IEnumerator HitCoroutine(int _damage, ITargetable targetHit)
    {
        Vector3 start = myCard.transform.position;
        start.z = -200;
        Vector3 end = targetHit.GetTargetGameObject().transform.position;
        end.z = -200;
        GameObject _BOBM = Instantiate(bobm, start, Quaternion.identity);
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
        return true;
    }

    public void Predict(GameObject obj)
    {
        var iTargetable = obj?.GetComponent<ITargetable>();
        if (iTargetable == target) return;
        if (iTargetable != target) target?.SetActivePrediction(false);
        target = iTargetable;

        if (target == null) return;
        target.SetActivePrediction(target.DieIfHit(damage));
    }
}
