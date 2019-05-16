using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Configuration;

namespace Kanro.MaidTranslate
{
    public class TranslateConfig
    {

        public TranslateConfig(ConfigFile config)
        {
            _config = config;
            _config.SaveOnConfigSet = true;
            _isDumpingTextWrapper = config.Wrap(nameof(TranslateConfig), nameof(IsDumpingText), "Dump text", false);
            _isDumpingTextureWrapper = config.Wrap(nameof(TranslateConfig), nameof(IsDumpingTexture), "Dump texture", false);
            _isDumpingUiWrapper = config.Wrap(nameof(TranslateConfig), nameof(IsDumpingUI), "Dump UI", false);
            _isDumpingSpriteWrapper = config.Wrap(nameof(TranslateConfig), nameof(IsDumpingSprite), "Dump sprite", false);
            _forceDumpingWrapper = config.Wrap(nameof(TranslateConfig), nameof(ForceDumping), "Force dump", false);
            _dumpSkippedTextWrapper = config.Wrap(nameof(TranslateConfig), nameof(DumpSkippedText), "Skip dump text regex", @"^[\u0020-\u00FF]*$");

            _isDumpingTextWrapper.Value = _isDumpingTextWrapper.Value;
            _isDumpingTextureWrapper.Value = _isDumpingTextureWrapper.Value;
            _isDumpingUiWrapper.Value = _isDumpingUiWrapper.Value;
            _isDumpingSpriteWrapper.Value = _isDumpingSpriteWrapper.Value;
            _forceDumpingWrapper.Value = _forceDumpingWrapper.Value;
            _dumpSkippedTextWrapper.Value = _dumpSkippedTextWrapper.Value;

            _isDumpingText = _isDumpingTextWrapper.Value;
            _isDumpingTexture = _isDumpingTextureWrapper.Value;
            _isDumpingUi = _isDumpingUiWrapper.Value;
            _isDumpingSprite = _isDumpingSpriteWrapper.Value;
            _forceDumping = _forceDumpingWrapper.Value;
            _dumpSkippedText = new Regex(_dumpSkippedTextWrapper.Value, RegexOptions.Compiled);
        }


        private readonly ConfigFile _config;
        private readonly ConfigWrapper<bool> _isDumpingTextWrapper;
        private readonly ConfigWrapper<bool> _isDumpingTextureWrapper;
        private readonly ConfigWrapper<bool> _isDumpingUiWrapper;
        private readonly ConfigWrapper<bool> _isDumpingSpriteWrapper;
        private readonly ConfigWrapper<bool> _forceDumpingWrapper;
        private readonly ConfigWrapper<String> _dumpSkippedTextWrapper;

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
            set => _dumpSkippedText = new Regex(_dumpSkippedTextWrapper.Value = value.ToString(), RegexOptions.Compiled);
        }
        public void Reload()
        {
            _config.Reload();

            _isDumpingText = _isDumpingTextWrapper.Value;
            _isDumpingTexture = _isDumpingTextureWrapper.Value;
            _isDumpingUi = _isDumpingUiWrapper.Value;
            _isDumpingSprite = _isDumpingSpriteWrapper.Value;
            _forceDumping = _forceDumpingWrapper.Value;
            _dumpSkippedText = new Regex(_dumpSkippedTextWrapper.Value, RegexOptions.Compiled);
        }
    }
}
