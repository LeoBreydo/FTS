using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CommonStructures
{
    public static class TagsHelper
    {
        public static string WrapTextTag(this string tagValue)
        {
            return tagValue.ToJson();
        }
        public static string UnwrapTextTag(this string tagValueWithoutQuotes)
        {
            return tagValueWithoutQuotes.FromJson<string>();
        }
        public static IEnumerable<Tuple<string, string>> SplitTags(this string str)
        {
            foreach (string nameVal in SplitCommas(str))
            {
                if (!string.IsNullOrEmpty(nameVal))
                {
                    int ixEq = nameVal.IndexOf('=');
                    if (ixEq < 0)
                        yield return new Tuple<string, string>(nameVal.Trim(), string.Empty);
                    else
                        yield return new Tuple<string, string>(nameVal[..ixEq].Trim(), nameVal[(ixEq + 1)..]);
                }
            }
        }
        private static IEnumerable<string> SplitCommas(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                int L = str.Length;
                int pos = 0;
                while (pos<L)
                {
                    int comma = GetNextComma(str, L, pos);
                    if (pos >= 0 && str.Length > pos) yield return str[pos..comma];
                    pos = comma + 1;
                }
            }
        }
        private static int GetNextComma(string str, int L, int pos)
        {
            for(int i=pos;i<L;)
            {
                switch (str[i])
                {
                    case ',':
                        return i;
                    case '[':
                    case '{':
                    case '(':
                        i = ClosingBracket(str, L, i) + 1;
                        break;
                    case '\"':
                    case '\'':
                        i = ClosingQuote(str, L,i) + 1;
                        break;
                    default:
                        ++i;
                        break;
                }
            }
            return L;
        }
        private static int ClosingBracket(string str,int L,int pos)
        {
            var openBrakets=new List<char>();
            int LBX = -1;
            for(int i=pos;i<L;++i)
            {
                char ch = str[i];
                switch (ch)
                {
                    case '\\':
                        ++i;
                        break;
                    case '[':
                        openBrakets.Add(']');
                        ++LBX;
                        break;
                    case '{':
                        openBrakets.Add('}');
                        ++LBX;
                        break;
                    case '(':
                        openBrakets.Add(')');
                        ++LBX;
                        break;
                    case '\"':
                    case '\'':
                        i = ClosingQuote(str, L, i) + 1;
                        break;

                    default:
                        if (ch==openBrakets[LBX])
                        {
                            openBrakets.RemoveAt(LBX);
                            if (--LBX<0) return i;
                        }
                        break;
                }
            }
            return L;
        }


        private static int ClosingQuote(string str, int L, int pos)
        {
            char quote = str[pos];
            for(int i=pos+1;i<L;++i)
            {
                char ch = str[i];
                if (ch=='\\')
                    ++i;
                else if (ch==quote)
                    return i;
            }
            return L;
        }

    }

    public class UnwrappedTags
    {
        public static UnwrappedTags Empty = new("");
        public readonly string Text;
        private Tuple<string, string>[] _tagsFields;
        public UnwrappedTags(string text)
        {
            Text = text??"";
        }

        public Tuple<string, string>[] GetAllTags()
        {
            return _tagsFields ?? (_tagsFields = Text.SplitTags().ToArray());
        }

        public string GetTagValue(string tagName)
        {
            return GetTagValue(tagName, FillsLogFormat.IsTextTag(tagName));
        }
        public string GetTagValue(string tagName, bool unwrapTextTag, bool toReturnNullIfNoTag = true)
        {
            var t = GetAllTags().FirstOrDefault(tf => string.Equals(tf.Item1, tagName,StringComparison.OrdinalIgnoreCase));
            if (t == null) return toReturnNullIfNoTag?null:string.Empty;

            return unwrapTextTag? t.Item2.UnwrapTextTag(): t.Item2;
        }

        public bool TryGetTagValueLong(string tagName, out long value)
        {
            string strVal = GetTagValue(tagName, false);
            if (string.IsNullOrEmpty(strVal))
            {
                value = 0;
                return false;
            }
            return long.TryParse(strVal, out value);
        }
        public bool TryGetTagValueDouble(string tagName, out double value)
        {
            string strVal = GetTagValue(tagName, false);
            if (string.IsNullOrEmpty(strVal))
            {
                value = 0;
                return false;
            }
            return double.TryParse(strVal, NumberStyles.Any, null, out value);
        }
        public bool TryGetTagValueDateTime(string tagName, out DateTime value)
        {
            string strVal = GetTagValue(tagName, false);
            if (string.IsNullOrEmpty(strVal))
            {
                value = DateTime.MinValue;
                return false;
            }
            return strVal.TryParseDateTime(out value);
        }
        public bool TryGetTagValueTimeStamp(string tagName, out DateTime value)
        {
            string strVal = GetTagValue(tagName, false);
            if (strVal == null)
            {
                value = DateTime.MinValue;
                return false;
            }
            return DateTime.TryParse(strVal, out value);
        }
        public bool TryGetTagValueBool(string tagName,out bool value)
        {
            string strVal = GetTagValue(tagName, false);
            if (string.IsNullOrEmpty(strVal))
            {
                value = false;
                return false;
            }
            if (string.Equals(strVal,"true",StringComparison.OrdinalIgnoreCase))
            {
                value = true;
                return true;
            }
            if (string.Equals(strVal, "false", StringComparison.OrdinalIgnoreCase))
            {
                value = false;
                return true;
            }
            value = false;
            return false;
        }

        public long GetTagValueLong(string tagName)
        {
            if (!TryGetTagValueLong(tagName, out var res))
                throw new Exception(string.Format("Tag '{0}' is not specified or invalid", tagName));
            return res;
        }
        public double GetTagValueDouble(string tagName)
        {
            if (!TryGetTagValueDouble(tagName, out var res))
                throw new Exception(string.Format("Tag '{0}' is not specified or invalid", tagName));
            return res;
        }
        public bool GetTagValueBool(string tagName)
        {
            string strVal = GetTagValue(tagName, false);
            if (string.IsNullOrEmpty(strVal))
                throw new Exception($"Tag '{tagName}' is not specified or invalid");
            return strVal.ToLower() switch
            {
                "true" => true,
                "false" => false,
                _ => throw new Exception($"Tag '{tagName}' is not specified or invalid")
            };
        }
        public bool GetTagValueBool(string tagName,bool toReturnIfTagNotExists)
        {
            string strVal = GetTagValue(tagName, false);
            if (strVal == null)
                return toReturnIfTagNotExists;
            return strVal.ToLower() switch
            {
                "true" => true,
                "false" => false,
                _ => throw new Exception(string.Format("Tag '{0}' is not specified or invalid", tagName))
            };
        }

        public DateTime GetTagValueTimeStamp(string tagName)
        {
            if (!TryGetTagValueTimeStamp(tagName, out var res))
                throw new Exception($"Tag '{tagName}' is not specified or invalid");
            return res;
        }

    }
    /// <summary>
    /// Specifies the format of the fills log files
    /// </summary>
    public static class FillsLogFormat
    {
        public static readonly string Title = string.Join("\t", new[]
                                                                       {
                                                                           "Time",
                                                                           "Symbol",
                                                                           "Account",
                                                                           "Strategy",
                                                                           "SymbolOpenedPos",
                                                                           "StrategyOpenedPos",
                                                                           "RecordType",
                                                                           "FilledAmount",
                                                                           "FilledPrice",
                                                                           "TransactTime",
                                                                           "Tags"
                                                                       });
        public static string ToTextTableRow(this ExecutionInfo executionInfo, string delimiter = "\t")
        {
            return string.Join("\t", new[]
                                         {
                                             executionInfo.Time.ToString(),
                                             executionInfo.Symbol,
                                             executionInfo.Account,
                                             executionInfo.StrategyID.ToString(),
                                             executionInfo.SymbolOpenedPosition.ToString(),
                                             executionInfo.StrategyOpenedPosition.ToString(),
                                             executionInfo.Type.ToString(),
                                             executionInfo.FilledAmount.ToString(),
                                             executionInfo.FilledPrice.ToString(),
                                             executionInfo.TransactTime.ToString(),
                                             executionInfo.TagsText
                                         }
                );
        }

        public static DateTime GetTimeOfTheMessage(string row, char delimiter = '\t')
        {
            return DateTime.Parse(row[..row.IndexOf(delimiter)]).ToUniversalTime();
        }
        public static ExecutionInfo FromTextTableRow(string row,char delimiter='\t')
        {
            string[] cells = row.Split(delimiter);
            if (cells.Length == 10)
            {
                // back compability with the old format (when had no accounts)
                return new ExecutionInfo
                           {
                               Time = DateTime.Parse(cells[0]),
                               Symbol = cells[1],
                               StrategyID = long.Parse(cells[2]),
                               SymbolOpenedPosition = long.Parse(cells[3]),
                               StrategyOpenedPosition = long.Parse(cells[4]),
                               Type = ParseExecutionInfoType(cells[5]),
                               FilledAmount = long.Parse(cells[6]),
                               FilledPrice = Math.Round(double.Parse(cells[7], CultureInfo.InvariantCulture), 10),
                               // the decimal error is possible when parse called
                               TransactTime = DateTime.Parse(cells[8]),
                               TagsText = cells[9]
                           };
            }
            return new ExecutionInfo
            {
                Time = DateTime.Parse(cells[0]),
                Symbol = cells[1],
                Account = cells[2],
                StrategyID = long.Parse(cells[3]),
                SymbolOpenedPosition = long.Parse(cells[4]),
                StrategyOpenedPosition = long.Parse(cells[5]),
                Type = ParseExecutionInfoType(cells[6]),
                FilledAmount = long.Parse(cells[7]),
                FilledPrice = Math.Round(double.Parse(cells[8], CultureInfo.InvariantCulture), 10),
                // the decimal error is possible when parse called
                TransactTime = DateTime.Parse(cells[9]),
                TagsText = cells[10]
            };

        }
        private static ExecutionInfoType ParseExecutionInfoType(string strType)
        {
            return strType switch
            {
                "WorkingState" => ExecutionInfoType.StateUpdated,
                _ => (ExecutionInfoType) Enum.Parse(typeof(ExecutionInfoType), strType)
            };
        }

        public const string TAG_BrokerID = "BrokerID";
        public const string TAG_ClOrdID = "ClOrdID";
        public const string TAG_OrderID = "OrderID";
        public const string TAG_ExecID = "ExecID";

        public const string TAG_GenOrderTime = "GenOrderTime";
        public const string TAG_GenOrderReason = "GenOrderReason";
        public const string TAG_AmountOrderedByStrategy = "AmountOrderedByStrategy";
        public const string TAG_GenOrderProviderID = "GenOrderProviderID";
        public const string TAG_GenOrderProviderBid = "GenOrderProviderBid";
        public const string TAG_GenOrderProviderAsk = "GenOrderProviderAsk";
        public const string TAG_GenOrderProviderPrice = "GenOrderProviderPrice";


        public const string TAG_SendOrderTime = "SendOrderTime";
        public const string TAG_PriceWhenSendOrder = "PriceWhenSendOrder";
        public const string TAG_BidWhenSendOrder = "BidWhenSendOrder";
        public const string TAG_AskWhenSendOrder = "AskWhenSendOrder";
        public const string TAG_AmountSpecifiedInTheOrder = "AmountSpecifiedInTheOrder";

        public const string TAG_OrderAcceptedBrokerTime = "OrderAcceptedBrokerTime";
        public const string TAG_OrderAcceptedLocalTime = "OrderAcceptedLocalTime";

        public const string TAG_FillsReceivedLocalTime = "FillsReceivedLocalTime";
        public const string TAG_ExternalInput = "ExternalInput";
        public const string TAG_WorkingState = "WorkingState";

        public const string TAG_ApplyError = "ApplyError";
        public const string TAG_RejReason = "RejReason";
        public const string TAG_RejectionBrokerTime = "RejectionBrokerTime";
        public const string TAG_RejectionLocalTime = "RejectionLocalTime";
        //public const string TAG_OrderStartTime = "OrderStartTime";
        public const string TAG_NonFilledOrderedAmount = "NonFilledOrderedAmount";

        /// <summary>
        ///  the value of the tag TAG_ApplyError in the case when the cancellation but not the real fills
        /// </summary>
        public const string ApplyErrorValue_CANCELLATION_FILLS = "CancellationFills";
        /// <summary>
        ///  the value of the tag TAG_ExternalInput in the case when the fills added to the system manually but not from TradingServer
        /// </summary>
        public const string ExternalInput_ManualFills = "ManualFills";
        /// <summary>
        ///  the value of the tag TAG_ExternalInput in the case when the order send initiated ouside the TradingServer
        /// </summary>
        public const string ExternalInput_ManualExecution = "ManualExecution";

        public const string TAG_BindedToRealStrategyWithID = "BindedToRealStrategyWithID";
        public const string TAG_SynchronizeActivityWithRealStrategy = "SynchronizeActivityWithRealStrategy";


        public static bool IsTextTag(string tagName)
        {
            return tagName switch
            {
                TAG_ApplyError => true,
                TAG_RejReason => true,
                _ => false
            };
        }


        public static void AddTag(this StringBuilder sb, string tagName, string tagValue)
        {
            if (IsTextTag(tagName))
                tagValue = tagValue.WrapTextTag();

            sb.AppendFormat(sb.Length == 0 ? "{0}={1}" : ", {0}={1}", tagName, tagValue);
        }



    }
}