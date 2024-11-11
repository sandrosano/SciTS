using System.Collections.Generic;

namespace BenchmarkTool.Generators
{
    // BatchSize means: Amount of DataPoints per Batch, therefore: If multiple dimensions, still 1 datapoint. -» increasing dimensions does not increase batchSizes
    public class Batch
    {
        public Batch (){
            RecordsList = new List<IRecord>();
        }

        public Batch(int size) // In case of DatalayertsDirect: dimensions*sensorAmountofClient
        {
            Size = size;
            RecordsArray = new IRecord[size];
        }
        public List<IRecord> RecordsList { get; set; } // for  outdated generator
        public IRecord[] RecordsArray { get; set; }
        public int Size { get; set; }
    }
}
