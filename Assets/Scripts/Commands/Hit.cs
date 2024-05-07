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
        targetHit.RPC_Command(GetComponent<NetworkObject>());
        mine.owner.gameManager.RPC_EnqueueChangeField();
    }

    public void ExecuteInRPC(ITargetable targetHit)
    {
        int _damage = targetHit.PredictHit(damage);
        GameManager.actionQueue.Enqueue(() => StartCoroutine(HitCoroutine(_damage, targetHit)));
        targetHit.Hit(damage);
        targetHit.CheckIsFirstDie();
        // ав╦ч!
        myCard.owner.gameManager.DoDeathRattleOneLayer();
    }

    IEnumerator HitCoroutine(int _damage, ITargetable targetHit)
    {
        Vector3 start = myCard.transform.position;
        Vector3 end = targetHit.GetTargetGameObject().transform.position;
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
