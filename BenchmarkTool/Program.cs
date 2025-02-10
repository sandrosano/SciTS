using System;
using System.Globalization;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;
using BenchmarkTool.System;
using System.Linq;
using System.Drawing.Text;
using MessagePack.Formatters;

namespace BenchmarkTool
{
    static class Program
    {

        public static string Mode { get; private set; }
        public static int _currentReadClientsNR { get; private set; }
        public static int _currentClientsNR { get; private set; }
        public static int _currentWriteClientsNR { get; private set; }
        public static int _currentWriteBatchSize { get; private set; }
        public static int _TestRetryIteration { get; private set; }
        public static int _currentlimit { get; private set; }


        static string GetNextTSPath()
        {
            string path = @"./.lastdate." +Config.GetTargetDatabase()+"."+ Config.GetPolyDimTableName() + ".scits";
            return path;
        }
        static async Task Main(string[] args)
        {
            try
            {

                Log.Logger = new LoggerConfiguration()
                            .MinimumLevel.Debug()
                            .WriteTo.File("ts-bench.log", restrictedToMinimumLevel: LogEventLevel.Information)
                            .CreateLogger();

                Console.WriteLine("Starting...");
                Log.Information("Application started");

                var action = args != null && args.Length > 0 ? args[0] : "read";
                string setting; string ingType; string patch;
                if (args.Length >= 2)
                {
                    action = args[0];
                    ingType = args[1];
                    Config.SetIngestionType(ingType);
                    if (args.Length >= 3)
                    {
                        setting = args[2];
                        Config.SetTargetDatabase(setting);
                        if (args.Length >= 4)
                        {
                            patch = args[3];
                            Config._PatchWorkArg = patch;

                        }
                    }

                }
                switch (action)
                {
                    case "populate":
                        Mode = "populate_Day+0_" + Config.GetIngestionType();
                        await PopulateOneDayRegularData(0);
                        break;
                    case var s when action.Contains("populate+"):
                        int i1 = s.IndexOf("+") + 1;
                        int i2 = s.Length;
                        int day = int.Parse(s.Substring(i1, i2 - i1));
                        Mode = "populate_Day+" + day + "_" + Config.GetIngestionType();
                        await PopulateOneDayRegularData(day);
                        break;

                    case var s when action.Contains("populateShort+"):
                        int j1 = s.IndexOf("+") + 1;
                        int j2 = s.Length;
                        double hours = double.Parse(s.Substring(j1, j2 - j1));
                        Mode = "populateShort_Hours+" + hours + "_" + Config.GetIngestionType();
                        await PopulateRegularData(0, hours);
                        break;



                    case "read":
                        Mode = "dedicated_" + Config.GetIngestionType();
                        await BenchmarkReadData();
                        break;
                    case "write":
                        Mode = "dedicated_" + Config.GetIngestionType();
                        await Batching();
                        break;

                    case "consecutive":
                        Mode = "dedicated_" + Config.GetIngestionType();
                        await Batching();
                        await BenchmarkReadData();
                        break;
                    case "mixed":
                        Mode = "mixed-LimitedQueries_" + Config.GetIngestionType();
                        await MixedWL();

                        Mode = "mixed-AggQueries_" + Config.GetIngestionType();
                        await MixedWL();
                        break;




                    case "mixed-LimitedQueries":
                        Mode = "mixed-LimitedQueries_" + Config.GetIngestionType();
                        await MixedWL();
                        break;
                    case "mixed-AggQueries":
                        Mode = "mixed-AggQueries_" + Config.GetIngestionType();
                        await MixedWL();
                        break;
                    case var s when action.Contains("mixed-") && action.Contains("%LimitedQueries"):
                        int ii1 = s.IndexOf("-") + 1;
                        int ii2 = s.IndexOf("%");
                        string sub = s.Substring(ii1, ii2 - ii1);
                        Config._actualMixedWLPercentage = int.Parse(sub);
                        Mode = "mixed-" + sub + "%LimitedQueries_" + Config.GetIngestionType();
                        await MixedWL();
                        break;

                    default:
                        Console.WriteLine($"Unknown option {action}");
                        break;
                }
                Log.Information("Completed...");
                Console.WriteLine("Completed...");
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private async static Task<QueryStatusWrite> RunIngestionTask(ClientWrite client)
        {
            return await client.RunIngestion(_TestRetryIteration);
        }

        private async static Task<QueryStatusRead> RunReadTask(ClientRead client)
        {
            return await client.RunQuery(_TestRetryIteration, _currentReadClientsNR, _currentlimit);
        }

        private async static Task PopulateOneDayRegularData(int dayAfterStartdate)
        {
            await PopulateRegularData(dayAfterStartdate, 24);
        }
        private async static Task PopulateRegularData(int dayAfterStartdate, double hours)
        {
            var init = Config.GetQueryType(); // Just for Init the Array

            int totalClientsNb = Config.GetClientNumberOptions().Last();
            int batchSize = Config.GetSensorNumber() * 60 * (1000 / Config.GetRegularTsScaleMilliseconds()) / totalClientsNb; // one minute ingestion
            DateTime currentbatchdate = Config.GetStartTime();
            foreach (var dimNb in Config.GetDataDimensionsNrOptions())
            {
                int minute = 0;
                Config._actualDataDimensionsNr = dimNb;

                while (minute < hours * 60) // if not all day ingested, continue
                {
                    long iterationTimestamp = Helper.GetNanoEpoch();
                    var clientArray = new ClientWrite[totalClientsNb];
                    for (var chosenClientIndex = 1; chosenClientIndex <= totalClientsNb; chosenClientIndex++)
                    {
                        currentbatchdate = Config.GetStartTime().AddDays(dayAfterStartdate).AddMinutes(minute);
                        clientArray[chosenClientIndex - 1] = new ClientWrite(iterationTimestamp, chosenClientIndex, totalClientsNb, Config.GetSensorNumber(), batchSize, dimNb, currentbatchdate);
                    }
                    minute++;


                    var glancesW = new GlancesStarter(Operation.BatchIngestion, totalClientsNb, batchSize, Config.GetSensorNumber(), iterationTimestamp, Config.GetGlancesOutput() + "-W");
                    var resultsW = new QueryStatusWrite[totalClientsNb];
                    await Parallel.ForEachAsync(Enumerable.Range(0, totalClientsNb), new ParallelOptions() { MaxDegreeOfParallelism = totalClientsNb }, async (index, token) => { resultsW[index] = await RunIngestionTask(clientArray[index]).ConfigureAwait(false); }).ConfigureAwait(false);
                    await glancesW.EndMonitorAsync().ConfigureAwait(false);

                    using (var csvLoggerW = new CsvLogger<LogRecordWrite>("write"))
                        foreach (var result in resultsW)
                        {
                            var recordW = result.PerformanceMetric.ToLogRecord(Mode, 0,
                                result.Timestamp, result.IterationTimestamp, result.StartDate, batchSize, totalClientsNb, Config.GetSensorNumber(),
                                result.Client, result.Iteration, dimNb, minute, false, Config.GetIsRegular());
                            csvLoggerW.WriteRecord(recordW);
                        }

                    var nextdate = currentbatchdate.AddMinutes(1);
                    if (!File.Exists(GetNextTSPath()))
                    {

                        using (FileStream fs = File.Create(GetNextTSPath()))
                        {
                            string dataasstring = nextdate.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                                                CultureInfo.InvariantCulture); //your data
                            byte[] info = new UTF8Encoding(true).GetBytes(dataasstring);
                            fs.Write(info, 0, info.Length);
                        }
                    }
                    GC.Collect();
                }
            }
        }

        private async static Task MixedWL()
        {
            await CustomizableBenchMarkRun(true, true);
            Console.Out.WriteLine("Mixed-Reads-WritesCompleted");
        }


        private async static Task CustomizableBenchMarkRun(bool write, bool read)
        {
            try
            {
                var init = Config.GetQueryType(); // Just for Init the Array
                var date = Config.GetStartTime();
                DateTime nextdate;
                DateTime patchMeandate = Config.GetStartTime(); ;
                int[] clientNumberArray = Config.GetClientNumberOptions();
                int[] percentageArray = Config.GetMixedWLPercentageOptions();
                int[] batchSizeArray = Config.GetBatchSizeOptions();
                int[] dimNbArray = Config.GetDataDimensionsNrOptions();
                var daySpan = Config.GetDaySpan();
                int getTestRetries = Config.GetTestRetries();
                int getConsecutiveTimeBatchesIterations = Config.GetConsecutiveTimeBatchesIterations();
                int sensorsNb = Config.GetSensorNumber();

                if ((Mode.Contains("mixed") | Config.GetPatchWorkMode() == true | (read == true & write == false)))
                {// Various methods to shorten the benchmark, where not necessary

                    if (read == true & write == false) // we dont need so much reeds
                    {
                        getConsecutiveTimeBatchesIterations = (int)Math.Ceiling((double)getConsecutiveTimeBatchesIterations / 2); // prevents mixed WL from  taking too long as it is just additional measurement 1.5
                    }
                    else
                    {

                        getConsecutiveTimeBatchesIterations = (int)Math.Ceiling((double)getConsecutiveTimeBatchesIterations / 2); // prevents mixed WL from  taking too long as it is just additional measurement 1.5
                        getTestRetries = (int)Math.Ceiling((double)getTestRetries / Config.GetMixedWLPercentageOptions().Length);// prevents mixed WL from  taking too long as it will be executed with both agg and limited queries

                        if (dimNbArray.Length > 1)
                        {
                            dimNbArray = new int[2] { Config.GetDataDimensionsNrOptions().First(), Config.GetDataDimensionsNrOptions().Last() };
                        } // prevents mixed WL from taking too long
                        if (batchSizeArray.Length > 4)
                        {
                            batchSizeArray = new int[4] { batchSizeArray.First(),batchSizeArray[1], batchSizeArray[batchSizeArray.Length / 2], batchSizeArray.Last() };//small batches are more interesting
                        }
                        if (clientNumberArray.Length > 4)
                        {
                            clientNumberArray = new int[4] { clientNumberArray.First(), clientNumberArray[clientNumberArray.Length / 2],clientNumberArray[clientNumberArray.Length-1], clientNumberArray.Last() }; //high clients are more intereesting
                        }

                    }


                }



                if (Mode.Contains("dedicated_") | Mode.Contains("mixed-AggQueries_") | ((write == true & read == true) == false))
                    percentageArray = new int[1] { 0 };
                if (Mode.Contains("%"))
                    percentageArray = new int[1] { Config._actualMixedWLPercentage };
                if (Mode.Contains("Limited"))
                    Config._QueryArray = new string[] { "RangeQueryRawAllDimsLimitedData" };

                _TestRetryIteration = 0;

                while (_TestRetryIteration < Config.GetTestRetries())
                {
                    _TestRetryIteration++;
                    int MXpercentageloop = 0;
                    foreach (var percentage in percentageArray)
                    {
                        Config._actualMixedWLPercentage = percentage;
                        int clientloop = 0;
                        foreach (var totalClientsNb in clientNumberArray)
                        {
                            _currentClientsNR = totalClientsNb;
                            _currentReadClientsNR = totalClientsNb;
                            _currentWriteClientsNR = totalClientsNb;

                            foreach (var dimNb in dimNbArray)
                            {
                                Config._actualDataDimensionsNr = dimNb;
                                int batchloop = 0;
                                foreach (var batchSize in batchSizeArray)
                                {
                                    for (int ConsecutiveTimeBatchesIterations = 0; ConsecutiveTimeBatchesIterations <= getConsecutiveTimeBatchesIterations; ConsecutiveTimeBatchesIterations++)
                                    {
                                        long iterationTimestamp = Helper.GetNanoEpoch();
                                        _currentWriteBatchSize = batchSize;
                                        _currentlimit = (int)((double)_currentWriteBatchSize * ((double)Config._actualMixedWLPercentage / 100));
                                        bool patchwork = false;

                                        if (write == true)
                                        {
                                            if (Config.GetPatchWorkMode() == true) // PatchWork Mode
                                            {
                                                patchwork = true;

                                                if (File.Exists(GetNextTSPath()))
                                                {
                                                    string readText = File.ReadAllText(GetNextTSPath());
                                                    patchMeandate = DateTime.Parse(readText);
                                                }

                                                nextdate = patchMeandate.AddMilliseconds( (int) ((batchSize / (sensorsNb / _currentClientsNR)) + 1) * Config.GetRegularTsScaleMilliseconds());

                                                using (FileStream fs = File.Create(GetNextTSPath()))
                                                {
                                                    string dataasstring = nextdate.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                                                                        CultureInfo.InvariantCulture);
                                                    byte[] info = new UTF8Encoding(true).GetBytes(dataasstring);
                                                    fs.Write(info, 0, info.Length);
                                                }

                                                Random r = new Random();
                                                double rr = r.Next(1,  daySpan * 60 * 60 * 24);
                                                date = patchMeandate.AddMilliseconds(Config.GetRegularTsScaleMilliseconds() * rr);
                                            }
                                            else
                                            {
                                                if (File.Exists(GetNextTSPath()))
                                                {
                                                    string readText = File.ReadAllText(GetNextTSPath());
                                                    date = DateTime.Parse(readText);
                                                }
                                                
                                                nextdate = date.AddMilliseconds((   (int) (batchSize / (sensorsNb / _currentClientsNR))) * Config.GetRegularTsScaleMilliseconds());

                                                using (FileStream fs = File.Create(GetNextTSPath()))
                                                {
                                                    string dataasstring = nextdate.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                                                                        CultureInfo.InvariantCulture);
                                                    byte[] info = new UTF8Encoding(true).GetBytes(dataasstring);
                                                    fs.Write(info, 0, info.Length);
                                                }

                                            }
                                            var clientArrayW = new ClientWrite[_currentWriteClientsNR];
                                            for (var chosenClientIndex = 1; chosenClientIndex <= _currentWriteClientsNR; chosenClientIndex++)
                                            {
                                                clientArrayW[chosenClientIndex - 1] = new ClientWrite(iterationTimestamp, chosenClientIndex, _currentWriteClientsNR, Config.GetSensorNumber(), batchSize, dimNb, date);
                                            }
                                            var glancesW = new GlancesStarter(Operation.BatchIngestion, _currentWriteClientsNR, batchSize, sensorsNb, iterationTimestamp, Config.GetGlancesOutput() + "-W");
                                            var resultsW = new QueryStatusWrite[_currentWriteClientsNR];
                                            await Parallel.ForEachAsync(Enumerable.Range(0, _currentWriteClientsNR), new ParallelOptions() { MaxDegreeOfParallelism = _currentWriteClientsNR }, async (index, token) => { resultsW[index] = await RunIngestionTask(clientArrayW[index]).ConfigureAwait(false); }).ConfigureAwait(false);
                                            await glancesW.EndMonitorAsync().ConfigureAwait(false);

                                            using (var csvLoggerW = new CsvLogger<LogRecordWrite>("write"))
                                            {
                                                foreach (var result in resultsW)
                                                {
                                                    var recordW = result.PerformanceMetric.ToLogRecord(Mode, percentage,
                                                        result.Timestamp, result.IterationTimestamp, result.StartDate, batchSize, _currentWriteClientsNR, sensorsNb,
                                                        result.Client, result.Iteration, dimNb, ConsecutiveTimeBatchesIterations, patchwork, Config.GetIsRegular());
                                                    csvLoggerW.WriteRecord(recordW);
                                                }
                                            }
                                        }

                                        if (read == true)
                                        {
                                            foreach (string Query in Config._QueryArray)
                                            {
                                                Config.QueryTypeOnRunTime = Query;
                                                var glancesR = new GlancesStarter(Config.QueryTypeOnRunTime.ToEnum<Operation>(), _currentClientsNR, batchSize, sensorsNb, iterationTimestamp, Config.GetGlancesOutput() + "-R");
                                                var clientArrayR = new ClientRead[_currentReadClientsNR];

                                                for (int chosenClientIndex = 1; chosenClientIndex <= _currentReadClientsNR; chosenClientIndex++)
                                                {
                                                    clientArrayR[chosenClientIndex - 1] = new ClientRead();

                                                }
                                                var resultsR = new QueryStatusRead[_currentReadClientsNR];
                                                await Parallel.ForEachAsync(Enumerable.Range(0, _currentReadClientsNR), new ParallelOptions() { MaxDegreeOfParallelism = _currentReadClientsNR }, async (index, token) => { resultsR[index] = await RunReadTask(clientArrayR[index]).ConfigureAwait(false); }).ConfigureAwait(false);
                                                await glancesR.EndMonitorAsync().ConfigureAwait(false);

                                                using (var csvLoggerR = new CsvLogger<LogRecordRead>("read"))
                                                {
                                                    foreach (var result in resultsR)
                                                    {
                                                        var recordR = result.PerformanceMetric.ToLogRecord(Mode, percentage, result.Timestamp, iterationTimestamp, result.StartDate, batchSize, _currentReadClientsNR, sensorsNb,
                                                        result.Client, result.Iteration, dimNb, Config.GetIsRegular());
                                                        csvLoggerR.WriteRecord(recordR);
                                                    }
                                                }

                                            }

                                        }
                                        GC.Collect();

                                    }
                                    batchloop++;
                                }
                                clientloop++;
                            }
                            MXpercentageloop++;
                            GC.Collect();
                        }
                    }

                }

            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }

        }

        private async static Task Batching()
        {
            await CustomizableBenchMarkRun(true, false);
            Console.Out.WriteLine("WritesCompleted");
        }




        private async static Task OldBatching(bool log)
        {




            var init = Config.GetQueryType(); // Just for Init the Array

            _TestRetryIteration = 0;
            {
                while (_TestRetryIteration < Config.GetTestRetries())
                {
                    _TestRetryIteration++;
                    int sensorsNb = Config.GetSensorNumber();
                    int[] clientNumberArray = Config.GetClientNumberOptions();
                    int[] batchSizeArray = Config.GetBatchSizeOptions();
                    var daySpan = Config.GetDaySpan();
                    int[] dimNbArray = Config.GetDataDimensionsNrOptions();
                    int clientloop = 0;
                    foreach (var ClientsNb in clientNumberArray)
                    {
                        var totalClientsNb = ClientsNb;
                        _currentWriteClientsNR = totalClientsNb;

                        foreach (var dimNb in dimNbArray)
                        {
                            Config._actualDataDimensionsNr = dimNb;
                            int batchloop = 0;

                            foreach (var batchSize in batchSizeArray)
                            {
                                long iterationTimestamp = Helper.GetNanoEpoch();
                                _currentWriteBatchSize = batchSize;



                                for (int ConsecutiveTimeBatchesIterations = 0; ConsecutiveTimeBatchesIterations <= Config.GetConsecutiveTimeBatchesIterations(); ConsecutiveTimeBatchesIterations++)
                                {




                                    if (_TestRetryIteration > Config.GetTestRetries())
                                    {
                                        totalClientsNb = clientNumberArray.Last() + 1;
                                    }


                                    var date = Config.GetStartTime(); var patchwork = false;
                                    if (Config.GetConsecutiveTimeBatchesIterations() == -1) // PatchWork Mode
                                    {
                                        patchwork = true;
                                        Random r = new Random();
                                        double rr = r.Next(60 * 24 / 2, 60 * 24 / 2);
                                        date = Config.GetStartTime().AddDays(batchloop + clientloop * daySpan).AddMilliseconds(Config.GetRegularTsScaleMilliseconds() * rr);


                                    }
                                    else // Continuous Mode
                                    {
                                        date = Config.GetStartTime().AddDays(batchloop + clientloop * daySpan).AddMilliseconds(Config.GetRegularTsScaleMilliseconds() * (batchSize / Config.GetSensorNumber()) * ConsecutiveTimeBatchesIterations + _TestRetryIteration);

                                    }
                                    var clientArrayW = new ClientWrite[totalClientsNb];

                                    for (var chosenClientIndex = 1; chosenClientIndex <= totalClientsNb; chosenClientIndex++)
                                    {
                                        clientArrayW[chosenClientIndex - 1] = new ClientWrite(iterationTimestamp, chosenClientIndex, totalClientsNb, Config.GetSensorNumber(), batchSize, dimNb, date);
                                    }
                                    var glancesW = new GlancesStarter(Operation.BatchIngestion, totalClientsNb, batchSize, sensorsNb, iterationTimestamp, Config.GetGlancesOutput() + "-W");
                                    var resultsW = new QueryStatusWrite[totalClientsNb];
                                    await Parallel.ForEachAsync(Enumerable.Range(0, totalClientsNb), new ParallelOptions() { MaxDegreeOfParallelism = totalClientsNb }, async (index, token) => { resultsW[index] = (await RunIngestionTask(clientArrayW[index]).ConfigureAwait(false)); }).ConfigureAwait(false);
                                    await glancesW.EndMonitorAsync().ConfigureAwait(false);

                                    using (var csvLoggerW = new CsvLogger<LogRecordWrite>("write"))
                                    {

                                        foreach (var result in resultsW)
                                        {
                                            var record = result.PerformanceMetric.ToLogRecord(Mode, 0,
                                                result.Timestamp, result.IterationTimestamp, result.StartDate, batchSize, totalClientsNb, sensorsNb,
                                                result.Client, result.Iteration, dimNb, ConsecutiveTimeBatchesIterations, patchwork, Config.GetIsRegular());
                                            csvLoggerW.WriteRecord(record);
                                        }
                                    }
                                    batchloop++; clientloop++;
                                }
                            }
                        }
                        GC.Collect();
                    }
                }
            }
        }

        private static async Task BenchmarkReadData()
        {
            await CustomizableBenchMarkRun(false, true);
            Console.Out.WriteLine("ReadsCompleted");
        }



        private static async Task OldBenchmarkReadData()
        {




            var init = Config.GetQueryType(); // Just to Init the Array
            int[] clientNumberArray = Config.GetClientNumberOptions();
            int[] dimNbArray = Config.GetDataDimensionsNrOptions();
            _TestRetryIteration = 0;
            {
                while (_TestRetryIteration < Config.GetTestRetries())
                {
                    _TestRetryIteration++;
                    foreach (var totalClientsNb in clientNumberArray)
                    {
                        _currentReadClientsNR = totalClientsNb;

                        foreach (var dimNb in dimNbArray)
                        {
                            Config._actualDataDimensionsNr = dimNb;
                            foreach (string Query in Config._QueryArray)
                            {
                                long iterationTimestamp = Helper.GetNanoEpoch();
                                if (_TestRetryIteration > Config.GetTestRetries())
                                {
                                    _currentReadClientsNR = clientNumberArray.Last() + 1;
                                }
                                _currentlimit = (int)(double)Config.GetBatchSizeOptions().Last();

                                Config.QueryTypeOnRunTime = Query;
                                var client = new ClientRead();
                                var sensorsNb = Config.GetSensorNumber();

                                var glancesR = new GlancesStarter(Config.QueryTypeOnRunTime.ToEnum<Operation>(), _currentClientsNR, _currentlimit, sensorsNb, iterationTimestamp, Config.GetGlancesOutput() + "-R");
                                var clientArrayR = new ClientRead[totalClientsNb];

                                for (int chosenClientIndex = 1; chosenClientIndex <= totalClientsNb; chosenClientIndex++)
                                {
                                    clientArrayR[chosenClientIndex - 1] = new ClientRead();
                                }
                                var resultsR = new QueryStatusRead[totalClientsNb];
                                await Parallel.ForEachAsync(Enumerable.Range(0, totalClientsNb), new ParallelOptions() { MaxDegreeOfParallelism = totalClientsNb }, async (index, token) => { resultsR[index] = await RunReadTask(clientArrayR[index]).ConfigureAwait(false); }).ConfigureAwait(false);
                                await glancesR.EndMonitorAsync().ConfigureAwait(false);

                                using (var csvLogger = new CsvLogger<LogRecordRead>("read"))
                                {
                                    foreach (var result in resultsR)
                                    {
                                        var record = result.PerformanceMetric.ToLogRecord(Mode, -1, result.Timestamp, result.IterationTimestamp, result.StartDate, _currentlimit, totalClientsNb, sensorsNb,
                                          result.Client, result.Iteration, dimNb, Config.GetIsRegular());
                                        csvLogger.WriteRecord(record);
                                    }
                                }
                            }
                        }
                    }
                    GC.Collect();
                }

            }
        }
    }
}
