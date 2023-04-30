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
using static FrequencyAnalysisApp.FullScreenHistogramView;

namespace FrequencyAnalysisApp.Views
{
    public sealed partial class HistogramView : UserControl
    {
        private PlotView _currentPlotView;

        public HistogramView() => InitializeComponent();

        public void Draw(IEnumerable<NodeHistogram> data, string title)
        {
            Title.Text = title ?? string.Empty;


            PlotModel model = new PlotModel();

            var barSeries = new ColumnSeries() { ItemsSource = data.Select(x => new ColumnItem(x.Value) { Color = x.Color }) };
            var categoryAxis = new CategoryAxis() { ItemsSource = data.Select(x => x.BarName), Position = AxisPosition.Bottom };
            model.Series.Add(barSeries);
            model.Axes.Add(categoryAxis);
            SetViewModel(model);
        }
        private async void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPlotView == null)
            {
                await PrefabsDialog.ShowErrorNotReult();
                return;
            }
            await PrefabsDialog.ViewWaitTask(() => FileSave.Save("png", _currentPlotView.SavePngAsync, "HistogramImage"), Consts.TitleFileSave);
        }
        private void FullOpen_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPlotView != null && Window.Current.Content is Frame frame)
                frame.Navigate(typeof(FullScreenHistogramView), new DataInput(this, _currentPlotView.Model));
        }
        private void SetViewModel(PlotModel plotModel)
        {
            _currentPlotView = new PlotView()
            {
                Model = plotModel
            };
            HistogramPlotContainer.Content = _currentPlotView;
        }
        internal void Reload() => SetViewModel(_currentPlotView.Model);

        public struct NodeHistogram
        {
            public string BarName { get; private set; }
            public OxyColor Color { get; private set; }
            public double Value { get; private set; }

            public NodeHistogram(string barName, OxyColor color, double value)
            {
                BarName=barName;
                Color=color;
                Value=value;
            }
        }
    }

}
