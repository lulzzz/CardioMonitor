﻿using System;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.CommonParams
{
    internal class CommonPatientCycleProcessingContextParams : ICycleProcessingContextParams
    {
        public static readonly Guid CommonPatientParamsId = new Guid("c571aac1-4c5c-4def-9296-1aef33953ae4");

        public Guid ParamsTypeId { get; } = CommonPatientParamsId;
        public Guid UniqObjectId { get; }

        public CommonPatientCycleProcessingContextParams(
            short heartRate, 
            short repsirationRate, 
            short spo2)
        {
            HeartRate = heartRate;
            RepsirationRate = repsirationRate;
            Spo2 = spo2;
            UniqObjectId = Guid.NewGuid();
        }
       

        /// <summary>
        /// Частота сердечных сокращений (ЧСС)
        /// </summary>
        public short HeartRate { get; }

        /// <summary>
        /// Частотат дыхания (ЧД)
        /// </summary>
        public short RepsirationRate { get;  }

        /// <summary>
        /// SPO2
        /// </summary>
        public short Spo2 { get;  }
    }

    [CanBeNull]
    internal static class CommonPatientParamsContextExtensions
    {
        public static CommonPatientCycleProcessingContextParams TryGetCommonPatientParams([NotNull] this CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return context.TryGet(CommonPatientCycleProcessingContextParams.CommonPatientParamsId) as CommonPatientCycleProcessingContextParams;
        }
    }
}