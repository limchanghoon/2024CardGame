using DG.Tweening;
using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameManager : NetworkBehaviour, IAfterSpawned
{
    // networkObject.HasStateAuthority : 마스터
    // Runner.IsSceneAuthority : 마스터

    public Queue<Action> deathRattleQueue = new Queue<Action>();
    public static Queue<Action> actionQueue = new Queue<Action>();
    public static bool isAction = false;

    public bool isGameEnd = false;

    NetworkObject networkObject;
    [Networked, OnChangedRender(nameof(OnTimerChanged))] public int timer { get; set; }
    public GameObject blockPanel;
    [SerializeField] GameObject resultPanel;
    [SerializeField] TextMeshProUGUI resultText;


    public CrystalUI myCrystalUI;
    public CrystalUI opponentCrystalUI;

    [SerializeField] TextMeshPro testText;
    [SerializeField] Button btn_EndTurn;
    [SerializeField] FieldCardTooltip fieldCardTooltip;
    [SerializeField] FieldCardTooltip currentAnimCard;
    [SerializeField] LinePainter linePainter;

    bool isReady = false;
    int readyCount = 0;
    int current = -1;
    bool isTurnDelay = false;
    bool isDeathRattling = false;

    //[Networked, Capacity(2)] NetworkArray<int> order { get; }

    Player[] _players = new Player[2];
    Player[] players { 
        get
        {
            if (_players[0] == null || _players[1] == null)
            {
                SettingAfterAllPlayerSpawned();
            }
            return _players;
        }
    }
    [HideInInspector] public HeroMono[] heroMonos = new HeroMono[2];

    [SerializeField] private MyObjectPool hitObjectPool;
    public EffectManager effectManager;
    public CardSO coinCardSO;


    private void Update()
    {
        if(!isAction && actionQueue.Count > 0)
        {
            isAction = true;
            actionQueue.Dequeue()?.Invoke();
        }
    }

    public void AfterSpawned()
    {
        isAction = false;
        isGameEnd = false;
        actionQueue.Clear();
        deathRattleQueue.Clear();
        if (Runner.SceneManager.MainRunnerScene.buildIndex == 0) return;
        networkObject = GetComponent<NetworkObject>();
        testText.text = "마스터 : " + networkObject.HasStateAuthority.ToString();

        if (networkObject.HasStateAuthority)
        {
            //int idx = 0;
            //foreach (var playerRef_ in Runner.ActivePlayers)
            //{
            //    order.Set(idx++, playerRef_.PlayerId);
            //}

            StartCoroutine(CheckPlayersCoroutine());
        }
    }

    private IEnumerator CheckPlayersCoroutine()
    {
        while (readyCount < 2)
        {
            RPC_CheckPlayers();
            yield return new WaitForSeconds(0.1f);
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
        btn_EndTurn.interactable = false;
        isTurnDelay = true;
        players[0].OnUpdateCrystal();
        players[1].OnUpdateCrystal();
        testText.text = "턴 넘어가는 중..";
        actionQueue.Enqueue(() =>
        {
            isTurnDelay = false;
            current = turn;
            DrawCard(current);
            if (IsMyTurn())
            {
                GetMyPlayer().RPC_StartTurn();
                testText.text = "내 차례";
                btn_EndTurn.interactable = true;
            }
            else
            {
                testText.text = "상대 차례";
                btn_EndTurn.interactable = false;
            }
            GameManager.isAction = false;
        });
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_SettingAfterAllPlayerSpawned()
    {
        SettingAfterAllPlayerSpawned();
    }

    private void SettingAfterAllPlayerSpawned()
    {
        var playerObjs = GameObject.FindGameObjectsWithTag("Player");
        //int p0 = playerObjs[0].GetComponent<NetworkObject>().Runner.LocalPlayer.PlayerId;

        // _players[0] : 마스터, _players[1] : 마스터 아님
        if (networkObject.HasStateAuthority)
        {
            if (playerObjs[0].GetComponent<Player>().networkObject.HasStateAuthority)
            {
                _players[0] = playerObjs[0].GetComponent<Player>();
                _players[1] = playerObjs[1].GetComponent<Player>();

                heroMonos[0] = playerObjs[0].GetComponent<HeroMono>();
                heroMonos[1] = playerObjs[1].GetComponent<HeroMono>();
            }
            else
            {
                _players[0] = playerObjs[1].GetComponent<Player>();
                _players[1] = playerObjs[0].GetComponent<Player>();

                heroMonos[0] = playerObjs[1].GetComponent<HeroMono>();
                heroMonos[1] = playerObjs[0].GetComponent<HeroMono>();
            }
        }
        else
        {
            if (playerObjs[0].GetComponent<Player>().networkObject.HasStateAuthority)
            {
                _players[0] = playerObjs[1].GetComponent<Player>();
                _players[1] = playerObjs[0].GetComponent<Player>();

                heroMonos[0] = playerObjs[1].GetComponent<HeroMono>();
                heroMonos[1] = playerObjs[0].GetComponent<HeroMono>();
            }
            else
            {
                _players[0] = playerObjs[0].GetComponent<Player>();
                _players[1] = playerObjs[1].GetComponent<Player>();

                heroMonos[0] = playerObjs[0].GetComponent<HeroMono>();
                heroMonos[1] = playerObjs[1].GetComponent<HeroMono>();
            }
        }

        //players[0].OnHandChanged();
        //players[1].OnHandChanged();

        heroMonos[0].transform.DOMove(heroMonos[0].HasStateAuthority ? new Vector3(0, -2.8f, 1) : new Vector3(0, 2.8f, 1), 1f);
        heroMonos[1].transform.DOMove(heroMonos[1].HasStateAuthority ? new Vector3(0, -2.8f, 1) : new Vector3(0, 2.8f, 1), 1f);
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

        // 동전 한 닢
        if (players[1].HasStateAuthority)
        {
            NetworkObject _coin = Runner.Spawn(players[1]._MagicCardPrefab, null, null, null, (_runner, _obj) =>
            {
                CardMono cardMono = _obj.GetComponent<CardMono>();
                cardMono.cardID = coinCardSO.cardID;
                cardMono.OwnerPlayer = players[1].networkObject;
                players[1].hand.Add(_obj.Id);
            });
        }


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

    public void TurnNext()
    {
        GetMyPlayer().RPC_EndTurn();
        RPC_TurnNext(1 - current);
    }

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
        testText.text = timer.ToString();
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
        if (player == players[0]) return 0;
        else return 1;
    }

    public bool IsMyTurn()
    {
        if (isTurnDelay) return false;
        return GetMyPlayer() == players[current];
    }

    public void ShowFieldCardTooltip(CardMono_Minion cardMono_Minion, Vector3 _pos)
    {
        fieldCardTooltip.Show(cardMono_Minion, _pos);
    }

    public void DisalbeFieldCardTooltip()
    {
        fieldCardTooltip.Disable();
    }

    public void UpdateFieldCardTooltip()
    {
        fieldCardTooltip.UpdateUI();
    }

    public void ShowCurrentAnimCard(CardMono _cardMono)
    {
        currentAnimCard.Show(_cardMono, new Vector3(-10, 0, 0));
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_EnqueueChangeField()
    {
        EnqueueChangeField();
    }

    // Queue에 한 레이어의 죽음의 메아리 발동 시키자
    public void DoDeathRattleOneLayer()
    {
        if (isDeathRattling) return;
        isDeathRattling = true;
        int cnt = deathRattleQueue.Count;
        if(cnt == 0)
        {
            isDeathRattling = false;
            return;
        }

        while (cnt-- > 0)
        {
            deathRattleQueue.Dequeue()?.Invoke();
        }
        isDeathRattling = false;
        DoDeathRattleOneLayer();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_CheckAllDie(NetworkId[] _needToCheck)
    {
        for(int i = 0;i< _needToCheck.Length; ++i)
        {
            GetNetworkObject(_needToCheck[i]).gameObject.GetComponent<ITargetable>()?.CheckIsFirstDie();
        }
    }

    public void EnqueueChangeField()
    {
        actionQueue.Enqueue(ChangeField);
    }

    public void ChangeField()
    {
        players[0].ChangeShowField();
        players[1].ChangeShowField();
        GameManager.isAction = false;
    }

    public void SetLineTarget(Vector3 p1, Vector3 p2, bool edgeOn, bool targetOn)
    {
        linePainter.Draw(p1, p2, edgeOn, targetOn);
    }

    public void GenerateHitText(int damage, Vector3 _pos)
    {
        hitObjectPool.CreateOjbect().GetComponent<PoolingHit>().Set(damage, _pos);
    }

    public void GameEnd()
    {
        if (isGameEnd) return;
        isGameEnd = true;
        blockPanel.SetActive(true);
        actionQueue.Enqueue(CheckWinner);
    }

    private void CheckWinner()
    {
        if(GetMyHereMono().isDie && GetOpponentHereMono().isDie)
            ShowResult(0);
        else if(GetMyHereMono().isDie)
            ShowResult(1);
        else if (GetOpponentHereMono().isDie)
            ShowResult(2);
        GameManager.isAction = false;
    }

    public void ShowResult(int result)
    {
        blockPanel.SetActive(true);
        resultPanel.SetActive(true);
        resultPanel.transform.DOScale(new Vector3(1f, 1f, 1f), 1f).SetEase(Ease.OutElastic);
        if (result == 0)
        {
            resultText.text = "무승부!";
        }
        else if (result == 1)
        {
            resultText.text = "패배!";
        }
        else
        {
            resultText.text = "승리!";
        }
    }

    public void Btn_LeftGame()
    {
        ShutdownGame(Runner);
    }

    public async void ShutdownGame(NetworkRunner runner)
    {
        await runner.Shutdown();
        SceneManager.LoadScene(0);
    }
}
