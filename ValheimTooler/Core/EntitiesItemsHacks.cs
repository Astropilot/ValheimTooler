using System.Collections.Generic;
using System.Linq;
using RapidGUI;
using UnityEngine;
using ValheimTooler.Utils;

namespace ValheimTooler.Core
{
    public static class EntitiesItemsHacks
    {
        private static string s_entityQuantityText = "1";
        private static int s_entityLevelIdx = 0;

        private static int s_entityPrefabIdx = 0;
        private static string s_entitySearchTerms = "";
        private static string s_previousSearchTerms = "";


        private static readonly List<string> s_entityLevels = new List<string>();

        private static readonly List<string> s_entityPrefabs = new List<string>();
        private static List<string> s_entityPrefabsFiltered = new List<string>();

        public static void Start()
        {
            for (var i = 1; i <= 5; i++)
            {
                s_entityLevels.Add(i.ToString());
            }

            var prefabFile = ResourceUtils.GetEmbeddedResource("Resources.prefabs.txt", null);

            var prefabFileContents = System.Text.Encoding.UTF8.GetString(prefabFile, 0, prefabFile.Length);

            using (System.IO.StringReader reader = new System.IO.StringReader(prefabFileContents))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    s_entityPrefabs.Add(line);
                    s_entityPrefabsFiltered.Add(line);
                }
            }
        }

        public static void Update()
        {
            return;
        }

        public static void DisplayGUI()
        {
            GUILayout.BeginVertical(Translator.Localize("$vt_entities_spawn_title"), GUI.skin.box, GUILayout.ExpandWidth(false));
            {
                GUILayout.Space(EntryPoint.s_boxSpacing);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(Translator.Localize("$vt_entities_spawn_entity_name :"));
                    s_entityPrefabIdx = RGUI.SearchableSelectionPopup(s_entityPrefabIdx, s_entityPrefabsFiltered.ToArray(), ref s_entitySearchTerms);

                    SearchItem(s_entitySearchTerms);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(Translator.Localize("$vt_entities_spawn_quantity :"));
                    s_entityQuantityText = GUILayout.TextField(s_entityQuantityText, GUILayout.ExpandWidth(true));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(Translator.Localize("$vt_entities_spawn_level :"));
                    s_entityLevelIdx = RGUI.SelectionPopup(s_entityLevelIdx, s_entityLevels.ToArray());
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button(Translator.Localize("$vt_entities_spawn_button")))
                {
                    if (int.TryParse(s_entityQuantityText, out int entityQuantity) && int.TryParse(s_entityLevels[s_entityLevelIdx], out int entityLevel))
                    {
                        if (entityQuantity <= 100 && s_entityPrefabIdx < s_entityPrefabsFiltered.Count && s_entityPrefabIdx >= 0)
                        {
                            SpawnEntities(s_entityPrefabsFiltered[s_entityPrefabIdx], entityLevel, entityQuantity);
                        }
                    }
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(Translator.Localize("$vt_entities_drops_title"), GUI.skin.box, GUILayout.ExpandWidth(false));
            {
                GUILayout.Space(EntryPoint.s_boxSpacing);
                if (GUILayout.Button(Translator.Localize("$vt_entities_drops_button")))
                {
                    RemoveAllDrops();
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(Translator.Localize("$vt_entities_item_giver_title"), GUI.skin.box, GUILayout.ExpandWidth(false));
            {
                GUILayout.Space(EntryPoint.s_boxSpacing);
                if (GUILayout.Button(EntryPoint.s_showItemGiver ? Translator.Localize("$vt_entities_item_giver_button_hide") : Translator.Localize("$vt_entities_item_giver_button_show")))
                {
                    EntryPoint.s_showItemGiver = !EntryPoint.s_showItemGiver;
                }
            }
            GUILayout.EndVertical();
        }

        private static void SearchItem(string search)
        {
            search = search.ToLower();
            if (s_previousSearchTerms.Equals(search))
            {
                return;
            }
            if (search.Length == 0)
            {
                s_entityPrefabsFiltered = s_entityPrefabs;
            }
            else
            {
                s_entityPrefabsFiltered = s_entityPrefabs.Where(i => i.ToLower().Contains(search)).ToList();
            }
            s_previousSearchTerms = search;
        }

        private static void SpawnEntity(GameObject entityPrefab, int level)
        {
            if (entityPrefab != null && Player.m_localPlayer != null)
            {
                Vector3 b = UnityEngine.Random.insideUnitSphere * 0.5f;
                Character component2 = UnityEngine.Object.Instantiate<GameObject>(entityPrefab, Player.m_localPlayer.transform.position + Player.m_localPlayer.transform.forward * 2f + Vector3.up + b, Quaternion.identity).GetComponent<Character>();

                if (component2 != null)
                {
                    component2.SetLevel(level);
                }
            }
        }

        private static void SpawnEntities(string entityPrefab, int level, int quantity)
        {
            if (ZNetScene.instance == null)
            {
                return;
            }

            GameObject prefab = ZNetScene.instance.GetPrefab(entityPrefab);

            if (prefab == null)
            {
                return;
            } 

            for (var i = 0; i < quantity; i++)
            {
                SpawnEntity(prefab, level);
            }
        }

        private static void RemoveAllDrops()
        {
            ItemDrop[] itemDrops = UnityEngine.Object.FindObjectsOfType<ItemDrop>();
            foreach (ItemDrop itemDrop in itemDrops)
            {
                ZNetView component = itemDrop.GetComponent<ZNetView>();
                if (component)
                {
                    component.Destroy();
                }
            }
        }
    }
}
