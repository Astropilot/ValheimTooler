using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ValheimTooler.Core.Extensions;
using ValheimTooler.UI;
using ValheimTooler.Utils;

namespace ValheimTooler.Core
{
    public static class ItemGiver
    {
        private static Rect s_itemGiverRect;
        private static Vector2 s_itemGiverScrollPosition;
        private static readonly List<InventoryItem> s_items = new List<InventoryItem>();
        private static List<InventoryItem> s_itemsFiltered = new List<InventoryItem>();
        private static List<GUIContent> s_itemsGUIFiltered = new List<GUIContent>();

        private static int s_selectedItem = 0;
        private static string s_quantityItem = "1";
        private static string s_qualityItem = "1";
        private static string s_searchTerms = "";
        private static string s_previousSearchTerms = "";

        public static void Start()
        {
            s_itemGiverRect = new Rect(ConfigManager.s_itemGiverWindowPosition.Value.x, ConfigManager.s_itemGiverWindowPosition.Value.y, 400, 400);

            LoadItems();
        }

        public static void LoadItems()
        {
            if (ObjectDB.instance == null || ObjectDB.instance.m_items == null)
            {
                return;
            }

            s_items.Clear();
            s_itemsFiltered.Clear();
            s_itemsGUIFiltered.Clear();

            foreach (GameObject gameObject in ObjectDB.instance.m_items)
            {
                if (gameObject == null)
                    continue;

                ItemDrop component = gameObject.GetComponent<ItemDrop>();

                if (component == null || component.m_itemData?.m_shared?.m_icons == null || component.m_itemData.m_shared.m_icons.Length == 0)
                    continue;

                for (var variant = 0; variant < component.m_itemData.m_shared.m_icons.Length; variant++)
                {
                    try
                    {
                        var iconSprite = component.m_itemData.m_shared.m_icons[variant];
                        if (iconSprite == null)
                            continue;

                        Texture texture = SpriteManager.TextureFromSprite(iconSprite);
                        string itemName = component.m_itemData.m_shared.m_name ?? "";
                        string localizedName = Localization.instance != null ? Localization.instance.Localize(itemName + (variant > 0 ? " Variant " + variant.ToString() : "")) : itemName;
                        var content = new GUIContent(texture, localizedName);
                        var inventoryItem = new InventoryItem(gameObject, component, content, variant);
                        s_itemsGUIFiltered.Add(content);
                        s_items.Add(inventoryItem);
                        s_itemsFiltered.Add(inventoryItem);
                    } catch
                    {
                        ZLog.Log($"[ValheimTooler - ItemGiver] Failed to load item {component.m_itemData?.m_shared?.m_name} with variant {variant}. This item will be ignored.");
                        continue;
                    }
                }
            }
        }

        public static void Update()
        {
            if (s_items.Count == 0 && ObjectDB.instance != null && ObjectDB.instance.m_items != null && ObjectDB.instance.m_items.Count > 0)
            {
                LoadItems();
            }
        }

        public static void DisplayGUI()
        {
            s_itemGiverRect = GUILayout.Window(1002, s_itemGiverRect, ItemGiverWindow, VTLocalization.instance.Localize("$vt_item_giver_title"));

            ConfigManager.s_itemGiverWindowPosition.Value = s_itemGiverRect.position;
        }

        public static void ItemGiverWindow(int windowID)
        {
            if (ObjectDB.instance == null || ObjectDB.instance.m_items.Count == 0)
                return;

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);

            labelStyle.normal.textColor = Color.white;

            GUILayout.Space(EntryPoint.s_boxSpacing);
            s_searchTerms = GUILayout.TextField(s_searchTerms);

            SearchItem(s_searchTerms);

            s_itemGiverScrollPosition = GUILayout.BeginScrollView(s_itemGiverScrollPosition, GUI.skin.box, GUILayout.Height(350));
            {
                s_selectedItem = GUILayout.SelectionGrid(s_selectedItem, s_itemsGUIFiltered.ToArray(), 4, InterfaceMaker.CustomSkin.GetStyle("flatButton"));
            }
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(VTLocalization.instance.Localize("$vt_item_giver_quantity :"), labelStyle, GUILayout.ExpandWidth(false));
                s_quantityItem = GUILayout.TextField(s_quantityItem, GUILayout.ExpandWidth(true));
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(VTLocalization.instance.Localize("$vt_item_giver_quality :"), labelStyle, GUILayout.ExpandWidth(false));
                s_qualityItem = GUILayout.TextField(s_qualityItem, GUILayout.ExpandWidth(true));
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button(VTLocalization.instance.Localize("$vt_item_giver_button")))
            {
                if (int.TryParse(s_quantityItem, out int quantity) && int.TryParse(s_qualityItem, out int quality))
                {
                    if (s_selectedItem >= 0 && s_selectedItem < s_itemsFiltered.Count && Player.m_localPlayer != null)
                    {
                        Player.m_localPlayer.VTAddItemToInventory(s_itemsFiltered[s_selectedItem].itemPrefab.name, quantity, quality, s_itemsFiltered[s_selectedItem].variant);
                    }
                }
            }

            if (GUI.tooltip != "")
            {
                GUIContent tooltip = new GUIContent(GUI.tooltip);
                Vector2 tooltip_size = new GUIStyle(GUI.skin.textField).CalcSize(tooltip);
                Vector2 tooltip_pos = new Vector2();

                if (Event.current.mousePosition.x + tooltip_size.x > s_itemGiverRect.width)
                    tooltip_pos.x = Event.current.mousePosition.x - ((Event.current.mousePosition.x + tooltip_size.x) - (s_itemGiverRect.width));
                else
                    tooltip_pos.x = Event.current.mousePosition.x;
                if (Event.current.mousePosition.y + 30 + tooltip_size.y > s_itemGiverRect.height)
                    tooltip_pos.y = Event.current.mousePosition.y + 30 - ((Event.current.mousePosition.y + 30 + tooltip_size.y) - (s_itemGiverRect.height));
                else
                    tooltip_pos.y = Event.current.mousePosition.y + 30;

                GUI.Box(new Rect(tooltip_pos.x, tooltip_pos.y, tooltip_size.x, tooltip_size.y), tooltip, GUI.skin.textField);
            }

            GUI.DragWindow();
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
                s_itemsFiltered = s_items;
                s_itemsGUIFiltered = s_itemsFiltered.Select(i => i.guiContent).ToList();
            }
            else
            {
                s_itemsFiltered = s_items.Where(i => {
                    string rawName = i.itemDrop?.m_itemData?.m_shared?.m_name ?? i.itemPrefab?.name ?? "";
                    string localized = Localization.instance != null ? Localization.instance.Localize(rawName + (i.variant > 0 ? " Variant " + i.variant.ToString() : "")) : rawName;
                    return localized.ToLower().Contains(search) || rawName.ToLower().Contains(search);
                }).ToList();
                s_itemsGUIFiltered = s_itemsFiltered.Select(i => i.guiContent).ToList();
            }
            s_previousSearchTerms = search;
        }

        class InventoryItem
        {
            public GameObject itemPrefab;
            public ItemDrop itemDrop;
            public GUIContent guiContent;
            public int variant;

            public InventoryItem(GameObject itemPrefab, ItemDrop itemDrop, GUIContent guiContent, int variant)
            {
                this.itemPrefab = itemPrefab;
                this.itemDrop = itemDrop;
                this.guiContent = guiContent;
                this.variant = variant;
            }
        }
    }
}
