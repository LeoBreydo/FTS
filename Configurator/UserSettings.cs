using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configurator
{
    public static class UserSettings
    {
        private static string _dateTimeFormat;

        public static string DateTimeFormat
        {
            get
            {
                if (_dateTimeFormat == null)
                {
                    _dateTimeFormat = ConfigurationManager.AppSettings["DateTimeFormat"];
                    if (string.IsNullOrEmpty(_dateTimeFormat))
                        _dateTimeFormat = "yyyy.MM.dd HH:mm";
                }
                return _dateTimeFormat;
            }
        }
    }
}
