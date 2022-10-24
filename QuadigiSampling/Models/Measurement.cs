using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuadigiSampling.Models
{
    public class Measurement
    {
        public DateTime MeasurementTime { get; }
        public double MeasurementValue { get; }
        public MeasurementType MeasurementType { get; }

        public Measurement(DateTime measurementTime, double measurementValue, MeasurementType measurementType)
        {
            this.MeasurementTime = measurementTime;
            this.MeasurementValue = measurementValue;
            this.MeasurementType = measurementType;
        }
    }
}
