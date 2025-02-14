using System;
using System.Collections.Generic;
using System.Linq;
using Clapsode.DataLayerTS;
using Clapsode.DataLayerTS.Models;

using BenchmarkTool.Queries;

namespace BenchmarkTool.Database.Queries
{
    public class DatalayertsQuery : IQuery<ContainerRequest>
    {

        public ContainerRequest RangeRaw => new ContainerRequest()
        {
            Selection = new Dictionary<string, IEnumerable<string>>(),
        };
                public ContainerRequest RangeRawAllDims => new ContainerRequest()
        {
            Selection = new Dictionary<string, IEnumerable<string>>(),
        };

                public ContainerRequest RangeRawLimited => new ContainerRequest()
        {
            Selection = new Dictionary<string,  IEnumerable<string>>(),
        };
                public ContainerRequest RangeRawAllDimsLimited => new ContainerRequest()
        {
            Selection = new Dictionary<string,  IEnumerable<string>>(),
        };

        public ContainerRequest RangeAgg => new ContainerRequest()
        {
            Selection = new Dictionary<string,  IEnumerable<string>>(),
            Transformations = new TransformationRequest[] {
                new TransformationRequest(){
                    Function = FunctionType.RESAMPLE,
                    Aggregations = new AggregationType[] { AggregationType.MEAN },
                    IntervalTicks = Config.GetAggregationInterval()* 36000000000 , 
                    Mapping =  AggregationMappingType.EXPAND,

                    },
                  }
        };

        public ContainerRequest OutOfRange => new ContainerRequest()
        {
            Selection = new Dictionary<string,  IEnumerable<string>>(),
            Transformations = new TransformationRequest[] {
                new TransformationRequest(){
                    Function = FunctionType.FILTER, },
                },

        };

        public ContainerRequest StdDev => new ContainerRequest()
        {
            Selection = new Dictionary<string,  IEnumerable<string>>(),
            Transformations = new TransformationRequest[] {
                new TransformationRequest(){
                    Function = FunctionType.RESAMPLE,
                    Aggregations = new AggregationType[] { AggregationType.STD }, 
                    Mapping =  AggregationMappingType.EXPAND,
                    }
                },
        };

        public ContainerRequest AggDifference => new ContainerRequest()
        {
            Selection = new Dictionary<string,  IEnumerable<string>>(),
            Transformations = new TransformationRequest[] {
                new TransformationRequest(){
                    Function = FunctionType.RESAMPLE, 
                    Aggregations = new AggregationType[] { AggregationType.DIF },
                    IntervalTicks = Config.GetAggregationInterval()* 36000000000 ,
                    Mapping =  AggregationMappingType.EXPAND,

                    }
                },
        };

    }

}