using System;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.FileSaving;
using CardioMonitor.FileSaving.Exceptions;
using CardioMonitor.Settings;
using JetBrains.Annotations;
using Markeli.Utils.Logging;
using Microsoft.Win32;
using ToastNotifications;
using ToastNotifications.Messages;

namespace CardioMonitor.Ui
{
    /// <inheritdoc />
    internal class SessionsFileUiManager : ISessionsFileUiManager
    {
        private readonly string _fileExtension = "cmsj";
        
        [NotNull]
        private readonly Notifier _notifier;
        [NotNull]
        private readonly ISessionFileManager _fileManager;
        [NotNull]
        private readonly ILogger _logger;
        
        /// <inheritdoc />
        public void Save(Patient patient, Session session)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));
            if (session == null) throw new ArgumentNullException(nameof(session));
            
            var dialog = new SaveFileDialog
            {
                Filter = GetFilter(),
                DefaultExt = _fileExtension
            };
            var dialogResult = dialog.ShowDialog() ?? false;
            if (!dialogResult) return;

            var filePath = dialog.SafeFileName;

            try
            {
                _fileManager.Save(patient, session, filePath);
                _notifier.ShowSuccess("Сеанс сохранен в файл");
            }
            catch (Exception e)
            {
                _logger.Error($"{GetType().Name}: ошибка сохранения результа сеанса в файл. Причина: {e.Message}", e);
                _notifier.ShowError("Ошибка сохранения сеанса в файл");
            }
        }

        private string GetFilter()
        {
            return $"Результаты сеанса (.{_fileExtension})|*.{_fileExtension}";
        }

        /// <inheritdoc />
        public SessionContainer Load()
        {
            var dialog = new OpenFileDialog
            {
                Filter = GetFilter(),
                DefaultExt = _fileExtension
            };
            var dialogResult = dialog.ShowDialog() ?? false;
            if (!dialogResult) return null;

            var filePath = dialog.SafeFileName;

            try
            {
                return _fileManager.Load(filePath);
            }
            catch (SavingException ex)
            {
                _notifier.ShowError(ex.Message);
                return null;
            }
            catch (SettingsException ex)
            {
                _notifier.ShowError(ex.Message);
                return null;
            }
            catch (Exception e)
            {
                _logger.Error($"{GetType().Name}: ошибка открытия результа сеанса из файла. Причина: {e.Message}", e);
                _notifier.ShowError("Не удалось открыть результаты сеанса из файла");
                return null;
            }
        }
    }
}