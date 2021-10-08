using System.Linq;

namespace Configurator.ViewModel
{
    public class TCfgPGEditor : BaseController
    {
        private const string CAT_SETTINGS = "Settings";


        private readonly TradingConfiguration _info;
        private readonly Controller _controller;

        public TCfgPGEditor(TradingConfiguration info,Controller controller)
        {
            _info = info;
            _controller = controller;
            Properties.Add(new PropertySpec("MaxErrorsPerDay", typeof(int?), CAT_SETTINGS));
            Properties.Add(new PropertySpec("SchedulerTimeStepInMinutes", typeof(int), CAT_SETTINGS,"Frequency of Scheduler actions in minutes, value must be in range [1..30]"));

            GetValue += _GetValue;
            SetValue += _SetValue;
        }


        private void _GetValue(object sender, PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "MaxErrorsPerDay":
                    e.Value = ConvertHelper.MaxErrorsToUser(_info.MaxErrorsPerDay);
                    break;
                case "SchedulerTimeStepInMinutes":
                    e.Value = _info.SchedulerTimeStepInMinutes;
                    break;
            }

        }

        private void _SetValue(object sender, PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
             
                case "MaxErrorsPerDay":
                    _info.MaxErrorsPerDay = ConvertHelper.AcceptMaxErrors((int?) e.Value);
                    break;
                case "SchedulerTimeStepInMinutes":
                    _controller.SetSchedulerTimeStepInMinutes((int)e.Value);
                    break;

            }
        }
    }

    public class MarketPGEditor : BaseController
    {
        private const string CAT_MAIN = "1. Instrument";
        private const string CAT_SETTINGS = "2. Restrictions";


        private readonly MarketRow _info;

        public MarketPGEditor(MarketRow info)
        {
            _info = info;
            Properties.Add(new PropertySpec(" Exchange", typeof(string), CAT_MAIN) { ReadOnly = true });
            Properties.Add(new PropertySpec(" Market", typeof(string), CAT_MAIN) { ReadOnly = true });
            Properties.Add(new PropertySpec("BigPointValue", typeof(int), CAT_MAIN));
            Properties.Add(new PropertySpec("MinMove", typeof(double), CAT_MAIN));

            Properties.Add(new PropertySpec("SessionCriticalLoss", typeof(decimal?), CAT_SETTINGS));
            Properties.Add(new PropertySpec("MaxErrorsPerDay", typeof(int?), CAT_SETTINGS));
            Properties.Add(new PropertySpec("MaxNbrContracts", typeof(int?), CAT_SETTINGS));

            GetValue += _GetValue;
            SetValue += _SetValue;
        }


        private void _GetValue(object sender, PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case " Exchange":
                    e.Value = _info.Exchange;
                    break;
                case " Market":
                    e.Value = _info.MarketName;
                    break;

                case "BigPointValue":
                    e.Value = _info.BigPointValue;
                    break;
                case "MinMove":
                    e.Value = _info.MinMove;
                    break;

                case "SessionCriticalLoss":
                    e.Value = _info.SessionCriticalLoss;
                    break;
                case "MaxErrorsPerDay":
                    e.Value = _info.MaxErrorsPerDay;
                    break;
                case "MaxNbrContracts":
                    e.Value = _info.MaxNbrContracts;
                    break;
            }

        }

        private void _SetValue(object sender, PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "BigPointValue":
                    _info.BigPointValue = (int)e.Value;
                    break;
                case "MinMove":
                    _info.MinMove = (double)e.Value;
                    break;

                case "SessionCriticalLoss":
                    _info.SessionCriticalLoss=(decimal?)e.Value;
                    break;
                case "MaxErrorsPerDay":
                    _info.MaxErrorsPerDay = (int?) e.Value;
                    break;
                case "MaxNbrContracts":
                    _info.MaxNbrContracts = (int?) e.Value;
                    break;
            }
        }
    }

    public class ExchangePGEditor : BaseController
    {
        private const string CAT_MAIN = "1. Instrument";
        private const string CAT_SETTINGS = "2. Restrictions";


        private readonly ExchangeRow _info;

        public ExchangePGEditor(ExchangeRow info)
        {
            _info = info;
            Properties.Add(new PropertySpec(" Exchange", typeof(string), CAT_MAIN) { ReadOnly = true });
            Properties.Add(new PropertySpec("Currency", typeof(string), CAT_MAIN, "Currency", typeof(ComboEditor),
                Controller.Instance.ListOfCurrencies.EnumerateCurrencies().Cast<object>().ToArray()));
            Properties.Add(new PropertySpec("MaxErrorsPerDay", typeof(int?), CAT_SETTINGS));

            GetValue += _GetValue;
            SetValue += _SetValue;
        }


        private void _GetValue(object sender, PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case " Exchange":
                    e.Value = _info.Exchange;
                    break;
                case "Currency":
                    e.Value = _info.Currency;
                    break;

                case "MaxErrorsPerDay":
                    e.Value = _info.MaxErrorsPerDay;
                    break;
            }

        }

        private void _SetValue(object sender, PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Currency":
                    _info.Currency = (string) e.Value;
                    break;
                case "MaxErrorsPerDay":
                    _info.MaxErrorsPerDay = (int?)e.Value;
                    break;
            }
        }
    }

}