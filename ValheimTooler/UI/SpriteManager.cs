using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ValheimTooler.UI
{
    public static class SpriteManager
    {
        private static readonly Dictionary<string, Texture2D> s_atlasCache;

        static SpriteManager()
        {
            s_atlasCache = new Dictionary<string, Texture2D>();
        }

        public static Texture2D TextureFromSprite(Sprite sprite, bool resize = true)
        {
            // Check if sprite.rect and sprite.textureRect are out of texture bounds
            Rect rect = sprite.textureRect;
            if (rect.x < 0 || rect.y < 0 || rect.x + rect.width > sprite.texture.width || rect.y + rect.height > sprite.texture.height)
            {
                Debug.LogError($"Sprite {sprite.name}'s textureRect is out of texture bounds!");
                return null;
            }

            // Return the original texture if the sprite covers the entire texture
            if (sprite.rect.width == sprite.texture.width && sprite.rect.height == sprite.texture.height)
            {
                return sprite.texture;
            }

            // Retrieve texture from cache to avoid redundant creation
            Texture2D spriteTexture;
            if (s_atlasCache.ContainsKey(sprite.texture.name))
            {
                spriteTexture = s_atlasCache[sprite.texture.name];
            }
            else
            {
                spriteTexture = DuplicateTexture(sprite.texture);
                s_atlasCache.Add(sprite.texture.name, spriteTexture);
            }

            // Create a new texture and copy pixels
            int rectWidth = Mathf.CeilToInt(sprite.textureRect.width);
            int rectHeight = Mathf.CeilToInt(sprite.textureRect.height);

            Texture2D newText = new Texture2D(rectWidth, rectHeight, TextureFormat.RGBA32, false);

            try
            {
                // Get pixel data from the specified area
                Color[] newColors = spriteTexture.GetPixels(
                    Mathf.CeilToInt(sprite.textureRect.x),
                    Mathf.CeilToInt(sprite.textureRect.y),
                    rectWidth,
                    rectHeight
                );

                // Verify that the pixel data size matches the target texture dimensions
                if (newColors.Length != rectWidth * rectHeight)
                {
                    Debug.LogError($"Sprite {sprite.name}'s pixel data size does not match! Expected {rectWidth * rectHeight}, got {newColors.Length}");
                    return null;
                }

                // Set pixels and apply changes
                newText.SetPixels(newColors);
                newText.Apply();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error processing Sprite {sprite.name}: {ex.Message}");
                return null;
            }

            // Resize the texture to 60x60 if needed
            if (resize && (newText.width > 200 || newText.height > 200))
            {
                Bilinear(newText, 60, 60);
            }

            return newText;
        }

        public static Texture2D Bilinear(Texture2D source, int newWidth, int newHeight)
        {
            // Create a new target texture
            Texture2D resizedTex = new Texture2D(newWidth, newHeight, source.format, false);

            // Perform bilinear interpolation for scaling
            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    // Calculate coordinates in the source texture
                    float u = x / (float)newWidth;
                    float v = y / (float)newHeight;

                    // Use GetPixelBilinear to get the color
                    Color color = source.GetPixelBilinear(u, v);
                    resizedTex.SetPixel(x, y, color);
                }
            }

            // Apply changes
            resizedTex.Apply();

            return resizedTex;
        }


        public static Texture2D DuplicateTexture(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;

            Texture2D readableText = new Texture2D(source.width, source.height);

            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);

            return readableText;
        }
    }
}
