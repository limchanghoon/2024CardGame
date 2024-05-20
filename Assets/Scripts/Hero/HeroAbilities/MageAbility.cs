using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageAbility : HeroAbility
{
    [SerializeField] int damage;
    [SerializeField] GameObject fireball;

    public override void Spawned()
    {
        base.Spawned();
        currentMouseEvent = new FieldMouseEvent_HeroAbility(this, OwnerPlayer.GetComponent<Player>());
    }

    public override void Execute(CardMono mine, NetworkId target)
    {
        if (!myPlayer.IsMyTurn()) return;
        RPC_Execute(myPlayer.gameManager.GetNetworkObject(target).GetComponent<NetworkObject>());

        // ав╦ч!
        myPlayer.gameManager.DoDeathRattleOneLayer();
        myPlayer.gameManager.RPC_EnqueueChangeField();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public override void RPC_Execute(NetworkObject _target)
    {
        ITargetable targetHit = _target.GetComponent<ITargetable>();
        int _damage = targetHit.PredictHit(damage);
        GameManager.actionQueue.Enqueue(() => StartCoroutine(HitCoroutine(_damage, targetHit)));
        targetHit.Hit(damage);
        targetHit.CheckIsFirstDie();
    }

    IEnumerator HitCoroutine(int _damage, ITargetable targetHit)
    {
        Vector3 start = transform.position;
        start.z = -200;
        Vector3 end = targetHit.GetTargetGameObject().transform.position;
        end.z = -200;
        GameObject ball = Instantiate(fireball, start, Quaternion.identity);
        Transform ballTr = ball.transform;
        ballTr.LookAt(end);

        ballTr.localScale = Vector3.zero;
        float t = 0f;
        while (t < 1f)
        {
            yield return null;
            t += Time.deltaTime;
            ballTr.localScale = Vector3.Lerp(Vector3.zero, new Vector3(0.5f, 0.5f, 0.5f), t);
        }

        t = 0f;
        while (t < 1f)
        {
            yield return null;
            t += Time.deltaTime * 1.5f;
            ball.transform.position = Vector3.Lerp(start, end, t);
        }
        Destroy(ball);
        targetHit.UpdateHit(_damage);
        GameManager.isAction = false;
    }

    public override bool IsNeedTarget()
    {
        return true;
    }

    public override void Predict(GameObject obj)
    {
        var iTargetable = obj?.GetComponent<ITargetable>();
        if (iTargetable == target) return;
        if (iTargetable != target) target?.SetActivePrediction(false);
        target = iTargetable;

        if (target == null) return;
        target.SetActivePrediction(target.DieIfHit(damage));
    }

    public override TargetType GetTargetType()
    {
        return TargetType.All;
    }
}
