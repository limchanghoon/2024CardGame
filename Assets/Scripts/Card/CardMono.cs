using System.Collections;
using TMPro;
using UnityEngine;
using DG.Tweening;
using Fusion;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Unity.VisualScripting;

public class CardMono : NetworkBehaviour
{
    NetworkObject networkObject;
    [SerializeField] CardSO cardSO;
    [SerializeField] SpriteRenderer spriteRender;
    [SerializeField] TextMeshPro nameText;
    [SerializeField] TextMeshPro costText;
    [SerializeField] TextMeshPro powerText;
    [SerializeField] TextMeshPro healthText;
    [SerializeField] GameObject frontFace;
    [SerializeField] GameObject backFace;
    [SerializeField] GameObject backFaceGlow;

    [Networked, OnChangedRender(nameof(OnisZoomingChanged))][SerializeField] private bool isZooming { get; set; }
    [Networked, OnChangedRender(nameof(OnisZoomingChanged))][SerializeField] private bool isDragging { get; set; }

    public Vector3 origin;
    Transform imageTr;

    [Networked] public NetworkId uniqueID {  get; set; }
    [Networked] public int cardID {  get; set; }
    AsyncOperationHandle<CardSO> op;

    private void Awake()
    {
        imageTr = transform.GetChild(0).GetComponent<Transform>();
        networkObject = GetComponent<NetworkObject>();

        transform.position = new Vector3(9999, 9999, 9999);
    }

    private void Start()
    {
        origin = transform.position;
    }

    public override void Spawned()
    {
        var op = Addressables.LoadAssetAsync<CardSO>("Assets/Data/CardData/" + cardID.ToString() + ".asset");
        CardSO _data = op.WaitForCompletion();
        if (op.Result != null)
        {
            cardSO = _data;
        }
    }

    private void OnDestroy()
    {
        if (op.IsValid())
            Addressables.Release(op);
    }

    private void OnMouseEnter()
    {
        if (!networkObject.HasInputAuthority) return;
        if (isDragging || isZooming) return;
        isZooming = true;
        imageTr.DOScale(Vector3.one * 1.5f, 0.2f);
        imageTr.DOLocalMove(new Vector3(0, 3f, -100f), 0f);
    }

    private void OnMouseExit()
    {
        if (!networkObject.HasInputAuthority) return;
        if (isDragging) return;
        isZooming = false;
        imageTr.DOScale(new Vector3(0.4f, 0.4f, 1f), 0.2f);
        imageTr.DOLocalMove(Vector3.zero, 0f);
    }

    private void OnMouseDown()
    {
        if (!networkObject.HasInputAuthority) return;
        OnMouseExit();
        isDragging = true;
        origin = transform.position;
    }

    private void OnMouseDrag()
    {
        if (!networkObject.HasInputAuthority) return;
        Vector3 _pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _pos.z = -100f;
        transform.position = _pos;
    }

    private void OnMouseUp()
    {
        if (!networkObject.HasInputAuthority) return;
        transform.DOMove(origin, 0f);
        StartCoroutine(OnMouseUpCoroutine());
    }

    private void OnisZoomingChanged()
    {
        if (networkObject.HasInputAuthority) return;
        if (isZooming || isDragging) backFaceGlow.SetActive(true);
        else backFaceGlow.SetActive(false);
    }

    // transform.DOMove(origin, 0f); 이후에 콜라이더도 완전히 이동한 뒤
    // OnMouseOver이면 OnMouseEnter실행 시킴
    private IEnumerator OnMouseUpCoroutine()
    {
        yield return new WaitForFixedUpdate();
        isDragging = false;
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Ray2D ray = new Ray2D(pos, Vector2.zero);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
        if (hit && hit.collider.gameObject == gameObject)
        {
            OnMouseEnter();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SetPosition(int idx)
    {
        if (networkObject.HasInputAuthority)
        {
            frontFace.SetActive(true);
            backFace.SetActive(false);

            nameText.text = cardSO.cardName;
            costText.text = cardSO.cost.ToString();
            powerText.text = cardSO.power.ToString();
            healthText.text = cardSO.health.ToString();

            transform.position = new Vector3(-5f + idx, -5f, -idx);
        }
        else
        {
            frontFace.SetActive(false);
            backFace.SetActive(true);

            transform.position = new Vector3(-5f + idx, 5f, -idx);
        }
    }
}
