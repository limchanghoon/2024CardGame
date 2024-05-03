using Fusion;
using TMPro;
using UnityEngine;

public class HeroMono : NetworkBehaviour, ITargetable
{
    [SerializeField] Player player;

    [SerializeField] TextMeshPro hpText;

    private int visiblePower = 0;
    private int visibleHealth = 0;

    public int currentPower { get; set; }
    public int currentHealth { get; set; }

    private void Awake()
    {
        currentPower = 0;
        currentHealth = 30;

        visiblePower = currentPower;
        visibleHealth = currentHealth;
    }

    public bool CanBeTarget()
    {
        return true;
    }

    public void Die()
    {

    }

    public TargetType GetTargetType()
    {
        if (Object.HasStateAuthority) return TargetType.MyHero;
        else return TargetType.OpponentHero;
    }

    [Rpc(RpcSources.All, RpcTargets.Proxies)]
    public void RPC_Hit(int damage)
    {
        Hit(damage);
    }

    public int Hit(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
        return damage;
    }

    public void UpdateHit(int damage)
    {
        if (damage < 0) return;
        player.gameManager.GenerateHitText(damage, transform.position);
        visibleHealth -= damage;
        hpText.text = visibleHealth.ToString();
    }

    public NetworkId GetNetworkId()
    {
        return player.networkObject.Id;
    }
}
