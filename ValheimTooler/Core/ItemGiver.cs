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
            s_itemGiverRect = new Rect(Screen.width - 400, 5, 400, 400);

            if (ObjectDB.instance == null)
            {
                return;
            }

            foreach (GameObject gameObject in ObjectDB.instance.m_items)
            {
                ItemDrop component = gameObject.GetComponent<ItemDrop>();

                if (component.m_itemData?.m_shared?.m_icons?.Length <= 0)
                    continue;

                for (var variant = 0; variant < component.m_itemData.m_shared.m_icons.Length; variant++)
                {
                    Texture texture = SpriteManager.TextureFromSprite(component.m_itemData.m_shared.m_icons[variant]);
                    var content = new GUIContent(texture, Localization.instance.Localize(component.m_itemData.m_shared.m_name + (variant > 0 ? " Variant " + variant.ToString() : "")));
                    var inventoryItem = new InventoryItem(gameObject, component, content, variant);
                    s_itemsGUIFiltered.Add(content);
                    s_items.Add(inventoryItem);
                    s_itemsFiltered.Add(inventoryItem);
                }
            }
        }

        public static void Update()
        {
            return;
        }

        public static void DisplayGUI()
        {
            s_itemGiverRect = GUILayout.Window(1002, s_itemGiverRect, ItemGiverWindow, Translator.Localize("$vt_item_giver_title"));
        }

        public static void ItemGiverWindow(int windowID)
        {
            if (ObjectDB.instance == null || ObjectDB.instance.m_items.Count == 0)
                return;

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
                GUILayout.Label(Translator.Localize("$vt_item_giver_quantity :"));
                s_quantityItem = GUILayout.TextField(s_quantityItem, GUILayout.ExpandWidth(true));
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(Translator.Localize("$vt_item_giver_quality :"));
                s_qualityItem = GUILayout.TextField(s_qualityItem, GUILayout.ExpandWidth(true));
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button(Translator.Localize("$vt_item_giver_button")))
            {
                if (int.TryParse(s_quantityItem, out int quantity) && int.TryParse(s_qualityItem, out int quality))
                {
                    Player.m_localPlayer.VTAddItemToInventory(s_itemsFiltered[s_selectedItem].itemPrefab.name, quantity, quality, s_itemsFiltered[s_selectedItem].variant);
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
                s_itemsFiltered = s_items.Where(i => Localization.instance.Localize(i.itemDrop.m_itemData.m_shared.m_name + (i.variant > 0 ? " Variant " + i.variant.ToString() : "")).ToLower().Contains(search)).ToList();
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
