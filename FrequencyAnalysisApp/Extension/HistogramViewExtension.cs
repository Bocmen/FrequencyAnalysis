using FrequencyAnalysisApp.Model;
using FrequencyAnalysisApp.Views;
using OxyPlot;
using System;
using System.Linq;

namespace FrequencyAnalysisApp.Extension
{
    public static class HistogramViewExtension
    {
        public static void Draw<T>(this HistogramView histogramView, FrequencyAnalysis<T> frequencyAnalysis, string title, Func<FrequencyAnalysisNode<T>, OxyColor> getColor = null, Func<FrequencyAnalysisNode<T>, double> order = null)
        {
            getColor = getColor ?? ((_) => OxyColors.DarkGreen);
            var dataDrav = (order == null ? frequencyAnalysis.GetResults() : frequencyAnalysis.GetResults().OrderByDescending(order)).Select(x => new HistogramView.NodeHistogram(x.Value.ToString(), getColor(x), x.Frequency));
            histogramView.Draw(dataDrav, $"{title}\nH(A)={Math.Round(frequencyAnalysis.MinInformationMeasure, 2)}");
        }
    }
}
