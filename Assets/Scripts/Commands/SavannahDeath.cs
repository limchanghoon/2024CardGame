using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavannahDeath : NetworkBehaviour, ICommand
{
    [SerializeField] GameObject _MinionCardPrefab;
    [SerializeField] CardSO cardSO;
    [SerializeField] int amount;

    public void Execute(CardMono mine, NetworkId target, CommandType _commandType)
    {
        if (!mine.owner.IsMyTurn()) return;
        int curCount = mine.owner.field.Count;
        int _location = (int)target.Raw;
        for (int i = 0; i < amount; ++i)
        {
            if (curCount + i == mine.owner.field.Capacity)
            {
                Debug.LogAssertion("ÇÊµå FULL");
                break;
            }
            NetworkObject hyena = Runner.Spawn(_MinionCardPrefab, null, null, null, (_runner, _obj) =>
            {
                CardMono cardMono = _obj.GetComponent<CardMono>();
                cardMono.cardID = cardSO.cardID;
                cardMono.OwnerPlayer = mine.owner.networkObject;
            });
            mine.owner.RPC_SpawnNewObject(hyena, _location);
        }

        // Á×¸Þ!
        mine.owner.gameManager.DoDeathRattleOneLayer();
        mine.owner.gameManager.RPC_EnqueueChangeField();
    }

    public bool IsNeedTarget()
    {
        return false;
    }
}
