﻿using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Bloomberglp.Blpapi;
using JetBlack.Bloomberg.Identifiers;
using JetBlack.Bloomberg.Models;
using JetBlack.Bloomberg.Utilities;

namespace JetBlack.Bloomberg.Managers
{
    internal class SecurityEntitlementsManager : RequestResponseManager<SecurityEntitlementsRequest, SecurityEntitlementsResponse>, ISecurityEntitlementsProvider
    {
        private readonly Service _service;
        private readonly Identity _identity;

        private readonly IDictionary<CorrelationID, IList<string>> _tickerMap = new Dictionary<CorrelationID, IList<string>>(); 

        public SecurityEntitlementsManager(Session session, Service service, Identity identity)
            : base(session)
        {
            _service = service;
            _identity = identity;
        }

        public override IObservable<SecurityEntitlementsResponse> ToObservable(SecurityEntitlementsRequest securityEntitlementsRequest)
        {
            return Observable.Create<SecurityEntitlementsResponse>(observer =>
            {
                var request = _service.CreateRequest(OperationNames.SecurityEntitlementsRequest);
                var securitiesElement = request.GetElement(ElementNames.Securities);
                var securities = new List<string>();
                securityEntitlementsRequest.Tickers.ForEach(ticker =>
                {
                    securitiesElement.AppendValue(ticker);
                    securities.Add(ticker);
                });

                var correlationId = new CorrelationID();
                Observers.Add(correlationId, observer);
                _tickerMap.Add(correlationId, securities);

                Session.SendRequest(request, _identity, correlationId);

                return Disposable.Create(() => Session.Cancel(correlationId));
            });
        }

        public override bool CanProcessResponse(Message message)
        {
            return MessageTypeNames.SecurityEntitlementsResponse.Equals(message.MessageType);
        }

        public override void ProcessResponse(Session session, Message message, bool isPartialResponse, Action<Session, Message, Exception> onFailure)
        {
            IObserver<SecurityEntitlementsResponse> observer;
            if (!Observers.TryGetValue(message.CorrelationID, out observer))
            {
                onFailure(session, message, new ApplicationException("Failed to find handler"));
                return;
            }

            var tickers = _tickerMap[message.CorrelationID];
            _tickerMap.Remove(message.CorrelationID);

            var securityEntitlementsResponse = new SecurityEntitlementsResponse();

            if (MessageTypeNames.SecurityEntitlementsResponse.Equals(message.MessageType))
            {
                var eidDataArrayElement = message.GetElement("eidData");
                for (var i = 0; i < eidDataArrayElement.NumValues; ++i)
                {
                    var ticker = tickers[i];
                    var eidDataElement = eidDataArrayElement.GetValueAsElement(i);

                    var status = eidDataElement.GetElementAsInt32("status");
                    var sequenceNumber = eidDataElement.GetElementAsInt32("sequenceNumber");

                    var entitlementIds = new List<int>();
                    var eidsElement = eidDataElement.GetElement("eids");
                    for (var j = 0; j < eidsElement.NumValues; j++)
                        entitlementIds.Add(eidsElement.GetValueAsInt32(j));

                    securityEntitlementsResponse.Add(ticker, new SecurityEntitlements(ticker, status, sequenceNumber, entitlementIds));
                }

                observer.OnNext(securityEntitlementsResponse);

                if (!isPartialResponse)
                {
                    observer.OnCompleted();
                    Observers.Remove(message.CorrelationID);
                }
            }
            else
                onFailure(session, message, new Exception("Unknown message type: " + message));
        }
    }
}
