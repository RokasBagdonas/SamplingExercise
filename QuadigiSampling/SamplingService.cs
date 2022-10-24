using QuadigiSampling.Models;

namespace QuadigiSampling
{
    public class SamplingService : ISamplingService
    {
        /// <summary>
        /// In seconds
        /// </summary>
        public readonly TimeSpan samplingInterval;
        public DateTime StartOfSampling
        {
            get;
            private set;
        }

        public SamplingService(DateTime startOfSampling, int samplingInterval = 300)
        {
            if (samplingInterval <= 0) throw new ArgumentException("sampling interval has to be a positive number");
            this.samplingInterval = new TimeSpan(0, 0, samplingInterval);
            this.StartOfSampling = startOfSampling;
        }

        public Dictionary<MeasurementType, List<Measurement>> Sample(List<Measurement> unsampledMeasurements)
        {
            //1. order by time and group by type
            var groupedByTypeAndOrderedByTime = unsampledMeasurements
                .Where(m => isValidMeasurement(m))
                .OrderBy(m => m.MeasurementTime)
                .GroupBy(m => m.MeasurementType);

            //2. for each measurement type, group by interval and pick last.
            var result = new List<Measurement>();
            foreach(var group in groupedByTypeAndOrderedByTime)
            {
                var a = group
                    .GroupBy(m => getInterval(m))
                    .Select(m => m.Last());

                result.AddRange(a);
            }

            var final = result
                .GroupBy(m => m.MeasurementType)
                .ToDictionary(group => group.Key, group => group.ToList());

            return final;
        }

        private long getInterval(Measurement m)
        {
            var modulusResult = m.MeasurementTime.Ticks / samplingInterval.Ticks;
            if (m.MeasurementTime.Ticks % samplingInterval.Ticks == 0 && m.MeasurementTime.Ticks != 0)
                return modulusResult - 1;
            return modulusResult;
        }

        private bool isValidMeasurement(Measurement measurement)
        {
            return measurement.MeasurementTime >= StartOfSampling;
        }
       
    }
}
