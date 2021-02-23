using System;
using System.Collections.Generic;
using UnityEngine;

namespace ValheimTooler.UI
{
    public static class SpriteManager
    {
        private static Dictionary<string, Texture2D> _atlasCache;

        static SpriteManager()
        {
            _atlasCache = new Dictionary<string, Texture2D>();
        }

        public static Texture2D TextureFromSprite(Sprite sprite)
        {
            if (sprite.rect.width == sprite.texture.width)
            {
                return sprite.texture;
            }

            Texture2D spriteTexture = null;

            if (_atlasCache.ContainsKey(sprite.texture.name))
            {
                spriteTexture = _atlasCache[sprite.texture.name];
            } else
            {
                spriteTexture = DuplicateTexture(sprite.texture);
                _atlasCache.Add(sprite.texture.name, spriteTexture);
            }

            Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            Color[] newColors = spriteTexture.GetPixels((int)Math.Ceiling(sprite.textureRect.x),
                                                        (int)Math.Ceiling(sprite.textureRect.y),
                                                        (int)Math.Ceiling(sprite.textureRect.width),
                                                        (int)Math.Ceiling(sprite.textureRect.height));
            newText.SetPixels(newColors);
            newText.Apply();
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
