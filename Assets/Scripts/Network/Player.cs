using DG.Tweening;
using Fusion;
using System.Collections.Generic;
using System.Linq;
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

    public GameObject _cardPrefab;
    public NetworkObject networkObject { get; private set; }

    [Networked, Capacity(7), OnChangedRender(nameof(OnFieldChanged))] public NetworkLinkedList<NetworkId> field { get; }
    //public List<NetworkId> field = new List<NetworkId>(7);
    [Networked, Capacity(10), OnChangedRender(nameof(OnHandChanged))] public NetworkLinkedList<NetworkId> hand { get; }
    [Networked, Capacity(60)] public NetworkLinkedList<NetworkId> cemetry { get; }
    [Networked, Capacity(50)] public NetworkLinkedList<NetworkId> deck { get; }
    private Dictionary<NetworkId, CardMono> cardDictionary = new Dictionary<NetworkId, CardMono>();

    private void Awake()
    {
        DontDestroyOnLoad(this);
        networkObject = GetComponent<NetworkObject>();
    }


    private void Start()
    {
        if (Object.HasStateAuthority)
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
    public void RPC_Debug(string str)
    {
        Debug.Log(str);
    }

    public void DrawMyCard()
    {
        gameManager.DrawCard(gameManager.GetPlayerOrder(this));
    }

    public void SpawnCardOfHand(NetworkId _uniqueID, int _location, NetworkId _target = default)
    {
        if (Object.HasStateAuthority)
            hand.Remove(_uniqueID);

        if (field.Count == field.Capacity)
        {
            Debug.LogAssertion("필드 FULL");
            return;
        }
        /*
        if (_location < field.Count)
        {
            field.Insert(_location, _uniqueID);
        }
        else
        {
            field.Add(_uniqueID);
        }
        */
        if (field.Count == 0)
        {
            field.Add(_uniqueID);
        }
        else
        {
            NetworkId _temp = field[field.Count - 1];
            for (int i = field.Count - 1; i > _location; i--)
            {
                field.Set(i, field[i - 1]);
            }
            field.Add(_temp);
            field.Set(_location, _uniqueID);
        }


        CardMono _cardMono = gameManager.GetCard(_uniqueID);
        _cardMono.RPC_ChangeCardState(CardState.Field);

        // 전투의 함성
        if (_cardMono.battleCry != null)
            _cardMono.BattleCry(_target);
    }

    public void DestroyCardOfField(CardMono _cardMono)
    {
        NetworkId _uniqueID = _cardMono.uniqueID;

        field.Remove(_uniqueID);

        if (cemetry.Count == cemetry.Capacity) cemetry.Remove(cemetry.Get(0));
        cemetry.Add(_uniqueID);

        _cardMono.RPC_ChangeCardState(CardState.Cemetry);

        // 죽음의 메아리 발동해야함
        if (_cardMono.deathRattle != null)
            _cardMono.DeathRattle();
    }

    public bool IsMyTurn()
    {
        return gameManager.IsMyTurn();
    }

    public void ShowFieldCardTooltip(CardMono cardMono)
    {
        //gameManager.ShowFieldCardTooltip(cardMono, GetFieldPos(field.IndexOf(cardMono.uniqueID)));
        gameManager.ShowFieldCardTooltip(cardMono, cardMono.transform.position);
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

    public void OnHandChanged()
    {
        float posY = networkObject.HasInputAuthority ? -5f : 5f;
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
            if (networkObject.HasInputAuthority)
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
                if (!networkObject.HasInputAuthority)
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
        float posY = networkObject.HasInputAuthority ? -1f : 1f;
        return new Vector3(-0.75f * (cnt - 1) + 1.5f * i, posY, 0f);
    } 

    public void OnFieldChanged()
    {
        for (int i = 0; i < field.Count; ++i)
        {
            gameManager.GetCard(field[i]).SetPR(GetFieldPos(i, field.Count), Quaternion.identity, 1f);
        }
    }

    public void OnCemetryChanged()
    {
        for (int i = 0; i < cemetry.Count; ++i)
        {
            CardMono _cardMono = gameManager.GetCard(cemetry[i]);
            _cardMono.transform.DOMove(_cardMono.networkObject.HasInputAuthority ? new Vector3(11f, -2.5f, 0f) : new Vector3(11f, 2.5f, 0f), 0);
        }
    }
}