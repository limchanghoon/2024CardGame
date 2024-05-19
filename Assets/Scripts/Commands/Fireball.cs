using Fusion;
using System.Collections;
using UnityEngine;

public class Fireball : NetworkBehaviour, ICommand
{
    [SerializeField] GameObject ballPrefab;
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] int damage;
    Vector3 start;

    public void Execute(CardMono mine, NetworkId target)
    {
        start = mine.owner.heroMono.transform.position;
        start.z = -200;
        if (!mine.owner.IsMyTurn()) return;

        RPC_Execute(mine.owner.gameManager.GetNetworkObject(target).GetComponent<NetworkObject>());

        // ав╦ч!
        mine.owner.gameManager.DoDeathRattleOneLayer();
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
        Vector3 end = targetHit.GetTargetGameObject().transform.position;
        end.z = -200;
        GameObject ball = Instantiate(ballPrefab, start, Quaternion.identity);
        Transform ballTr = ball.transform;
        ballTr.LookAt(end);

        ballTr.localScale = Vector3.zero;
        float t = 0f;
        while (t < 1f)
        {
            yield return null;
            t += Time.deltaTime;
            ballTr.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
        }

        t = 0f;
        while (t < 1f)
        {
            yield return null;
            t += Time.deltaTime * 1.5f;
            ball.transform.position = Vector3.Lerp(start, end, t);
        }
        Destroy(ball);
        Instantiate(explosionPrefab, end, Quaternion.identity);
        targetHit.UpdateHit(_damage);
        GameManager.isAction = false;
    }


    public bool IsNeedTarget()
    {
        return true;
    }
}
