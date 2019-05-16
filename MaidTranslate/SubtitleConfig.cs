using System;
using BepInEx;
using BepInEx.Configuration;

namespace Kanro.MaidTranslate
{
    public class SubtitleConfig
    {
        public SubtitleConfig(ConfigFile config)
        {
            _config = config;
            _config.SaveOnConfigSet = true;
            _enableSubtitleWrapper = config.Wrap(nameof(SubtitleConfig), nameof(EnableSubtitle), "Enable yotogi subtitle",true);
            _maxSubtitleWrapper = config.Wrap(nameof(SubtitleConfig), nameof(MaxSubtitle), "Max shown subtitles", -1);

            _enableSubtitleWrapper.Value = _enableSubtitleWrapper.Value;
            _maxSubtitleWrapper.Value = _maxSubtitleWrapper.Value;

            _enableSubtitle = _enableSubtitleWrapper.Value;
            _maxSubtitle = _maxSubtitleWrapper.Value;
        }

        private readonly ConfigFile _config;
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
            _config.Reload();
            EnableSubtitle = _enableSubtitleWrapper.Value;
            MaxSubtitle = _maxSubtitleWrapper.Value;
        }
    }
}