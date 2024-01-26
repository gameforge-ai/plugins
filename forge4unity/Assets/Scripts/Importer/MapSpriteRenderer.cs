using UnityEngine;

[RequireComponent (typeof(SpriteRenderer))]
public class MapSpriteRenderer : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private Vector3 worldPosition;

    public string mapId;
    public void Initialize(string mapId)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        this.mapId = mapId;
    }

    public void PaintSprite(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }

    public void Update()
    {
        worldPosition = transform.position;
    }
}
