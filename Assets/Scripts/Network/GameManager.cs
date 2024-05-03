using DG.Tweening;
using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GameManager : NetworkBehaviour, IAfterSpawned
{
    // networkObject.HasStateAuthority : 마스터
    // Runner.IsSceneAuthority : 마스터

    NetworkObject networkObject;
    [Networked, OnChangedRender(nameof(OnTimerChanged))] public int timer { get; set; }
    [SerializeField] TextMeshProUGUI textMeshProUGUI;
    [SerializeField] Button btn_EndTurn;
    [SerializeField] FieldCardTooltip fieldCardTooltip;
    [SerializeField] LinePainter linePainter;
    [SerializeField] GameObject _cardPrefab;

    bool isReady = false;
    int readyCount = 0;
    [SerializeField] int current = -1;
    [Networked, Capacity(2)] NetworkArray<int> order { get; }

    Player[] players = new Player[2];
    [HideInInspector] public HeroMono[] heroMonos = new HeroMono[2];

    [SerializeField] private MyObjectPool hitObjectPool;
    public EffectManager effectManager;

    public void AfterSpawned()
    {
        if (Runner.SceneManager.MainRunnerScene.buildIndex == 0) return;
        networkObject = GetComponent<NetworkObject>();
        textMeshProUGUI.text = "마스터 : " + networkObject.HasStateAuthority.ToString();

        if (networkObject.HasStateAuthority)
        {
            int idx = 0;
            foreach (var playerRef_ in Runner.ActivePlayers)
            {
                order.Set(idx++, playerRef_.PlayerId);
            }

            StartCoroutine(CheckPlayersCoroutine());
        }
    }

    private IEnumerator CheckPlayersCoroutine()
    {
        while (readyCount < 2)
        {
            RPC_CheckPlayers();
            yield return new WaitForSeconds(1f);
        }
        RPC_SettingAfterAllPlayerSpawned();
        RPC_StartGame();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_CheckPlayers()
    {
        if (isReady) return;
        isReady = true;

        RPC_SendAck();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SendAck() => ++readyCount;

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_TurnNext(int turn)
    {
        current = turn;
        DrawCard(current);
        if (IsMyTurn())
        {
            textMeshProUGUI.text = "내 차례";
            btn_EndTurn.interactable = true;
        }
        else
        {
            textMeshProUGUI.text = "상대 차례";
            btn_EndTurn.interactable = false;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_SettingAfterAllPlayerSpawned()
    {
        var _players = GameObject.FindGameObjectsWithTag("Player");
        int p0 = _players[0].GetComponent<NetworkObject>().Runner.LocalPlayer.PlayerId;
        int p1 = _players[1].GetComponent<NetworkObject>().Runner.LocalPlayer.PlayerId;
        if (p0 == order[0])
        {
            players[0] = _players[0].GetComponent<Player>();
            players[1] = _players[1].GetComponent<Player>();

            heroMonos[0] = _players[0].GetComponent<HeroMono>();
            heroMonos[1] = _players[1].GetComponent<HeroMono>();
        }
        else
        {
            players[0] = _players[1].GetComponent<Player>();
            players[1] = _players[0].GetComponent<Player>();

            heroMonos[0] = _players[1].GetComponent<HeroMono>();
            heroMonos[1] = _players[0].GetComponent<HeroMono>();
        }

        players[0].OnHandChanged();
        players[0].OnFieldChanged();
        players[1].OnHandChanged();
        players[1].OnFieldChanged();

        heroMonos[0].transform.DOMove(heroMonos[0].HasInputAuthority ? new Vector3(0, -2.5f, 1) : new Vector3(0, 2.5f, 1), 1f);
        heroMonos[1].transform.DOMove(heroMonos[1].HasInputAuthority ? new Vector3(0, -2.5f, 1) : new Vector3(0, 2.5f, 1), 1f);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_StartGame()
    {
        for (int i = 0; i < 3; ++i)
        {
            DrawCard(0);
            DrawCard(1);
        }
        DrawCard(1);

        if (!networkObject.HasStateAuthority) return;

        RPC_TurnNext(0);
    }
    
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_Debug(string str) => Debug.Log(str);

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_DrawCard(int turn_) => DrawCard(turn_);

    // turn_에 해당하는 플레이어만 수행함
    public void DrawCard(int turn_)
    {
        Player curPlayer = players[turn_];
        if (!curPlayer.HasStateAuthority) return;

        if (curPlayer.deck.Count == 0)
        {
            Debug.Log("덱 없음! => 데미지 받아야함!");
            return;
        }
        if (curPlayer.hand.Count == curPlayer.hand.Capacity)
        {
            Debug.Log("핸드 꽉참! => 드로우 카드 삭제");
            curPlayer.deck.Remove(curPlayer.deck.ElementAt(0));
            return;
        }

        NetworkId temp = curPlayer.deck.ElementAt(0);

        curPlayer.hand.Add(temp);
        curPlayer.deck.Remove(temp);
    }

    public void TurnNext() => RPC_TurnNext(1 - current);

    public CardMono GetCard(NetworkId _networkId)
    {
        CardMono result = players[0].GetMyCard(_networkId);
        if(result == null)
        {
            result = players[1].GetMyCard(_networkId);
        }
        // 먼가 문제가 생겨서 카드가 등록이 안됨!
        if(result == null)
        {
            var objs = GameObject.FindGameObjectsWithTag("Card");
            foreach (var obj in objs)
            {
                CardMono _cardMono = obj.GetComponent<CardMono>();
                _cardMono.owner.AddToCardDictionary(_cardMono.uniqueID, _cardMono);
                if (_networkId == _cardMono.uniqueID) result = _cardMono;
            }
        }
        return result;
    }

    public GameObject GetNetworkObject(NetworkId _networkId)
    {
        GameObject result = GetCard(_networkId)?.gameObject;
        if(result == null)
        {
            if (players[0].networkObject.Id == _networkId) result = heroMonos[0].gameObject;
            if (players[1].networkObject.Id == _networkId) result = heroMonos[1].gameObject;
        }
        return result;
    }

    public void OnTimerChanged()
    {
        textMeshProUGUI.text = timer.ToString();
    }

    public Player GetMyPlayer()
    {
        if (players[0] == null) return null;
        for(int i = 0;i < players.Length; ++i)
        {
            if (players[i].HasStateAuthority) return players[i];
        }
        return null;
    }

    public Player GetOppenetPlayer()
    {
        if (players[0] == null) return null;
        for (int i = 0; i < players.Length; ++i)
        {
            if (!players[i].HasStateAuthority) return players[i];
        }
        return null;
    }

    public Player GetCardOwner(CardMono _cardMono)
    {
        NetworkId _networkId = _cardMono.uniqueID;
        if (players[0].GetMyCard(_networkId) != null)
            return players[0];
        else
            return players[1];
    }

    public HeroMono GetMyHereMono()
    {
        for (int i = 0; i < players.Length; ++i)
        {
            if (players[i].HasStateAuthority) return heroMonos[i];
        }
        return null;
    }

    public HeroMono GetOpponentHereMono()
    {
        for (int i = 0; i < players.Length; ++i)
        {
            if (!players[i].HasStateAuthority) return heroMonos[i];
        }
        return null;
    }

    public int GetPlayerOrder(Player player)
    {
        if (order[0] == player.Runner.LocalPlayer.PlayerId) return 0;
        else return 1;
    }

    public bool IsMyTurn()
    {
        return order.Get(current) == Runner.LocalPlayer.PlayerId;
    }

    public void ShowFieldCardTooltip(CardMono cardMono, Vector3 _pos)
    {
        fieldCardTooltip.Show(cardMono, _pos);
    }

    public void DisalbeFieldCardTooltip()
    {
        fieldCardTooltip.Disable();
    }

    public void UpdateFieldCardTooltip()
    {
        fieldCardTooltip.UpdateUI();
    }

    public void SetLineTarget(Vector3 p1, Vector3 p2, bool edgeOn, bool targetOn)
    {
        linePainter.Draw(p1, p2, edgeOn, targetOn);
    }

    public void GenerateHitText(int damage, Vector3 _pos)
    {
        hitObjectPool.CreateOjbect().GetComponent<PoolingHit>().Set(damage, _pos);
    }

    [ContextMenu("UPDATEUPDATE")]
    public void UPDATEUPDATE()
    {
        RPC_UPDATEUPDATE();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_UPDATEUPDATE()
    {
        players[0].OnFieldChanged();
        players[1].OnFieldChanged();
    }
}
