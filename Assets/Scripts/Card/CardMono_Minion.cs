
using DG.Tweening;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class CardMono_Minion : CardMono, ITargetable
{
    [Header("핸드")]
    [SerializeField] TextMeshPro powerText;
    [SerializeField] TextMeshPro healthText;
    [SerializeField] GameObject fieldFace;

    [Header("필드")]
    [SerializeField] TextMeshPro fieldPowerText;
    [SerializeField] TextMeshPro fieldHealthText;
    [SerializeField] GameObject canAttackGlow;
    [SerializeField] GameObject deathRattleIcon;
    [SerializeField] GameObject tauntIcon;
    [SerializeField] GameObject diePrediction;

    HandMouseEvent_Minion handMouseEvent;
    FieldMouseEvent_Minion fieldMouseEvent;

    //public GameObject battleCryObj { get; private set; }
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

    //GameObject deathRattleObj { get; set; }
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


    private int canAttackCount = 0;
    public bool canAttack
    {
        get { return canAttackCount > 0; }
        private set { canAttackCount = 1; }
    }

    bool isDie = false;

    public SpecialAbilityEnum specialAbilityEnum { get; set; }

    private int visiblePower = 0;
    private int visibleHealth = 0;
    public int currentPower { get; set; }
    public int currentHealth { get; set; }


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

        owner = OwnerPlayer.GetComponent<Player>();
        owner.AddToCardDictionary(uniqueID, this);

        handMouseEvent = new HandMouseEvent_Minion(this);
        fieldMouseEvent = new FieldMouseEvent_Minion(this);
        currentMouseEvent = handMouseEvent;

        nameText.text = cardSO.cardName;
        costText.text = cardSO.cost.ToString();
        powerText.text = cardSO.power.ToString();
        healthText.text = cardSO.health.ToString();

        transform.DOMove(networkObject.HasInputAuthority ? new Vector3(11f, -2.5f, 0f) : new Vector3(11f, 2.5f, 0f), 0);
        frontFace.SetActive(networkObject.HasInputAuthority);
        backFace.SetActive(!networkObject.HasInputAuthority);
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


    public void SetActivePrediction(bool _active)
    {
        diePrediction.SetActive(_active);
    }

    public void UpdateFieldText()
    {
        fieldPowerText.text = visiblePower.ToString();
        fieldHealthText.text = visibleHealth.ToString();
    }

    public int PredictHit(int damage)
    {
        if (damage <= 0) return -1;
        return damage;
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

    public GameObject GetTargetGameObject() => gameObject;

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
        Debug.Log("전투의 함성 이펙트 넣어야함!!");
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
        RPC_DoEffect(CommandType.BattleCry);
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
        RPC_DoEffect(CommandType.DeathRattle);
        RPC_DeathRattle();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_DeathRattle()
    {
        deathRattle?.Execute(this, default);
    }

    public override void Predict(GameObject obj)
    {
        var iTargetable = obj?.GetComponent<ITargetable>();
        if (iTargetable == target) return;
        if (iTargetable != target) target?.SetActivePrediction(false);
        target = iTargetable;
        if (target == null) return;
        target.SetActivePrediction(target.DieIfHit(currentPower));
    }
}
