using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ValheimTooler.Utils
{
    public static class ResourceUtils
    {
        private static Dictionary<string, GameObject> s_cachedPrefabs = new Dictionary<string, GameObject>();

        public static byte[] ReadAllBytes(this Stream input)
        {
            byte[] array = new byte[16384];
            byte[] result;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                int count;
                while ((count = input.Read(array, 0, array.Length)) > 0)
                {
                    memoryStream.Write(array, 0, count);
                }
                result = memoryStream.ToArray();
            }
            return result;
        }

        public static byte[] GetEmbeddedResource(string resourceFileName, Assembly containingAssembly = null)
        {
            if (containingAssembly == null)
            {
                containingAssembly = Assembly.GetCallingAssembly();
            }
            
            string name = Enumerable.Single<string>(containingAssembly.GetManifestResourceNames(), (string str) => str.EndsWith(resourceFileName));
            byte[] result;
            using (Stream manifestResourceStream = containingAssembly.GetManifestResourceStream(name))
            {
                byte[] array = manifestResourceStream.ReadAllBytes();
                if (array.Length == 0)
                {
                    Debug.LogWarning(string.Format("The resource %1 was not found", resourceFileName));
                }
                result = array;
            }
            return result;
        }

        public static Texture2D LoadTexture(byte[] texData)
        {
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            MethodInfo method = typeof(Texture2D).GetMethod("LoadImage", new Type[]
            {
                typeof(byte[])
            });
            if (method != null)
            {
                method.Invoke(texture2D, new object[]
                {
                    texData
                });
            }
            else
            {
                Type type = Type.GetType("UnityEngine.ImageConversion, UnityEngine.ImageConversionModule");
                if (type == null)
                {
                    throw new ArgumentNullException("converter");
                }
                MethodInfo method2 = type.GetMethod("LoadImage", new Type[]
                {
                    typeof(Texture2D),
                    typeof(byte[])
                });
                if (method2 == null)
                {
                    throw new ArgumentNullException("converterMethod");
                }
                method2.Invoke(null, new object[]
                {
                    texture2D,
                    texData
                });
            }
            return texture2D;
        }

        public static GameObject GetHiddenPrefab(string name)
        {
            if (s_cachedPrefabs.TryGetValue(name.ToLower(), out GameObject ret) && ret != null)
                return ret;

            var objects = Resources.FindObjectsOfTypeAll<Transform>()
                .Where(t => t.parent == null)
                .Select(x => x.gameObject);

            foreach (var prefab in objects)
            {
                if (prefab.name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    if (s_cachedPrefabs.ContainsKey(name.ToLower()))
                        s_cachedPrefabs.Remove(name.ToLower());

                    s_cachedPrefabs.Add(name.ToLower(), prefab);
                    return prefab;
                }
            }

            return null;
        }
    }
}
