using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Kanro.MaidTranslate.Hook;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Inject;

namespace Kanro.MaidTranslate
{
    public static class UnityPatcher
    {
        public static IEnumerable<string> TargetDLLs { get; } = new[] { "UnityEngine.UI.dll", "Assembly-CSharp.dll" };

        private static AssemblyDefinition pluginAssembly = AssemblyDefinition.ReadAssembly(@"BepInEx\plugins\MaidTranslate.dll");
        private static TypeDefinition hookCenter = pluginAssembly.MainModule.GetType($"Kanro.MaidTranslate.Hook.{nameof(HookCenter)}");

        public static void Patch(AssemblyDefinition assembly)
        {
            switch (assembly.Name.Name)
            {
                case "UnityEngine.UI":
                    PatchUnity(assembly);
                    break;
                case "Assembly-CSharp":
                    PatchGame(assembly);
                    break;
            }
        }

        private static void PatchUnity(AssemblyDefinition assembly)
        {
            var textControl = assembly.MainModule.GetType("UnityEngine.UI.Text");
            var textSetter = textControl.GetMethod("set_text");
            textSetter.InjectWith(hookCenter.GetMethod(nameof(HookCenter.Text_SetText)),
                tag: (int)TextType.Text,
                flags: InjectFlags.PassParametersRef
                       | InjectFlags.PassInvokingInstance
                       | InjectFlags.PassTag);
            Trace.WriteLine("[MaidTranslate] UnityEngine.UI.Text patched");


            var imageControl = assembly.MainModule.GetType("UnityEngine.UI.Image");
            var spriteSetter = imageControl.GetMethod("set_sprite");
            spriteSetter.InjectWith(hookCenter.GetMethod(nameof(HookCenter.Image_SetSprite)),
                flags: InjectFlags.PassParametersRef
                       | InjectFlags.PassInvokingInstance);
            Trace.WriteLine("[MaidTranslate] UnityEngine.UI.Image patched");
            
            var maskableGraphic = assembly.MainModule.GetType("UnityEngine.UI.MaskableGraphic");
            var onEnable = maskableGraphic.GetMethod("OnEnable");
            onEnable.InjectWith(hookCenter.GetMethod(nameof(HookCenter.MaskableGraphic_OnEnable)),
                flags: InjectFlags.PassInvokingInstance);
            Trace.WriteLine("[MaidTranslate] UnityEngine.UI.MaskableGraphic patched");
        }

        private static void PatchGame(AssemblyDefinition assembly)
        {
            var uiLabelControl = assembly.MainModule.GetType("UILabel");
            var uiLabelTextSetter = uiLabelControl.GetMethod("ProcessText", typeof(bool), typeof(bool));
            uiLabelTextSetter.InjectWith(hookCenter.GetMethod(nameof(HookCenter.UILabel_ProcessText)),
                tag: (int)TextType.UiLabel,
                flags: InjectFlags.PassInvokingInstance
                       | InjectFlags.PassFields
                       | InjectFlags.PassTag,
                typeFields: new[] { uiLabelControl.GetField("mText") });
            Trace.WriteLine("[MaidTranslate] UILabel patched");

            var scriptManager = assembly.MainModule.GetType("ScriptManager");
            var replaceCharaName = scriptManager.GetMethod("ReplaceCharaName", typeof(string));
            replaceCharaName.InjectWith(hookCenter.GetMethod(nameof(HookCenter.ScriptManager_ReplaceCharaName)),
                tag: (int)TextType.Template,
                flags: InjectFlags.PassParametersRef
                       | InjectFlags.PassTag);
            Trace.WriteLine("[MaidTranslate] ScriptManager patched");

            var scheduleApi = assembly.MainModule.GetType("Schedule.ScheduleAPI");
            var infoReplace = scheduleApi.GetMethod("InfoReplace");
            infoReplace.InjectWith(hookCenter.GetMethod(nameof(HookCenter.ScheduleAPI_InfoReplace)),
                tag: (int)TextType.Template,
                flags: InjectFlags.PassParametersRef
                       | InjectFlags.PassTag);
            Trace.WriteLine("[MaidTranslate] Schedule.ScheduleAPI patched");

            var freeSceneUi = assembly.MainModule.GetType("FreeScene_UI");
            var freeSceneStart = freeSceneUi.GetMethod("FreeScene_Start");
            freeSceneStart.InjectWith(hookCenter.GetMethod(nameof(HookCenter.FreeScene_UI_FreeScene_Start)),
                tag: (int)TextType.Const,
                flags: InjectFlags.PassParametersRef | InjectFlags.PassTag);
            Trace.WriteLine("[MaidTranslate] FreeScene_UI patched");

            var trophyUi = assembly.MainModule.GetType("Trophy_UI");
            var trophyStart = trophyUi.GetMethod("Trophy_Start");
            trophyStart.InjectWith(hookCenter.GetMethod(nameof(HookCenter.Trophy_UI_Trophy_Start)),
                tag: (int)TextType.Const,
                flags: InjectFlags.PassParametersRef | InjectFlags.PassTag);
            Trace.WriteLine("[MaidTranslate] Trophy_UI patched");
            
            var importCm = assembly.MainModule.GetType("ImportCM");
            var loadTexture = importCm.GetMethod("LoadTexture");
            loadTexture.InjectWith(hookCenter.GetMethod(nameof(HookCenter.ImportCM_LoadTexture)),
                flags: InjectFlags.PassParametersVal | InjectFlags.ModifyReturn);
            importCm = assembly.MainModule.GetType("ImportCM");
            loadTexture = importCm.GetMethod("LoadTexture");
            var retInstruction = loadTexture.Body.Instructions.Last();
            var il = loadTexture.Body.GetILProcessor();
            il.InsertBefore(retInstruction, il.Create(OpCodes.Ldarg, 1));
            il.InsertBefore(retInstruction, il.Create(OpCodes.Callvirt, 
                assembly.MainModule.ImportReference(hookCenter.GetMethod(nameof(HookCenter.ImportCM_LoadedTexture)))));
            Trace.WriteLine("[MaidTranslate] ImportCM patched");
            
            var uiWidget = assembly.MainModule.GetType("UIWidget");
            var getMainTextureTarget = uiWidget.GetMethod("get_mainTexture");
            getMainTextureTarget.InjectWith(hookCenter.GetMethod(nameof(HookCenter.UIWidget_GetMainTexture)),
                flags: InjectFlags.PassInvokingInstance);
            
            var awakeTarget = uiWidget.GetMethod("Awake");
            awakeTarget.InjectWith(hookCenter.GetMethod(nameof(HookCenter.UIWidget_Awake)),
                flags: InjectFlags.PassInvokingInstance);
            Trace.WriteLine("[MaidTranslate] UIWidget patched");
            
            var yotogiKagManager = assembly.MainModule.GetType("YotogiKagManager");
            var tagHitRet = yotogiKagManager.GetMethod("TagHitRet");
            tagHitRet.InjectWith(hookCenter.GetMethod(nameof(HookCenter.YotogiKagManager_Tag)),
                flags: InjectFlags.PassInvokingInstance
                | InjectFlags.PassParametersVal);

            var tagTalk = yotogiKagManager.GetMethod("TagTalk");
            tagTalk.InjectWith(hookCenter.GetMethod(nameof(HookCenter.YotogiKagManager_Tag)),
                flags: InjectFlags.PassInvokingInstance
                       | InjectFlags.PassParametersVal);

            var tagTalkRepeat = yotogiKagManager.GetMethod("TagTalkRepeat");
            tagTalkRepeat.InjectWith(hookCenter.GetMethod(nameof(HookCenter.YotogiKagManager_Tag)),
                flags: InjectFlags.PassInvokingInstance
                       | InjectFlags.PassParametersVal);
            Trace.WriteLine("[MaidTranslate] YotogiKagManager patched");

            var audioSourceMgr = assembly.MainModule.GetType("AudioSourceMgr");
            var loadPlay = audioSourceMgr.GetMethod("Play");
            loadPlay.InjectWith(hookCenter.GetMethod(nameof(HookCenter.AudioSourceMgr_Play)), 
                flags: InjectFlags.PassInvokingInstance);
            Trace.WriteLine("[MaidTranslate] AudioSourceMgr patched");
        }

        public static void Initialize()
        {
            Trace.WriteLine("[MaidTranslate] UnityPatcher initialized");
        }

        public static void Finish()
        {
            Trace.WriteLine("[MaidTranslate] UnityPatcher patch finish");
        }
    }
}