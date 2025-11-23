using UnityEngine;
using UnityEngine.EventSystems;

public class MergeableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Merge Data")]
    public ItemData data;              // MergeManager buraya erişiyor
    public Tile currentTile;           // Hangi tile üzerinde
    private Camera cam;

    private Vector3 offset;            // Mouse ile obje arasındaki fark
    private Plane dragPlane;           // Yüzey düzlemi
    private float fixedY = 0.5f;       // Objelerin sabit yüksekliği

    void Awake()
    {
        cam = Camera.main;
    }

    void Start()
    {
        // Eğer tile atanmadıysa parent'tan bul
        if (currentTile == null)
        {
            currentTile = GetComponentInParent<Tile>();
            if (currentTile != null)
                currentTile.SetItem(this);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentTile != null)
            currentTile.ClearItem();

        dragPlane = new Plane(Vector3.up, new Vector3(0, fixedY, 0));

        Ray ray = cam.ScreenPointToRay(eventData.position);
        if (dragPlane.Raycast(ray, out float distance))
        {
            offset = transform.position - ray.GetPoint(distance);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Ray ray = cam.ScreenPointToRay(eventData.position);
        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 targetPos = ray.GetPoint(distance) + offset;
            targetPos.y = fixedY; // sabit yükseklik
            transform.position = Vector3.Lerp(transform.position, targetPos, 25f * Time.deltaTime);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        MergeManager.Instance.HandleDrop(this, transform.position);
    }
}
