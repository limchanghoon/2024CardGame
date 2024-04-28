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
    public CardSO cardSO {  get; private set; }
    [SerializeField] SpriteRenderer spriteRender;
    [SerializeField] TextMeshPro nameText;
    [SerializeField] TextMeshPro costText;
    [SerializeField] TextMeshPro powerText;
    [SerializeField] TextMeshPro healthText;
    [SerializeField] GameObject frontFace;
    [SerializeField] GameObject backFace;
    [SerializeField] GameObject fieldFace;
    [SerializeField] GameObject backFaceGlow;

    [SerializeField] TextMeshPro fieldPowerText;
    [SerializeField] TextMeshPro fieldHealthText;

    [HideInInspector, Networked] public NetworkObject Target { get; set; }
    public Player owner { get; private set; }
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

    public int currentPower {  get; set; }
    public int currentHealth {  get; set; }

    public override void Spawned()
    {
        networkObject = GetComponent<NetworkObject>();

        var op = Addressables.LoadAssetAsync<CardSO>("Assets/Data/CardData/" + cardID.ToString() + ".asset");
        CardSO _data = op.WaitForCompletion();
        if (op.Result != null)
        {
            cardSO = _data;
            SetAsSO();
        }

        owner = Target.GetComponent<Player>();

        handMouseEvent = new HandMouseEvent(this);
        fieldMouseEvent = new FieldMouseEvent(this);
        currentMouseEvent = handMouseEvent;

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

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_ChangeState(CardState cardState)
    {
        switch(cardState)
        {
            case CardState.Hand:
                currentMouseEvent = handMouseEvent;

                frontFace.SetActive(networkObject.HasInputAuthority);
                backFace.SetActive(!networkObject.HasInputAuthority);
                fieldFace.SetActive(false);
                gameObject.layer = LayerMask.NameToLayer("HandCard");
                break;
            case CardState.Field:
                currentMouseEvent = fieldMouseEvent;

                UpdateFieldText();
                frontFace.SetActive(false);
                backFace.SetActive(false);
                fieldFace.SetActive(true);
                gameObject.layer = LayerMask.NameToLayer("FieldCard");
                break;
            case CardState.Cemetry:
                currentMouseEvent = null;

                // 임시
                transform.position = networkObject.HasInputAuthority ? new Vector3(11f, -2.5f, 0f) : new Vector3(11f, 2.5f, 0f);

                break;
            default:
                Debug.LogAssertion("CardState 이상한 것 들어옴!! : " + cardState.ToString());
                break;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_Hit(int damage)
    {
        currentHealth -= damage;
        UpdateFieldText();
        if (currentHealth <= 0 && networkObject.HasInputAuthority)
        {
            Debug.Log("죽었어!!");
            owner.DestroyCardOfField(this);
            owner.gameManager.UpdateFieldCardTooltip();
        }
    }

    public void SetPR(Vector3 des, Quaternion rot, float _t)
    {
        transform.DOMove(des, _t);
        transform.DORotateQuaternion(rot, _t);

        originPos = des;
        originRot = rot;
    }

    public GameObject GetBackFaceGlow()
    {
        return backFaceGlow;
    }

    public void UpdateFieldText()
    {
        fieldPowerText.text = currentPower.ToString();
        fieldHealthText.text = currentHealth.ToString();
    }

    public void SetAsSO()
    {
        currentPower = cardSO.power;
        currentHealth = cardSO.health;
    }
}
