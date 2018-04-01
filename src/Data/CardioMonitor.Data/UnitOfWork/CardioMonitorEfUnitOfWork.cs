﻿using System;
using CardioMonitor.Data.Common.UnitOfWork;
using CardioMonitor.Data.Contracts.Repositories;
using CardioMonitor.Data.Contracts.UnitOfWork;
using CardioMonitor.Data.Ef.Context;
using CardioMonitor.Data.Ef.Repositories;

namespace CardioMonitor.Data.Ef.UnitOfWork
{
    public class CardioMonitorEfUnitOfWork : Common.UnitOfWork.UnitOfWork, ICardioMonitorUnitOfWork
    {
        public CardioMonitorEfUnitOfWork(IUnitOfWorkContext context) : base(context)
        {
            if (context== null) throw new ArgumentNullException(nameof(context));
            var cardioMonitorContext = context.Context as CardioMonitorContext;
            if (cardioMonitorContext == null) throw new ArgumentException(nameof(context));

            Patients = new PatientsRepository(cardioMonitorContext);
            Sessions = new SessionsRepository(cardioMonitorContext);
        }

        public IPatientsRepository Patients { get; }
        public ISessionsRepository Sessions { get; }
    }
}