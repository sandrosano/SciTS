using System;
using System.Collections.Generic;


namespace BenchmarkTool.Generators
{
    public interface IDataGenerator
    {
        Batch GenerateBatch(int batchSize, int sensorStartId, decimal sensorsPerClient, int offset, int clientOffset, DateTime date);
<<<<<<< HEAD
        Batch GenerateBatch(int batchSize, List<int> sensorIdsForThisClientList, DateTime date, int dimensions);
=======
        Batch GenerateBatch(int batchSize, List<int> sensorIdsForThisClientList,   DateTime date);
        Batch GenerateBatch(int batchSize, List<int> sensorIdsForThisClientList,   DateTime date, int dimensions );
>>>>>>> 7455ced (changes in dumy db)

    }
}
