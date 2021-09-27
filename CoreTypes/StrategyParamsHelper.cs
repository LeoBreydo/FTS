using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreTypes
{
    public static class StrategyParamsHelper
    {
        public static List<string> GetInstrumentsFromStrategyParams(this StrategyParameters parameters)
        {
            return parameters.Parameters.Where(p => p.Name.StartsWith("Timeframe", StringComparison.OrdinalIgnoreCase))
                .Select(tf=>ExtractInstrumentNameFromParam(tf.Value).Trim())
                .Where(instr => !string.IsNullOrEmpty(instr))
                .ToList();
        }

        private static string ExtractInstrumentNameFromParam(string tfParam)
        {
            // instrument is expected in square brackets like '[MKT]' or '[MKT]:t:imeframe'
            if (tfParam.StartsWith("["))
            {
                var closeIndex=tfParam.IndexOf(']');
                if (closeIndex < 0) closeIndex = tfParam.Length;
                return tfParam.Substring(1, closeIndex - 1).ToUpper();
            }
            // compatibility with old versions
            int ixSep = tfParam.IndexOf(':'); // new used style like 'MKT:t:imeframe'
            int ixSep2 = tfParam.IndexOf('.'); // old style like 'MKT.t:imeframe'
            if (ixSep < 0) ixSep = tfParam.Length;
            if (ixSep2 < 0) ixSep2 = tfParam.Length;
            var ret = tfParam.Substring(0, Math.Min(ixSep, ixSep2)).Trim();
            if (IsIdentifier(ret) && !IsTimeframeOrBarFormingPolicy(ret))
                return ret;
            return null;
        }

        private static bool IsIdentifier(string arg)
        {
            if (string.IsNullOrEmpty(arg)) return false;
            bool bFirstCh = true;
            foreach (char ch in arg)
            {
                if (bFirstCh)
                {
                    bFirstCh = false;
                    if (!(char.IsLetter(ch) || ch == '_')) return false;
                }
                else
                {
                    if (!(char.IsLetterOrDigit(ch) || ch == '_'))
                        return false;
                }
            }
            return true;
        }
        private static bool IsTimeframeOrBarFormingPolicy(string arg)
        {
            switch (arg.ToLower())
            {
                case "": 
                    return false;
                // bar forming policies
                case "b":
                case "a":
                case "m":
                case "t":
                    return true;
            }

            if (int.TryParse(arg, out int nbr) && nbr > 0) return true;
            switch (char.ToLower(arg[0]))
            {
                case 's':
                case 'h':
                case 'd':
                case 'w':
                    return (int.TryParse(arg.Substring(1), out nbr) && nbr > 0);
            }

            return false;
        }

    }
}