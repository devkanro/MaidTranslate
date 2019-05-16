using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using Kanro.MaidTranslate.Hook;
using Kanro.MaidTranslate.Util;
using UnityEngine;

namespace Kanro.MaidTranslate
{
    [BepInPlugin(GUID: "MaidDebug", Name: "Kanro.MaidDebug", Version:"5.0")]
    public class MaidDebug : BaseUnityPlugin
    {
        public void OnGUI()
        {
            try
            {
                if (Event.current.type != EventType.KeyUp) return;

                switch (Event.current.keyCode)
                {
                    case KeyCode.Keypad0:
                        var data = GUIUtility.systemCopyBuffer.Unescape();
                        var eventArgs = new TextTranslationEventArgs(data, TextType.Text)
                        {
                            Debug = true
                        };
                        HookCenter.InvokeTextTranslation(null, eventArgs);
                        GUIUtility.systemCopyBuffer = eventArgs.Translation?.Escape() ?? "{No translation}";
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex);
                throw;
            }
        }
    }
}
