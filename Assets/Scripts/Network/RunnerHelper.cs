using Fusion.Sockets;
using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using System.IO;

public class RunnerHelper : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    private NetworkObject spawnedCharacter;

    public BasicSpawner basicSpawner { get; set; }


    #region INetworkRunnerCallbacks콜백함수

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("OnConnectedToServer");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.Log("OnConnectFailed");
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        Debug.Log("OnConnectRequest");
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        Debug.Log("OnCustomAuthenticationResponse");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log("OnDisconnectedFromServer");
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        Debug.Log("OnHostMigration");
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {

    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        Debug.Log("OnInputMissing");
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        Debug.Log("OnObjectEnterAOI");
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        Debug.Log("OnObjectExitAOI");
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer)
        {
            Debug.Log("OnPlayerJoined");
            spawnedCharacter = runner.Spawn(_playerPrefab, null, null, player);
        }
        if (runner.SceneManager.MainRunnerScene.buildIndex == 0)
        {
            if (runner.IsSceneAuthority)
            {
                basicSpawner.btn_Join.SetActive(false);
                basicSpawner.btn_Shutdown.SetActive(true);
                basicSpawner.btn_Start.SetActive(true);
                basicSpawner.roomName.text = "방장 : " + runner.ActivePlayers.Count().ToString() + "/" + runner.Config.Simulation.PlayerCount.ToString();
            }
            else
            {
                basicSpawner.btn_Join.SetActive(false);
                basicSpawner.btn_Shutdown.SetActive(true);
                basicSpawner.btn_Start.SetActive(false);
                basicSpawner.roomName.text = "게스트 : " + runner.ActivePlayers.Count().ToString() + "/" + runner.Config.Simulation.PlayerCount.ToString();
            }
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer)
        {
            runner.Despawn(spawnedCharacter);
            spawnedCharacter = null;
        }

        if (runner.SceneManager.MainRunnerScene.buildIndex == 0)
        {
            if (runner.IsSceneAuthority)
            {
                basicSpawner.btn_Join.SetActive(false);
                basicSpawner.btn_Shutdown.SetActive(true);
                basicSpawner.btn_Start.SetActive(true);
                basicSpawner.roomName.text = "방장 : " + runner.ActivePlayers.Count().ToString() + "/" + runner.Config.Simulation.PlayerCount.ToString();
            }
            else
            {
                basicSpawner.btn_Join.SetActive(false);
                basicSpawner.btn_Shutdown.SetActive(true);
                basicSpawner.btn_Start.SetActive(false);
                basicSpawner.roomName.text = "게스트 : " + runner.ActivePlayers.Count().ToString() + "/" + runner.Config.Simulation.PlayerCount.ToString();
            }
        }
        else if (runner.SceneManager.MainRunnerScene.buildIndex == 1)
        {
            ShutdownGame(runner);
        }
    }

    private async void ShutdownGame(NetworkRunner runner)
    {
        await runner.Shutdown();
        SceneManager.LoadScene(0);
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        Debug.Log("OnReliableDataProgress");
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        Debug.Log("OnReliableDataReceived");
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("OnSceneLoadDone");
        if (runner.SceneManager.MainRunnerScene.buildIndex == 1)
        {
            // 임시로 0번 덱으로 시작
            int slotIdx = 0;
            string dirPath = Path.Combine(Application.persistentDataPath, "Deck");
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            string path = Path.Combine(dirPath, "Slot" + slotIdx.ToString() + ".json");
            if (!File.Exists(path))
            {
                Debug.LogAssertion("덱 정보가 없음!!!");
                return;
            }

            string loadedJson = File.ReadAllText(path);
            var loadedData = JsonUtility.FromJson<DeckData>(loadedJson);
            loadedData.Shuffle();

            Player player = spawnedCharacter.GetComponent<Player>();
            for (int i = 0; i < 30; ++i)
            {
                var _netObject = runner.Spawn(player._cardPrefab, null, null, runner.LocalPlayer, (_runner, _obj) =>
                {
                    CardMono cardMono = _obj.GetComponent<CardMono>();
                    cardMono.uniqueID = _obj.Id;
                    cardMono.cardID = loadedData.cardIDs[i];
                    cardMono.Target = spawnedCharacter;
                    player.deck.Add(_obj.Id);
                });
            }
        }
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        Debug.Log("OnSceneLoadStart");
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log("OnSessionListUpdated ? "+ sessionList.Count);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("OnShutdown");
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        Debug.Log("OnUserSimulationMessage");
    }

    #endregion
}
