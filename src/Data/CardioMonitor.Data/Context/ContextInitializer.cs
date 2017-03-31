using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CardioMonitor.Data.Ef.Context
{
    public class ContextInitializer : CreateDatabaseIfNotExists<CardioMonitorContext>
    {
        private const string SqlPostfix = ".sql";
        private const string CreateScryptName = "create.sql";
        private const string UserNameTemplate = "%owner%";
        private const string DbNameTemplate = "%db_name%";

        private readonly Regex _userNameRegex = new Regex("User Id=(.*);");

        public override void InitializeDatabase(CardioMonitorContext context)
        {
            var matches = _userNameRegex.Match(context.Database.Connection.ConnectionString);
            if (!matches.Success || matches.Groups.Count <= 0) return;

            var userName = matches.Groups[0].Value;

            var scrypts = GetScrypts(context.Database.Connection.Database, userName);
            foreach (var scrypt in scrypts)
            {
                context.Database.ExecuteSqlCommand(scrypt);
            }
            base.InitializeDatabase(context);
        }

        protected override void Seed(CardioMonitorContext context)
        {
            
            base.Seed(context);
        }

        private string[] GetScrypts(string dbName, string userName)
        {
            var currentAssembly = Assembly.GetAssembly(typeof(ContextInitializer));
            var scriptsNames = currentAssembly.GetManifestResourceNames().
                Where(x => x.EndsWith(SqlPostfix)).
                OrderBy(x => x);

            var scrypts = new List<string>(scriptsNames.Count());
            foreach (var scriptsName in scriptsNames)
            {
                using (var stream = currentAssembly.GetManifestResourceStream(scriptsName))
                {
                    if (stream == null) continue;
                    using (var reader = new StreamReader(stream))
                    {
                        var scrypt = reader.ReadToEnd();
                        if (String.Equals(scriptsName, CreateScryptName))
                        {

                            scrypt = scrypt.Replace(UserNameTemplate, userName);
                            scrypt = scrypt.Replace(DbNameTemplate, dbName);
                        }
                        scrypts.Add(scrypt);
                    }
                }
            }

            return scrypts.ToArray();
        }
    }
}
