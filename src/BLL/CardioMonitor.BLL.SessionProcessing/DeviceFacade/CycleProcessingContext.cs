using System;
using System.Collections.Concurrent;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Exceptions;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade
{
    internal class CycleProcessingContext
    {
        private readonly ConcurrentDictionary<Guid, ICycleProcessingContextParams> _context;

        public CycleProcessingContext()
        {
            _context = new ConcurrentDictionary<Guid, ICycleProcessingContextParams>();
        }

        public void AddOrUpdate([NotNull] ICycleProcessingContextParams @params)
        {
            if (@params == null) throw new ArgumentNullException(nameof(@params));
            
            _context.AddOrUpdate(@params.ParamsTypeId, @params, (guid, o) => @params);
        }

        public ICycleProcessingContextParams TryGet(Guid id)
        {
            _context.TryGetValue(id, out var value);
            return value;
        }
    }
}