using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using Kanro.MaidTranslate.Hook;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Logger = BepInEx.Logger;

namespace Kanro.MaidTranslate
{
    [BepInPlugin(GUID: "MaidTranslate", Name: "Kanro.ObjectControl", Version: "0.3")]
    public class ObjectControl : BaseUnityPlugin
    {
        private bool showingUI = false;
        private Rect UI = new Rect(120, 600, 500, 400);
        private Vector2 scrollPosition = Vector2.zero;
        private Vector2 scrollPosition2 = Vector2.zero;

        protected void OnGUI()
        {
            if (showingUI)
            {
                UI = GUILayout.Window(589, UI, WindowFunction, $"Scene: {CurrentScene.name}");
            }
        }

        protected void Update()
        {
            try
            {
                if (Event.current == null) return;
                if (Event.current.type != EventType.KeyUp) return;
                if (!Event.current.alt) return;

                switch (Event.current.keyCode)
                {
                    case KeyCode.F3:
                        showingUI = !showingUI;
                        Logger.Log(LogLevel.Info, $"[ObjectControl] Object control ui {(showingUI ? "Enabled" : "Disabled")}.");
                        break;
                    case KeyCode.F11:
                        SelectedObject = null;
                        var objectPath = GUIUtility.systemCopyBuffer;
                        if (string.IsNullOrEmpty(objectPath))
                        {
                            return;
                        }

                        SelectedObject = GameObject.Find(objectPath);
                        Logger.Log(LogLevel.Info,
                            SelectedObject == null
                                ? "[ObjectControl] No object selected."
                                : $"[ObjectControl] {SelectedObject.name} selected.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex);
                throw;
            }
        }

        private Scene CurrentScene { get; set; }

        public void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            SelectedObject = null;
            CurrentScene = arg0;
        }

        private void WindowFunction(int windowID)
        {
            try
            {
                var next = SelectedObject;
                GUILayout.BeginHorizontal();
                {
                    if (SelectedObject != null)
                    {
                        GUILayout.Label($"{SelectedObject.name}({SelectedObject.GetType().Name}");
                        if (GUILayout.Button("Clear"))
                        {
                            next = null;
                        }
                        if (GUILayout.Button(SelectedObject.activeSelf ? "Disable" : "Enable"))
                        {
                            SelectedObject.SetActive(!SelectedObject.activeSelf);
                        }
                    }
                    else
                    {
                        GUILayout.Label("No select");
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Label("Children");
                GUILayout.BeginVertical(GUI.skin.box);
                {
                    scrollPosition2 = GUILayout.BeginScrollView(scrollPosition2, false, true);
                    {
                        GUILayout.BeginVertical();
                        {
                            if (SelectedObject != null)
                            {
                                foreach (Transform t in SelectedObject.transform)
                                {
                                    if (ObjectControlButtons(t.gameObject))
                                    {
                                        next = t.gameObject;
                                    }
                                }
                            }
                            else
                            {
                                foreach (var obj in CurrentScene.GetRootGameObjects())
                                {
                                    if (ObjectControlButtons(obj))
                                    {
                                        next = obj;
                                    }
                                }
                            }
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndScrollView();
                }
                GUILayout.EndVertical();

                GUILayout.Label("Parent");
                GUILayout.BeginVertical(GUI.skin.box);
                {
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
                    {
                        GUILayout.BeginVertical();
                        {
                            var current = SelectedObject;

                            while (current?.transform?.parent != null)
                            {
                                if (ObjectControlButtons(current.transform.parent.gameObject))
                                {
                                    next = current.transform.parent.gameObject;
                                }
                                current = current.transform.parent.gameObject;
                            }
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndScrollView();
                }
                GUILayout.EndVertical();

                GUI.DragWindow();

                if (SelectedObject != next)
                {
                    SelectedObject = next;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex);
                throw;
            }
        }

        private bool ObjectControlButtons(GameObject obj)
        {
            bool result = false;
            GUILayout.BeginHorizontal();
            {
                if (obj == SelectedObject)
                {
                    GUILayout.Label(obj.name);
                    if (GUILayout.Button($"{obj.name}({obj.GetType().Name})"))
                    {
                        result = true;
                    }
                }
                else
                {
                    if (GUILayout.Button($"{obj.name}({obj.GetType().Name})"))
                    {
                        result = true;
                    }
                }

                if (GUILayout.Button(obj.activeSelf ? "Disable" : "Enable"))
                {
                    obj.SetActive(!obj.activeSelf);
                }
            }
            GUILayout.EndHorizontal();
            return result;
        }

        private GameObject SelectedObject { get; set; }
    }
}
