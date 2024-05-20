using Fusion;
using UnityEngine;

public class HeroAbility : NetworkBehaviour, ICommand, IPredict
{
    [HideInInspector, Networked] public NetworkObject OwnerPlayer { get; set; }
    protected Player myPlayer;

    public override void Spawned()
    {
        myPlayer = OwnerPlayer.GetComponent<Player>();
        transform.parent = OwnerPlayer.transform;
        transform.localPosition = new Vector3(1.5f, 0f, 0f);
    }

    // ¸¶¿ì½º Å¸°Ù
    protected ITargetable target;

    protected IMyMouseEvent currentMouseEvent;

    private void OnMouseEnter()
    {
        if (currentMouseEvent == null) return;
        currentMouseEvent.OnMyMouseEnter();
    }

    private void OnMouseExit()
    {
        if (currentMouseEvent == null) return;
        currentMouseEvent.OnMyMouseExit();
    }

    private void OnMouseDown()
    {
        if (currentMouseEvent == null) return;
        currentMouseEvent.OnMyMouseDown();
    }

    private void OnMouseDrag()
    {
        if (currentMouseEvent == null) return;
        currentMouseEvent.OnMyMouseDrag();
    }

    private void OnMouseUp()
    {
        if (currentMouseEvent == null) return;
        currentMouseEvent.OnMyMouseUp();
    }

    protected void OnIsZoomingChanged()
    {
        if (currentMouseEvent == null) return;
        currentMouseEvent.OnIsZoomingChanged();
    }

    public virtual void Execute(CardMono mine, NetworkId target)
    {
        throw new System.NotImplementedException();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public virtual void RPC_Execute(NetworkObject target)
    {
        throw new System.NotImplementedException();
    }

    public virtual bool IsNeedTarget()
    {
        throw new System.NotImplementedException();
    }

    public virtual void Predict(GameObject obj)
    {

    }

    public virtual TargetType GetTargetType()
    {
        throw new System.NotImplementedException();
    }
}
