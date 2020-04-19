using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Yukarinette;

namespace ExFilterPlugin
{
    public class ExFilterPlugin : IYukarinetteFilterInterface
    {
        /// <summary>
        /// 設定パネル
        /// </summary>
        private readonly ExFilterPluginConfigPanel _config;

        //音声プレイヤー変数
        private IWavePlayer _wavePlayer;

        //オーディオファイルリーダー保持用変数
        private AudioFileReader _reader;

        public ExFilterPlugin()
        {
            _config = new ExFilterPluginConfigPanel();
            YukarinetteLogger.Instance.Info($"{typeof(ExFilterPlugin)}: 正常起動しました。");
            YukarinetteConsoleMessage.Instance.WriteMessage($"{typeof(ExFilterPlugin)}: Awake");
        }

        /// <summary>
        /// フィルタープラグイン名
        /// </summary>
        public override string Name => typeof(ExFilterPlugin).ToString();

        /// <summary>
        /// 設定パネルを返す
        /// </summary>
        /// <returns></returns>
        public override UserControl GetSettingUserControl()
        {
            return _config;
        }

        /// フィルタープラグインがロードされる際に呼ばれます。ロードはゆかりねっと起動時に一度だけ呼ばれます。
        public override void Loaded()
        {
            var endPoints = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            if (endPoints.Count <= _config.Setting.SettingDate.AudioDeviceIndex ||
                _config.Setting.SettingDate.AudioDeviceIndex < 0)
            {
                YukarinetteLogger.Instance.Warn($"Audio Device Index is Out Of Range!");
                return;
            }

            var audioDevice = endPoints[_config.Setting.SettingDate.AudioDeviceIndex];

            //デバイス、共有モード、イベントコールバック無効、レイテンシ=100ms でプレイヤーを設定
            _wavePlayer = new WasapiOut(audioDevice, AudioClientShareMode.Shared, false, 100);
        }

        /// ゆかりねっとが終了する際に呼ばれます。終了時に一度だけ呼ばれます。
        public override void Closed()
        {
            //リソース破棄
            if (_wavePlayer != null)
            {
                _wavePlayer.Stop();
                _wavePlayer.Dispose();
                _wavePlayer = null;
            }

            if (_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }
        }

        /// <summary>
        /// フィルター実行
        /// </summary>
        /// <param name="text">音声認識で取得した文字列</param>
        /// <param name="words">音声認識で取得した文字列を形態素解析した結果の単語配列</param>
        /// <returns></returns>
        public override YukarinetteFilterPluginResult Filtering(string text, YukarinetteWordDetailData[] words)
        {
            var filterPluginResult = new YukarinetteFilterPluginResult {Text = text};

            foreach (var mapping in _config.Map.MappingList.Where(mapping =>
                Regex.IsMatch(text, mapping.RegexWord)))
            {
                filterPluginResult.Text = string.IsNullOrWhiteSpace(mapping.DisplayWord) ? text : mapping.DisplayWord;

                //音を再生する処理
                PlaySound(mapping.AudioFilePath);
            }

            return filterPluginResult;
        }

        //音声ファイルを再生する関数
        private void PlaySound(string audioFilePath)
        {
            //音声デバイスが設定されていない場合は再取得
            if (_wavePlayer == null)
                Loaded();

            //音声再生をTry
            try
            {
                //再生してるなら止める
                if (_wavePlayer.PlaybackState != PlaybackState.Stopped)
                    _wavePlayer.Stop();

                //音声ファイルのロード
                //対応フォーマット：.wav, .mp3, .aiff, MFCreateSourceReaderFromURL()で読めるもの（動画ファイルも可？）
                _reader = new AudioFileReader(audioFilePath);

                //sampleChannelにreaderをセット
                var sampleChannel = new SampleChannel(_reader, true);

                /*
                //ここでボリューム等（？）の設定が可能？
                //サンプルの AudioPlaybackPanel.cs を参照
                sampleChannel.PreVolumeMeter+= OnPreVolumeMeter;
                setVolumeDelegate = vol => sampleChannel.Volume = vol;
                */

                //sampleChannelのボリュームをセット
                var postVolumeMeter = new MeteringSampleProvider(sampleChannel);

                /*
                //ここでボリューム等（？）の設定が可能？
                //サンプルの AudioPlaybackPanel.cs を参照
                postVolumeMeter.StreamVolume += OnPostVolumeMeter;
                */

                //プレイヤーに設定をセット
                _wavePlayer.Init(postVolumeMeter);

                //再生
                YukarinetteLogger.Instance.Info($"Sound Play Start");
                _wavePlayer.Play();

                //YukarinetteLogger.Instance.Info("Sound Play End");
            }
            catch (YukarinetteException ex)
            {
                //エラー文出力
                YukarinetteLogger.Instance.Error($"Sound Play Error!");
                YukarinetteLogger.Instance.Error($"{audioFilePath}");
                YukarinetteLogger.Instance.Error($"{ex.Message}");
                YukarinetteConsoleMessage.Instance.WriteMessage($"{typeof(ExFilterPlugin)}: エラーが発生しました。再生を中止します。");
                YukarinetteConsoleMessage.Instance.WriteMessage(ex);
            }
        }
    }
}