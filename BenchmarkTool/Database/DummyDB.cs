using MySqlConnector;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using BenchmarkTool.Queries;
using BenchmarkTool.Generators;
using System.Diagnostics;
using System.Globalization;

namespace BenchmarkTool.Database
{
    public class DummyDB : IDatabase
    {

        public void Cleanup()
        {
        }

        public void Close()
        {

        }

        public void Init()
        {

        }
        public void CheckOrCreateTable()
        {
    
        }

        public Task<QueryStatusRead> OutOfRangeQuery(OORangeQuery query)
        {
            throw new NotImplementedException();
        }

        public Task<QueryStatusRead> RangeQueryAgg(RangeQuery rangeQuery)
        {
            throw new NotImplementedException();
        }

        public Task<QueryStatusRead> StandardDevQuery(SpecificQuery query)
        {
            throw new NotImplementedException();
        }

        public Task<QueryStatusRead> AggregatedDifferenceQuery(ComparisonQuery query)
        {
            throw new NotImplementedException();
        }

        public Task<QueryStatusWrite> WriteRecord(IRecord record)
        {
            throw new NotImplementedException();
        }

        public async Task<QueryStatusWrite> WriteBatch(Batch batch)
        {
            try
            {


                Stopwatch sw = new Stopwatch();

                sw.Start();
                await File.AppendAllTextAsync("/tmp/dummy_empty_" + DateTime.Now.Day.ToString() + ".txt", " ");

                sw.Stop();

                return new QueryStatusWrite(true, new PerformanceMetricWrite(sw.Elapsed.TotalMicroseconds, batch.Size, 0, Operation.BatchIngestion));
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(String.Format("Failed to insert batch into MySQL. Exception: {0}", ex.ToString()));
                return new QueryStatusWrite(false, 0, new PerformanceMetricWrite(0, 0, batch.Size, Operation.BatchIngestion), ex, ex.ToString());
            }
        }
        public Task<QueryStatusRead> RangeQueryRaw(RangeQuery rangeQuery)
        {
            throw new NotImplementedException();
        }

        public Task<QueryStatusRead> RangeQueryRawAllDims(RangeQuery rangeQuery)
        {
            throw new NotImplementedException();
        }
        public Task<QueryStatusRead> RangeQueryRawLimited(RangeQuery rangeQuery, int limit)
        {
            throw new NotImplementedException();
        }

        public Task<QueryStatusRead> RangeQueryRawAllDimsLimited(RangeQuery rangeQuery, int limit)
        {
            throw new NotImplementedException();
        }
    }
}
