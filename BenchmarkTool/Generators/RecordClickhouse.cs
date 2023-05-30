using System;
using System.Collections;

namespace BenchmarkTool.Generators
{
    public class RecordClickhouse : IRecord, IEnumerable
    {
        public int SensorID { get; set; }
<<<<<<< HEAD
        public float Value { get; set; }
=======
        // public float Value { get; set; }
>>>>>>> 7455ced (changes in dumy db)
        public float[] ValuesArray { get; set; }
        public DateTime Time { get; set; }

        public RecordClickhouse(int sensorId, DateTime timestamp, float value)
        {
            SensorID = sensorId;
            Time = timestamp;
<<<<<<< HEAD
            Value = value;
        }
public bool polyDim { get;  }

=======
            // Value = value;
            ValuesArray[0] = value;

        }
public bool polyDim { get;  }

>>>>>>> 7455ced (changes in dumy db)
        public RecordClickhouse(int sensorId, DateTime timestamp, float[] values)
        {
            SensorID = sensorId;
            Time = timestamp;
            ValuesArray = values;
        }
        float getFirstValue(){
<<<<<<< HEAD
            return ValuesArray[1];
=======
            return ValuesArray[0];
>>>>>>> 7455ced (changes in dumy db)
        }
        public IEnumerator GetEnumerator()
        {
            yield return SensorID;
<<<<<<< HEAD
            yield return Value;
=======
>>>>>>> 7455ced (changes in dumy db)
            yield return ValuesArray;
            yield return Time;
        }
    }
}
