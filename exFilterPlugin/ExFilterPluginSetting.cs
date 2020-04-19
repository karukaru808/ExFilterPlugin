using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Yukarinette;

namespace ExFilterPlugin
{
    public class ExFilterPluginSetting
    {
        //設定項目
        public bool HasHeaderRecord => false;
        private readonly List<Date> _settingDate;

        public Date SettingDate
        {
            get => _settingDate.First();
            set => _settingDate[0] = value;
        }

        //設定項目初期化
        public ExFilterPluginSetting()
        {
            _settingDate = new List<Date>() {new Date()};
        }

        public class Date
        {
            public string CsvPath { get; set; }
            public int AudioDeviceIndex { get; set; }

            public Date()
            {
                //設定ファイルの位置情報
                CsvPath = Path.Combine(Path.Combine(YukarinetteCommon.AppSettingFolder, "Plugins"),
                    $"{Path.GetFileName(Assembly.GetExecutingAssembly().Location)}.csv");

                //音声出力先情報
                AudioDeviceIndex = -1;
            }
        }
    }
}