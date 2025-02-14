﻿using CsvHelper;
using Serilog;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using BenchmarkTool;

namespace BenchmarkTool
{
    public class PerformanceMetricRead : PerformanceMetricBase<LogRecordRead>
    {
        public DateTime StartDate { get; private set; }
        public long Duration { get; private set; }
        public int Aggregation { get; private set; }

        public PerformanceMetricRead(double latency,  long succeededDataPoints,
                                     long failedDataPoints, DateTime date,
                                     long duration, int aggregation, Operation operation)
             : base(latency, succeededDataPoints, failedDataPoints, operation)
        {
            Aggregation = aggregation;
            Duration = duration;
            StartDate = date;
        }

        public override LogRecordRead ToLogRecord(string mode,  int percentage,long timestamp ,long iterationTimestamp, DateTime startDate, int batchSize,
                                                int clientsNb, int sensorNb,
                                                int client, int iteration, int dimNb, bool regularity)
        {
            return new LogRecordRead(Latency,ClientLatency, SucceededDataPoints, timestamp,   iterationTimestamp,  sensorNb,
                                    FailedDataPoints, PerformedOperation, mode,   percentage,client, clientsNb,
                                    StartDate, Duration, Aggregation,iteration ,dimNb, regularity);
        }
    }
}
