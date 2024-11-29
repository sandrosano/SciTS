
 
# Assert Glances is running propery and sending data. For monitoring in second terminal: 
watch -n1 tail -n4 *.csv ts-bench.log

#DummyDB to Benchmark the Program itself - use this to understand if Glances is Working!!
dotnet run --project SciTS/BenchmarkTool write regular DummyDB 2>&1 | tee RrDummy.log &&
dotnet run --project SciTS/BenchmarkTool write irregular DummyDB 2>&1 | tee RrDummy.log &&
dotnet run --project SciTS/BenchmarkTool write regular DummyDB patch 2>&1 | tee RrDummypatch.log &&
dotnet run --project SciTS/BenchmarkTool write irregular DummyDB patch 2>&1 | tee RrDummypatch.log &&


# Clickhouse
## First populate all the DBS with at least one full day of data:    
dotnet run --project SciTS/BenchmarkTool populate regular ClickhouseDB 2>&1 | tee RpopCH.log && 
dotnet run --project SciTS/BenchmarkTool populate irregular ClickhouseDB 2>&1 | tee IRpopCH.log && touch populated

## Then, go into the server, login to the database via client or GUI and read out all table size metrics, save to file.
du -h /store/clickhouse | tee CH-FS.info

## Now the  I/O benchmark
dotnet run --project SciTS/BenchmarkTool consecutive regular ClickhouseDB 2>&1 | tee R-CH.log && 
dotnet run --project SciTS/BenchmarkTool consecutive irregular ClickhouseDB 2>&1 | tee IR-CH.log && 
dotnet run --project SciTS/BenchmarkTool write regular ClickhouseDB patch 2>&1 | tee R-CH-PW.log && 
dotnet run --project SciTS/BenchmarkTool write irregular ClickhouseDB patch 2>&1 | tee IR-CH-PW.log &&
dotnet run --project SciTS/BenchmarkTool mixed-AggQueries regular ClickhouseDB 2>&1 | tee  Rmixed-AggQueriesCH.log &&
dotnet run --project SciTS/BenchmarkTool mixed-LimitedQueries regular ClickhouseDB 2>&1 | tee  Rmixed-LimitedQueriesCH.log && touch finallyfinishedCH


# Influx
## First populate all the DBS with at least one full day of data:    
dotnet run --project SciTS/BenchmarkTool populate regular InfluxDB 2>&1 | tee RpopIF.log && 
dotnet run --project SciTS/BenchmarkTool populate irregular InfluxDB 2>&1 | tee IRpopCH.log && touch populatedIF

## Then, go into the server, login to the database via client or GUI and read out all table size metrics, save to file.
du -h /store/influxdb | tee IF-FS.info

## Now the  I/O benchmark
dotnet run --project SciTS/BenchmarkTool consecutive regular InfluxDB 2>&1 | tee R-IF.log && 
dotnet run --project SciTS/BenchmarkTool consecutive irregular InfluxDB 2>&1 | tee IR-IF.log && 
dotnet run --project SciTS/BenchmarkTool write regular InfluxDB patch 2>&1 | tee R-IF-PW.log && 
dotnet run --project SciTS/BenchmarkTool write irregular InfluxDB patch 2>&1 | tee IR-IF-PW.log &&
dotnet run --project SciTS/BenchmarkTool mixed-AggQueries regular InfluxDB 2>&1 | tee  Rmixed-AggQueriesIF.log &&
dotnet run --project SciTS/BenchmarkTool mixed-LimitedQueries regular InfluxDB 2>&1 | tee  Rmixed-LimitedQueriesIF.log && touch finallyfinishedIF




# TimescaleDB
## First populate all the DBS with at least one full day of data:    
dotnet run --project SciTS/BenchmarkTool populate regular TimescaleDB 2>&1 | tee RpopTS.log && 
dotnet run --project SciTS/BenchmarkTool populate irregular TimescaleDB 2>&1 | tee IRpopTS.log && touch populatedTS

## Then, go into the server, login to the database via client or GUI and read out all table size metrics, save to file.
du -h /store/postgresql/ | tee TS-FS.info

## Now the  I/O benchmark
dotnet run --project SciTS/BenchmarkTool consecutive regular TimescaleDB 2>&1 | tee R-TS.log && 
dotnet run --project SciTS/BenchmarkTool consecutive irregular TimescaleDB 2>&1 | tee IR-TS.log && 
dotnet run --project SciTS/BenchmarkTool write regular TimescaleDB patch 2>&1 | tee R-TS-PW.log && 
dotnet run --project SciTS/BenchmarkTool write irregular TimescaleDB patch 2>&1 | tee IR-TS-PW.log &&
dotnet run --project SciTS/BenchmarkTool mixed-AggQueries regular TimescaleDB 2>&1 | tee  Rmixed-AggQueriesTS.log &&
dotnet run --project SciTS/BenchmarkTool mixed-LimitedQueries regular TimescaleDB 2>&1 | tee  Rmixed-LimitedQueriesTS.log && touch finallyfinishedIF


