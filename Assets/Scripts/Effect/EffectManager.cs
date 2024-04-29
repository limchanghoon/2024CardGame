using DG.Tweening;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [SerializeField] GameObject testEffect;

    public void DoHitEffect(Vector3 _pos)
    {
        testEffect.transform.DOMove(_pos, 0);
        testEffect.SetActive(true);
    }
}
