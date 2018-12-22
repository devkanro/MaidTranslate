using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using Kanro.MaidTranslate.Hook;
using Kanro.MaidTranslate.Translation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Logger = BepInEx.Logger;

namespace Kanro.MaidTranslate
{
    [BepInPlugin(GUID: "MaidTranslate", Name: "Kanro.MaidTranslate", Version: "0.2")]
    public class MaidTranslate : BaseUnityPlugin
    {
        public TranslateConfig Config { get; }

        public TranslationResource Resource { get; }

        public ResourceDumper Dumper { get; }

        private Scene? CurrentScene { get; set; }

        private Dictionary<string, string> OriginalTextCache { get; } = new Dictionary<string, string>();

        public MaidTranslate()
        {
            try
            {
                Config = new TranslateConfig(this);
                Dumper = new ResourceDumper(Config);
                Resource = new TranslationResource();

                HookCenter.TextTranslation += OnTextTranslation;
                HookCenter.ArcTextureTranslation += OnArcTextureTranslation;
                HookCenter.ArcTextureLoaded += OnArcTextureLoaded;
                HookCenter.SpriteTextureTranslation += OnSpriteTextureTranslation;
                HookCenter.UITextureTranslation += OnUITextureTranslation;
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex);
                throw;
            }
        }

        public void Start()
        {
            Logger.Log(LogLevel.Debug, $"[MaidTranslate] Plugin initialized.");
        }

        public void Update()
        {
        }

        public void OnGUI()
        {
            try
            {
                if (Event.current.type != EventType.KeyUp) return;
                if (!Event.current.alt) return;

                switch (Event.current.keyCode)
                {
                    case KeyCode.F1:
                        Config.Reload();
                        Resource.Reload();
                        ReTranslation();
                        Logger.Log(LogLevel.Info, $"[MaidTranslate] Config reloaded.");
                        break;

                    case KeyCode.F5:
                        Config.IsDumpingText = !Config.IsDumpingText;
                        Logger.Log(LogLevel.Info, $"[MaidTranslate] Dumping text {(Config.IsDumpingText ? "Enabled" : "Disabled")}.");
                        break;
                    case KeyCode.F6:
                        Config.IsDumpingTexture = !Config.IsDumpingTexture;
                        Logger.Log(LogLevel.Info, $"[MaidTranslate] Dumping texture {(Config.IsDumpingTexture ? "Enabled" : "Disabled")}.");
                        break;
                    case KeyCode.F7:
                        Config.IsDumpingUI = !Config.IsDumpingUI;
                        Logger.Log(LogLevel.Info, $"[MaidTranslate] Dumping ui {(Config.IsDumpingUI ? "Enabled" : "Disabled")}.");
                        break;
                    case KeyCode.F8:
                        Config.IsDumpingSprite = !Config.IsDumpingSprite;
                        Logger.Log(LogLevel.Info, $"[MaidTranslate] Dumping sprite {(Config.IsDumpingSprite ? "Enabled" : "Disabled")}.");
                        break;

                    case KeyCode.F9:
                        Dumper.DumpResource(CurrentScene);
                        break;
                    case KeyCode.F10:
                        Dumper.DumpObjects(CurrentScene);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex);
                throw;
            }
        }

        public void OnDestroy()
        {
            try
            {
                HookCenter.TextTranslation -= OnTextTranslation;
                HookCenter.ArcTextureTranslation -= OnArcTextureTranslation;
                HookCenter.ArcTextureLoaded -= OnArcTextureLoaded;
                HookCenter.SpriteTextureTranslation -= OnSpriteTextureTranslation;
                HookCenter.UITextureTranslation -= OnUITextureTranslation;
                SceneManager.sceneLoaded -= OnSceneLoaded;

                Dumper.Dispose();
                Logger.Log(LogLevel.Debug, $"[MaidTranslate] Plugin destroyed.");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex);
                throw;
            }
        }

        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            try
            {
                CurrentScene = arg0;
                OriginalTextCache.Clear();
                ReTranslation();
                Logger.Log(LogLevel.Debug, $"[MaidTranslate] Scene '{arg0.name}({arg0.buildIndex})' loaded in {arg1} mode.");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex);
                throw;
            }
        }

        private void OnTextTranslation(object sender, TextTranslationEventArgs e)
        {
            try
            {
                if (OriginalTextCache.ContainsKey(e.Text))
                {
                    if (Config.IsDumpingText)
                    {
                        e.Text = OriginalTextCache[e.Text];
                    }
                    else
                    {
                        return;
                    }
                }

                if (Resource.TranslateText(CurrentScene, sender, e, out var result))
                {
                    e.Translation = result;
                    OriginalTextCache[e.Translation] = e.Text;
                }
                else
                {
                    e.Translation = result;
                    Dumper.DumpText(CurrentScene, sender, e);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex);
            }
        }

        private void OnArcTextureTranslation(object sender, TextureTranslationEventArgs e)
        {
            if (Resource.TranslateTexture(CurrentScene, e, out var resource))
            {
                e.Translation = resource;
                Logger.Log(LogLevel.Debug, $"[MaidTranslate] Texture '{e.Name}' translated.");
            }
        }

        private void OnArcTextureLoaded(object sender, ArcTextureLoadedEventArgs e)
        {
            try
            {
                if (Dumper.DumpTexture(CurrentScene, e))
                {
                    Logger.Log(LogLevel.Debug, $"[MaidTranslate] Texture '{e.Name}' dumped.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex);
            }
        }

        private void OnUITextureTranslation(object sender, TextureTranslationEventArgs e)
        {
            if (Resource.TranslateUI(CurrentScene, e, out var resource))
            {
                e.Translation = resource;
                Logger.Log(LogLevel.Debug, $"[MaidTranslate] UI '{e.Name}' translated.");
            }
            else
            {
                try
                {
                    if (Dumper.DumpUI(CurrentScene, e))
                    {
                        Logger.Log(LogLevel.Debug, $"[MaidTranslate] UI '{e.Name}' dumped.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, ex);
                }
            }
        }

        private void OnSpriteTextureTranslation(object sender, TextureTranslationEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Name))
            {
                Logger.Log(LogLevel.Debug, $"[MaidTranslate] Sprite({(sender as Image)?.name}) skipped due to name is empty.");
                return;
            }

            if (Resource.TranslateSprite(CurrentScene, e, out var resource))
            {
                e.Translation = resource;
                Logger.Log(LogLevel.Debug, $"[MaidTranslate] Sprite '{e.Name}' translated.");
            }
            else
            {
                try
                {
                    if (Dumper.DumpSprite(CurrentScene, e))
                    {
                        Logger.Log(LogLevel.Debug, $"[MaidTranslate] Sprite '{e.Name}' dumped.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, ex);
                }
            }
        }

        private void OnResourceTranslation()
        {
            var textures = Resources.FindObjectsOfTypeAll(typeof(Texture2D));

            foreach (var texture in textures)
            {
                try
                {
                    if (Resource.TranslateResource(CurrentScene, (Texture2D)texture))
                    {
                        Logger.Log(LogLevel.Debug, $"[MaidTranslate] Resource '{texture.name}' translated.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, ex);
                }
            }

            Resources.UnloadUnusedAssets();
        }

        private void ReTranslation()
        {
            var processedTextures = new HashSet<string>();
            foreach (var widget in FindObjectsOfType<UIWidget>())
            {
                if (widget is UILabel label)
                {
                    label.ProcessText();
                }
                else
                {
                    var texName = widget.mainTexture?.name;
                    if (texName == null) continue;

                    if (texName.StartsWith("!"))
                    {
                        widget.mainTexture.name = texName.Substring(1);
                    }
                    HookCenter.UIWidget_GetMainTexture(widget);
                }
            }
            foreach (var graphic in FindObjectsOfType<MaskableGraphic>())
            {
                if (graphic is Image img && img.sprite != null)
                {
                    if (img.sprite.name.StartsWith("!"))
                    {
                        img.sprite.name = img.sprite.name.Substring(1);
                    }
                }
                HookCenter.MaskableGraphic_OnEnable(graphic);
            }
            OnResourceTranslation();
        }
    }
}