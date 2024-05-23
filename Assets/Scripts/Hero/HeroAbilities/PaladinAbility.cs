using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaladinAbility : HeroAbility
{
    [SerializeField] GameObject _MinionCardPrefab;
    [SerializeField] CardSO cardSO;

    public override void Spawned()
    {
        base.Spawned();
        currentMouseEvent = new FieldMouseEvent_HeroAbility(this, OwnerPlayer.GetComponent<Player>());
    }

    public override void Execute(CardMono mine, NetworkId target, CommandType _commandType)
    {
        if (!myPlayer.IsMyTurn()) return;
        if (!myPlayer.IsCrystalEnough(cost)) return;
        if (!DecreaseCount()) return;
        myPlayer.RPC_UseCrystal(cost);
        if (myPlayer.field.Count == myPlayer.field.Capacity)
        {
            Debug.LogAssertion("ÇÊµå FULL");
            return;
        }
        NetworkObject soldier = Runner.Spawn(_MinionCardPrefab, null, null, null, (_runner, _obj) =>
        {
            CardMono cardMono = _obj.GetComponent<CardMono>();
            cardMono.cardID = cardSO.cardID;
            cardMono.OwnerPlayer = myPlayer.networkObject;
        });
        myPlayer.RPC_SpawnNewObject(soldier, myPlayer.field.Count);
    }

    public override bool IsNeedTarget()
    {
        return false;
    }
}
