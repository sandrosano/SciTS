﻿using System;

namespace BenchmarkTool.Database
{
    class DatabaseFactory
    {

        private string _database;

        public DatabaseFactory()
        {
            _database = Config.GetTargetDatabase();
        }

        public IDatabase Create()
        {
            switch (_database)
            {
                case Constants.DatalayertsDBClass:
                    return new DatalayertsDB();
                case Constants.DatalayertsDBasVectClass:
                    return new DatalayertsDBasVect();
                case Constants.TimescaleDBClass:
                    return new TimescaleDB();
                case Constants.InfluxDBClass:
                    return new InfluxDB();
                case Constants.ClickhouseClass:
                    return new ClickhouseDB();
                case Constants.PostgresClass:
                    return new PostgresDB();
                case Constants.DummyClass:
                    return new DummyDB();
                case Constants.FtpNoDBClass:
                    return new FtpNoDB();
                case Constants.FtpNoDBasVectClass:
                    return new FtpNoDBasVect();
                case Constants.MySQLClass:
                    return new MySQLDB();
                case Constants.VictoriametricsDBClass:
                    return new VictoriametricsDB();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}

