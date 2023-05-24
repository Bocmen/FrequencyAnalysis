using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FrequencyAnalysisApp.Model
{
    public class FrequencyAnalysis<T>: IEnumerable<T>
    {
        public int CountValues { get; private set; }
        public int CountUniqle { get; private set; }

        public double MinInformationMeasure => Math.Abs(_frequencyAnalysis.Select(x =>
        {
            double frequency = (double)x.Value / CountValues;
            return frequency * Math.Log(frequency, 2); 
        }).Sum());

        private readonly Dictionary<T, int> _frequencyAnalysis = new Dictionary<T, int>();

        public void AddValue(T value)
        {
            CountValues++;
            if (_frequencyAnalysis.ContainsKey(value))
                _frequencyAnalysis[value]++;
            else
            {
                _frequencyAnalysis.Add(value, 1);
                CountUniqle++;
            }
        }
        public bool TryGetFrequency(T value, out double frequency)
        {
            if (_frequencyAnalysis.TryGetValue(value, out int count))
            {
                frequency = (double)count / CountValues;
                return true;
            }
            frequency = double.NaN;
            return false;
        }
        public IEnumerable<FrequencyAnalysisNode<T>> GetResults() => _frequencyAnalysis.Select(x => new FrequencyAnalysisNode<T>(x.Key, x.Value, (double)x.Value / CountValues));

        public IEnumerator<T> GetEnumerator() => _frequencyAnalysis.Keys.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _frequencyAnalysis.Keys.GetEnumerator();
    }
}
