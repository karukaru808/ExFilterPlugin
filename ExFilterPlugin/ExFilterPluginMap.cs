using System.Collections.Generic;

namespace ExFilterPlugin
{
    public class ExFilterPluginMap
    {
        public bool HasHeaderRecord => true;
        public List<Date> MappingList;

        public ExFilterPluginMap()
        {
            MappingList = new List<Date>();
        }

        public class Date
        {
            public string RegexWord { get; }
            public string DisplayWord { get; }
            public string AudioFilePath { get; }

            public Date()
            {
                RegexWord = "キーワード（正規表現）";
                DisplayWord = "表示文字";
                AudioFilePath = "ファイルパス（フルパス）";
            }
        }
    }
}