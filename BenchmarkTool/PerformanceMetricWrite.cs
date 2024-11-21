using System;
using BenchmarkTool;
namespace BenchmarkTool
{
    public class PerformanceMetricWrite : PerformanceMetricBase<LogRecordWrite>
    {

        public PerformanceMetricWrite(double latency, long succeededDataPoints,
                                     long failedDataPoints, Operation operation)
                : base(latency, succeededDataPoints, failedDataPoints, operation)
        { }


        public override LogRecordWrite ToLogRecord(string mode, int percentage, long timestamp, long iterationTimestamp, DateTime startDate, int batchSize,
                                                    int clientsNb, int sensorNb,
                                                    int client, int iteration, int dimNb, bool regularity)
        {
            return new LogRecordWrite(Latency, ClientLatency, SucceededDataPoints, timestamp, iterationTimestamp, startDate,
                                    FailedDataPoints, PerformedOperation, mode, percentage, clientsNb,
                                    batchSize, sensorNb, client, iteration, dimNb, regularity);
        }
        public LogRecordWrite ToLogRecord(string mode, int percentage, long timestamp, long iterationTimestamp, DateTime startDate, int batchSize,
                                                            int clientsNb, int sensorNb,
                                                            int client, int iteration, int dimNb,
        int ConsecutiveTimeBatchesIterations, bool patchwork, bool regularity)
        {
            return new LogRecordWrite(Latency, ClientLatency, SucceededDataPoints, timestamp, iterationTimestamp, startDate,
                                    FailedDataPoints, PerformedOperation, mode, percentage, clientsNb,
                                    batchSize, sensorNb, client, iteration, dimNb,
  ConsecutiveTimeBatchesIterations, patchwork, regularity);
        }
    }
}
