using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FrequencyAnalysisApp
{
    public sealed partial class MainPage : Page
    {
        public MainPage() => InitializeComponent();

        private void TextFrequencyAnalysis_Click(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(TextAnalysis));
        private void ImageFrequencyAnalysis_Click(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(ImageAnalysis));
    }
}
