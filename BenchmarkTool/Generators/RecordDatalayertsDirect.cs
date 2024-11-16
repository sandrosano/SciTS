using System;
using System.Collections;
using Clapsode.DataLayerTS.Models;

namespace BenchmarkTool.Generators
{
    public class RecordDatalayertsDirect : IRecord, IEnumerable
    {

        // Direct means: vectors are generated in extra Generator directly as arrays, so that in DB class no conversion needs to be done
        // Records means here: Channel 
        // Amount_of_Values is defined by batch size, as it is a stream of temporal values from T to T+1 ... T+BS
        // timestamp means always start-timestamp of Batch

        public RecordDatalayertsDirect(int sensorId, DateTime timestamp, double[] values)
        {
            SensorID = sensorId; DimID=0;
            Time = timestamp;
            ValuesArray = values;
        }
        public RecordDatalayertsDirect(int sensorId, int dimId, DateTime timestamp, double[] values)
        {
            SensorID = sensorId; DimID=dimId;
            Time = timestamp;
            ValuesArray = values;
        }
        public double GetFirstValue(){ // workaround to get the particular DIM which is not part of the interface, out of here. Logic of Batch-Class and IRecords is row-oriented, should be changed if more vector based logics than this one enter the benchmark.
            return DimID;
        }
         public double[] ValuesArray { get; set; }

        public int SensorID { get; set; }   
        public int DimID { get; set; }     

        public DateTime Time { get; set; }

        public IEnumerator GetEnumerator()
        {
            yield return SensorID;

            yield return Time;
            yield return ValuesArray;

        }

    }
}
