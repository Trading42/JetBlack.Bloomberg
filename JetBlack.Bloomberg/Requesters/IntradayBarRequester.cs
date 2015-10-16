﻿using System;
using System.Collections.Generic;
using Bloomberglp.Blpapi;

namespace JetBlack.Bloomberg.Requesters
{
    public class IntradayBarRequester : Requester
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public EventType EventType { get; set; }
        public int? Interval { get; set; }
        public bool? GapFillInitialBar { get; set; }
        public bool? ReturnEids { get; set; }
        public bool? ReturnRelativeDate { get; set; }
        public bool? AdjustmentNormal { get; set; }
        public bool? AdjustmentAbnormal { get; set; }
        public bool? AdjustmentSplit { get; set; }
        public bool? AdjustmentFollowDPDF { get; set; }

        public override IEnumerable<Request> CreateRequests(Service refDataService)
        {
            var requests = new List<Request>();

            foreach (var ticker in Tickers)
            {
                var request = refDataService.CreateRequest(OperationNames.IntradayBarRequest);

                request.Set(ElementNames.Security, ticker);
                request.Set(ElementNames.StartDateTime, new Datetime(StartDateTime.Year, StartDateTime.Month, StartDateTime.Day, StartDateTime.Hour, StartDateTime.Minute, StartDateTime.Second, StartDateTime.Millisecond));
                request.Set(ElementNames.EndDateTime, new Datetime(EndDateTime.Year, EndDateTime.Month, EndDateTime.Day, EndDateTime.Hour, EndDateTime.Minute, EndDateTime.Second, EndDateTime.Millisecond));
                request.Set(ElementNames.EventType, EventType.ToString());
                if (Interval.HasValue)
                    request.Set(ElementNames.Interval, Interval.Value);
                if (GapFillInitialBar.HasValue)
                    request.Set(ElementNames.GapFillInitialBar, GapFillInitialBar.Value);
                if (ReturnEids.HasValue)
                    request.Set(ElementNames.ReturnEids, ReturnEids.Value);
                if (ReturnRelativeDate.HasValue)
                    request.Set(ElementNames.ReturnRelativeDate, ReturnRelativeDate.Value);
                if (AdjustmentNormal.HasValue)
                    request.Set(ElementNames.AdjustmentNormal, AdjustmentNormal.Value);
                if (AdjustmentAbnormal.HasValue)
                    request.Set(ElementNames.AdjustmentAbnormal, AdjustmentAbnormal.Value);
                if (AdjustmentSplit.HasValue)
                    request.Set(ElementNames.AdjustmentSplit, AdjustmentSplit.Value);
                if (AdjustmentFollowDPDF.HasValue)
                    request.Set(ElementNames.AdjustmentFollowDPDF, AdjustmentFollowDPDF.Value);

                requests.Add(request);
            }

            return requests;
        }
    }
}
