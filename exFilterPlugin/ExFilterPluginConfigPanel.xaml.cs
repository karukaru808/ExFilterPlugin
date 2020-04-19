using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using CsvHelper;
using Yukarinette;

namespace ExFilterPlugin
{
    /// <summary>
    /// フィルタープラグイン設定画面
    /// </summary>
    public partial class ExFilterPluginConfigPanel
    {
        public ExFilterPluginSetting Setting = new ExFilterPluginSetting();
        public ExFilterPluginMap Map = new ExFilterPluginMap();

        private static string ConfigPath => Path.Combine(Path.Combine(YukarinetteCommon.AppSettingFolder, "Plugins"),
            $"{Path.GetFileName(Assembly.GetExecutingAssembly().Location)}.config");

        public ExFilterPluginConfigPanel()
        {
            InitializeComponent();

            YukarinetteConsoleMessage.Instance.WriteMessage("TEST1");
            var t = Load<ExFilterPluginSetting.Date>(ConfigPath, Setting.HasHeaderRecord);
            YukarinetteConsoleMessage.Instance.WriteMessage("TEST --- 1");
            YukarinetteConsoleMessage.Instance.WriteMessage($"{t == null}");
            //なんかNULLってる
            Setting.SettingDate = t[0];
            YukarinetteConsoleMessage.Instance.WriteMessage("TEST333");
            Map.MappingList = Load<ExFilterPluginMap.Date>(Setting.SettingDate.CsvPath, Map.HasHeaderRecord);

            YukarinetteConsoleMessage.Instance.WriteMessage("TEST2");
            UpdateAudioDevice(null, null);
            AudioDeviceComboBox.SelectedIndex = Setting.SettingDate.AudioDeviceIndex;
            CsvPathTextBox.Text = Setting.SettingDate.CsvPath;
        }

        private void SaveConfig(object sender, RoutedEventArgs e)
        {
            Setting.SettingDate.AudioDeviceIndex = AudioDeviceComboBox.SelectedIndex;
            Setting.SettingDate.CsvPath = CsvPathTextBox.Text;

            CsvWriter(ConfigPath, Setting.HasHeaderRecord, new List<ExFilterPluginSetting.Date>
            {
                Setting.SettingDate
            });

            Map.MappingList = Load<ExFilterPluginMap.Date>(Setting.SettingDate.CsvPath, Map.HasHeaderRecord);

            YukarinetteConsoleMessage.Instance.WriteMessage("Save Config");
        }

        private List<T> Load<T>(string path, bool hasHeader) where T : new()
        {
            if (!File.Exists(path))
            {
                //CSVファイルが存在しないなら新規作成
                CsvWriter(path, hasHeader, new List<T> {new T()});
            }

            return CsvReader<T>(path, hasHeader);
        }

        //CSVファイル読込
        private List<T> CsvReader<T>(string path, bool hasHeader)
        {
            YukarinetteConsoleMessage.Instance.WriteMessage("TEST - 1");

            var result = new List<T>();
            using (var sr = new StreamReader(path))
            {
                using (var csv = new CsvReader(sr, CultureInfo.CurrentCulture))
                {
                    if (csv.Read())
                    {
                        YukarinetteConsoleMessage.Instance.WriteMessage("TEST - 2");
                        csv.Configuration.HasHeaderRecord = hasHeader;
                        csv.Configuration.AutoMap<T>();
                        YukarinetteConsoleMessage.Instance.WriteMessage("TEST - 3");

                        result = csv.GetRecords<T>() as List<T>;
                    }

                    YukarinetteConsoleMessage.Instance.WriteMessage("TEST - 4");

                    csv.Dispose();
                }

                YukarinetteConsoleMessage.Instance.WriteMessage("TEST - 5");
                sr.Dispose();
            }

            YukarinetteConsoleMessage.Instance.WriteMessage("TEST - 6");
            return result;
        }

        //CSVファイル書き込み
        private void CsvWriter<T>(string path, bool hasHeader, IEnumerable<T> writeData)
        {
            using (var sw = new StreamWriter(path))
            {
                using (var csv = new CsvWriter(sw, CultureInfo.CurrentCulture))
                {
                    csv.Configuration.HasHeaderRecord = hasHeader;
                    csv.WriteRecords(writeData);
                    csv.Dispose();
                }

                sw.Dispose();
            }
        }

        private void OpenCsvSelectWindow(object sender, RoutedEventArgs e)
        {
            var initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (string.IsNullOrEmpty(CsvPathTextBox.Text) && File.Exists(CsvPathTextBox.Text))
            {
                initialDirectory = Path.GetDirectoryName(CsvPathTextBox.Text);
            }

            var openFileDialog = new OpenFileDialog
            {
                FileName = "",
                InitialDirectory = initialDirectory,
                Filter = "CSV (*.csv)|*.csv",
                Title = "CSVファイル を指定してください。"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                CsvPathTextBox.Text = openFileDialog.FileName;
            }

            YukarinetteConsoleMessage.Instance.WriteMessage("Selected CSV");
        }

        private void UpdateAudioDevice(object sender, RoutedEventArgs e)
        {
            //要素を追加する前に全て削除する
            AudioDeviceComboBox.Items.Clear();

            var endPoints = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            foreach (var endPoint in endPoints)
            {
                //表示セット
                AudioDeviceComboBox.Items.Add(endPoint.FriendlyName);
            }

            YukarinetteConsoleMessage.Instance.WriteMessage("Update Audio Device");
        }
    }
}