using System;
using System.IO;
using System.Xml.Linq;

namespace Scada.Core.Config
{
    public class SettingsReader
    {
        private readonly XDocument _doc;

        public SettingsReader(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Settings.xml not found", filePath);

            _doc = XDocument.Load(filePath);
        }

        private string GetValue(string key)
        {
            var element = _doc.Root.Element(key);
            if (element == null)
                return string.Empty;

            var valueElement = element.Element("Value");
            return valueElement == null ? string.Empty : valueElement.Value.Trim();
        }

        // ===== 你現在最需要的：SQL Connection String =====
        public string GetSqlConnectionString()
        {
            string server = GetValue("DatabaseAddress");
            string dbName = GetValue("DataBaseName");
            string account = GetValue("DataBaseAccount");
            string password = GetValue("DataBasePassword");

            return
                $"Server={server};" +
                $"Database={dbName};" +
                $"User Id={account};" +
                $"Password={password};" +
                $"TrustServerCertificate=True;";
        }
    }
}
