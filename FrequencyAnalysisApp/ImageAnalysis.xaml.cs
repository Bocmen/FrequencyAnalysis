using FrequencyAnalysisApp.Extension;
using FrequencyAnalysisApp.Model;
using FrequencyAnalysisApp.Tools;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace FrequencyAnalysisApp
{
    public sealed partial class ImageAnalysis : Page
    {
        private static readonly Windows.Storage.Pickers.FileOpenPicker _fileOpenPicker = new Windows.Storage.Pickers.FileOpenPicker()
        {
            SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary,
            FileTypeFilter =
            {
                ".png",
                ".jpg",
                ".bmp"
            }
        };

        private readonly FrequencyImageAnalysis _analysisResult = new FrequencyImageAnalysis();
        private HistogramType currentViewsHistogram = HistogramType.None;

        public ImageAnalysis() => InitializeComponent();
        #region AppCommandBar
        private void BackButton_Click(object sender, RoutedEventArgs e) => Frame.GoBack();
        private async void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            await PrefabsDialog.ViewWaitTask(async () =>
            {
                var file = await _fileOpenPicker.PickSingleFileAsync();
                if (file != null)
                {
                    await _analysisResult.Load(file);
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.SetSource(await file.OpenReadAsync());
                    CurrentImage.Source = bitmap;
                    ClearHistogramContainer();
                }
            }, "Загрузка и анализ изображения");
        }
        private async Task<bool> IsNotContainsResilt()
        {
            if (_analysisResult.IsLoaded) return false;
            await PrefabsDialog.ShowErrorNotReult();
            return true;
        }
        private async void SaveCsvButton_Click(object sender, RoutedEventArgs e)
        {
            if (await IsNotContainsResilt()) return;
            await PrefabsDialog.ViewWaitTask(() => FileSave.Save("csv", async (file) =>
            {
                using (StreamWriter sw = new StreamWriter(await file.OpenStreamForWriteAsync(), FileSave.DefaultEncoding))
                {
                    await sw.WriteLineAsync($"Объект анализа;{Consts.FrequencyAnalysis_NameCountValues};{Consts.FrequencyAnalysis_NameCountUniqle};{Consts.FrequencyAnalysis_NameMinInformationMeasure};");
                    
                    Task writeInfoNodeAnalysis<T>(string name, FrequencyAnalysis<T> nodeFrequencyAnalysis) => sw.WriteLineAsync($"{name};{nodeFrequencyAnalysis.CountValues};{nodeFrequencyAnalysis.CountUniqle};{nodeFrequencyAnalysis.MinInformationMeasure}");
                    await writeInfoNodeAnalysis(Consts.NameComponent_Red, _analysisResult.Red);
                    await writeInfoNodeAnalysis(Consts.NameComponent_Green, _analysisResult.Green);
                    await writeInfoNodeAnalysis(Consts.NameComponent_Blue, _analysisResult.Blue);
                    await writeInfoNodeAnalysis(Consts.NameComponent_Alpha, _analysisResult.Alpha);
                    await writeInfoNodeAnalysis(Consts.NameComponent_Transparency, _analysisResult.Transparency);
                    await writeInfoNodeAnalysis(Consts.NameComponent_Color, _analysisResult.Color);

                    await sw.WriteLineAsync($"{Consts.NameComponent_Red};{Consts.NameField_Count};{Consts.NameField_Frequency};{Consts.NameComponent_Green};{Consts.NameField_Count};{Consts.NameField_Frequency};{Consts.NameComponent_Blue};{Consts.NameField_Count};{Consts.NameField_Frequency};{Consts.NameComponent_Alpha};{Consts.NameField_Count};{Consts.NameField_Frequency};{Consts.NameComponent_Transparency};{Consts.NameField_Count};{Consts.NameField_Frequency};{Consts.NameComponent_Red};{Consts.NameComponent_Green};{Consts.NameComponent_Blue};{Consts.NameComponent_Alpha};{Consts.NameComponent_Color};{Consts.NameField_Count};{Consts.NameField_Frequency};");
                    var rEnumarator = _analysisResult.Red.GetResults().GetEnumerator();
                    var gEnumarator = _analysisResult.Green.GetResults().GetEnumerator();
                    var bEnumarator = _analysisResult.Blue.GetResults().GetEnumerator();
                    var aEnumarator = _analysisResult.Alpha.GetResults().GetEnumerator();
                    var tEnumarator = _analysisResult.Transparency.GetResults().GetEnumerator();
                    var cEnumarator = _analysisResult.Color.GetResults().GetEnumerator();

                    async Task<bool> tryWriteComponentColor(IEnumerator<FrequencyAnalysisNode<byte>> componentEnumerator)
                    {
                        if (componentEnumerator.MoveNext())
                        {
                            await sw.WriteAsync($"{componentEnumerator.Current.Value};{componentEnumerator.Current.Count};{componentEnumerator.Current.Frequency};");
                            return true;
                        }
                        await sw.WriteAsync(";;;");
                        return false;
                    }
                    bool isLooping = true;
                    while (isLooping)
                    {
                        isLooping = false;
                        isLooping |= await tryWriteComponentColor(rEnumarator);
                        isLooping |= await tryWriteComponentColor(gEnumarator);
                        isLooping |= await tryWriteComponentColor(bEnumarator);
                        isLooping |= await tryWriteComponentColor(aEnumarator);
                        isLooping |= await tryWriteComponentColor(tEnumarator);
                        if (cEnumarator.MoveNext())
                        {
                            await sw.WriteLineAsync($"{cEnumarator.Current.Value.R};{cEnumarator.Current.Value.G};{cEnumarator.Current.Value.B};{cEnumarator.Current.Value.A};{cEnumarator.Current.Value};{cEnumarator.Current.Count};{cEnumarator.Current.Frequency};");
                            isLooping = true;
                        }
                        else
                            await sw.WriteLineAsync(";;;;;;;");
                    }
                }
            }), Consts.TitleFileSave);
        }
        #endregion
        private class FrequencyImageAnalysis
        {
            public FrequencyAnalysis<byte> Red { get; private set; }
            public FrequencyAnalysis<byte> Green { get; private set; }
            public FrequencyAnalysis<byte> Blue { get; private set; }
            public FrequencyAnalysis<byte> Alpha { get; private set; }
            public FrequencyAnalysis<byte> Transparency { get; private set; }
            public FrequencyAnalysis<OxyColor> Color { get; private set; }

            public bool IsLoaded { get; private set; }

            public async Task Load(StorageFile storageFile)
            {
                using (Stream file = await storageFile.OpenStreamForReadAsync())
                {
                    var bitmapDecoder = await BitmapDecoder.CreateAsync(file.AsRandomAccessStream());
                    PixelDataProvider pixelDataProvider = await bitmapDecoder.GetPixelDataAsync(
                        BitmapPixelFormat.Rgba8,
                        BitmapAlphaMode.Premultiplied,
                        new BitmapTransform(),
                        ExifOrientationMode.RespectExifOrientation,
                        ColorManagementMode.ColorManageToSRgb);
                    var bytes = pixelDataProvider.DetachPixelData();

                    FrequencyAnalysis<byte> redAnalysis = new FrequencyAnalysis<byte>();
                    FrequencyAnalysis<byte> greenAnalysis = new FrequencyAnalysis<byte>();
                    FrequencyAnalysis<byte> blueAnalysis = new FrequencyAnalysis<byte>();
                    FrequencyAnalysis<byte> alphaAnalysis = new FrequencyAnalysis<byte>();
                    FrequencyAnalysis<byte> transparencyAnalysis = new FrequencyAnalysis<byte>();
                    FrequencyAnalysis<OxyColor> colorAnalysis = new FrequencyAnalysis<OxyColor>();

                    for (int i = 0; i < bytes.Length; i+=4)
                    {
                        byte r = bytes[i],
                             g = bytes[i + 1],
                             b = bytes[i + 2],
                             a = bytes[i + 3];
                        redAnalysis.AddValue(r);
                        greenAnalysis.AddValue(g);
                        blueAnalysis.AddValue(b);
                        alphaAnalysis.AddValue(a);
                        transparencyAnalysis.AddValue((byte)((r + r + b + g + g + g) / 6)); // Неточная формула для вычисления яркости
                        colorAnalysis.AddValue(OxyColor.FromArgb(a, r, g, b));
                    }

                    Red = redAnalysis;
                    Green = greenAnalysis;
                    Blue = blueAnalysis;
                    Transparency = transparencyAnalysis;
                    Color = colorAnalysis;
                    Alpha = alphaAnalysis;
                    IsLoaded = true;
                }
            }
        }

        private async Task AddHistogram<T>(FrequencyAnalysis<T> frequencyAnalysis, string title, OxyColor oxyColor, HistogramType histogramType, Func<FrequencyAnalysisNode<T>, double> order = null)
        {
            if (await IsNotContainsResilt()) return;
            if (currentViewsHistogram.HasFlag(histogramType)) return;
            var view = new Views.HistogramView();
            view.MinHeight = Frame.ActualHeight * 0.6;
            view.Draw(frequencyAnalysis, title, (v) => v is OxyColor colorParse ? colorParse : oxyColor, order);
            currentViewsHistogram |= histogramType;
            HistogramContainer.Children.Add(view);
        }
        private void ClearHistogramContainer()
        {
            currentViewsHistogram = HistogramType.None;
            HistogramContainer.Children.Clear();
        }
        [Flags]
        public enum HistogramType
        {
            None = 0,
            Color = 1,
            Red = 2,
            Green = 4,
            Blue = 8,
            Alpha = 16,
            Transparency = 32
        }

        private async void ColorHistogram_Click(object sender, RoutedEventArgs e)
        {
            if (await IsNotContainsResilt())
            {
                FlayoutWarningColorHistogram.Hide();
                return;
            }
            CountUniqleColorHistogram.Text = $"Текущие кол-во уникальных: {_analysisResult.Color.CountUniqle}";
        }
        private async void ColorHistogramApplay_Click(object sender, RoutedEventArgs e)
            => await AddHistogram(_analysisResult.Color, ColorHistogram.Content as string, OxyColors.DarkGreen, HistogramType.Color);
        private async void AlphaHistogram_Click(object sender, RoutedEventArgs e)
            => await AddHistogram(_analysisResult.Alpha, AlphaHistogram.Content as string, OxyColors.Black, HistogramType.Alpha, (v) => v.Value);
        private async void RedHistogram_Click(object sender, RoutedEventArgs e)
            => await AddHistogram(_analysisResult.Red, RedHistogram.Content as string, OxyColors.Red, HistogramType.Red, (v) => v.Value);
        private async void GreenHistogram_Click(object sender, RoutedEventArgs e)
            => await AddHistogram(_analysisResult.Green, GreenHistogram.Content as string, OxyColors.Green, HistogramType.Green, (v) => v.Value);
        private async void BlueHistogram_Click(object sender, RoutedEventArgs e)
            => await AddHistogram(_analysisResult.Blue, BlueHistogram.Content as string, OxyColors.Blue, HistogramType.Blue, (v) => v.Value);
        private async void TransparencyHistogram_Click(object sender, RoutedEventArgs e)
            => await AddHistogram(_analysisResult.Transparency, TransparencyHistogram.Content as string, OxyColors.Black, HistogramType.Transparency, (v) => v.Value);
        private void ClearHistogramContainer_Click(object sender, RoutedEventArgs e) => ClearHistogramContainer();
    }
}
