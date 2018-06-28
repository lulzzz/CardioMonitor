using System;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.FileSaving.Containers.V1;
using JetBrains.Annotations;

namespace CardioMonitor.FileSaving.Mappers.V1
{
    internal static class SessionContainerMapperV1
    {
        public static SessionContainer ToDomain([NotNull] this StoredSessionContainerV1 container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            
            return new SessionContainer
            {
                Patient = container.Patient.ToDomain(),
                Session = container.Session.ToDomain()
            };
        }
        
        public static StoredSessionContainerV1 ToStored([NotNull] this SessionContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            
            return new StoredSessionContainerV1
            {
                Patient = container.Patient.ToStored(),
                Session = container.Session.ToStored()
            };
        }
    }
}