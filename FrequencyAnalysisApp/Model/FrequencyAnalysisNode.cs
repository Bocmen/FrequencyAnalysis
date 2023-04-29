namespace FrequencyAnalysisApp.Model
{
    public struct FrequencyAnalysisNode<T>
    {
        public T Value { get; private set; }
        public int Count { get; private set; }
        public double Frequency { get; private set; }

        public FrequencyAnalysisNode(T value, int count, double frequency)
        {
            Value=value;
            Count=count;
            Frequency=frequency;
        }
    }
}
