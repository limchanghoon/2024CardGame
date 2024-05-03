
using Fusion;

[System.Serializable]
public struct NetworkData : INetworkStruct
{
    public NetworkId networkId;
    public int damage;

    public NetworkData(NetworkId networkId, int damage)
    {
        this.networkId = networkId;
        this.damage = damage;
    }
}
