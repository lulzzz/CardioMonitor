using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.Pipelines
{
    public class PipelineContext
    {
        private readonly ConcurrentDictionary<Guid, IContextParams> _context;

        public PipelineContext()
        {
            _context = new ConcurrentDictionary<Guid, IContextParams>();
        }

        public void AddOrUpdate([NotNull] IContextParams @params)
        {
            if (@params == null) throw new ArgumentNullException(nameof(@params));
            
            _context.AddOrUpdate(@params.ParamsTypeId, @params, (guid, o) => @params);
        }

        public IContextParams TryGet(Guid id)
        {
            _context.TryGetValue(id, out var value);
            return value;
        }
    }
}