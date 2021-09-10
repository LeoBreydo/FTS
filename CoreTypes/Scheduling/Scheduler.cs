using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreTypes
{
    public class Scheduler
    {
        private readonly SortedList<DateTime, List<(int id, TradingRestriction tr)>> _timeLine = new ();
        private readonly SortedList<int, CommandDestination> _cdMap = new();
        private readonly int _schedulerTimeStepInMinutes;
        private bool _firstCall = true;
        private readonly bool _isEmpty;
        public Scheduler(TradingConfiguration config)
        {
            var scheduledIntervals = config.ScheduledIntervals;
            if (scheduledIntervals == null || scheduledIntervals.Count == 0)
            {
                _isEmpty = true;
                return;
            }

            _cdMap.Add(config.Id,CommandDestination.Service);
            foreach (var lex in config.Exchanges)
            {
                _cdMap.Add(lex.Id,CommandDestination.Exchange);
                foreach(var mt in lex.Markets)
                    _cdMap.Add(mt.Id,CommandDestination.Market);
            }
            
            _schedulerTimeStepInMinutes = config.SchedulerTimeStepInMinutes;
            var groups = scheduledIntervals.GroupBy(si => si.TargetId);

            foreach (var g in groups)
            {
                var id = g.Key;
                if (!_cdMap.ContainsKey(id)) continue;

                var temp0 = FirstStep(g, out var temp1);
                var temp2 = SecondStep(temp1, temp0);
                foreach (var (dt, tr) in temp2)
                {
                    if(!_timeLine.ContainsKey(dt)) _timeLine.Add(dt, new List<(int, TradingRestriction)>());
                    _timeLine[dt].Add((id,tr));
                }
            }

            _isEmpty = _timeLine.Count == 0;
        }

        private static TradingRestriction[] FirstStep(IEnumerable<ScheduledInterval> g, out List<(int, DateTime, TradingRestriction)> temp1)
        {
            var intervalList = g.ToList();
            var temp0 = Enumerable.Repeat(TradingRestriction.NoRestrictions, intervalList.Count).ToArray();
            temp1 = new();
            var idx = 0;
            foreach (var si in intervalList)
            {
                if (si.SoftStopTime != null) temp1.Add((idx, si.SoftStopTime.Value, TradingRestriction.SoftStop));
                temp1.Add((idx, si.HardStopTime, TradingRestriction.HardStop));
                temp1.Add((idx, si.NoRestrictionTime, TradingRestriction.NoRestrictions));
                ++idx;
            }
            temp1 = temp1.OrderBy(t => t.Item2).ToList();
            return temp0;
        }
        private static List<(DateTime, TradingRestriction)> SecondStep(List<(int, DateTime, TradingRestriction)> temp1, IList<TradingRestriction> temp0)
        {
            List<(DateTime, TradingRestriction)> temp2 = new();
            var dt = DateTime.MinValue;
            foreach (var (idx, dateTime, tradingRestriction) in temp1)
            {
                if (dateTime > dt)
                {
                    var r = temp0.Max();
                    temp2.Add((dt, r));
                    dt = dateTime;
                }
                temp0[idx] = tradingRestriction;
            }
            temp2.Add((dt, temp0.Max()));
            temp2.RemoveAt(0);
            return temp2;
        }

        
        private DateTime _lastUsedDateTime = DateTime.MinValue;
        public List<ICommand> GetCommands(DateTime utcNow)
        {
            if (_isEmpty) return new();
            List<(int id, TradingRestriction tr)> lst = null;
            if (_firstCall)
            {
                _firstCall = false;
                var keys = _timeLine.Keys.ToArray();
                var maxKey = Array.BinarySearch(keys, utcNow);
                if (maxKey >= 0) lst = _timeLine[keys[maxKey]];
                else
                {
                    var k = ~maxKey - 1;
                    if (k >= 0) lst = _timeLine[keys[k]];
                }
            }
            else
            {
                if (utcNow.Second == 0)
                {
                    var n = utcNow.Minute;
                    var q = n / _schedulerTimeStepInMinutes;
                    var dt = utcNow.AddMinutes(_schedulerTimeStepInMinutes * q - n);
                    if (dt > _lastUsedDateTime)
                    {
                        _lastUsedDateTime = dt;
                        if (_timeLine.ContainsKey(dt)) lst = _timeLine[dt];
                    }
                }
            }

            return lst == null || lst.Count == 0 
                ? new() 
                : lst.Select(t => new RestrictionCommand(_cdMap[t.id], CommandSource.Scheduler, t.id, t.tr))
                    .Cast<ICommand>()
                    .ToList();
        }
    }
}
