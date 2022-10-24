using QuadigiSampling.Models;

namespace QuadigiSampling
{
    public interface ISamplingService
    {
        Dictionary<MeasurementType, List<Measurement>> Sample(List<Measurement> unsampledMeasurements);
    }
}