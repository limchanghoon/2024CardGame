using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LinePainter : MonoBehaviour
{
    LineRenderer lineRenderer;
    [SerializeField] GameObject lineEdge;
    [SerializeField] GameObject doubleCircle;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void Draw(Vector3 p1, Vector3 p2, bool edgeOn, bool targetOn)
    {
        p1.z = -100f;
        p2.z = -100f;
        lineRenderer.SetPosition(0, p1);
        lineRenderer.SetPosition(1, p2);
        lineEdge.transform.position = p2;
        Vector3 v = p2 - p1;
        lineEdge.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg - 90f);

        lineEdge.SetActive(edgeOn);
        doubleCircle.SetActive(targetOn);
    }


}
