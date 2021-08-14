﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CoreTypes
{
    public class InfoLogger
    {
        private readonly BlockingCollection<string> _tiList = new();
        private readonly BlockingCollection<string> _barList = new();
        private readonly BlockingCollection<string> _tradeList = new();
        private readonly BlockingCollection<string> _orderStateMessages = new();
        private readonly BlockingCollection<string> _msgList = new();
        private readonly BlockingCollection<string> _errorsList = new();

        private readonly int _saveEveryNthMinute;
        private DateTime _lastSaveTime;

        private readonly string _pathPrefix;

        // pathPrefix MUST be valid!!!
        public InfoLogger(int saveEveryNthMinute, string pathPrefix)
        {
            if (saveEveryNthMinute < 5) saveEveryNthMinute = 5;
            _saveEveryNthMinute = saveEveryNthMinute;
            _lastSaveTime = DateTime.UtcNow.AddMinutes(-_saveEveryNthMinute);
            _pathPrefix = pathPrefix;
            if (!File.Exists(_pathPrefix + ".ticks.txt"))
            {
                var sw = File.CreateText(_pathPrefix + ".ticks.txt");
                sw.Close();
                sw.Dispose();
            }
            if (!File.Exists(_pathPrefix + ".bars.txt"))
            {
                var sw = File.CreateText(_pathPrefix + ".bars.txt");
                sw.Close();
                sw.Dispose();
            }
            if (!File.Exists(_pathPrefix + ".trades.txt"))
            {
                var sw = File.CreateText(_pathPrefix + ".trades.txt");
                sw.Close();
                sw.Dispose();
            }
            if (!File.Exists(_pathPrefix + ".reports.txt"))
            {
                var sw = File.CreateText(_pathPrefix + ".reports.txt");
                sw.Close();
                sw.Dispose();
            }
            if (!File.Exists(_pathPrefix + ".messages.txt"))
            {
                var sw = File.CreateText(_pathPrefix + ".messages.txt");
                sw.Close();
                sw.Dispose();
            }
            if (!File.Exists(_pathPrefix + ".errors.txt"))
            {
                var sw = File.CreateText(_pathPrefix + ".errors.txt");
                sw.Close();
                sw.Dispose();
            }
        }

        ~InfoLogger()
        {
            _tiList?.CompleteAdding();
            _tiList?.Dispose();

            _barList?.CompleteAdding();
            _barList?.Dispose();

            _tradeList?.CompleteAdding();
            _tradeList?.Dispose();

            _orderStateMessages?.CompleteAdding();
            _orderStateMessages?.Dispose();

            _msgList?.CompleteAdding();
            _msgList?.Dispose();

            _errorsList?.CompleteAdding();
            _errorsList?.Dispose();
        }

        public void PostToLog(DateTime utcNow, List<string> tickInfoList, List<string> newBars,
            List<string> newTrades, List<OrderStateMessage> orderStateMessageList,
            List<Tuple<string, string>> textMessageList, List<(string,string,string)> errorMessages)
        {
            var dt = $"{utcNow:yyyyMMdd:HHmmss}";
            foreach(var item in tickInfoList)_tiList.Add(item);
            foreach(var item in newBars) _barList.Add(item);
            foreach(var item in newTrades) _tradeList.Add(item);
            foreach(var item in orderStateMessageList) _orderStateMessages.Add(item.ToString());
            foreach(var (item1, item2) in textMessageList) _msgList.Add($"{dt} {item1} : {item2}");
            foreach (var (item1, item2, item3) in errorMessages) _errorsList.Add($"{dt} {item1} {item2} : {item3}");

            if ((utcNow - _lastSaveTime).TotalMinutes >= _saveEveryNthMinute)
            {
                _lastSaveTime = utcNow;
                Flush();
            }
        }

        public void Flush()
        {
            Task.Factory.StartNew(() =>
            {
                using (var sw = File.AppendText(_pathPrefix + ".ticks.txt"))
                {
                    var cnt = _tiList.Count;
                    var consumed = 0;
                    if (cnt > 0)
                        foreach (var ti in _tiList.GetConsumingEnumerable())
                        {
                            sw.WriteLine(ti);
                            if (++consumed == cnt) break;
                        }
                }
                using (var sw = File.AppendText(_pathPrefix + ".bars.txt"))
                {
                    var cnt = _barList.Count;
                    var consumed = 0;
                    if (cnt > 0)
                        foreach (var b in _barList.GetConsumingEnumerable())
                        {
                            sw.WriteLine(b);
                            if (++consumed == cnt) break;
                        }
                }
                using (var sw = new StreamWriter(_pathPrefix + ".trades.txt"))
                {
                    var cnt = _tradeList.Count;
                    var consumed = 0;
                    if (cnt > 0)
                        foreach (var t in _tradeList.GetConsumingEnumerable())
                        {
                            sw.WriteLine(t);
                            if (++consumed == cnt) break;
                        }
                }
                using (var sw = new StreamWriter(_pathPrefix + ".reports.txt"))
                {
                    var cnt = _orderStateMessages.Count;
                    var consumed = 0;
                    if (cnt > 0)
                        foreach (var r in _orderStateMessages.GetConsumingEnumerable())
                        {
                            sw.WriteLine(r);
                            if (++consumed == cnt) break;
                        }
                }
                using (var sw = new StreamWriter(_pathPrefix + ".messages.txt"))
                {
                    var cnt = _msgList.Count;
                    var consumed = 0;
                    if (cnt > 0)
                        foreach (var m in _msgList.GetConsumingEnumerable())
                        {
                            sw.WriteLine(m);
                            if (++consumed == cnt) break;
                        }
                }
                using (var sw = new StreamWriter(_pathPrefix + ".errors.txt"))
                {
                    var cnt = _errorsList.Count;
                    var consumed = 0;
                    if (cnt > 0)
                        foreach (var e in _errorsList.GetConsumingEnumerable())
                        {
                            sw.WriteLine(e);
                            if (++consumed == cnt) break;
                        }
                }
            });
        }
    }
}