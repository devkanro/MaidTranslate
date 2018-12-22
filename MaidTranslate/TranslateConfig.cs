using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BepInEx;

namespace Kanro.MaidTranslate
{
    public class TranslateConfig
    {

        public TranslateConfig(MaidTranslate plugin)
        {
            Config.SaveOnConfigSet = true;
            _isDumpingTextWrapper = new ConfigWrapper<bool>(nameof(IsDumpingText), plugin);
            _isDumpingTextureWrapper = new ConfigWrapper<bool>(nameof(IsDumpingTexture), plugin);
            _isDumpingUiWrapper = new ConfigWrapper<bool>(nameof(IsDumpingUI), plugin);
            _isDumpingSpriteWrapper = new ConfigWrapper<bool>(nameof(IsDumpingSprite), plugin);
            _forceDumpingWrapper = new ConfigWrapper<bool>(nameof(ForceDumping), plugin);
            _dumpSkippedTextWrapper = new ConfigWrapper<Regex>(nameof(DumpSkippedText), plugin, it => new Regex(it, RegexOptions.Compiled), it => it.ToString(), new Regex(@"^[\u0020-\u00FF]*$", RegexOptions.Compiled));
            
            _isDumpingText = _isDumpingTextWrapper.Value;
            _isDumpingTexture = _isDumpingTextureWrapper.Value;
            _isDumpingUi = _isDumpingUiWrapper.Value;
            _isDumpingSprite = _isDumpingSpriteWrapper.Value;
            _forceDumping = _forceDumpingWrapper.Value;
            _dumpSkippedText = _dumpSkippedTextWrapper.Value;
            Config.SaveConfig();
        }


        private readonly ConfigWrapper<bool> _isDumpingTextWrapper;
        private readonly ConfigWrapper<bool> _isDumpingTextureWrapper;
        private readonly ConfigWrapper<bool> _isDumpingUiWrapper;
        private readonly ConfigWrapper<bool> _isDumpingSpriteWrapper;
        private readonly ConfigWrapper<bool> _forceDumpingWrapper;
        private readonly ConfigWrapper<Regex> _dumpSkippedTextWrapper;

        private bool _isDumpingText;
        private bool _isDumpingTexture;
        private bool _isDumpingUi;
        private bool _isDumpingSprite;
        private bool _forceDumping;
        private Regex _dumpSkippedText;

        public bool IsDumpingText
        {
            get => _isDumpingText;
            set => _isDumpingText = _isDumpingTextWrapper.Value = value;
        }

        public bool IsDumpingTexture
        {
            get => _isDumpingTexture;
            set => _isDumpingTexture = _isDumpingTextureWrapper.Value = value;
        }

        public bool IsDumpingUI
        {
            get => _isDumpingUi;
            set => _isDumpingUi = _isDumpingUiWrapper.Value = value;
        }

        public bool IsDumpingSprite
        {
            get => _isDumpingSprite;
            set => _isDumpingSprite = _isDumpingSpriteWrapper.Value = value;
        }

        public bool ForceDumping
        {
            get => _forceDumping;
            set => _forceDumping = _forceDumpingWrapper.Value = value;
        }

        public Regex DumpSkippedText
        {
            get => _dumpSkippedText;
            set => _dumpSkippedText = _dumpSkippedTextWrapper.Value = value;
        }
        public void Reload()
        {
            Config.ReloadConfig();

            _isDumpingText = _isDumpingTextWrapper.Value;
            _isDumpingTexture = _isDumpingTextureWrapper.Value;
            _isDumpingUi = _isDumpingUiWrapper.Value;
            _isDumpingSprite = _isDumpingSpriteWrapper.Value;
            _forceDumping = _forceDumpingWrapper.Value;
            _dumpSkippedText = _dumpSkippedTextWrapper.Value;
        }
    }
}
