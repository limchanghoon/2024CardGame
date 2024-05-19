using TMPro;
using UnityEngine;
using DG.Tweening;
using Fusion;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CardMono : NetworkBehaviour, IPredict
{
    public NetworkObject networkObject;
    public CardSO cardSO { get; protected set; }

    [Header("핸드")]
    [SerializeField] protected SpriteRenderer cardRender;
    [SerializeField] protected SpriteRenderer bgRender;
    [SerializeField] protected Sprite legendSprite;
    [SerializeField] protected TextMeshPro nameText;
    [SerializeField] protected TextMeshPro costText;
    [SerializeField] protected TextMeshPro abilityText;
    [SerializeField] protected GameObject frontFace;
    [SerializeField] protected GameObject backFace;
    [SerializeField] protected GameObject backFaceGlow;
    [SerializeField] protected GameObject canUseGlow;
  

    [HideInInspector, Networked] public NetworkObject OwnerPlayer { get; set; }
    public Player owner { get; protected set; }
    [Networked, OnChangedRender(nameof(OnIsZoomingChanged))] public bool isZooming { get; set; }
    [Networked, OnChangedRender(nameof(OnIsZoomingChanged))] public bool isDragging { get; set; }

    public Vector3 originPos { get; set; }
    public Quaternion originRot { get; set; }
    public Transform imageTr;

    [Networked] public NetworkId uniqueID {  get; set; }
    [Networked] public int cardID {  get; set; }
    protected AsyncOperationHandle<CardSO> op;


    // 마우스 타겟
    protected ITargetable target;

    protected IMyMouseEvent currentMouseEvent;

    private void OnDestroy()
    {
        if (op.IsValid())
            Addressables.Release(op);
    }

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

    public GameObject GetBackFaceGlow()
    {
        return backFaceGlow;
    }

    public void SetPR(Vector3 des, Quaternion rot, float _t)
    {
        DOTween.Kill(transform);
        transform.DOMove(des, _t);
        transform.DORotateQuaternion(rot, _t);

        originPos = des;
        originRot = rot;
    }

    public NetworkId GetNetworkId()
    {
        return uniqueID;
    }

    public virtual void Predict(GameObject obj)
    {

    }
}
