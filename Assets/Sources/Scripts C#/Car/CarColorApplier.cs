using UnityEngine;

public class CarColorApplier : MonoBehaviour
{
    [SerializeField] private SpriteRenderer carSpriteRenderer;

    public void ApplyColor(Sprite sprite)
    {
        if (carSpriteRenderer != null)
        {
            carSpriteRenderer.sprite = sprite;
        }
    }
}