# SciTS v2, 2024 update

A tool to benchmark Time-series on different databases


# Citation and Dataset 
Our 2024 Dataset:
[![DOI](https://zenodo.org/badge/DOI/10.5281/zenodo.14954540.svg)](https://doi.org/10.5281/zenodo.14954540)

Please keep updated for our soon to be published scientific article, containing the evaluation of the SciTS-v2 benchmark.
 

Software DOI: 
[![DOI](https://zenodo.org/badge/429005385.svg)](https://zenodo.org/badge/latestdoi/429005385)


# Features

- reworked architecture
- adds mixed, online workloads
- adds regular and irregular ingestion modes.
- adds multiple values per time series ("Dimensions")
- adds limited queries
- adds CLI arguments
- adds ClientLatency metric to measure differences in local processing.

for questions on these features, please contact info@saninfo.de

Requires .NET 7.x cross-platform framework.


# How to run

1. Create your workload as `App.config` (case-sensitive) in `BenchmarkTool`.
2. Edit the connection strings to your database servers in the workload file.
3. Choose the target database in the workload file using `TargetDatabase` element.
4. run `dotnet run --project BenchmarkTool write` if it's an ingestion workload,
and `dotnet run --project BenchmarkTool read` if it's a query workload.
x. Use `Scripts/ccache.sh <database-service-name>` to clear the cache between query tests.

## Additional Command Line options:

`dotnet run --project BenchmarkTool [action] [regular/irregular] [DatabaseNameDB]`

Available Actions:

* read: start the specified retrieval and aggregation workloads.
* write: start the ingestion across specified batchsize, number of clients, dimensions.
* mixed-AggQueries: start the online, mixed workload benchmark as a mixture of aggregated quieries and Ingestion-Parameters
* mixed-LimitedQueries: start the online, mixed workload benchmark as a mixture of queried and ingested datapoints according the specified percentage parameter and the requested Ingestion-Parameters. E.g. 100% means that as much datapoints are retrieved as ingested.


## System Metrics using Glances

This tool uses [glances](https://github.com/nicolargo/glances/).
1. Install glances with all plugins on the database server using `pip install glances[all]`
2. Run glances REST API on the database server using `glances -w --disable-webui`

## Workload Definition Files

you can open Default-App.config edit it and save it as App.config.
It has following content:
```xml
<?xml version="1.0" encoding="utf-8"?>

<configuration>
    <appSettings>
<!-- Attention: This file "AppDefault.config" is to be renamed in "App.config", after updating the "###" and other fields.  -->
  
    <!-- Datalayerts connection settings -->
        <add key="DatalayertsConnection" value="https://datalayerts.com" />
        <add key="DatalayertsUser" value="###" />
        <add key="DatalayertsPassword" value="###" />

    <!-- Postgres connection settings -->
        <add key="PostgresConnection" value="Server=localhost;Port=5432;Database=postgres;User Id=postgres;Password=###;" />

    <!-- Timescale connection settings -->
        <add key="TimescaleConnection" value="Server=localhost;Port=6432;Database=postgres;User Id=postgres;Password=###;CommandTimeout=300" />

    <!-- InfluxDB connection settings --> 
        <add key="InfluxDBHost" value="http://localhost:8086" />  
        <add key="InfluxDBToken" value="u7Ek4P5s0Nle61QQF1nNA3ywL1JYZky6rHRXxkPBX5bY4H3YFJ6T4KApWSRhaKNj_kHgx70ZLBowB6Di4t2YXg==" />
        <add key="InfluxDBBucket" value="scitsdb" />
        <add key="InfluxDBOrganization" value="scits" />  

    <!-- Clickhouse connection settings -->
        <add key="ClickhouseHost" value="localhost" />
        <add key="ClickhousePort" value="9000" />
        <add key="ClickhouseUser" value="default" />
        <add key="ClickhouseDatabase" value="default" />
 
    <!-- General Settings-->
          <add key="PrintModeEnabled" value="false" />
          <add key="TestRetries" value="2" />
          <add key="DaySpan" value="1" />
        <!-- Could be: DummyDB,  PostgresDB , DatalayertsDB , ClickhouseDB , TimescaleDB , InfluxDB -->
          <add key="TargetDatabase" value="DummyDB" />
          <add key="StartTime" value="2022-01-01T00:00:00.00" />
          <add key="RegularTsScaleMilliseconds" value="1000" /> 
        <!-- Where to store metrics files: The Programm will split the files in "[...]Read.csv" and "[...]Write.csv" -->
          <add key="MetricsCSVPath" value="Metrics_Source_Month-Day" />
          
    <!-- System Metrics Options -->
          <add key="GlancesOutput" value="Glances_Source_Month-Day.csv"/>
          <add key="GlancesUrl" value="http://localhost:61208" />
          <add key="GlancesDatabasePid" value="1" />
          <add key="GlancesPeriod" value="1" />
          <add key="GlancesNIC" value="lo" />
          <add key="GlancesDisk" value="sda1" />
          <add key="GlancesStorageFileSystem" value="/" />
        <!-- Insert multiple dimensionnrs, e.g.  1,6 ,12 ,50, 100, -->
          <add key="DataDimensionsNrOptions" value="1,6" />  

    <!-- Read Query Options -->
        <!-- Could be: Agg, All, RangeQueryRawData, RangeQueryRawAllDimsData, RangeQueryRawLimitedData, RangeQueryRawAllDimsLimitedData  RangeQueryAggData, OutOfRangeQuery, DifferenceAggQuery, STDDevQuery -->
          <add key="QueryType" value="All" />
          <add key="AggregationIntervalHour" value="1" />
          <add key="DurationMinutes" value="60" />
          <add key="SensorsFilter" value="1,2,3,4" /> <!--  or "All" -->
          <add key="SensorID" value="1" />
          <add key="MaxValue" value="0.9" />
          <add key="MinValue" value="0.1" />
          <add key="FirstSensorID" value="1" />
          <add key="SecondSensorID" value="2" />

    <!-- Ingestion -->
        <!-- Could be: regular, irregular -->
          <add key="IngestionType" value="regular" /> 
        <!-- Coulde be:  33, 100 , 300  -->
          <add key="MixedWLPercentageOptions" value="33, 100,300" />
        <!-- Could be: array, column. Array is not fully implemented in all DBMS. -->
          <add key="MultiDimensionStorageType" value="column" />
        <!-- 10, 1000, 5000, 10000 , 50000 -->
          <add key="BatchSizeOptions" value=" 100 , 1000, 6000 " />
        <!-- Number of concurrent clients e.g.(1,8,16) must be less than sensors. BatchSizes will be shared out between the clients -->
          <add key="ClientNumberOptions" value="1 , 8" />
          <add key="SensorNumber" value="100" />   

    </appSettings>

</configuration>
```

### Workload Files
You simply use the ./Workloads/test2024.sh script, which gives also a good example on how to use the command line arguments.

For SciTSv1 Workloads, from the paper published by Jalal et al. 2022, 
you can choose from the available workloads by choosing a `*.config` file from `Workloads` folder.
The file to workload mapping is as follow:



| Workload    | Workload file                      |
| ----------- | ---------------------------------- |
| 2022 WLs    | SciTS-v1                           |
| ----------- |                                    |
| Q1          | query-q1.config                    |
| Q2          | query-q2.config                    |
| Q3          | query-q3.config                    |
| Q4          | query-q4.config                    |
| Q5          | query-q5.config                    |
| Batching    | ingestion-batching-1client.config  |
| Concurrency | ingestion-batching-nclients.config |
| Scaling     | ingestion-scaling.config           |
|.............|....................................|
|Collection   | SciTS-v2                           |
|of 2024 WLs  | test2024.sh                        |
