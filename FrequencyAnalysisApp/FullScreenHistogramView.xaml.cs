using FrequencyAnalysisApp.Extension;
using FrequencyAnalysisApp.Tools;
using FrequencyAnalysisApp.Views;
using OxyPlot;
using OxyPlot.Windows;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FrequencyAnalysisApp
{
    public sealed partial class FullScreenHistogramView : Page
    {
        private DataInput _dataInput;
        private PlotView _plotView;

        public FullScreenHistogramView() => InitializeComponent();
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is DataInput dataInput)
            {
                _dataInput = dataInput;
                dataInput.PlotModel.AttachToView(null);
                _plotView = new PlotView()
                {
                    Model = dataInput.PlotModel
                };
                Content = _plotView;
            }
            else
                throw new Exception("Неожиданное поведение");
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _dataInput.PlotModel.AttachToView(null);
            _dataInput.HistogramView.Reload();
            Frame.GoBack();
        }

        private async void SaveImage_Click(object sender, RoutedEventArgs e) => await PrefabsDialog.ViewWaitTask(() => FileSave.Save("png", _plotView.SavePngAsync, "HistogramImage"), Consts.TitleFileSave);

        public struct DataInput
        {
            public HistogramView HistogramView { get; private set; }
            public PlotModel PlotModel { get; private set; }

            public DataInput(HistogramView histogramView, PlotModel plotModel)
            {
                HistogramView=histogramView;
                PlotModel=plotModel;
            }
        }
    }
}
