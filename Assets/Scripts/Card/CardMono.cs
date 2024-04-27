using System.Collections;
using TMPro;
using UnityEngine;
using DG.Tweening;
using Fusion;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

public class CardMono : NetworkBehaviour
{
    public NetworkObject networkObject;
    [SerializeField] CardSO cardSO;
    [SerializeField] SpriteRenderer spriteRender;
    [SerializeField] TextMeshPro nameText;
    [SerializeField] TextMeshPro costText;
    [SerializeField] TextMeshPro powerText;
    [SerializeField] TextMeshPro healthText;
    [SerializeField] GameObject frontFace;
    public GameObject backFace;
    public GameObject backFaceGlow;

    [HideInInspector, Networked] public NetworkObject Target { get; set; }
    [HideInInspector] public Player owner;
    [Networked, OnChangedRender(nameof(OnIsZoomingChanged))] public bool isZooming { get; set; }
    [Networked, OnChangedRender(nameof(OnIsZoomingChanged))] public bool isDragging { get; set; }

    public Vector3 originPos { get; set; }
    public Quaternion originRot { get; set; }
    public Transform imageTr;

    [Networked] public NetworkId uniqueID {  get; set; }
    [Networked] public int cardID {  get; set; }
    AsyncOperationHandle<CardSO> op;

    IMyMouseEvent currentMouseEvent;
    HandMouseEvent handMouseEvent;
    FieldMouseEvent fieldMouseEvent;

    public override void Spawned()
    {
        networkObject = GetComponent<NetworkObject>();

        var op = Addressables.LoadAssetAsync<CardSO>("Assets/Data/CardData/" + cardID.ToString() + ".asset");
        CardSO _data = op.WaitForCompletion();
        if (op.Result != null)
        {
            cardSO = _data;
        }

        owner = Target.GetComponent<Player>();

        if (networkObject.HasInputAuthority)
        {
            handMouseEvent = new HandMouseEvent(this);
            fieldMouseEvent = new FieldMouseEvent(this);
            currentMouseEvent = handMouseEvent;
        }

        nameText.text = cardSO.cardName;
        costText.text = cardSO.cost.ToString();
        powerText.text = cardSO.power.ToString();
        healthText.text = cardSO.health.ToString();

        transform.position = networkObject.HasInputAuthority ? new Vector3(11f, -2.5f, 0f) : new Vector3(11f, 2.5f, 0f);
        frontFace.SetActive(networkObject.HasInputAuthority);
        backFace.SetActive(!networkObject.HasInputAuthority);
    }

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

    private void OnIsZoomingChanged()
    {
        if (currentMouseEvent == null) return;
        currentMouseEvent.OnIsZoomingChanged();
    }

    private bool DraggingCardInMyHandArea()
    {
        if (currentMouseEvent == null) return false;
        return currentMouseEvent.DraggingCardInMyHandArea();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_ChangeState(CardState cardState)
    {
        switch(cardState)
        {
            case CardState.Hand:
                currentMouseEvent = handMouseEvent;

                frontFace.SetActive(networkObject.HasInputAuthority);
                backFace.SetActive(!networkObject.HasInputAuthority);

                break;
            case CardState.Field:
                currentMouseEvent = fieldMouseEvent;

                frontFace.SetActive(true);
                backFace.SetActive(false);
                break;
            case CardState.Cemetry:
                break;
            default:
                Debug.LogAssertion("CardState 이상한 것 들어옴!! : " + cardState.ToString());
                break;
        }
    }


    public void SetPR(Vector3 des, Quaternion rot, float _t)
    {
        transform.DOMove(des, _t);
        transform.DORotateQuaternion(rot, _t);

        originPos = des;
        originRot = rot;
    }

    public bool IsMyTurn()
    {
        return owner.IsMyTurn();
    }
}
