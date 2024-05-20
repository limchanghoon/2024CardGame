using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BasicSpawner : MonoBehaviour
{
    [SerializeField] private MyJsonManager myJsonManager;
    [SerializeField] private GameObject _networkRunnerPrefab;
    public TextMeshProUGUI roomName;
    public GameObject btn_SelectDeck;
    public GameObject btn_Shutdown;
    public GameObject btn_Start;
    public GameObject btn_MyRoom;
    public GameObject selectDeckPanel;
    [SerializeField] private Button[] deckBtns;
    [SerializeField] private TextMeshProUGUI[] deckTexts;
    NetworkRunner newRunner;

    int page = 0;
    public int selectedIndex = 0;

    public void GoToSelectDeck()
    {
        BtnControll();
        selectDeckPanel.SetActive(true);
        UpdateDeckUI();
    }

    public void ShowNextPage()
    {
        page = page == 0 ? 1 : 0;
        UpdateDeckUI();
    }

    private void UpdateDeckUI()
    {
        for (int i = 0; i < 10; ++i)
        {
            var deckData = myJsonManager.LoadDeckData(page * 10 + i);
            deckTexts[i].text = deckData.DeckName;
            deckBtns[i].interactable = deckData.IsCompleted();
        }
    }

    public void SelectDeck(int idx)
    {
        selectedIndex = page * 10 + idx;
        PlayerPrefs.SetInt("SelectedIndex", selectedIndex);
        selectDeckPanel.SetActive(false);
        JoinGame();
    }

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
        newRunner.GetComponent<RunnerHelper>().basicSpawner = this;

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

    public void BtnControll(bool selectDeck = false, bool shutdown = false, bool start = false, bool myRoom = false)
    {
        btn_SelectDeck.SetActive(selectDeck);
        btn_Shutdown.SetActive(shutdown);
        btn_Start.SetActive(start);
        btn_MyRoom.SetActive(myRoom);
    }
}
