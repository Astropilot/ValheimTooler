using System;
using UnityEngine;
using ValheimTooler.UI;

namespace RapidGUI
{
    public static partial class RGUI
    {
        static int s_popupControlId;
        static readonly PopupWindow s_popupWindow = new PopupWindow();
        static readonly SearchablePopupWindow s_searchablePopupWindow = new SearchablePopupWindow();

        public static string SelectionPopup(string current, string[] displayOptions)
        {
            var idx = Array.IndexOf(displayOptions, current);
            GUILayout.Box(current, InterfaceMaker.CustomSkin.textField);
            var newIdx = PopupOnLastRect(idx, displayOptions);
            if ( newIdx != idx)
            {
                current = displayOptions[newIdx];
            }
            return current;
        }

        public static int SelectionPopup(int selectionIndex, string[] displayOptions)
        {
            var label = (selectionIndex < 0 || displayOptions.Length <= selectionIndex) ? "" : displayOptions[selectionIndex];
            GUILayout.Box(label, InterfaceMaker.CustomSkin.textField);
            return PopupOnLastRect(selectionIndex, displayOptions);
        }

        public static string SearchableSelectionPopup(string current, string[] displayOptions, ref string searchTerms)
        {
            var idx = Array.IndexOf(displayOptions, current);
            GUILayout.Box(current, InterfaceMaker.CustomSkin.textField);
            var newIdx = SearchablePopupOnLastRect(idx, displayOptions, ref searchTerms);
            if (newIdx != idx)
            {
                current = displayOptions[newIdx];
            }
            return current;
        }

        public static int SearchableSelectionPopup(int selectionIndex, string[] displayOptions, ref string searchTerms)
        {
            var label = (selectionIndex < 0 || displayOptions.Length <= selectionIndex) ? "" : displayOptions[selectionIndex];
            GUILayout.Box(label, InterfaceMaker.CustomSkin.textField);
            return SearchablePopupOnLastRect(selectionIndex, displayOptions, ref searchTerms);
        }


        public static int PopupOnLastRect(string[] displayOptions, string label = "") => PopupOnLastRect(-1, displayOptions, -1, label);
        public static int PopupOnLastRect(string[] displayOptions, int button, string label = "") => PopupOnLastRect(-1, displayOptions, button, label);
        public static int PopupOnLastRect(int selectionIndex, string[] displayOptions, int mouseButton=-1, string label = "") => Popup(GUILayoutUtility.GetLastRect(), mouseButton, selectionIndex, displayOptions, label);


        public static int SearchablePopupOnLastRect(string[] displayOptions, ref string searchTerms, string label = "") => SearchablePopupOnLastRect(-1, displayOptions, ref searchTerms, -1, label);
        public static int SearchablePopupOnLastRect(string[] displayOptions, int button, ref string searchTerms, string label = "") => SearchablePopupOnLastRect(-1, displayOptions, ref searchTerms, button, label);
        public static int SearchablePopupOnLastRect(int selectionIndex, string[] displayOptions, ref string searchTerms, int mouseButton = -1, string label = "") => SearchablePopup(GUILayoutUtility.GetLastRect(), mouseButton, selectionIndex, displayOptions, ref searchTerms, label);


        public static int Popup(Rect launchRect, int mouseButton, int selectionIndex, string[] displayOptions, string label = "")
        {
            var ret = selectionIndex;
            var controlId = GUIUtility.GetControlID(FocusType.Passive);

            // not Popup Owner
            if (s_popupControlId != controlId)
            {
                var ev = Event.current;
                var pos = ev.mousePosition;

                if ((ev.type == EventType.MouseUp)
                    && ((mouseButton < 0) || (ev.button == mouseButton))
                    && launchRect.Contains(pos)
                    && displayOptions != null 
                    && displayOptions.Length > 0
                    )
                {
                    s_popupWindow.pos = RGUIUtility.GetMouseScreenPos(Vector2.one * 150f);
                    s_popupControlId = controlId;
                    ev.Use();
                }
            }
            // Active
            else
            {
                var type = Event.current.type;
                
                var result = s_popupWindow.result;
                if (result.HasValue && type == EventType.Layout)
                {
                    if (result.Value >= 0) // -1 when the popup is closed by clicking outside the window
                    {
                        ret = result.Value;
                    }
                    s_popupWindow.result = null;
                    s_popupControlId = 0;
                }
                else
                {

                    if ((type == EventType.Layout) || (type == EventType.Repaint))
                    {
                        var buttonStyle = new GUIStyle(InterfaceMaker.CustomSkin.button)
                        {
                            padding = new RectOffset(24, 48, 2, 2)
                        };
                        var contentSize = Vector2.zero;
                        for (var i = 0; i < displayOptions.Length; ++i)
                        {
                            var textSize = buttonStyle.CalcSize(RGUIUtility.TempContent(displayOptions[i]));
                            contentSize.x = Mathf.Max(contentSize.x, textSize.x);
                            contentSize.y += textSize.y;
                        }

                        var margin = buttonStyle.margin;
                        contentSize.y += Mathf.Max(0, displayOptions.Length - 1) * Mathf.Max(margin.top, margin.bottom); // is this right?

                        var vbarSkin = InterfaceMaker.OldSkin.verticalScrollbar;
                        var vbarSize = vbarSkin.CalcScreenSize(Vector2.zero);
                        var vbarMargin = vbarSkin.margin;

                        var hbarSkin = InterfaceMaker.OldSkin.horizontalScrollbar;
                        var hbarSize = hbarSkin.CalcScreenSize(Vector2.zero);
                        var hbarMargin = hbarSkin.margin;

                        const float offset = 5f;
                        contentSize += new Vector2(vbarSize.x + vbarMargin.horizontal, hbarSize.y + hbarMargin.vertical) + Vector2.one * offset;
                        var size = InterfaceMaker.CustomSkin.GetStyle("popup").CalcScreenSize(contentSize);
                        var maxSize = new Vector2(Screen.width, Screen.height) - s_popupWindow.pos;

                        s_popupWindow.size = Vector2.Min(size, maxSize);
                    }

                    s_popupWindow.label = label;
                    s_popupWindow.displayOptions = displayOptions;
                    WindowInvoker.Add(s_popupWindow);
                }
            }

            return ret;
        }

        public static int SearchablePopup(Rect launchRect, int mouseButton, int selectionIndex, string[] displayOptions, ref string searchTerms, string label = "")
        {
            var ret = selectionIndex;
            var controlId = GUIUtility.GetControlID(FocusType.Passive);

            // not Popup Owner
            if (s_popupControlId != controlId)
            {
                var ev = Event.current;
                var pos = ev.mousePosition;

                if ((ev.type == EventType.MouseUp)
                    && ((mouseButton < 0) || (ev.button == mouseButton))
                    && launchRect.Contains(pos)
                    && displayOptions != null
                    && displayOptions.Length > 0
                    )
                {
                    s_searchablePopupWindow.pos = RGUIUtility.GetMouseScreenPos(Vector2.one * 150f);
                    s_popupControlId = controlId;
                    ev.Use();
                }
            }
            // Active
            else
            {
                var type = Event.current.type;

                var result = s_searchablePopupWindow.result;
                if (s_searchablePopupWindow._searchTerms != null)
                {
                    if (!searchTerms.Equals(s_searchablePopupWindow._searchTerms))
                    {
                        ret = -1;
                    }
                    searchTerms = s_searchablePopupWindow._searchTerms;
                }
                if (result.HasValue && type == EventType.Layout)
                {
                    if (result.Value >= 0) // -1 when the popup is closed by clicking outside the window
                    {
                        ret = result.Value;
                    }
                    s_searchablePopupWindow.result = null;
                    s_popupControlId = 0;
                }
                else
                {

                    if ((type == EventType.Layout) || (type == EventType.Repaint))
                    {
                        var buttonStyle = new GUIStyle(InterfaceMaker.CustomSkin.button)
                        {
                            padding = new RectOffset(24, 48, 2, 2)
                        };
                        var textfieldStyle = InterfaceMaker.CustomSkin.textField;
                        var contentSize = Vector2.zero;

                        for (var i = 0; i < displayOptions.Length; ++i)
                        {
                            var textSize = buttonStyle.CalcSize(RGUIUtility.TempContent(displayOptions[i]));
                            contentSize.x = Mathf.Max(contentSize.x, textSize.x);
                            contentSize.y += textSize.y;
                        }

                        if (displayOptions.Length == 0)
                        {
                            contentSize.x += 150;
                        }

                        var margin = buttonStyle.margin;
                        contentSize.y += Mathf.Max(0, displayOptions.Length - 1) * Mathf.Max(margin.top, margin.bottom); // is this right?

                        var vbarSkin = InterfaceMaker.CustomSkin.verticalScrollbar;
                        var vbarSize = vbarSkin.CalcScreenSize(Vector2.zero);
                        var vbarMargin = vbarSkin.margin;

                        var hbarSkin = InterfaceMaker.CustomSkin.horizontalScrollbar;
                        var hbarSize = hbarSkin.CalcScreenSize(Vector2.zero);
                        var hbarMargin = hbarSkin.margin;

                        const float offset = 5f;
                        contentSize += new Vector2(vbarSize.x + vbarMargin.horizontal, hbarSize.y + hbarMargin.vertical) + Vector2.one * offset;

                        contentSize.y += textfieldStyle.CalcHeight(RGUIUtility.TempContent("t"), contentSize.x);

                        var size = InterfaceMaker.CustomSkin.GetStyle("popup").CalcScreenSize(contentSize);
                        var maxSize = new Vector2(Screen.width, Screen.height) - s_searchablePopupWindow.pos;

                        s_searchablePopupWindow.size = Vector2.Min(size, maxSize);
                    }

                    s_searchablePopupWindow.SetSearchTerms(searchTerms);
                    s_searchablePopupWindow.label = label;
                    s_searchablePopupWindow.displayOptions = displayOptions;
                    WindowInvoker.Add(s_searchablePopupWindow);
                }
            }

            return ret;
        }


        class PopupWindow : IDoGUIWindow
        {
            public string label;
            public Vector2 pos;
            public Vector2 size;
            public int? result;
            public string[] displayOptions;
            public Vector2 scrollPosition;

            protected static readonly int s_popupWindowId = "Popup".GetHashCode();

            public Rect GetWindowRect() => new Rect(pos, size);

            public virtual void DoGUIWindow()
            {
                GUI.skin = InterfaceMaker.CustomSkin;

                GUI.ModalWindow(s_popupWindowId, GetWindowRect(), (id) =>
                {
                    using (var sc = new GUILayout.ScrollViewScope(scrollPosition))
                    {
                        scrollPosition = sc.scrollPosition;

                        for (var j = 0; j < displayOptions.Length; ++j)
                        {
                            if (GUILayout.Button(displayOptions[j], InterfaceMaker.CustomSkin.button))
                            {
                                result = j;
                            }
                        }
                    }

                    var ev = Event.current;
                    if ((ev.rawType == EventType.MouseDown) && !(new Rect(Vector2.zero, size).Contains(ev.mousePosition)))
                    {
                        result = -1; ;
                    }
                }
                , label, InterfaceMaker.CustomSkin.GetStyle("popup"));
            }

            public void CloseWindow() { result = -1; }
        }

        class SearchablePopupWindow : PopupWindow
        {
            public string _searchTerms;
            public string[] _displayOptionsCopy;

            public void SetSearchTerms(string searchTerms)
            {
                _searchTerms = searchTerms;
            }

            public override void DoGUIWindow()
            {
                GUI.skin = InterfaceMaker.CustomSkin;

                if ((Event.current.type == EventType.Layout))
                {
                    _displayOptionsCopy = (string[])displayOptions.Clone();
                }

                GUI.ModalWindow(s_popupWindowId, GetWindowRect(), (id) =>
                {
                    _searchTerms = GUILayout.TextField(_searchTerms);

                    if (_displayOptionsCopy.Length == 0)
                    {
                        GUILayout.Label("No results has been found!", GUILayout.Width(150));
                    }
                    else
                    {
                        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                        for (var j = 0; j < _displayOptionsCopy.Length; ++j)
                        {
                            if (GUILayout.Button(_displayOptionsCopy[j], InterfaceMaker.CustomSkin.button))
                            {
                                result = j;
                            }
                        }
                        GUILayout.EndScrollView();
                    }

                    var ev = Event.current;
                    if ((ev.rawType == EventType.MouseDown) && !(new Rect(Vector2.zero, size).Contains(ev.mousePosition)))
                    {
                        result = -1;
                        ;
                    }
                }
                , label, InterfaceMaker.CustomSkin.GetStyle("popup"));
            }
        }
    }
}
