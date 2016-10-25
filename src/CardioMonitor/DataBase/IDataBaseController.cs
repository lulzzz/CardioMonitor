namespace CardioMonitor.DataBase
{
    /// <summary>
    /// Контроллер для взаимодействия с базой
    /// </summary>
    public interface IDataBaseController
    {
        ISqlDataReader ConnectDb(string query);

        void DisсonnectDb(ISqlDataReader reader);

        void ExecuteQuery(string query);

        bool GetConnectionStatus();

        void CheckConnection();
    }
}