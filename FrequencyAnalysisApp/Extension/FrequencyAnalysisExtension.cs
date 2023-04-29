using FrequencyAnalysisApp.Model;
using System.Collections.Generic;

namespace FrequencyAnalysisApp.Extension
{
    public static class FrequencyAnalysisExtension
    {
        public static void AddValues<T>(this FrequencyAnalysis<T> frequencyAnalysis, IEnumerable<T> values)
        {
            foreach(var value in values)
                frequencyAnalysis.AddValue(value);
        }
    }
}
