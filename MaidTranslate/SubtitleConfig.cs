using System;
using BepInEx;

namespace Kanro.MaidTranslate
{
    public class SubtitleConfig
    {
        public SubtitleConfig(YotogiSubtitles plugin)
        {
            Config.SaveOnConfigSet = true;
            _enableSubtitleWrapper = new ConfigWrapper<bool>(nameof(EnableSubtitle), plugin, true);
            _maxSubtitleWrapper = new ConfigWrapper<int>(nameof(MaxSubtitle), plugin, -1);

            _enableSubtitle = _enableSubtitleWrapper.Value;
            _maxSubtitle = _maxSubtitleWrapper.Value;
        }
        
        private readonly ConfigWrapper<bool> _enableSubtitleWrapper;
        private readonly ConfigWrapper<int> _maxSubtitleWrapper;
        
        private bool _enableSubtitle;
        private int _maxSubtitle;

        public bool EnableSubtitle
        {
            get => _enableSubtitle;
            set => _enableSubtitle = _enableSubtitleWrapper.Value = value;
        }

        public int MaxSubtitle
        {
            get => _maxSubtitle;
            set => _maxSubtitle = _maxSubtitleWrapper.Value = value;
        }

        public void Reload()
        {
            Config.ReloadConfig();
            EnableSubtitle = _enableSubtitleWrapper.Value;
            MaxSubtitle = _maxSubtitleWrapper.Value;
        }
    }
}