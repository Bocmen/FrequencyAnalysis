using FrequencyAnalysisApp.Model;
using FrequencyAnalysisApp.Views;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FrequencyAnalysisApp.Extension
{
    public static class HistogramViewExtension
    {
        public delegate OxyColor GetColor<T>(FrequencyAnalysisNode<T> node);

        //public static void Draw<T>(this HistogramView histogramView, FrequencyAnalysis<T> frequencyAnalysis, string title, GetColor<T> getColor = null, Func<FrequencyAnalysisNode<T>, double> order = null)
        //{
        //    getColor = getColor ?? ((_) => OxyColors.DarkGreen);
        //    var dataDrav = (order == null ? frequencyAnalysis.GetResults() : frequencyAnalysis.GetResults().OrderByDescending(order)).Select(x => new HistogramView.NodeHistogram(x.Value.ToString(), getColor(x), x.Frequency));
        //    histogramView.Draw(dataDrav, $"{title}\nЭнтропия[{Math.Round(frequencyAnalysis.MinInformationMeasure, 2)}], Кол-во значений[{frequencyAnalysis.CountValues}], из них уникальных[{frequencyAnalysis.CountUniqle}]");
        //}

        public static void Draw<T>(this HistogramView histogramView, FrequencyAnalysis<T> frequencyAnalysis, string title, SortValue sortValue = SortValue.Default, SortType sortType = SortType.Ascending, IEnumerable<T> additionalElements = null, GetColor<T> getColor = null)
        {
            getColor = getColor ?? ((_) => OxyColors.DarkGreen);
            var dataHistogram = frequencyAnalysis.GetResults();
            var additionalElementsCast = additionalElements?.Where(x => !dataHistogram.Any(y => x.Equals(y.Value))).Select(x => new FrequencyAnalysisNode<T>(x, 0, 0)).ToList(); // Bug без ToList() возникает 'Access violation'??
            if (additionalElementsCast != null && additionalElementsCast.Any())
                dataHistogram = dataHistogram.Concat(additionalElementsCast);
            Func<FrequencyAnalysisNode<T>, object> order = null;
            if (sortValue == SortValue.Value)
                order = (node) => node.Value;
            else
                order = (node) => node.Frequency;
            switch (sortType)
            {
                case SortType.Ascending:
                    dataHistogram = dataHistogram.OrderBy(order);
                    break;
                case SortType.Descending:
                    dataHistogram = dataHistogram.OrderByDescending(order);
                    break;
            }
            var t = dataHistogram.ToList();
            histogramView.Draw(dataHistogram.Select(x => new HistogramView.NodeHistogram(x.Value.ToString(), getColor(x), x.Frequency)), $"{title}\nЭнтропия[{Math.Round(frequencyAnalysis.MinInformationMeasure, 2)}], Кол-во значений[{frequencyAnalysis.CountValues}], из них уникальных[{frequencyAnalysis.CountUniqle}]");
        }

        public enum SortType : byte
        {
            Ignore,
            Ascending,
            Descending,
        }
        public enum SortValue : byte
        {
            Default,
            Value,
            Frequency
        }
    }
}
