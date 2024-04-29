using DG.Tweening;
using TMPro;
using UnityEngine;

public class PoolingHit : PoolingObject
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] TextMeshPro text;
    [SerializeField] Gradient gradient;

    float t = 0f;
    float targetT = 3f;

    public void Set(int _damage, Vector3 _pos)
    {
        text.text = "-" + _damage.ToString();
        transform.DOMove(_pos, 0);
    }

    private void OnEnable()
    {
        t = 0f;
        Color newColor = gradient.Evaluate(0);
        spriteRenderer.color = newColor;
        text.color = newColor;
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
