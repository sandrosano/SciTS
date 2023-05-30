using InfluxDB.Client.Core;
using System;

namespace BenchmarkTool.Generators
{
    public class RecordVictoriametrics : IRecord
    {
        public int SensorID { get; set; }
<<<<<<< HEAD
        public bool polyDim { get; }

        public float Value { get; set; }
=======
        // public bool polyDim { get; }

        // public float Value { get; set; }
>>>>>>> 7455ced (changes in dumy db)
        public float[] ValuesArray { get; set; }
         public DateTime Time { get; set; }
        float getFirstValue()
        {
<<<<<<< HEAD
            return ValuesArray[1];
=======
            return ValuesArray[0];
>>>>>>> 7455ced (changes in dumy db)
        }
        public RecordVictoriametrics(int sensorId, DateTime timestamp, float value)
        {
            SensorID = sensorId;
            Time = TimeZoneInfo.ConvertTimeToUtc(timestamp);
<<<<<<< HEAD
            Value = value;
=======
            // Value = value;
                        ValuesArray[0] = value;

>>>>>>> 7455ced (changes in dumy db)
        }
        public RecordVictoriametrics(int sensorId, DateTime timestamp, float[] values)
        {
            SensorID = sensorId;
            Time = TimeZoneInfo.ConvertTimeToUtc(timestamp);
            ValuesArray = values;
        }
    }

}
