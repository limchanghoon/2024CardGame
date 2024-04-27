using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GameManager : NetworkBehaviour, IAfterSpawned
{
    // networkObject.HasStateAuthority : 마스터
    // Runner.IsSceneAuthority : 마스터

    NetworkObject networkObject;
    [Networked] public TickTimer StartTimer { get; set; }
    [Networked, OnChangedRender(nameof(OnTimerChanged))] public int timer { get; set; }
    [SerializeField] TextMeshProUGUI textMeshProUGUI;
    [SerializeField] Button btn_EndTurn;
    [SerializeField] GameObject _cardPrefab;

    bool isReady = false;
    int readyCount = 0;
    [SerializeField] int current = -1;
    [Networked, Capacity(2)] public NetworkArray<int> order { get; }

    Player[] players = new Player[2];

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
    public void RPC_StartGame()
    {
        var _players = GameObject.FindGameObjectsWithTag("Player");
        int p1 = _players[0].GetComponent<NetworkObject>().Runner.LocalPlayer.PlayerId;
        int p2 = _players[1].GetComponent<NetworkObject>().Runner.LocalPlayer.PlayerId;
        if(p1 == order[0])
        {
            players[0] = _players[0].GetComponent<Player>();
            players[1] = _players[1].GetComponent<Player>();
        }
        else
        {
            players[0] = _players[1].GetComponent<Player>();
            players[1] = _players[0].GetComponent<Player>();
        }

        players[0].gameManager = this;
        players[1].gameManager = this;
        if (!networkObject.HasStateAuthority) return;

        RPC_TurnNext(0);
    }
    
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_Debug(string str) => Debug.Log(str);

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

    /*
    public override void FixedUpdateNetwork()
    {
        if (StartTimer.Expired(Runner))
        {
            //Runner.Despawn(Object);
        }
        else if(timer >= StartTimer.RemainingTime(Runner))
        {
            timer = (int)StartTimer.RemainingTime(Runner);
        }
    }
    */

    public void TurnNext() => RPC_TurnNext(1 - current);

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

    public bool IsMyTurn()
    {
        return order.Get(current) == Runner.LocalPlayer.PlayerId;
    }
}
