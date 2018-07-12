namespace CardioMonitor.FileSaving.Containers.V1
{
    internal class StoredSessionContainerV1
    {
        /// <summary>
        /// Пациент
        /// </summary>
        public StoredPatientV1 Patient { get; set; }

        /// <summary>
        /// Сеанс
        /// </summary>
        public StoredSessionV1 Session { get; set; }
    }
}
