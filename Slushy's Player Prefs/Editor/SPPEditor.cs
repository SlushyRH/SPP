using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using System.Security.Cryptography;
using Unity.Plastic.Newtonsoft.Json.Linq;

namespace SRH
{
    public class SPPEditor : EditorWindow
    {
        [System.Serializable]
        public class PrefStruct
        {
            public string Key;
            public string OgKey;
            public string Value;
            public bool Encrypted;

            public PrefStruct(string key, string ogKey, string value, bool encrypted)
            {
                this.Key = key;
                this.OgKey = ogKey;
                this.Value = value;
                this.Encrypted = encrypted;
            }
        }

        private enum ListOrder
        {
            NONE = 0,
            ACSENDING = 1,
            DESCENDING =2
        }

        private enum SearchType
        {
            KEY = 0,
            VALUE = 1
        }

        private ListOrder listOrder;
        private SearchType searchType;
        private ReorderableList userPrefs;
        private List<PrefStruct> userPrefsList;
        private List<PrefStruct> defaultUserPrefsList;
        Vector2 scrollPos;
        string search;

        [MenuItem("SRH/SPP/Player Prefs Viewer", false, 1)]
        private static void Init()
        {
            // setup window
            SPPEditor editor = GetWindow(typeof(SPPEditor), false, "Slushy's Player Prefs Viewer") as SPPEditor;
            editor.minSize = new Vector2(400, 400);
            editor.Show();
        }

        private void OnEnable()
        {
            // enable the needed stuff
            listOrder = (ListOrder)EditorPrefs.GetInt("SlushysPrefViewerListOrder");
            InitList();
        }

        private void InitList(string searchContent = null)
        {
            defaultUserPrefsList = SPPEditorUtility.GetAllPrefs();

            // filter search if not null
            if (!String.IsNullOrEmpty(searchContent))
            {
                if (searchType == SearchType.KEY)
                    defaultUserPrefsList = defaultUserPrefsList.Where(k => k.Key.ToUpper().Contains(searchContent.ToUpper())).ToList();
                else if (searchType == SearchType.VALUE)
                    defaultUserPrefsList = defaultUserPrefsList.Where(k => k.Value.ToUpper().Contains(searchContent.ToUpper())).ToList();
            }

            // sort list by the order selected
            switch (listOrder)
            {
                case ListOrder.NONE:
                    userPrefsList = defaultUserPrefsList;
                    break;
                case ListOrder.ACSENDING:
                    userPrefsList = defaultUserPrefsList.OrderBy(x => x.Key).ToList();
                    break;
                case ListOrder.DESCENDING:
                    userPrefsList = defaultUserPrefsList.OrderBy(x => x.Key).ToList();
                    userPrefsList.Reverse();
                    break;
                default:
                    userPrefsList = defaultUserPrefsList;
                    break;
            }

            // init reordable list and draw the elements
            userPrefs = new ReorderableList(userPrefsList, userPrefsList.GetType(), false, true, false, false);
            userPrefs.drawHeaderCallback = DrawListHeader;
            userPrefs.drawElementCallback = DrawListElement;
        }

        private void DrawListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Player Prefs");
        }

        private void DrawListElement(Rect rect, int i, bool isActive, bool isFocused)
        {
            PrefStruct current = userPrefsList[i];

            // get rect for the following
            Rect labelRect = new Rect(10, rect.y, 100, EditorGUIUtility.singleLineHeight);
            Rect encryptRect = new Rect(75, rect.y, 100, EditorGUIUtility.singleLineHeight);
            Rect keyRect = new Rect(100, rect.y, 180, EditorGUIUtility.singleLineHeight);
            Rect valueRect = new Rect(290, rect.y, Screen.width - 330, EditorGUIUtility.singleLineHeight);
            Rect editRect = new Rect(Screen.width - 30, rect.y, 20, EditorGUIUtility.singleLineHeight);

            // draw the following
            GUI.enabled = false;
            EditorGUI.LabelField(labelRect, "Encrypted");
            current.Encrypted = GUI.Toggle(encryptRect, current.Encrypted, "");
            current.Key = EditorGUI.TextField(keyRect, current.Key);
            current.Value = EditorGUI.TextField(valueRect, current.Value);
            GUI.enabled = true;

            // delete button
            if (GUI.Button(editRect, "X"))
            {
                // confim deletion of key
                if (EditorUtility.DisplayDialog("Confirm Deletion", $"Are you sure you want to delete this player pref: {current.Key}?", "Yes", "No"))
                {
                    PlayerPrefs.DeleteKey(current.Key);
                    PlayerPrefs.Save();

                    RefreshList();
                }
            }
        }

        private void OnGUI()
        {
            try
            {
                // draw all the ui shit and stuff
                GUILayout.BeginHorizontal();

                GUILayout.BeginHorizontal(); // search bar
                GUILayout.Label("Search: ");
                EditorGUI.BeginChangeCheck();
                searchType = (SearchType)EditorGUILayout.EnumPopup(searchType, GUILayout.Width(60));
                search = GUILayout.TextField(search, GUILayout.Width(180));
                if (EditorGUI.EndChangeCheck())
                {
                    InitList(search);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();

                // list order
                string sortOrderLabel = "Sort Order";
                if (listOrder == ListOrder.ACSENDING)
                    sortOrderLabel = "Ascending";
                else if (listOrder == ListOrder.DESCENDING)
                    sortOrderLabel = "Descending";

                // sort order
                if (GUILayout.Button(sortOrderLabel))
                {
                    // increment order, save it then refresh
                    listOrder++;
                    if ((int)listOrder >= Enum.GetValues(typeof(ListOrder)).Length)
                        listOrder = 0;

                    EditorPrefs.SetInt("SlushysPrefViewerListOrder", (int)listOrder);
                    RefreshList();
                }

                // refresh
                if (GUILayout.Button("Refresh"))
                {
                    RefreshList();
                }

                // delete all
                if (GUILayout.Button("Delete All"))
                {
                    if (EditorUtility.DisplayDialog("Confirm Deletion", $"Are you sure you want to delete all player prefs? You cannot undo this action.", "Yes", "No"))
                    {
                        PlayerPrefs.DeleteAll();
                        PlayerPrefs.Save();

                        RefreshList();
                    }
                }

                GUILayout.EndHorizontal();
                scrollPos = GUILayout.BeginScrollView(scrollPos);
                userPrefs.DoLayoutList(); // Have the ReorderableList do its work
                GUILayout.EndScrollView();
            }
            catch { }
        }

        internal void RefreshList()
        {
            // refresh list
            search = null;
            InitList();
        }
    }
}