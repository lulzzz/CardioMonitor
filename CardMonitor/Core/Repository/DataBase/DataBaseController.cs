using System;
using MySql.Data.MySqlClient;

namespace CardioMonitor.Core.Repository.DataBase
{

    internal class DataBaseController
    {
        private static MySqlConnection _myConnect;
        private bool _isOpen = false;
        
        public DataBaseController()
        {
            var initComand = "Database=" + Settings.Settings.Instance.DataBase.DataBase + ";Data Source=" +
                             Settings.Settings.Instance.DataBase.Source +
                             ";User Id=" + Settings.Settings.Instance.DataBase.User + ";Password=gfhjkm" + ";Allow Zero Datetime=True;Convert Zero Datetime=True;charset=utf8";
            _myConnect =  new MySqlConnection(initComand);

        }

        //Multitasking Questions There
        public MySqlDataReader ConnectDB(string query)
        {
            try
            {
                if (_isOpen) { throw new AccessViolationException();}
                _isOpen = true;
                var cmd = new MySqlCommand(query, _myConnect);
                _myConnect.Open();
                var reader = cmd.ExecuteReader();
                return reader;
            }
            catch(Exception e)
            {
                //Logger.LogException(e);
                throw;
            }
        }

        public int DisConnectDB(MySqlDataReader reader)
        {
            try
            {
                _isOpen = false;
                reader.Close();
                _myConnect.Close();
                return 1;
            }
            catch(Exception e)
            {
                //Logger.LogException(e);
                return 0;
            }
        }

        public int ExecuteQuery(string query)
        {
            try
            {
                if (_isOpen) { return 0;}
                var cmd = new MySqlCommand(query, _myConnect);
                _myConnect.Open();
                cmd.ExecuteNonQuery();
                _myConnect.Close();
                return 1;
            }
            catch(Exception)
            {
                //Logger.LogException(e);
                throw;
            }
        }

        public int CheckConnection()
        {
            try
            {
                _myConnect.Open();
                _myConnect.Close();
               // Logger.LogSystemEvent("DataBase Connection Established");
                return 1;
            }
            catch (Exception e)
            {
               // Logger.LogException(e);
                return 0;
            }
        }
    }
}
