using System.Threading.Tasks;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.CoreContracts.Session;
using JetBrains.Annotations;

namespace CardioMonitor.Ui
{
    /// <summary>
    /// Вспомогательный класс для UI для сохранения сеанса в файл и его загрузки
    /// </summary>
    /// <remarks>
    /// Содержит диалоговые окна для выбора места сохранения
    /// </remarks>
    internal interface ISessionsFileUiManager
    {
        void Save([NotNull] Patient patient, [NotNull] Session session);
        
        [CanBeNull]
        SessionContainer Load();
    }
}