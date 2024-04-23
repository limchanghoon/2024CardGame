using Fusion;
using System.Linq;
using TMPro;
using UnityEngine;

public class BasicSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _networkRunnerPrefab;
    public TextMeshProUGUI roomName;
    public GameObject btn_Join;
    public GameObject btn_Shutdown;
    public GameObject btn_Start;
    public GameObject btn_MyRoom;
    public NetworkRunner newRunner;
    RunnerHelper runnerHelper;

    public async void JoinGame()
    {
        BtnControll();
        string LocalRoomName = "TestRoom";
        bool joinRandomRoom = false;

        StartGameArgs startGameArgs = new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = joinRandomRoom ? string.Empty : LocalRoomName,
            PlayerCount = 2,
        };

        newRunner = Instantiate(_networkRunnerPrefab).GetComponent<NetworkRunner>();
        runnerHelper = newRunner.GetComponent<RunnerHelper>();
        runnerHelper.basicSpawner = this;

        StartGameResult result = await newRunner.StartGame(startGameArgs);
        if (!result.Ok)
        {
            BtnControll(true, false, false, false);
            Debug.LogError(result.ErrorMessage);

        }
    }

    public async void ShutdownGame()
    {
        BtnControll();
        await newRunner.Shutdown();
        roomName.text = string.Empty;
        BtnControll(true, false, false, true);
    }

    public void BtnControll(bool join = false, bool shutdown = false, bool start = false, bool myRoom = false)
    {
        btn_Join.SetActive(join);
        btn_Shutdown.SetActive(shutdown);
        btn_Start.SetActive(start);
        btn_MyRoom.SetActive(myRoom);
    }
}
