using System;
using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.UI;
using Logger = BepInEx.Logger;

namespace Kanro.MaidTranslate.Hook
{
    public static class HookCenter
    {
        private static readonly byte[] EmptyTextureData = new byte[0];

        public static void Text_SetText(int tag, Text control, ref String value)
        {
            try
            {
                var args = new TextTranslationEventArgs(value, (TextType)tag, TextSource.UnityText);
                TextTranslation?.Invoke(control, args);
                if (args.Translation != null)
                {
                    value = args.Translation;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex);
                throw;
            }
        }

        public static void UILabel_ProcessText(int tag, UILabel control, ref String value)
        {
            try
            {
                var args = new TextTranslationEventArgs(value, (TextType)tag, TextSource.UILabel);
                TextTranslation?.Invoke(control, args);
                if (args.Translation != null)
                {
                    value = args.Translation;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex);
                throw;
            }
        }

        public static void ScriptManager_ReplaceCharaName(int tag, ref String value)
        {
            try
            {
                var args = new TextTranslationEventArgs(value, (TextType)tag, TextSource.ScriptManager);
                TextTranslation?.Invoke(null, args);
                if (args.Translation != null)
                {
                    value = args.Translation;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex);
                throw;
            }
        }

        public static void ScheduleAPI_InfoReplace(int tag, ref int nightWorkId, ref String value)
        {
            try
            {
                var args = new TextTranslationEventArgs(value, (TextType)tag, TextSource.ScheduleAPI);
                TextTranslation?.Invoke(null, args);
                if (args.Translation != null)
                {
                    value = args.Translation;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex);
                throw;
            }
        }

        public static void FreeScene_UI_FreeScene_Start(int tag, ref String value, ref Action endAction)
        {
            try
            {
                var args = new TextTranslationEventArgs(value, (TextType)tag, TextSource.FreeSceneUI);
                TextTranslation?.Invoke(null, args);
                if (args.Translation != null)
                {
                    value = args.Translation;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex);
                throw;
            }
        }

        public static void Trophy_UI_Trophy_Start(int tag, ref String value)
        {
            try
            {
                var args = new TextTranslationEventArgs(value, (TextType)tag, TextSource.TrophyUI);
                TextTranslation?.Invoke(null, args);
                if (args.Translation != null)
                {
                    value = args.Translation;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex);
                throw;
            }
        }

        public static void Image_SetSprite(Image control, ref Sprite value)
        {
            try
            {
                if (value == null)
                    return;

                var spriteName = value.name;
                if (spriteName == null || spriteName.StartsWith("!"))
                    return;

                var args = new TextureTranslationEventArgs(value.name, value.texture, TextureType.Sprite);

                SpriteTextureTranslation?.Invoke(control, args);

                if (args.Translation == null)
                    return;

                var newSprite = Sprite.Create(args.Translation.CreateTexture2D(), value.rect, value.pivot);
                newSprite.name = "!" + spriteName;
                value = newSprite;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex);
                throw;
            }
        }

        public static bool ImportCM_LoadTexture(out TextureResource result, AFileSystemBase fileSystem, string name)
        {
            try
            {
                name = name.Replace(".tex", "");
                var args = new TextureTranslationEventArgs(name, TextureType.Arc);
                ArcTextureTranslation?.Invoke(null, args);
                result = args.Translation;
                return args.Translation != null;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex);
                throw;
            }
        }

        public static TextureResource ImportCM_LoadedTexture(TextureResource resource, string name)
        {
            try
            {
                var args = new ArcTextureLoadedEventArgs(name.Replace(".tex", ""), resource);
                ArcTextureLoaded?.Invoke(null, args);
                return resource;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex);
                throw;
            }
        }

        public static void UIWidget_GetMainTexture(UIWidget control)
        {
            try
            {
                var texture2D = control.material?.mainTexture as Texture2D;

                if (texture2D == null)
                    return;

                var textureName = texture2D.name;
                if (string.IsNullOrEmpty(textureName))
                    return;
                if (textureName.StartsWith("!"))
                {
                    return;
                }

                var args = new TextureTranslationEventArgs(textureName, texture2D, TextureType.UI);

                UITextureTranslation?.Invoke(control, args);

                if (args.Translation == null)
                    return;

                texture2D.name = "!" + textureName;
                texture2D.LoadImage(EmptyTextureData);
                texture2D.LoadImage(args.Translation.data);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex);
                throw;
            }
        }

        public static void MaskableGraphic_OnEnable(MaskableGraphic graphic)
        {
            switch (graphic)
            {
                case Text text:
                    text.text = text.text;
                    break;
                case Image image:
                    image.sprite = image.sprite;
                    break;
            }
        }

        private static YotogiKagHitRetEventArgs yotogiTagEventArgs;

        public static void YotogiKagManager_Tag(YotogiKagManager manager, KagTagSupport kagTag)
        {
            try
            {
                var list = kagTag.GetTagList();

                Logger.Log(LogLevel.Debug, $"[Yotogi] Kag Tag({list["tagname"]})");

                switch (list["tagname"])
                {
                    case "talk":
                    case "talkrepeat":
                        foreach (var tag in list)
                        {
                            Logger.Log(LogLevel.Debug, $"[Yotogi] Kag Tag {tag.Key}:{tag.Value}");
                        }
                        yotogiTagEventArgs = new YotogiKagHitRetEventArgs(list["voice"] + ".ogg");
                        yotogiTagEventArgs.TagCallStack.Add(kagTag);
                        break;
                    case "hitret":
                        if (yotogiTagEventArgs == null)
                        {
                            return;
                        }

                        yotogiTagEventArgs.Text = manager.kag.GetText();

                        var args = new TextTranslationEventArgs(manager.kag.GetText(), TextType.Text, TextSource.Yotogi);
                        TextTranslation?.Invoke(manager, args);
                        
                        if (args.Translation != null)
                        {
                            yotogiTagEventArgs.Translation = args.Translation;
                        }

                        yotogiTagEventArgs.TagCallStack.Add(kagTag);
                        YotogiKagHitRet?.Invoke(manager, yotogiTagEventArgs);
                        yotogiTagEventArgs = null;
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex);
                throw;
            }
        }

        public static void AudioSourceMgr_Play(AudioSourceMgr manager)
        {
            PlaySound?.Invoke(manager, null);
        }

        public static void UIWidget_Awake(UIWidget control) => UIWidget_GetMainTexture(control);

        public static event EventHandler<TextTranslationEventArgs> TextTranslation;

        public static event EventHandler<TextureTranslationEventArgs> ArcTextureTranslation;

        public static event EventHandler<ArcTextureLoadedEventArgs> ArcTextureLoaded;

        public static event EventHandler<TextureTranslationEventArgs> SpriteTextureTranslation;

        public static event EventHandler<TextureTranslationEventArgs> UITextureTranslation;

        public static event EventHandler<YotogiKagHitRetEventArgs> YotogiKagHitRet;

        public static event EventHandler PlaySound;
    }
}