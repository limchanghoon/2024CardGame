using Fusion;
using System.Collections;
using System.ComponentModel;
using TMPro;
using UnityEngine;

public class HeroMono : NetworkBehaviour, ITargetable
{
    [SerializeField] Player player;
    [SerializeField] TextMeshPro hpText;
    [SerializeField] GameObject diePrediction;

    private int visiblePower = 0;
    private int visibleHealth = 0;

    public int currentPower { get; set; }
    public int currentHealth { get; set; }
    public bool isDie { get; private set; }
    public bool isTaunt { get; set; }

    private void Awake()
    {
        currentPower = 0;
        currentHealth = 30;
        isTaunt = false;
        isDie = false;

        visiblePower = currentPower;
        visibleHealth = currentHealth;
    }

    public bool CanBeTarget()
    {
        return !isDie;
    }

    public bool CanBeDirectAttackTarget()
    {
        if (CanBeTarget())
        {
            if (player.IsTauntInField())
            {
                if (isTaunt) return true;
                return false;
            }
            return true;
        }
        return false;
    }

    public void Die()
    {
        if (isDie) return;
        isDie = true;
        player.gameManager.GameEnd();
    }

    public TargetType GetTargetType()
    {
        if (Object.HasStateAuthority) return TargetType.MyHero;
        else return TargetType.OpponentHero;
    }

    public int PredictHit(int damage)
    {
        if (damage < 0) return -1;
        return damage;
    }

    public void Hit(int damage)
    {
        if (damage < 0) return;
        currentHealth -= damage;
    }

    public bool CheckIsFirstDie()
    {
        if (isDie) return false;
        if (currentHealth <= 0)
        {
            Die();
            return true;
        }
        return false;
    }

    public bool DieIfHit(int damage)
    {
        if (damage >= currentHealth) return true;
        return false;
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

    public void SetActivePrediction(bool _active)
    {
        diePrediction.SetActive(_active);
    }

    public GameObject GetTargetGameObject() => gameObject;
}
