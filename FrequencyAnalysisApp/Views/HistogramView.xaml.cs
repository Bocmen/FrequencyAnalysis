using FrequencyAnalysisApp.Extension;
using FrequencyAnalysisApp.Model;
using FrequencyAnalysisApp.Tools;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FrequencyAnalysisApp.Views
{
    public sealed partial class HistogramView : UserControl
    {
        private List<(string value, double frequency)> _data = new List<(string value, double frequency)>();
        private PlotView _currentPlotView;

        public HistogramView() => InitializeComponent();

        public void Draw<T>(FrequencyAnalysis<T> dataFrequencyAnalysis, string title, OxyColor color, Func<T, string> valueConverter = null)
        {
            Title.Text = title ?? string.Empty;
            _data.Clear();
            valueConverter = valueConverter ?? ((val) => valueConverter.ToString());
            _data.AddRange(dataFrequencyAnalysis.GetResults().OrderByDescending(x => x.Count).Select(x => (valueConverter(x.Value), x.Frequency)));


            PlotModel model = new PlotModel();

            var drawResult = dataFrequencyAnalysis.GetResults().OrderByDescending(x => x.Count);

            var barSeries = new BarSeries() { ItemsSource = drawResult.Select(x => new BarItem(x.Frequency)), FillColor = color };
            var categoryAxis = new CategoryAxis() { ItemsSource = drawResult.Select(x => $"'{x.Value}'"), Position = AxisPosition.Left };
            model.Series.Add(barSeries);
            model.Axes.Add(categoryAxis);
            SetViewModel(model);
        }
        private async void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            if(_currentPlotView == null)
            {
                await PrefabsDialog.ShowErrorNotReult();
                return;
            }
            await PrefabsDialog.ViewWaitTask(() => FileSave.Save("png", _currentPlotView.SavePngAsync, "HistogramImage"), Consts.TitleFileSave);
        }
        private void FullOpen_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPlotView != null && Window.Current.Content is Frame frame)
                frame.Navigate(typeof(FullScreenHistogramView), this);
        }
        private void SetViewModel(PlotModel plotModel)
        {
            _currentPlotView = new PlotView()
            {
                Model = plotModel
            };
            HistogramPlotContainer.Content = _currentPlotView;
        }

        private class FullScreenHistogramView: Page
        {
            private HistogramView _histogramView;
            private PlotView _plotView;

            protected override void OnNavigatedTo(NavigationEventArgs e)
            {
                if (e.Parameter is HistogramView histogramView)
                {
                    _histogramView = histogramView;
                    Button btnBack = new Button()
                    {
                        VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top,
                        Style = (Style)Application.Current.Resources["NavigationBackButtonNormalStyle"]
                    };
                    btnBack.Click += Back_Click;
                    AppBarButton saveImage = new AppBarButton()
                    {
                        Icon = new SymbolIcon(Symbol.Camera)
                    };
                    saveImage.Click += SaveImage_Click1;
                    TopAppBar = new CommandBar()
                    {
                        Content = btnBack,
                        PrimaryCommands =
                        {
                            saveImage
                        }
                    };
                    _histogramView._currentPlotView.Model.AttachToView(null);
                    _plotView = new PlotView()
                    {
                        Model = _histogramView._currentPlotView.Model
                    };
                    Content = _plotView;
                }
                else
                    throw new Exception("Неожиданное поведение");
            }

            private async void SaveImage_Click1(object sender, RoutedEventArgs e) => await PrefabsDialog.ViewWaitTask(() => FileSave.Save("png", _plotView.SavePngAsync, "HistogramImage"), Consts.TitleFileSave);

            private void Back_Click(object sender, RoutedEventArgs e)
            {
                _histogramView._currentPlotView.Model.AttachToView(null);
                _histogramView.SetViewModel(_histogramView._currentPlotView.Model);
                Frame.GoBack();
            }
        }
    }
}
