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
            if (sprite.rect.width == sprite.texture.width)
            {
                return sprite.texture;
            }

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

            Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            Color[] newColors = spriteTexture.GetPixels(Mathf.CeilToInt(sprite.textureRect.x),
                                                        Mathf.CeilToInt(sprite.textureRect.y),
                                                        Mathf.CeilToInt(sprite.textureRect.width),
                                                        Mathf.CeilToInt(sprite.textureRect.height));
            newText.SetPixels(newColors);
            newText.Apply();

            if (resize && (newText.width > 200 || newText.height > 200))
                newText.Resize(60, 60);

            return newText;
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
