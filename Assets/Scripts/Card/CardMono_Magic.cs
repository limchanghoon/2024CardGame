using DG.Tweening;
using Fusion;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class CardMono_Magic : CardMono
{

    HandMouseEvent_Magic handMouseEvent;


    [Networked] public NetworkObject magicObj { get; private set; }

    private ICommand _magic;
    public ICommand magic
    {
        get
        {
            if (magicObj == null) return null;
            if (_magic == null) _magic = magicObj.GetComponent<ICommand>();
            return _magic;
        }
        set
        {
            _magic = value;
        }
    }


    public override void Spawned()
    {
        networkObject = GetComponent<NetworkObject>();

        var op = Addressables.LoadAssetAsync<CardSO>("Assets/Data/CardData/" + cardID.ToString() + ".asset");
        CardSO _data = op.WaitForCompletion();
        if (op.Result != null)
        {
            cardSO = _data;
            SetAsSO();
        }

        owner = OwnerPlayer.GetComponent<Player>();
        owner.AddToCardDictionary(uniqueID, this);

        handMouseEvent = new HandMouseEvent_Magic(this);
        currentMouseEvent = handMouseEvent;

        nameText.text = cardSO.cardName;
        costText.text = cardSO.cost.ToString();

        transform.DOMove(networkObject.HasInputAuthority ? new Vector3(11f, -2.5f, 0f) : new Vector3(11f, 2.5f, 0f), 0);
        frontFace.SetActive(networkObject.HasInputAuthority);
        backFace.SetActive(!networkObject.HasInputAuthority);
    }


    public void SetAsSO()
    {
        abilityText.text = cardSO.infomation;

        //if (cardSO.grade == CardGrade.Lengend)
        //    bgRender.sprite = legendSprite;

        if (networkObject.HasInputAuthority)
        {
            if (cardSO.magic)
            {
                magicObj = Runner.Spawn(cardSO.magic, null, null, Runner.LocalPlayer);
            }
        }
    }

    public override void Predict(GameObject obj)
    {

    }

    public void FireMagic(NetworkId _target)
    {
        if (!owner.IsMyTurn()) return;
        if (magicObj == null) return;
        RPC_FireMagic(_target);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_FireMagic(NetworkId _target)
    {
        magic?.Execute(this, _target);
    }

}
