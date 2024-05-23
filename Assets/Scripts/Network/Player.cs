using DG.Tweening;
using DG.Tweening.Core.Easing;
using Fusion;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    private GameManager _gameManager;
    public GameManager gameManager {
        get
        {
            if (_gameManager == null)
                _gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
            return _gameManager;
        }
    }
    public HeroMono heroMono { get; private set; }
    public HeroAbility heroAbility { get; set; }
    public NetworkObject networkObject { get; private set; }

    public GameObject _MinionCardPrefab;
    public GameObject _MagicCardPrefab;

    //[Networked, Capacity(7), OnChangedRender(nameof(OnFieldChanged))] public NetworkLinkedList<NetworkId> field { get; }
    public List<NetworkId> showField = new List<NetworkId>(7);
    public List<NetworkId> field = new List<NetworkId>(7);
    [Networked, Capacity(10), OnChangedRender(nameof(OnHandChanged))] public NetworkLinkedList<NetworkId> hand { get; }
    [Networked, Capacity(60)] public NetworkLinkedList<NetworkId> cemetry { get; }
    [Networked, Capacity(50)] public NetworkLinkedList<NetworkId> deck { get; }
    private Dictionary<NetworkId, CardMono> cardDictionary = new Dictionary<NetworkId, CardMono>();

    private int _maxCrystal;
    public int maxCrystal 
    {
        get { return _maxCrystal; }
        private set { _maxCrystal = value >= 10 ? 10 : value; }
    }
    public int currnetCrystal { get; private set; }


    private void Awake()
    {
        DontDestroyOnLoad(this);
        maxCrystal = 0;
        currnetCrystal = 0;
        heroMono = GetComponent<HeroMono>();
        networkObject = GetComponent<NetworkObject>();
    }


    private void Start()
    {
        if (networkObject.HasStateAuthority)
        {
            Button button = GameObject.Find("BasicSpawner").GetComponent<BasicSpawner>().btn_Start.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { StartGame(button); });
        }
    }

    public void StartGame(Button button)
    {
        if (Runner.ActivePlayers.Count() == Runner.Config.Simulation.PlayerCount && Runner.IsSceneAuthority)
        {
            Runner.LoadScene(SceneRef.FromIndex(1), LoadSceneMode.Single);
            button.interactable = false;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_StartTurn()
    {
        heroAbility.Reload();
        maxCrystal++;
        currnetCrystal = maxCrystal;
        OnUpdateCrystal();
        for (int i = 0; i < field.Count; i++)
        {
            ((CardMono_Minion)GetMyCard(field[i])).canAttack = true;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_EndTurn()
    {
        for (int i = 0; i < field.Count; i++)
        {
            ((CardMono_Minion)GetMyCard(field[i])).canAttack = false;
        }
    }

    public void DrawMyCard()
    {
        gameManager.RPC_DrawCard(gameManager.GetPlayerOrder(this));
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_UseMagicOfHand(NetworkId _uniqueID, NetworkId _target = default)
    {
        hand.Remove(_uniqueID);

        CardMono_Magic _cardMono = (CardMono_Magic)gameManager.GetCard(_uniqueID);
        UseCrystal(_cardMono.cost);
        _cardMono.SetPR(new Vector3(9999, 9999, 9999), Quaternion.identity, 0);
        GameManager.actionQueue.Enqueue(() => { gameManager.ShowCurrentAnimCard(_cardMono); GameManager.isAction = false; });
        _cardMono.FireMagic(_target);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SpawnNewObject(NetworkObject newNetworkObject, int _location)
    {
        if (field.Count == field.Capacity)
        {
            Debug.LogAssertion("필드 FULL");
            return;
        }
        CardMono_Minion _cardMono = (CardMono_Minion)newNetworkObject.GetComponent<CardMono>();

        if (_location < field.Count)
            field.Insert(_location, newNetworkObject.Id);
        else
            field.Add(newNetworkObject.Id);
        AddToCardDictionary(newNetworkObject.Id, _cardMono);
        
        _cardMono.SetPR(new Vector3(9999, 9999, 9999), Quaternion.identity, 0);
        _cardMono.ChangeCardState(CardState.Field);
        GameManager.actionQueue.Enqueue(() => SpawnMinion(newNetworkObject.Id, _location, default));
        gameManager.EnqueueChangeField();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SpawnMinionOfHand(NetworkId _uniqueID, int _location, NetworkId _target = default)
    {
        if (field.Count == field.Capacity)
        {
            Debug.LogAssertion("필드 FULL");
            return;
        }

        hand.Remove(_uniqueID);
        if (_location < field.Count)
            field.Insert(_location, _uniqueID);
        else
            field.Add(_uniqueID);

        CardMono_Minion _cardMono = (CardMono_Minion)gameManager.GetCard(_uniqueID);
        UseCrystal(_cardMono.cost);
        _cardMono.SetPR(new Vector3(9999, 9999, 9999), Quaternion.identity, 0);
        _cardMono.ChangeCardState(CardState.Field);
        GameManager.actionQueue.Enqueue(() => SpawnMinion(_uniqueID, _location, _target));
        gameManager.EnqueueChangeField();
        // 전투의 함성
        _cardMono.BattleCry(_target);
    }

    private void SpawnMinion(NetworkId _uniqueID, int _location, NetworkId _target = default)
    {
        if (_location < field.Count)
        {
            showField.Insert(_location, _uniqueID);
        }
        else
        {
            showField.Add(_uniqueID);
        }
        CardMono_Minion _cardMono_Minion = (CardMono_Minion)gameManager.GetCard(_uniqueID);
        _cardMono_Minion.SetPR(GetFieldPos(_location, showField.Count), Quaternion.identity, 0);
        gameManager.ShowCurrentAnimCard(_cardMono_Minion);

        _cardMono_Minion.ActiveSpecialAbility();
        GameManager.isAction = false;
    }

    public void DestroyCardOfField(CardMono_Minion _cardMono)
    {
        NetworkId _uniqueID = _cardMono.uniqueID;

        int _location = field.IndexOf(_uniqueID);
        field.RemoveAt(_location);

        if (networkObject.HasStateAuthority)
        {
            if (cemetry.Count == cemetry.Capacity) cemetry.Remove(cemetry.Get(0));
            cemetry.Add(_uniqueID);
        }

        _cardMono.ChangeCardState(CardState.Cemetry);

        // 죽음의 메아리 Queue에 집어 넣는다!
        if (IsMyTurn())
            gameManager.deathRattleQueue.Enqueue(() => _cardMono.DeathRattle(_location));
    }

    public bool IsMyTurn()
    {
        return gameManager.IsMyTurn();
    }

    public void AddToCardDictionary(NetworkId _networkId, CardMono _cardMono)
    {
        if (!cardDictionary.ContainsKey(_networkId))
            cardDictionary.Add(_networkId, _cardMono);
    }

    public CardMono GetMyCard(NetworkId _networkId)
    {
        cardDictionary.TryGetValue(_networkId, out var result);
        return result;
    }

    public bool IsTauntInField()
    {
        if(gameManager.GetMyHereMono().isTaunt) return true;

        for (int i = 0; i < field.Count; i++)
        {
            if (((CardMono_Minion)GetMyCard(field[i])).specialAbilityEnum.HasFlag(SpecialAbilityEnum.taunt)) 
                return true;
        }
        return false;
    }

    public bool IsCrystalEnough(int need)
    {
        return need <= currnetCrystal;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_UseCrystal(int need)
    {
        UseCrystal(need);
    }

    public void UseCrystal(int need)
    {
        currnetCrystal -= need;
        OnUpdateCrystal();
    }

    public void UpCurrentCrystal(int amount)
    {
        currnetCrystal += amount;
        OnUpdateCrystal();
    }

    public void OnUpdateCrystal()
    {
        if (networkObject.HasStateAuthority)
        {
            gameManager.myCrystalUI.UpdateCryStal(currnetCrystal, maxCrystal);

            heroAbility.canUseGlow.SetActive(IsMyTurn() && heroAbility.count > 0 && heroAbility.cost <= currnetCrystal);
            for (int i = 0; i < hand.Count; i++)
            {
                var _card = GetMyCard(hand[i]);
                _card.canUseGlow.SetActive(IsMyTurn() && _card.cost <= currnetCrystal);
            }
        }
        else
        {
            gameManager.opponentCrystalUI.UpdateCryStal(currnetCrystal, maxCrystal);
        }
    }

    public void OnHandChanged()
    {
        float posY = networkObject.HasStateAuthority ? -5f : 5f;
        int count = hand.Count;

        if (count == 0)
        {
            return;
        }
        else if (count == 1)
        {
            gameManager.GetCard(hand[0]).SetPR(new Vector3(0f, posY, 0f), Quaternion.identity, 1f);
        }
        else if (count == 2)
        {
            gameManager.GetCard(hand[0]).SetPR(new Vector3(-0.5f, posY, 0f), Quaternion.identity, 1f);
            gameManager.GetCard(hand[1]).SetPR(new Vector3(0.5f, posY, -1f), Quaternion.identity, 1f);
        }
        else if (count == 3)
        {
            gameManager.GetCard(hand[0]).SetPR(new Vector3(-1f, posY, 0f), Quaternion.identity, 1f);
            gameManager.GetCard(hand[1]).SetPR(new Vector3(0f, posY, -1f), Quaternion.identity, 1f);
            gameManager.GetCard(hand[2]).SetPR(new Vector3(1f, posY, -2f), Quaternion.identity, 1f);
        }
        else
        {
            Vector3 startPos = new Vector3(-0.25f * count - 0.75f, posY, 0);
            Vector3 endPos = new Vector3(0.25f * count + 0.75f, posY, 0);

            Quaternion startRot = Quaternion.Euler(0f, 0f, -15f - count * 1.5f);
            Quaternion endRot = Quaternion.Euler(0f, 0f, 15f + count * 1.5f);
            if (networkObject.HasStateAuthority)
            {
                startRot.z *= -1;
                endRot.z *= -1;
            }

            float interval = 1f / (count - 1);
            float t = 0f;
            for (int i = 0; i < count; ++i)
            {
                startPos.z = -i;
                endPos.z = -i;
                float curve = Mathf.Sin(t * Mathf.PI) * 0.4f;
                if (!networkObject.HasStateAuthority)
                    curve *= -1;
                gameManager.GetCard(hand[i]).SetPR(Vector3.Lerp(startPos, endPos, t) + Vector3.up * curve, Quaternion.Lerp(startRot, endRot, t), 1f);
                t += interval;
            }
        }
    }

    // 1=> -0
    // 2=> -0.75 +0.75
    // 3=> -1.5 -0 +1.5
    // 4=> -2.25 -0.75 +0.75 +2.25
    // 5=> -3 -1.5 -0 +1.5 +3
    public Vector3 GetFieldPos(int i, int cnt = -1)
    {
        if (cnt < 0)
            cnt = field.Count;
        float posX = -0.75f * (cnt - 1) + 1.5f * i;
        float posY = networkObject.HasStateAuthority ? -1f : 1f;
        return new Vector3(posX, posY, 0f);
    }

    public void ChangeShowField()
    {
        for (int i = 0; i < showField.Count; ++i)
        {
            gameManager.GetCard(showField[i]).SetPR(GetFieldPos(i, showField.Count), Quaternion.identity, 1f);
        }
    }

    public void OnCemetryChanged()
    {
        for (int i = 0; i < cemetry.Count; ++i)
        {
            CardMono _cardMono = gameManager.GetCard(cemetry[i]);
            _cardMono.transform.DOMove(_cardMono.networkObject.HasStateAuthority ? new Vector3(11f, -2.5f, 0f) : new Vector3(11f, 2.5f, 0f), 0);
        }
    }
}