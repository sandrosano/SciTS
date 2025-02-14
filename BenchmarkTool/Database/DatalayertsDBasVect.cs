using Clapsode.DataLayerTS;
using Clapsode.DataLayerTS.Models;
using Serilog;
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkTool.Queries;
using BenchmarkTool.Generators;
using System.Diagnostics;
using BenchmarkTool.Database.Queries;
using MemoryPack;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace BenchmarkTool.Database
{

    public class DatalayertsDBasVect : IDatabase
    {

        private static ReusableClient _client = new ReusableClient(Config.GetDatalayertsConnection(), Config.GetDatalayertsUser(), Config.GetDatalayertsPassword()){UseCompression=false, IngestionBatchSize=256};

        public IQuery<ContainerRequest> _iquery;

        private int _aggInterval;

        public void Init()
        {
            // _client =  habe ich als statisch deklariert
            _iquery = new DatalayertsQuery();
            _aggInterval = Config.GetAggregationInterval();
        }
        public void CheckOrCreateTable()
        {
            //not needed, DLTS creates automaticly.
        }

        public void Cleanup() { }

        public void Close() { }

        public async Task<QueryStatusWrite> WriteBatch(Batch batch)
        {

            try
            {
                VectorContainer<double> vectorContainer;
                TimeSeriesPoint<double> timeSeriesPoint;
                IEnumerable<TimeSeriesPoint<double>> pointContainer;

                if (Config.GetIngestionType() == "irregular")
                {

                   throw new NotImplementedException("Use DatalayertsDB (not ..asVect Class)");

                }
                else //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                {

                    var firsttime = batch.RecordsArray.First().Time;
                    DateTime roundedDate = new DateTime(firsttime.Year, firsttime.Month, firsttime.Day, firsttime.Hour, firsttime.Minute, firsttime.Second, DateTimeKind.Utc);
                    int dataDims = Config.GetDataDimensionsNr();
                    int anzahlTimestepsPerDimSensor = batch.RecordsArray.First().ValuesArray.Length;
                    

                    vectorContainer = new VectorContainer<double>()
                    {
                        FirstTimestamp = roundedDate,
                        IntervalTicks = 10000 * Config.GetRegularTsScaleMilliseconds(), // second = 10mil
                        LastTimestamp = roundedDate.AddMilliseconds(anzahlTimestepsPerDimSensor * Config.GetRegularTsScaleMilliseconds()-1000)
                    };

                    vectorContainer.Vectors = new TimeSeriesVector<double>[batch.RecordsArray.Length];

                    {
                        int vectorID = 0;
                        foreach (var currentVector in batch.RecordsArray)
                        {




                            vectorContainer.Vectors[vectorID] = new TimeSeriesVector<double>();

                            vectorContainer.Vectors[vectorID].Directory = GetDirectoryName();
 
                                vectorContainer.Vectors[vectorID].Series = "sensor_id_" + currentVector.SensorID + $"_{Constants.Value}_" + currentVector.GetFirstValue();
                     
                            vectorContainer.Vectors[vectorID].Values = currentVector.ValuesArray;
                            vectorID++;
                        }
                    ;

                    }
                }



// {{{

//          var bytes = JsonSerializer.SerializeToUtf8Bytes(vectorContainer);

    

//                 var name = "./test.json";
//     using (FileStream fs = File.Create(name))
//                                                 {
                                           
//                                                     byte[] info = bytes; // new UTF8Encoding(true).GetBytes(vectorContainer);
//                                                     fs.Write(info, 0, info.Length);
//                                                 }

// }}}



                Stopwatch sw2 = Stopwatch.StartNew();
                await _client.IngestVectorsAsync<double>(vectorContainer, OverwriteMode.older, TimeSeriesCreationTimestampStorageType.NONE, default).ConfigureAwait(false);
                sw2.Stop();

                return new QueryStatusWrite(true, new PerformanceMetricWrite(sw2.Elapsed.TotalMicroseconds, batch.Size, 0, Operation.BatchIngestion));

            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to insert batch into DatalayerTS. Exception: {0}", ex.ToString()));
                return new QueryStatusWrite(false, 0, new PerformanceMetricWrite(0, 0, batch.Size, Operation.BatchIngestion), ex, ex.ToString());
            }

        }

        public Task<QueryStatusWrite> WriteRecord(IRecord record)
        {
            throw new NotImplementedException();
        }

        public async Task<QueryStatusRead> RangeQueryRaw(RangeQuery query)
        {
            try
            {

                var DltsQuery = _iquery.RangeRaw;

                DltsQuery.FirstTimestamp = DateTime.SpecifyKind(query.StartDate, DateTimeKind.Utc);

                DltsQuery.LastTimestamp = DateTime.SpecifyKind(query.EndDate, DateTimeKind.Utc);

                string dir = GetDirectoryName();
                string[] series = GetSeriesNames(query.SensorIDs, new int[1] { 0 });
                DltsQuery.Selection.Add(dir, series);

                Stopwatch sw = Stopwatch.StartNew();
                var readResult = await _client.RetrieveVectorsAsync<double>(DltsQuery, true, true).ConfigureAwait(false);
                var points = 0;
    points = readResult.Vectors.Length * readResult.Vectors.First().Values.Length;                _aggInterval = 0;
                sw.Stop();
                // await Print(readResult, query.ToString(), Config.GetPrintModeEnabled());

                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.Elapsed.TotalMicroseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawData));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Range Query Raw Data. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawData), ex, ex.ToString());
            }
        }
        public async Task<QueryStatusRead> RangeQueryRawAllDims(RangeQuery query)
        {
            try
            {
                var DltsQuery = _iquery.RangeRawAllDims;

                DltsQuery.FirstTimestamp = DateTime.SpecifyKind(query.StartDate, DateTimeKind.Utc);

                DltsQuery.LastTimestamp = DateTime.SpecifyKind(query.EndDate, DateTimeKind.Utc);

                string dir = GetDirectoryName();
                string[] series = GetSeriesNames(query.SensorIDs);
                DltsQuery.Selection.Add(dir, series);



{{{

         var bytes = JsonSerializer.SerializeToUtf8Bytes(DltsQuery);

    

                var name = "./test.json";
    using (FileStream fs = File.Create(name))
                                                {
                                           
                                                    byte[] info = bytes; // new UTF8Encoding(true).GetBytes(vectorContainer);
                                                    fs.Write(info, 0, info.Length);
                                                }

}}}
                Stopwatch sw = Stopwatch.StartNew();
                var readResult = await _client.RetrieveVectorsAsync<double>(DltsQuery, true, true).ConfigureAwait(false);
                var points = 0;

                _aggInterval = 0;
                sw.Stop();

 

    points = readResult.Vectors.Length * readResult.Vectors.First().Values.Length;                // await Print(readResult, query.ToString(), Config.GetPrintModeEnabled());

                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.Elapsed.TotalMicroseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawAllDimsData));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Range Query Raw alld Data. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawAllDimsData), ex, ex.ToString());
            }
        }
        public async Task<QueryStatusRead> RangeQueryRawLimited(RangeQuery query, int limit)
        {
            try
            {

                var DltsQuery = _iquery.RangeRawLimited;

                DltsQuery.FirstTimestamp = DateTime.SpecifyKind(query.StartDate, DateTimeKind.Utc);

                DltsQuery.LastTimestamp = DateTime.SpecifyKind(query.StartDate, DateTimeKind.Utc).AddMilliseconds(Config.GetRegularTsScaleMilliseconds() * limit);

                string dir = GetDirectoryName();
                string[] series = GetSeriesNames(query.SensorIDs, new int[1] { 0 });
                DltsQuery.Selection.Add(dir, series);

                Stopwatch sw = Stopwatch.StartNew();
                var readResult = await _client.RetrieveVectorsAsync<double>(DltsQuery, true, true).ConfigureAwait(false);
                var points = 0;
    points = readResult.Vectors.Length * readResult.Vectors.First().Values.Length;                _aggInterval = 0;
                sw.Stop();
                // await Print(readResult, query.ToString(), Config.GetPrintModeEnabled());

                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.Elapsed.TotalMicroseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawData));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Range Query Raw lim Data. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawData), ex, ex.ToString());
            }
        }
        public async Task<QueryStatusRead> RangeQueryRawAllDimsLimited(RangeQuery query, int limit)
        {
            try
            {
                var DltsQuery = _iquery.RangeRawAllDimsLimited;

                DltsQuery.FirstTimestamp = DateTime.SpecifyKind(query.StartDate, DateTimeKind.Utc);

                DltsQuery.LastTimestamp = DateTime.SpecifyKind(query.StartDate, DateTimeKind.Utc).AddMilliseconds(Config.GetRegularTsScaleMilliseconds() * limit);

                string dir = GetDirectoryName();
                string[] series = GetSeriesNames(query.SensorIDs);
                DltsQuery.Selection.Add(dir, series);

                Stopwatch sw = Stopwatch.StartNew();
                var readResult = await _client.RetrieveVectorsAsync<double>(DltsQuery, true, true).ConfigureAwait(false);
                var points = 0;
    points = readResult.Vectors.Length * readResult.Vectors.First().Values.Length;                _aggInterval = 0;
                sw.Stop();
                // await Print(readResult, query.ToString(), Config.GetPrintModeEnabled());

                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.Elapsed.TotalMicroseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawAllDimsData));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Range Query AllD lim Data. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawAllDimsData), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusRead> RangeQueryAgg(RangeQuery query)
        {
            try
            {
                var DltsQuery = _iquery.RangeAgg;

                DltsQuery.FirstTimestamp = DateTime.SpecifyKind(query.StartDate, DateTimeKind.Utc);
                DltsQuery.LastTimestamp = DateTime.SpecifyKind(query.EndDate, DateTimeKind.Utc);

                string dir = GetDirectoryName();
                string[] series = GetSeriesNames(query.SensorIDs.ToArray(),new int[1]{ 0});
                DltsQuery.Selection.Add(dir, series);

                Stopwatch sw = Stopwatch.StartNew();


{{{

         var bytes = JsonSerializer.SerializeToUtf8Bytes(DltsQuery);

    

                var name = "./test.json";
    using (FileStream fs = File.Create(name))
                                                {
                                           
                                                    byte[] info = bytes; // new UTF8Encoding(true).GetBytes(vectorContainer);
                                                    fs.Write(info, 0, info.Length);
                                                }

}}}



                var readResult = await _client.RetrieveVectorsAsync<double>(DltsQuery, true).ConfigureAwait(false);
                var points = 0;
                points = readResult.Vectors.Length * readResult.Vectors.First().Values.Length;
                _aggInterval = (int)Config.GetAggregationInterval();
                sw.Stop();
                // await Print(readResult, query.ToString(), Config.GetPrintModeEnabled());
                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.Elapsed.TotalMicroseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryAggData));
            }
            catch (Exception ex)
            { 
                Log.Error(String.Format("Failed to execute AGG Query    . Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryAggData), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusRead> OutOfRangeQuery(OORangeQuery query)
        {
            try
            {
                var DltsQuery = _iquery.OutOfRange;

                DltsQuery.FirstTimestamp = DateTime.SpecifyKind(query.StartDate, DateTimeKind.Utc);
                DltsQuery.LastTimestamp = DateTime.SpecifyKind(query.EndDate, DateTimeKind.Utc);

                string dir = GetDirectoryName();
                string[] series = GetSeriesNames(query.SensorID);

                DltsQuery.Selection.Add(dir, series);

                DltsQuery.Transformations.First().Function = FunctionType.FILTER;
                DltsQuery.Transformations.First().Min = query.MaxValue;
                DltsQuery.Transformations.First().Max = query.MinValue;

                Stopwatch sw = Stopwatch.StartNew();
                var points = 0;
                var readResult = await _client.RetrievePointsAsync<double>(DltsQuery, false, false, default).ConfigureAwait(false);

    points = readResult.Count();
                _aggInterval = 0;
                sw.Stop();

                // await Print(readResult, query.ToString(), Config.GetPrintModeEnabled());

                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.Elapsed.TotalMicroseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.OutOfRangeQuery));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute OORange Query  Data. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.OutOfRangeQuery), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusRead> AggregatedDifferenceQuery(ComparisonQuery query)
        {
            try
            {
                var DltsQuery = _iquery.AggDifference;

                DltsQuery.FirstTimestamp = DateTime.SpecifyKind(query.StartDate, DateTimeKind.Utc);
                DltsQuery.LastTimestamp = DateTime.SpecifyKind(query.EndDate, DateTimeKind.Utc);

                string dir = GetDirectoryName();
                string[] series = new string[] { GetSeriesNames(new int[1]{query.FirstSensorID}, new int[1]{0})[0], GetSeriesNames(new int[1]{query.SecondSensorID}, new int[1]{0})[0] };
                DltsQuery.Selection.Add(dir, series);

                Stopwatch sw = Stopwatch.StartNew();
                var readResult = await _client.RetrieveVectorsAsync<double>(DltsQuery, true).ConfigureAwait(false);
                var points = 0;
    points = readResult.Vectors.Length * readResult.Vectors.First().Values.Length;                _aggInterval = (int)Config.GetAggregationInterval();
                sw.Stop();
                // await Print(readResult, query.ToString(), Config.GetPrintModeEnabled());
                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.Elapsed.TotalMicroseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.DifferenceAggQuery));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Agg DiFF Query    . Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.DifferenceAggQuery), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusRead> StandardDevQuery(SpecificQuery query)
        {
            try
            {
                var DltsQuery = _iquery.StdDev;

                DltsQuery.FirstTimestamp = DateTime.SpecifyKind(query.StartDate, DateTimeKind.Utc);
                DltsQuery.LastTimestamp = DateTime.SpecifyKind(query.EndDate, DateTimeKind.Utc);

                string dir = GetDirectoryName();
                string[] series = GetSeriesNames(query.SensorID);
                DltsQuery.Selection.Add(dir, series);

                Stopwatch sw = Stopwatch.StartNew();
                var readResult = await _client.RetrieveVectorsAsync<double>(DltsQuery, true, default).ConfigureAwait(false);
                var points = 0;
    points = readResult.Vectors.Length * readResult.Vectors.First().Values.Length;                _aggInterval = (int)Config.GetAggregationInterval();
                sw.Stop();
                // await Print(readResult, query.ToString(), Config.GetPrintModeEnabled());

                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.Elapsed.TotalMicroseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.STDDevQuery));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Range Query Raw Data. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.STDDevQuery), ex, ex.ToString());
            }
        }

        private async void Print(VectorContainer<double> readResult, string type, bool enabled)
        {
            if (enabled == true)
            {
                for (int c = 0; c < readResult.Timestamps.Length; c++)
                {
                    for (int d = 0; d < readResult.Vectors.Length; d++)
                    {
                        if (readResult.Vectors[d].Values[c] > 0)
                            await Console.Out.WriteLineAsync(" read | " + readResult.Vectors[d].Values[c].ToString() + " at " + readResult.Timestamps[c].ToString() + "in: " + readResult.Vectors[d].Series + "from Query:| " + type);
                    }
                }
            }
        }

        private string GetDirectoryName()
        {

            return Config.GetPolyDimTableName() + "_in_" + Config.GetRegularTsScaleMilliseconds().ToString() + "_ms_steps";

        }

        private string[] GetSeriesNames(int SensorID)
        {
            int[] AllDim = new int[Config.GetDataDimensionsNr()];
            AllDim = Enumerable.Range(0, Config.GetDataDimensionsNr()).ToArray();

            return GetSeriesNames(new int[1] { SensorID }, AllDim);
        }
        private string GetSeriesNames(int SensorID, int dim)
        {
            return GetSeriesNames(new int[1] { SensorID }, new int[1] { dim }).First();
        }
        private string[] GetSeriesNames(int[] SensorIDs)
        {
            int[] AllDim = new int[Config.GetDataDimensionsNr()];
            AllDim = Enumerable.Range(0, Config.GetDataDimensionsNr()).ToArray();

            return GetSeriesNames(SensorIDs, AllDim);
        }

        private string[] GetSeriesNames(int[] SensorIDs, int[] dimensions)
        {
            string[] series;

            series = new String[Config.GetDataDimensionsNr() * Config.GetSensorNumber() + 1];
            foreach (int c in SensorIDs)
            {
                foreach (int d in dimensions)
                    series[c * Config.GetDataDimensionsNr() + d] = "sensor_id_" + c + $"_{Constants.Value}_" + d;
            }

            return series.Where(c => c != null).ToArray();
        }

    }
}