﻿using DevExtreme.AspNet.Data.Aggregation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevExtreme.AspNet.Data.RemoteGrouping {

    class RemoteGroupTransformer {

        public static RemoteGroupingResult Run(IEnumerable<IRemoteGroup> flatGroups, int groupCount, SummaryInfo[] totalSummary, SummaryInfo[] groupSummary) {
            var accessor = new RemoteGroupAccessor();

            List<Group> hierGroups = null;

            if(groupCount > 0) {
                hierGroups = new GroupHelper<IRemoteGroup>(accessor).Group(
                    flatGroups,
                    Enumerable.Range(0, groupCount).Select(i => new GroupingInfo { Selector = "K" + i }).ToArray()
                );
            }

            IEnumerable dataToAggregate = hierGroups;
            if(dataToAggregate == null)
                dataToAggregate = flatGroups;

            var transformedTotalSummary = TransformSummary(totalSummary, "T");
            var transformedGroupSummary = TransformSummary(groupSummary, "G");

            transformedTotalSummary.Add(new SummaryInfo { SummaryType = "remoteCount" });

            var totals = new AggregateCalculator<IRemoteGroup>(dataToAggregate, accessor, transformedTotalSummary, transformedGroupSummary).Run();
            var totalCount = (int)totals.Last();

            totals = totals.Take(totals.Length - 1).ToArray();
            if(totals.Length < 1)
                totals = null;

            return new RemoteGroupingResult {
                Groups = hierGroups,
                Totals = totals,
                TotalCount = totalCount
            };
        }

        static List<SummaryInfo> TransformSummary(SummaryInfo[] original, string fieldPrefix) {
            var result = new List<SummaryInfo>();

            if(original != null) {
                for(var i = 0; i < original.Length; i++) {
                    result.Add(new SummaryInfo {
                        Selector = fieldPrefix + i,
                        SummaryType = TransformSummaryType(original[i].SummaryType)
                    });
                }
            }

            return result;
        }

        static string TransformSummaryType(string type) {
            if(type == "count")
                return "remoteCount";

            if(type == "avg")
                return "remoteAvg";

            return type;
        }


    }

}
