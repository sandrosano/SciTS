 using System;
using System.Linq;
 
using System.Threading.Tasks;
using BenchmarkTool.Queries;
using BenchmarkTool.Generators;
using System.Diagnostics;
 using FluentFTP;
 using System.Text.Json;

namespace BenchmarkTool.Database
{
    public class FtpNoDB : IDatabase
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
            client.Config.ConnectTimeout = 100000;
            client.Config.DataConnectionConnectTimeout = 100000;
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

                var hash = batch.GetHashCode().ToString();


                var bytes = JsonSerializer.SerializeToUtf8Bytes(batch);

    

                var name = "./scits/"+BenchmarkTool.Program.Mode+"/row/"+Config._actualDataDimensionsNr+"D/test-row-"+BenchmarkTool.Program.Mode.ToString()+"-Dim" +
                Config._actualDataDimensionsNr.ToString()+"-bs"+
                batch.Size.ToString()+  "time"
                
                
                 + batch.RecordsArray.First().Time.ToFileTimeUtc() + "-plus-" + 
                ((int) (  batch.RecordsArray.Last().Time.ToFileTimeUtc() - batch.RecordsArray.First().Time.ToFileTimeUtc() ) /10000 ).ToString()
                
                 + "ms-S-"+batch.RecordsArray.First().SensorID.ToString() + "-"+batch.RecordsArray.Last().SensorID.ToString()  + ".json";

                await client.AutoConnect();
                Stopwatch sw = new Stopwatch();

                sw.Start();
                await client.UploadBytes(bytes, name,   FtpRemoteExists.Overwrite,    true);
                sw.Stop();


                return new QueryStatusWrite(true, new PerformanceMetricWrite(sw.Elapsed.TotalMicroseconds, bytes.Length / 8 * Config._actualDataDimensionsNr , 0, Operation.BatchIngestion)); // divided by 8 so that in the analysis the bytes show up correctly as MB, as all the other databastes measure batchsize in double float (64bit,8By) therefore the analysis multiplies  with 8, and then with the amount of values per Datapoint
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
