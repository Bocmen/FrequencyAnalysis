using OxyPlot;
using System.Linq;
using System.Reflection;

namespace FrequencyAnalysisApp.Extension
{
    public static class PlotModelExtensions
    {
        public static void AttachToView(this PlotModel plotModel, IPlotView plotView)
        {
            // Because of issue https://github.com/oxyplot/oxyplot/issues/497 
            // only one view can ever be attached to one plotmodel.
            // We have to force detach from any previous view and then attach to the new one.
            MethodInfo BaseAttachMethod = plotModel.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                .Where(methodInfo => methodInfo.IsFinal && methodInfo.IsPrivate)
                                .FirstOrDefault(methodInfo => methodInfo.Name.EndsWith(nameof(IPlotModel.AttachPlotView)));

            if (plotView != null && plotModel.PlotView != null && !Equals(plotView, plotModel.PlotView))
            {
                BaseAttachMethod.Invoke(plotModel, new object[] { null });
            }
            BaseAttachMethod.Invoke(plotModel, new object[] { plotView });
        }
    }
}
