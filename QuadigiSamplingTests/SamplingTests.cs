using QuadigiSampling;
using QuadigiSampling.Models;
using System;
using System.Collections.Generic;
using Xunit;
using Moq;
using FluentAssertions;

namespace QuadigiSamplingTests
{
    public class SamplingTests
    {
        private DateTime startOfSampling = default(DateTime);
        private List<Measurement> unsampledMeassurements;
        private SamplingService samplingService;

        public SamplingTests()
        {
            samplingService = new SamplingService(startOfSampling, 300);
        }

        [Fact]
        public void NoSamples_Returns_Empty()
        {
            startOfSampling = DateTime.Now;
            unsampledMeassurements = new List<Measurement>();
            Dictionary<MeasurementType, List<Measurement>> result = samplingService.Sample(unsampledMeassurements);
            Assert.Empty(result);
        }

        [Fact]
        public void OneMeasurement_AtStartOfSampling()
        {
            //arrange
            startOfSampling = new DateTime(2020, 03, 04);
            var measurement = new Measurement(startOfSampling, It.IsAny<double>(), It.IsAny<MeasurementType>());
            unsampledMeassurements = new List<Measurement>
            {
                measurement
            };

            //act 
            var result = samplingService.Sample(unsampledMeassurements);

            //assert 
            result.Should().HaveCount(1);
            result.Should().ContainKey(measurement.MeasurementType)
                .WhoseValue[0].Should().BeEquivalentTo(measurement);
        }

        [Fact]
        public void OneMeasurement_BeforeStartOfSampling_Result_Empty()
        {
            var soP = new DateTime(1918, 01, 02);
            samplingService =  new SamplingService(soP);
            var measurement = new Measurement(samplingService.StartOfSampling.AddSeconds(-1), It.IsAny<double>(), It.IsAny<MeasurementType>());
            unsampledMeassurements = new List<Measurement> { measurement };

            var result = samplingService.Sample(unsampledMeassurements);

            result.Should().BeEmpty();
        }

        [Fact]
        public void SameType_Two_Measurements_In_Same_SamplingInterval_Newer_Taken()
        {
            startOfSampling = new DateTime(1963, 01, 25);
            var measurement1 = new Measurement(startOfSampling.AddSeconds(1), It.IsAny<double>(), MeasurementType.Temperature);
            var measurement2 = new Measurement(startOfSampling.AddSeconds(samplingService.samplingInterval.TotalSeconds - 1), It.IsAny<double>(), measurement1.MeasurementType);
            unsampledMeassurements = new List<Measurement> { measurement1, measurement2 };

            var result = samplingService.Sample(unsampledMeassurements);

            result.Should().HaveCount(1);
            result.Should()
                .ContainKey(measurement1.MeasurementType)
                .WhoseValue[0].Should().Be(measurement2);
        }


        [Fact]
        public void TwoTypesOfMeasurements()
        {
            var measurement1 = new Measurement(startOfSampling, It.IsAny<double>(), MeasurementType.Temperature);
            var measurement2 = new Measurement(startOfSampling, It.IsAny<double>(), MeasurementType.HeartRate);

            var unsampledMeasurements = new List<Measurement>() { measurement1, measurement2 };
            var result = samplingService.Sample(unsampledMeasurements);

            result.Should().HaveCount(2);
            result.Should()
                .ContainKey(measurement1.MeasurementType)
                .WhoseValue[0].Should().BeEquivalentTo(measurement1);

            result.Should()
                .ContainKey(measurement2.MeasurementType)
                .WhoseValue[0].Should().BeEquivalentTo(measurement2);
        }


        [Fact]
        public void SameType_ThreeMeasurements_TwoInSameInterval()
        {
            var interval1 = startOfSampling;
            var interval2 = startOfSampling.AddSeconds(samplingService.samplingInterval.TotalSeconds + 1); 
            var measurement1 = new Measurement(interval1, It.IsAny<double>(), MeasurementType.Temperature);
            var measurement2 = new Measurement(interval2, It.IsAny<double>(), MeasurementType.Temperature);
            var measurement3 = new Measurement(interval1, It.IsAny<double>(), MeasurementType.Temperature);

            var unsampledMeasurements = new List<Measurement>() { measurement1, measurement2, measurement3 };
            var result = samplingService.Sample(unsampledMeasurements);

            result.Should().HaveCount(1);
            
            result.Should()
                .ContainKey(measurement1.MeasurementType)
                .WhoseValue.Count.Should().Be(2);

            var resultingList = result[MeasurementType.Temperature];

            resultingList.Should()
                .ContainInOrder(measurement3, measurement2);
        }

        [Fact]
        public void SameType_TwoMeasurements_SecondAtTheIntervalEnd()
        {
            var measurement1 = new Measurement(startOfSampling, It.IsAny<double>(), MeasurementType.Temperature);
            var secondMeasurementTime = startOfSampling.AddSeconds(samplingService.samplingInterval.TotalSeconds);
            var measurement2 = new Measurement(secondMeasurementTime, It.IsAny<double>(), MeasurementType.Temperature);

            var unsampledMeasurements = new List<Measurement>() { measurement1, measurement2 };
            var result = samplingService.Sample(unsampledMeasurements);

            result.Should().HaveCount(1);

            result.Should()
                .ContainKey(measurement1.MeasurementType)
                .WhoseValue[0].Should().Be(measurement2);
        }

        [Fact]
        public void SameType_ThreeMeasurements_EachDifferentIntervals_Ordered_By_Time_Ascending()
        {
            var measurement1 = new Measurement(startOfSampling.AddSeconds(samplingService.samplingInterval.TotalSeconds*2 + 1), It.IsAny<double>(), MeasurementType.Temperature);
            var measurement2 = new Measurement(startOfSampling, It.IsAny<double>(), MeasurementType.Temperature);
            var measurement3 = new Measurement(startOfSampling.AddSeconds(samplingService.samplingInterval.TotalSeconds + 1), It.IsAny<double>(), MeasurementType.Temperature);

            var unsampledMeasurements = new List<Measurement>() { measurement1, measurement2, measurement3 };
            var result = samplingService.Sample(unsampledMeasurements);

            result.Should().HaveCount(1);

            var resultList = result[measurement1.MeasurementType];
            resultList.Should().HaveCount(3);
            resultList.Should().ContainInOrder(measurement2, measurement3, measurement1);
        }


        [Fact]
        public void Exercise_Example()
        {
            startOfSampling = new DateTime(2017, 01, 03, 10, 00, 00);
            var m1 = new Measurement(new DateTime(2017, 01, 03, 10, 04, 45), 35.79, MeasurementType.Temperature);
            var m2 = new Measurement(new DateTime(2017, 01, 03, 10, 01, 18), 98.78, MeasurementType.SpO2);
            var m3 = new Measurement(new DateTime(2017, 01, 03, 10, 09, 07), 35.01, MeasurementType.Temperature);
            var m4 = new Measurement(new DateTime(2017, 01, 03, 10, 03, 34), 96.49, MeasurementType.SpO2);
            var m5 = new Measurement(new DateTime(2017, 01, 03, 10, 02, 01), 35.82, MeasurementType.Temperature);
            var m6 = new Measurement(new DateTime(2017, 01, 03, 10, 05, 00), 97.17, MeasurementType.SpO2);
            var m7 = new Measurement(new DateTime(2017, 01, 03, 10, 05, 01), 95.08, MeasurementType.SpO2);
           
            var unsampledMeasurements = new List<Measurement>()
            {
                m1, m2, m3, m4, m5, m6, m7
            };
            var result = samplingService.Sample(unsampledMeasurements);

            result.Should().HaveCount(2);
            // ordered
            var tempResults = result[m1.MeasurementType];
            tempResults.Should().HaveCount(2)
                .And.ContainInOrder(m1, m3);

            var spo2Results = result[m2.MeasurementType];
            spo2Results.Should().HaveCount(2)
                .And.ContainInOrder(m6, m7);
        }


    }
}