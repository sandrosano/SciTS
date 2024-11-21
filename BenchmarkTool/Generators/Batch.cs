using System.Collections.Generic;

namespace BenchmarkTool.Generators
{
    // BatchSize means: Amount of DataPoints per Batch, therefore: If multiple dimensions, still 1 datapoint. -» increasing dimensions does not increase batchSizes
    public class Batch
    {
        public Batch()
        {
            RecordsList = new List<IRecord>();
        }

        public Batch(int size)// ROW-based   
        {
            Size = size;

            RecordsArray = new IRecord[size];
        }
        public Batch(int size, int NR_of_dimensions_as_separate_vectors, int sensorAmountofClient)// Vector-based  // In case of DatalayertsDBasVect / FTPnoDBasVect: dimensions*sensorAmountofClient
        {
            Size = size; // multiplied the RecordsArray.Lenght with length of internal arrays gives "Size"

            RecordsArray = new IRecord[   NR_of_dimensions_as_separate_vectors * sensorAmountofClient];
        }
        public List<IRecord> RecordsList { get; set; } // for  outdated generator
        public IRecord[] RecordsArray { get; set; }
        public int Size { get; set; }
    }
}
