using DG.Tweening;
using TMPro;
using UnityEngine;

public class PoolingHit : PoolingObject
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] TextMeshPro text;
    [SerializeField] Gradient gradient;

    float t = 0f;
    float targetT = 2f;

    public void Set(int _damage, Vector3 _pos)
    {
        _pos.z = -200;
        text.text = "-" + _damage.ToString();
        transform.DOMove(_pos, 0);
        t = 0f;
    }

    private void Update()
    {
        if (t < targetT)
        {
            t += Time.deltaTime;
            Color newColor = gradient.Evaluate(t / targetT);
            spriteRenderer.color = newColor;
            text.color = newColor;
            if (t >= targetT)
            {
                DestroyObject();
            }
        }
    }
}
