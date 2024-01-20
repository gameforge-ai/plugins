using UnityEngine;

[RequireComponent (typeof(SpriteRenderer))]
public class MapSpriteRenderer : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    
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
}
