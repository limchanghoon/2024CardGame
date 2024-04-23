using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    public GameObject _cardPrefab;
    BasicSpawner basicSpawner;

    [Networked, Capacity(10), OnChangedRender(nameof(OnHandChanged))] public NetworkArray<NetworkId> hand { get; }
    [Networked, Capacity(50)] public NetworkArray<NetworkId> deck { get; }

    [Networked, Capacity(50)] private NetworkDictionary<NetworkId, NetworkObject> cardDictionary_NetworkObject { get; }
    private Dictionary<NetworkId, CardMono> cardDictionary = new Dictionary<NetworkId, CardMono>();

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        if (Object.HasStateAuthority) {
            basicSpawner = GameObject.Find("BasicSpawner").GetComponent<BasicSpawner>();
            Button button = basicSpawner.btn_Start.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(StartGame);
        }
    }

    public void StartGame()
    {
        if (Runner.ActivePlayers.Count() < Runner.Config.Simulation.PlayerCount)
        {
            RPC_Debug("인원수 부족!");
        }
        else if (Runner.IsSceneAuthority)
        {
            RPC_Debug("게임 시작!");
            Runner.LoadScene(SceneRef.FromIndex(1), LoadSceneMode.Single);
        }
        else
        {
            RPC_Debug("방장이 아닙니다!");
        }
    }

    public void AddToCardDictionary(NetworkId _networkId, NetworkObject networkObject)
    {
        cardDictionary_NetworkObject.Add(_networkId, networkObject);
    }

    public CardMono GetCard(NetworkId _networkId)
    {
        CardMono cardMono;
        if(cardDictionary.TryGetValue(_networkId, out cardMono))
        {
            return cardMono;
        }
        else
        {
            cardDictionary.Add(_networkId, cardDictionary_NetworkObject[_networkId].GetComponent<CardMono>());
            return cardDictionary[_networkId];
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_Debug(string str)
    {
        Debug.Log(str);
    }

    public void OnHandChanged()
    {

    }
}