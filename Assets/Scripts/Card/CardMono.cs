using TMPro;
using UnityEngine;
using DG.Tweening;
using Fusion;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CardMono : NetworkBehaviour, ITargetable
{
    public NetworkObject networkObject;
    public CardSO cardSO { get; private set; }

    public ICommand battleCry { get; private set; }
    public ICommand deathRattle { get; private set; }

    [Header("핸드")]
    [SerializeField] SpriteRenderer cardRender;
    [SerializeField] SpriteRenderer bgRender;
    [SerializeField] Sprite legendSprite;
    [SerializeField] TextMeshPro nameText;
    [SerializeField] TextMeshPro costText;
    [SerializeField] TextMeshPro powerText;
    [SerializeField] TextMeshPro healthText;
    [SerializeField] GameObject frontFace;
    [SerializeField] GameObject backFace;
    [SerializeField] GameObject fieldFace;
    [SerializeField] GameObject backFaceGlow;
    [SerializeField] GameObject canUseGlow;
    
    [Header("필드")]
    [SerializeField] TextMeshPro fieldPowerText;
    [SerializeField] TextMeshPro fieldHealthText;
    [SerializeField] GameObject canAttackGlow;
    [SerializeField] GameObject deathRattleIcon;
    private int canAttackCount = 0;
    public bool canAttack { 
        get { return canAttackCount > 0; }
        private set { canAttackCount = 1; } 
    }

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

    private int visiblePower = 0;
    private int visibleHealth = 0;
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
        owner.AddToCardDictionary(uniqueID, this);

        handMouseEvent = new HandMouseEvent(this);
        fieldMouseEvent = new FieldMouseEvent(this);
        currentMouseEvent = handMouseEvent;

        nameText.text = cardSO.cardName;
        costText.text = cardSO.cost.ToString();
        powerText.text = cardSO.power.ToString();
        healthText.text = cardSO.health.ToString();

        transform.DOMove(networkObject.HasInputAuthority ? new Vector3(11f, -2.5f, 0f) : new Vector3(11f, 2.5f, 0f), 0);
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
    public void RPC_ChangeCardState(CardState cardState)
    {
        switch (cardState)
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
                gameObject.layer = LayerMask.NameToLayer("Targetable");
                break;
            case CardState.Cemetry:
                currentMouseEvent = null;

                Debug.Log("CardState.Cemetry : " + cardSO.cardName);

                break;
            default:
                Debug.LogAssertion("CardState 이상한 것 들어옴!! : " + cardState.ToString());
                break;
        }
    }

    // 여기서 RPC로 타겟 다 정해서 전달...?
    public void BattleCry(NetworkId _target)
    {
        if (!networkObject.HasInputAuthority) return;
        //battleCry.Execute(this, _target);
        RPC_BattleCry(_target);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_BattleCry(NetworkId _target)
    {
        battleCry.Execute(this, _target);
    }

    // 여기서 RPC로 타겟 다 정해서 전달...?
    public void DeathRattle()
    {
        if (!networkObject.HasInputAuthority) return;
        //deathRattle.Execute(this, default);
        RPC_DeathRattle();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_DeathRattle()
    {
        deathRattle.Execute(this, default);
    }

    public int Hit(int damage)
    {
        if (damage <= 0) return -1;
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            owner.DestroyCardOfField(this);
        }
        return damage;
    }

    public void UpdateHit(int damage)
    {
        if (damage < 0) return;
        owner.gameManager.GenerateHitText(damage, transform.position);
        visibleHealth -= damage;
        UpdateFieldText();
        if (visibleHealth <= 0)
        {
            transform.DOShakePosition(0.5f, 0.5f).SetDelay(0.1f).OnComplete(Die);
        }
    }

    public void Die()
    {
        //임시
        owner.gameManager.effectManager.DoHitEffect(transform.position);
        transform.DOMove(networkObject.HasInputAuthority ? new Vector3(11f, -2.5f, 0f) : new Vector3(11f, 2.5f, 0f), 0);
        owner.gameManager.UpdateFieldCardTooltip();
    }


    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_Attack(NetworkId _uniqueID)
    {
        var _targetObj = owner.gameManager.GetNetworkObject(_uniqueID);
        var _target = _targetObj.GetComponent<ITargetable>();

        int opponentHit = _target.Hit(currentPower);
        int myHit = Hit(_target.currentPower);

        Vector3 _origin = transform.position;

        DOTween.Sequence().Append(transform.DOMove(_targetObj.transform.position, 0.2f).SetEase(Ease.OutCirc))
            .Append(transform.DOMove(_origin, 0.1f))
            .OnComplete(() => { _target.UpdateHit(opponentHit); UpdateHit(myHit); });
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
        fieldPowerText.text = visiblePower.ToString();
        fieldHealthText.text = visibleHealth.ToString();
    }

    public void SetAsSO()
    {
        currentPower = cardSO.power;
        currentHealth = cardSO.health;

        visiblePower = currentPower;
        visibleHealth = currentHealth;
        if (cardSO.grade == CardGrade.Lengend)
            bgRender.sprite = legendSprite;

        if (cardSO.battleCry)
            battleCry = new Hit(2);
        if (cardSO.deathRattle)
        {
            deathRattle = new DrawCard(2);
            deathRattleIcon.SetActive(true);
        }

    }

    public TargetType GetTargetType()
    {
        if (networkObject.HasInputAuthority) return TargetType.MyMinion;
        else return TargetType.OpponentMinion;
    }

    public bool CanBeTarget()
    {
        return currentHealth > 0;
    }

    public NetworkId GetNetworkId()
    {
        return uniqueID;
    }
}
