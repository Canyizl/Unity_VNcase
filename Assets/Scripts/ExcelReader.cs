using System.Collections.Generic;
using System.IO;
using ExcelDataReader;
using System.Text;
public class ExcelReader
{
    public struct ExcelData
    {
        public string speakerName;
        public string speakingContent;
        public string avatarImageFileName;
        public string vocalAudioFileName;
        public string backgroundImageFileName;
        public string backgroundMusicFileName;
        public string character1Action;
        public string coordinateX1;
        public string character1ImageFileName;
        public string character2Action;
        public string coordinateX2;
        public string character2ImageFileName;
        public string englishName;
        public string englishContent;
        public string japaneseName;
        public string japaneseContent;
    }
    public static List<ExcelData> ReadExcel(string filePath)
    {
        List<ExcelData> excelData = new List<ExcelData>();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                do
                {
                    while (reader.Read())
                    {
                        ExcelData data = new ExcelData();
                        data.speakerName = GetCellString(reader, 0);
                        data.speakingContent = GetCellString(reader, 1);
                        data.avatarImageFileName = GetCellString(reader, 2);
                        data.vocalAudioFileName = GetCellString(reader, 3);
                        data.backgroundImageFileName = GetCellString(reader, 4);
                        data.backgroundMusicFileName = GetCellString(reader, 5);
                        data.character1Action = GetCellString(reader, 6);
                        data.coordinateX1 = GetCellString(reader, 7);
                        data.character1ImageFileName = GetCellString(reader, 8);
                        data.character2Action = GetCellString(reader, 9);
                        data.coordinateX2 = GetCellString(reader, 10);
                        data.character2ImageFileName = GetCellString(reader, 11);
                        data.englishName = GetCellString(reader, 12);
                        data.englishContent = GetCellString(reader, 13);
                        data.japaneseName = GetCellString(reader, 14);
                        data.japaneseContent = GetCellString(reader, 15);
                        excelData.Add(data);
                    }
                } while (reader.NextResult());
            }
        }
        return excelData;
    }
    private static string GetCellString(IExcelDataReader reader, int index)
    {
        return reader.IsDBNull(index) ? string.Empty : reader.GetValue(index)?.ToString();
    }
}
