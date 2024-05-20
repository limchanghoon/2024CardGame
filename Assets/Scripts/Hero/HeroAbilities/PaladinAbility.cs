using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaladinAbility : HeroAbility
{
    [SerializeField] GameObject _MinionCardPrefab;
    [SerializeField] int cardID;

    public override void Spawned()
    {
        base.Spawned();
        currentMouseEvent = new FieldMouseEvent_HeroAbility(this, OwnerPlayer.GetComponent<Player>());
    }

    public override void Execute(CardMono mine, NetworkId target)
    {
        if (!myPlayer.IsMyTurn()) return;
        if (myPlayer.field.Count == myPlayer.field.Capacity)
        {
            Debug.LogAssertion("ÇÊµå FULL");
            return;
        }
        NetworkObject soldier = Runner.Spawn(_MinionCardPrefab, null, null, Runner.LocalPlayer, (_runner, _obj) =>
        {
            CardMono cardMono = _obj.GetComponent<CardMono>();
            cardMono.uniqueID = _obj.Id;
            cardMono.cardID = cardID;
            cardMono.OwnerPlayer = myPlayer.networkObject;
        });
        myPlayer.RPC_SpawnNewObject(soldier, myPlayer.field.Count);
    }

    public override bool IsNeedTarget()
    {
        return false;
    }
}
