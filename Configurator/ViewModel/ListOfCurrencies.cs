using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Configurator.ViewModel
{
    public class ListOfCurrencies
    {
        private readonly string _fileName;
        private readonly List<string> _currencies=new List<string>();

        private static readonly List<string> _commonCurrencies = new List<string>
        {
            "USD", "EUR", "AUD", "GBP", "CAD", "JPY", "CHF", "CNH", "SGD", "NZD"
        };
        public IEnumerable<string> EnumerateCurrencies() => _currencies;
        public static event Action OnNewCurrencyAdded = delegate { };
        public ListOfCurrencies(string fileName)
        {
            _fileName = fileName;
            Load();

            if (_currencies.Count == 0)
            {
                // create default list
                _currencies.AddRange(_commonCurrencies);
                _currencies.Sort();
                Save();
            }
        }
        private void Load()
        {
            try
            {
                if (!File.Exists(_fileName)) return;
                _currencies.Clear();
                _currencies.AddRange(
                    File
                        .ReadAllLines(_fileName)
                        .Select(ccy => ccy?.Trim().ToUpper())
                        .Where(IsNormalCcy)
                        .OrderBy(x=>x)
                        .Distinct()
                );
            }
            catch
            {
                return;
            }
        }
        private static bool IsNormalCcy(string currency)
        {
            if (string.Equals(currency, "UNK", StringComparison.OrdinalIgnoreCase)) return false;

            return currency.Length == 3 && currency.All(char.IsLetter);
        }

        private bool Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(_fileName);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllLines(_fileName, _currencies);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string AcceptCurrency(string ccy)
        {
            if (ccy == null) throw new Exception("Invalid currency value");
            ccy = ccy.Trim().ToUpper();
            if (!IsNormalCcy(ccy))
                throw new Exception("Invalid currency value");

            if (_currencies.Contains(ccy))
                return ccy;

            if (DialogResult.Yes !=
                MessageBox.Show(Form.ActiveForm,
                    string.Format(@"Currency '{0}' was never used before.
Will you save this value to the currencies list?", ccy),
                    "Question",
                    MessageBoxButtons.YesNo))
                throw new Exception("Invalid currency value");

            _currencies.Add(ccy);
            Save();
            OnNewCurrencyAdded();

            return ccy;
        }







    }
}