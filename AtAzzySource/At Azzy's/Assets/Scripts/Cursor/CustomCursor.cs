using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    public Sprite cursorSprite;
    public Vector2 hotspot = Vector2.zero;
    public CursorMode cursorMode = CursorMode.Auto;
    public int cursorScale = 3;

    void Start()
    {
        Texture2D cursorTexture = SpriteToTexture2D(cursorSprite, cursorScale);
        Cursor.SetCursor(cursorTexture, hotspot, cursorMode);
    }

    Texture2D SpriteToTexture2D(Sprite sprite, int scale)
    {
        int width = (int)sprite.rect.width * scale;
        int height = (int)sprite.rect.height * scale;

        // Create texture with RGBA32 format (Unity cursor requires RGBA32)
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        // Get original pixels
        Color[] pixels = sprite.texture.GetPixels(
            (int)sprite.textureRect.x,
            (int)sprite.textureRect.y,
            (int)sprite.textureRect.width,
            (int)sprite.textureRect.height
        );

        // Scale pixels manually
        Color[] scaledPixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                scaledPixels[y * width + x] =
                    pixels[(y / scale) * (int)sprite.rect.width + (x / scale)];
            }
        }

        texture.SetPixels(scaledPixels);
        texture.Apply();
        return texture;
    }
}