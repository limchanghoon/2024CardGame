using DG.Tweening;
using Fusion;
using UnityEngine;

public class HeroAbility : NetworkBehaviour, ICommand, IPredict, ICanUseGlow
{
    [HideInInspector, Networked] public NetworkObject OwnerPlayer { get; set; }
    [SerializeField] protected Sprite heroSprite;
    protected SpriteRenderer spriteRenderer;
    protected Player myPlayer;

    [field : SerializeField] public GameObject canUseGlow { get; set; }
    public int cost;

    public int count { get; set; }

    public override void Spawned()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        myPlayer = OwnerPlayer.GetComponent<Player>();
        myPlayer.heroAbility = this;
        myPlayer.GetComponent<SpriteRenderer>().sprite = heroSprite;
        transform.parent = OwnerPlayer.transform;
        transform.localPosition = new Vector3(5f, 0f, 0f);
    }

    public void Reload()
    {
        RPC_ColorChange(true);
        count = 1;
    }

    public bool DecreaseCount()
    {
        if (count <= 0) return false;
        count--;
        if(count <= 0)
        {
            RPC_ColorChange(false);
        }
        return true;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_ColorChange(bool isON)
    {
        if (isON) spriteRenderer.DOColor(Color.white, 0.5f);
        else spriteRenderer.DOColor(Color.black, 0.5f);
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

    public virtual void Execute(CardMono mine, NetworkId target, CommandType _commandType)
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
