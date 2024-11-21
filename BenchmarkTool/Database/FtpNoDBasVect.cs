using System;
using System.Linq;

using System.Text.Json;
using System.Threading.Tasks;
using BenchmarkTool.Queries;
using BenchmarkTool.Generators;
using System.Diagnostics;
using FluentFTP;
// using System.IO;
namespace BenchmarkTool.Database
{
    public class FtpNoDBasVect : IDatabase
    {
        private FluentFTP.AsyncFtpClient client;

        public void Cleanup()
        {
        }

        public void Close()
        {

        }

        public async void Init()
        {

            client = new AsyncFtpClient(Config.GetFTPConnection(), Config.GetFTPUser(), Config.GetFTPPassword());



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
            if (Config.GetIngestionType() == "irregular")
            {

                throw new NotImplementedException("Use FtpNoDB (not asVect Class)");

            }
            else
            {
                try
                {
                    var hash = batch.GetHashCode().ToString();


                    var bytes = JsonSerializer.SerializeToUtf8Bytes(batch);



                    var name = "./scits/vec/test-vector-" + BenchmarkTool.Program.Mode.ToString() + "-" +
                    Config._actualDataDimensionsNr.ToString() + "-bs" +
                    batch.Size.ToString() + "time"

                     + batch.RecordsArray.First().Time.ToFileTimeUtc() + "-plus-" +
                    (batch.RecordsArray.First().ValuesArray.Length * Config.GetRegularTsScaleMilliseconds() ).ToString()
                    + "ms-S-" + batch.RecordsArray.First().SensorID.ToString() + "-" + batch.RecordsArray.Last().SensorID.ToString() + ".json";

                    await client.AutoConnect();
                    Stopwatch sw = new Stopwatch();

                    sw.Start();
                    await client.UploadBytes(bytes, name,   FtpRemoteExists.Overwrite,    true);
                    sw.Stop();

                    return new QueryStatusWrite(true, new PerformanceMetricWrite(sw.Elapsed.TotalMicroseconds, batch.Size, 0, Operation.BatchIngestion));
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error(String.Format("Failed to insert batch into MySQL. Exception: {0}", ex.ToString()));
                    return new QueryStatusWrite(false, 0, new PerformanceMetricWrite(0, 0, batch.Size, Operation.BatchIngestion), ex, ex.ToString());
                }
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
