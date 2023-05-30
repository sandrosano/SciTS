using System;
using System.Collections;

namespace BenchmarkTool.Generators
{
    public class RecordDatalayerts : IRecord, IEnumerable
    {
        public RecordDatalayerts(int sensorId, DateTime timestamp, float value)
        {
            SensorID = sensorId;
            Time = timestamp;
<<<<<<< HEAD
            Value = value;
=======
            // Value = value;
            ValuesArray[0] = value;
>>>>>>> 7455ced (changes in dumy db)
        }
        public RecordDatalayerts(int sensorId, DateTime timestamp, float[] values)
        {
            SensorID = sensorId;
            Time = timestamp;
            ValuesArray = values;
<<<<<<< HEAD
            polyDim = true;
        }
        float getFirstValue(){
            return ValuesArray[1];
        }
         public float[] ValuesArray { get; set; }
         public float Value { get; set; }
         public bool polyDim { get;  }
=======
            // polyDim = true;
        }
        float getFirstValue(){
            return ValuesArray[0];
        }
         public float[] ValuesArray { get; set; }
         public float Value { get; set; }
        //  public bool polyDim { get;  }
>>>>>>> 7455ced (changes in dumy db)

        public int SensorID { get; set; }

        public DateTime Time { get; set; }

        public IEnumerator GetEnumerator()
        {
            yield return SensorID;

            yield return Time;
<<<<<<< HEAD
            if (polyDim == true)
                yield return ValuesArray;
            else
                yield return Value;
=======
            // if (polyDim == true)
                yield return ValuesArray;
            // else
            //     yield return Value;
>>>>>>> 7455ced (changes in dumy db)
        }

    }
}
