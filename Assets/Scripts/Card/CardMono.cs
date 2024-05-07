using TMPro;
using UnityEngine;
using DG.Tweening;
using Fusion;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;

public class CardMono : NetworkBehaviour, ITargetable, IPredict
{
    public NetworkObject networkObject;
    public CardSO cardSO { get; private set; }

    [Networked] public NetworkObject battleCryObj { get; private set; }

    private ICommand _battleCry;
    public ICommand battleCry
    {
        get
        {
            if (battleCryObj == null) return null;
            if (_battleCry == null) _battleCry = battleCryObj.GetComponent<ICommand>();
            return _battleCry;
        }
        set
        {
            _battleCry = value;
        }
    }

    [Networked] NetworkObject deathRattleObj { get; set; }
    private ICommand _deathRattle;
    public ICommand deathRattle
    {
        get
        {
            if (deathRattleObj == null) return null;
            if (_deathRattle == null) _deathRattle = deathRattleObj.GetComponent<ICommand>();
            return _deathRattle;
        }
        set
        {
            _deathRattle = value;
        }
    }

    [Header("핸드")]
    [SerializeField] SpriteRenderer cardRender;
    [SerializeField] SpriteRenderer bgRender;
    [SerializeField] Sprite legendSprite;
    [SerializeField] TextMeshPro nameText;
    [SerializeField] TextMeshPro costText;
    [SerializeField] TextMeshPro powerText;
    [SerializeField] TextMeshPro healthText;
    [SerializeField] TextMeshPro abilityText;
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
    [SerializeField] GameObject tauntIcon;
    [SerializeField] GameObject diePrediction;
    private int canAttackCount = 0;
    public bool canAttack { 
        get { return canAttackCount > 0; }
        private set { canAttackCount = 1; } 
    }

    public SpecialAbilityEnum specialAbilityEnum { get; set; }
    // 마우스 타겟
    ITargetable target;

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
    bool isDie = false;

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

    //[Rpc(RpcSources.All, RpcTargets.All)]
    public void ChangeCardState(CardState cardState)
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
                gameObject.layer = LayerMask.NameToLayer("CemetryCard");
                break;
            default:
                Debug.LogAssertion("CardState 이상한 것 들어옴!! : " + cardState.ToString());
                break;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_DoEffect(CommandType _commandType)
    {
        Vector3 _pos = transform.position;
        switch (_commandType)
        {
            case CommandType.BattleCry:
                GameManager.actionQueue.Enqueue(() => EffectBattelCry(_pos));
                break;

            case CommandType.DeathRattle:
                GameManager.actionQueue.Enqueue(() => EffectDeathRattle(_pos));
                break;
            default:
                break;
        }
    }

    public void EffectBattelCry(Vector3 _pos)
    {
        GameManager.isAction = false;
    }

    public void EffectDeathRattle(Vector3 _pos)
    {
        owner.gameManager.effectManager.DoDeathRattleEffect(_pos);
        GameManager.isAction = false;
    }

    public void BattleCry(NetworkId _target)
    {
        if (!owner.IsMyTurn()) return;
        if (battleCryObj == null) return;
        RPC_BattleCry(_target);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_BattleCry(NetworkId _target)
    {
        battleCry?.Execute(this, _target);
    }

    public void DeathRattle()
    {
        if (!owner.IsMyTurn()) return;
        if (deathRattleObj == null) return;
        RPC_DeathRattle();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_DeathRattle()
    {
        deathRattle?.Execute(this, default);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_Command(NetworkObject networkObject)
    {
        networkObject.GetComponent<ICommand>()?.ExecuteInRPC(this);
    }

    public int PredictHit(int damage)
    {
        if (damage <= 0) return -1;
        return damage;
    }

    public void Hit(int damage)
    {
        if (damage < 0) return;
        currentHealth -= damage;
    }

    // 여기서 죽음의 메아리 시전가능
    public bool CheckIsFirstDie()
    {
        if (isDie) return false;
        if (currentHealth <= 0)
        {
            isDie = true;
            owner.DestroyCardOfField(this);
            return true;
        }
        return false;
    }

    public bool DieIfHit(int damage)
    {
        if (damage >= currentHealth) return true;
        return false;
    }

    public void UpdateHit(int damage)
    {
        if (damage < 0) return;
        owner.gameManager.GenerateHitText(damage, transform.position);
        visibleHealth -= damage;
        UpdateFieldText();
        if (visibleHealth <= 0)
        {
            owner.showField.Remove(uniqueID);
            transform.DOShakePosition(0.5f, 0.5f).SetDelay(0.1f).OnComplete(Die);
        }
    }

    public void Die()
    {
        //임시
        owner.gameManager.effectManager.DoDieEffect(transform.position);
        SetPR(networkObject.HasInputAuthority ? new Vector3(11f, -2.5f, 0f) : new Vector3(11f, 2.5f, 0f), Quaternion.identity, 0);
        owner.gameManager.UpdateFieldCardTooltip();
        //owner.gameManager.ChangeField();
    }

    public void SetActivePrediction(bool _active)
    {
        diePrediction.SetActive(_active);
    }


    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_Attack(NetworkId _uniqueID)
    {
        var _targetObj = owner.gameManager.GetNetworkObject(_uniqueID);
        var _target = _targetObj.GetComponent<ITargetable>();

        int opponentHit = _target.PredictHit(currentPower);
        int myHit = PredictHit(_target.currentPower);

        GameManager.actionQueue.Enqueue(() =>
        {
            Vector3 _origin = transform.position;
            DOTween.Sequence().Append(transform.DOMove(_targetObj.transform.position, 0.2f).SetEase(Ease.OutCirc))
                .Append(transform.DOMove(_origin, 0.1f))
                .OnComplete(() => { _target.UpdateHit(opponentHit); UpdateHit(myHit); GameManager.isAction = false; });
        });
        _target.Hit(currentPower);
        Hit(_target.currentPower);

        _target.CheckIsFirstDie();
        CheckIsFirstDie();
        // 죽메!
        owner.gameManager.DoDeathRattleOneLayer();
        owner.gameManager.EnqueueChangeField();
    }

    public void SetPR(Vector3 des, Quaternion rot, float _t)
    {
        DOTween.Kill(transform);
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
        abilityText.text = cardSO.infomation;

        visiblePower = currentPower;
        visibleHealth = currentHealth;
        if (cardSO.grade == CardGrade.Lengend)
            bgRender.sprite = legendSprite;

        if (networkObject.HasInputAuthority)
        {
            if (cardSO.battleCry)
            {
                battleCryObj = Runner.Spawn(cardSO.battleCry, null, null, Runner.LocalPlayer);
            }
            if (cardSO.deathRattle)
            {
                deathRattleObj = Runner.Spawn(cardSO.deathRattle, null, null, Runner.LocalPlayer);
            }
        }
        if (cardSO.deathRattle)
            deathRattleIcon.SetActive(true);

        // 도발, 돌진, 천상의 보호막 등 특수 능력 처리
        specialAbilityEnum = cardSO.speicalAbilityEnum;
        if (specialAbilityEnum.HasFlag(SpecialAbilityEnum.taunt))
            tauntIcon.SetActive(true);

    }

    public TargetType GetTargetType()
    {
        if (networkObject.HasInputAuthority) return TargetType.MyMinion;
        else return TargetType.OpponentMinion;
    }

    public bool CanBeTarget()
    {
        return currentHealth > 0 && !isDie;
    }

    public bool CanBeDirectAttackTarget()
    {
        if (CanBeTarget())
        {
            if (owner.IsTauntInField())
            {
                if (specialAbilityEnum.HasFlag(SpecialAbilityEnum.taunt)) return true;
                return false;
            }
            return true;
        }
        return false;
    }

    public NetworkId GetNetworkId()
    {
        return uniqueID;
    }

    public void Predict(GameObject obj)
    {
        var iTargetable = obj?.GetComponent<ITargetable>();
        if (iTargetable == target) return;
        if (iTargetable != target) target?.SetActivePrediction(false);
        target = iTargetable;

        if (target == null) return;
        target.SetActivePrediction(target.DieIfHit(currentPower));
    }

    public GameObject GetTargetGameObject() => gameObject;
}
