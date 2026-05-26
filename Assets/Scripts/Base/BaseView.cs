using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BaseView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite baseSprite;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        ApplySprite();
    }

    public void SetSprite(Sprite sprite)
    {
        baseSprite = sprite;
        ApplySprite();
    }

    private void ApplySprite()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.sprite = baseSprite;
    }
}