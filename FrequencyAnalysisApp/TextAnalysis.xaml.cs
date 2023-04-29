using FrequencyAnalysisApp.Extension;
using FrequencyAnalysisApp.Model;
using FrequencyAnalysisApp.Tools;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FrequencyAnalysisApp
{
    public sealed partial class TextAnalysis : Page
    {
        private readonly Windows.Storage.Pickers.FileOpenPicker _fileOpenPicker = new Windows.Storage.Pickers.FileOpenPicker()
        {
            SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary,
            FileTypeFilter =
            {
                ".txt"
            }
        };
        private readonly TextContainer _textContainer;
        private FrequencyAnalysis<char> _analysisResult;

        public TextAnalysis()
        {
            InitializeComponent();
            _textContainer = new TextContainer(FieldMultiLineText);
        }
        
        #region ButtonsCommandBar
        private async void OpenFileButton_Click(object sender, RoutedEventArgs e) => await _textContainer.SetFileContent(await _fileOpenPicker.PickSingleFileAsync());
        private async Task<bool> IsNotContainsResilt()
        {
            if (_analysisResult == null)
            {
                await PrefabsDialog.ShowErrorNotReult();
                return true;
            }
            return false;
        }
        private string CharProtection(char @char) => @char == '\n' ? "\\\\n" : @char.ToString();
        private async void SaveTxtButton_Click(object sender, RoutedEventArgs e)
        {
            if (await IsNotContainsResilt()) return;
            await PrefabsDialog.ViewWaitTask(() => FileSave.Save("txt", async (file) =>
            {
                using (StreamWriter sw = new StreamWriter(await file.OpenStreamForWriteAsync(), FileSave.DefaultEncoding))
                {
                    await sw.WriteLineAsync("Глобальные результаты анализа.\n");
                    await sw.WriteLineAsync($"{Consts.FrequencyAnalysis_NameCountValues}: {_analysisResult.CountValues}");
                    await sw.WriteLineAsync($"{Consts.FrequencyAnalysis_NameCountUniqle}: {_analysisResult.CountUniqle}");
                    await sw.WriteLineAsync($"{Consts.FrequencyAnalysis_NameMinInformationMeasure}: {_analysisResult.MinInformationMeasure}");
                    await sw.WriteLineAsync("\nЧастотные характеристики каждого элемента");
                    foreach (var item in _analysisResult.GetResults())
                        await sw.WriteLineAsync($"{Consts.NameField_Value} [{CharProtection(item.Value)}], {Consts.NameField_Count} [{item.Count}], {Consts.NameField_Frequency} [{item.Frequency}]");
                }
            }, "TextAnalysisTextContent"), Consts.TitleFileSave);
        }
        private async void SaveCsvButton_Click(object sender, RoutedEventArgs e)
        {
            if (await IsNotContainsResilt()) return;
            await PrefabsDialog.ViewWaitTask(() => FileSave.Save("csv", async (file) =>
            {
                using (StreamWriter sw = new StreamWriter(await file.OpenStreamForWriteAsync(), FileSave.DefaultEncoding))
                {
                    await sw.WriteLineAsync($"{Consts.FrequencyAnalysis_NameCountValues};{Consts.FrequencyAnalysis_NameCountUniqle};{Consts.FrequencyAnalysis_NameMinInformationMeasure};");
                    await sw.WriteLineAsync($"{_analysisResult.CountValues};{_analysisResult.CountUniqle};{_analysisResult.MinInformationMeasure};");
                    await sw.WriteLineAsync($"{Consts.NameField_Value};{Consts.NameField_Count};{Consts.NameField_Frequency};");
                    foreach (var item in _analysisResult.GetResults())
                        await sw.WriteLineAsync($"{CharProtection(item.Value)};{item.Count};{item.Frequency};");
                }
            }, "ExcelAnalysisTextContent"), Consts.TitleFileSave);
        }
        private void BackButton_Click(object sender, RoutedEventArgs e) => Frame.GoBack();
        #endregion
        #region Reader
        private class TextContainer
        {
            private const int BufferRead = 1024;
            private const uint MaxSizeFile = 5242880;
            private const string MessageFileMaxSize = "Размер файла превышает лимиты.\nПоэтому он не будет отображён.";

            private readonly TextBox _textBox;
            private StorageFile _file;

            public TextContainer(TextBox textBox)
            {
                _textBox = textBox;
            }

            private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
            {
                _file = null;
                _textBox.TextChanged -= TextBox_TextChanged;
            }

            public async Task SetFileContent(StorageFile file)
            {
                if (file == null) return;
                var properties = await file.GetBasicPropertiesAsync();
                if (properties.Size > MaxSizeFile)
                {
                    _textBox.TextChanged -= TextBox_TextChanged;
                    _textBox.Text = MessageFileMaxSize;
                    _textBox.TextChanged += TextBox_TextChanged;
                    _file = file;
                }
                else
                {
                    using (StreamReader streamFile = new StreamReader(await file.OpenStreamForReadAsync()))
                        _textBox.Text = await streamFile.ReadToEndAsync();
                }
            }

            public async Task<Reader> GetReader()
            {
                if (_file == null)
                {
                    return new Reader(_textBox.Text);
                }
                else
                {
                    Stream streamFile = await _file.OpenStreamForReadAsync();
                    return new Reader(CreateEnumeratorStream(streamFile), streamFile.Dispose);
                }
            }
            private IEnumerable<char> CreateEnumeratorStream(Stream stream)
            {
                if (stream.Position != 0) stream.Seek(0, SeekOrigin.Begin);
                StreamReader streamReader = new StreamReader(stream);
                char[] buffer = new char[BufferRead];
                int countRead;
                do
                {
                    countRead = streamReader.ReadBlock(buffer, 0, BufferRead);
                    foreach (var elem in buffer) yield return elem;
                } while (countRead > 0);
                yield break;
            }
        }
        private class Reader : IDisposable, IEnumerable<char>
        {
            private readonly IEnumerable<char> _chars;
            private readonly Action _disponse;

            public Reader(IEnumerable<char> chars, Action disponse = null)
            {
                _chars=chars;
                _disponse=disponse;
            }

            public void Dispose() => _disponse?.Invoke();

            public IEnumerator<char> GetEnumerator() => _chars.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => _chars.GetEnumerator();
        }
        #endregion
        #region Анализ текста
        private IEnumerable<char> SelectorDictonary(IEnumerable<char> chars)
        {
            if (string.IsNullOrWhiteSpace(PatternDictonaryChar.Text)) return chars;
            string pattern = new string(CaseEditor(PatternDictonaryChar.Text).Distinct().ToArray());
            return chars.Where(x => pattern.Contains(x));
        }
        private IEnumerable<char> CaseEditor(IEnumerable<char> chars)
        {
            switch (ComboBoxCaseFilter.SelectedIndex)
            {
                case 0: return chars;
                case 1: return chars.Select(x => char.ToLower(x));
                case 2: return chars.Select(x => char.ToUpper(x));
            }
            throw new Exception();
        }
        private async Task<FrequencyAnalysis<char>> CreateAnalysis()
        {
            FrequencyAnalysis<char> result = new FrequencyAnalysis<char>();
            using (var reader = await _textContainer.GetReader())
                result.AddValues(SelectorDictonary(CaseEditor(reader)));
            return result;
        }
        private async void DrawPlotsButton_Click(object sender, RoutedEventArgs e)
        {
            _analysisResult = await CreateAnalysis();
            HistogramPlot.Draw(_analysisResult, "Гистограмма текста", (_) => OxyColors.DarkGreen, (v) => v.Count);
        }
        #endregion
        #region Работа со словарём элементов
        private static string GetComboBoxDictonaryText(ComboBox comboBox) => comboBox.SelectedItem is ComboBoxDictonaryNode data ? data.Data ?? string.Empty : string.Empty;
        private void CreateDictonaryButton_Click(object sender, RoutedEventArgs e) => PatternDictonaryChar.Text = string.Join(string.Empty,
                    GetComboBoxDictonaryText(ComboBoxDictonaryNumbers),
                    GetComboBoxDictonaryText(ComboBoxDictonaryCyrillic),
                    GetComboBoxDictonaryText(ComboBoxDictonaryLatinAlphabet),
                    GetComboBoxDictonaryText(ComboBoxDictonaryPunctuation)
            );
        #endregion
    }
}
