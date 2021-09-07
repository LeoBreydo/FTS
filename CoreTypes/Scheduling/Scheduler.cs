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
        public Scheduler(TradingConfiguration config)
        {
            List<ScheduledInterval> scheduledIntervals = config.ScheduledIntervals;
            _cdMap.Add(config.Id,CommandDestination.Service);
            foreach (var lex in config.Exchanges)
            {
                _cdMap.Add(lex.Id,CommandDestination.Exchange);
                foreach(var mt in lex.Markets)
                    _cdMap.Add(mt.Id,CommandDestination.Market);
            }
            if (scheduledIntervals == null || scheduledIntervals.Count == 0) return;
            _schedulerTimeStepInMinutes = config.SchedulerTimeStepInMinutes;
            var groups = scheduledIntervals.GroupBy(si => si.TargetId);
            foreach (var g in groups)
            {
                var id = g.Key;
                var intervalList = g.ToList();
                var temp0 = Enumerable.Repeat(TradingRestriction.NoRestrictions, intervalList.Count).ToArray();

                List<(int, DateTime, TradingRestriction)> temp1 = new ();
                var idx = 0;
                foreach (var si in intervalList)
                {
                    if(si.SoftStopTime != null) temp1.Add((idx,si.SoftStopTime.Value,TradingRestriction.SoftStop));
                    temp1.Add((idx,si.HardStopTime,TradingRestriction.HardStop));
                    temp1.Add((idx,si.NoRestrictionTime,TradingRestriction.NoRestrictions));
                    ++idx;
                }
                temp1 = temp1.OrderBy(t => t.Item2).ToList();

                List<(DateTime, TradingRestriction)> temp2 = new();
                var dt = DateTime.MinValue;
                foreach (var t in temp1)
                {
                    if (t.Item2 > dt)
                    {
                        var r = temp0.Max();
                        temp2.Add((dt,r));
                        dt = t.Item2;
                    }
                    temp0[t.Item1] = t.Item3;
                }
                temp2.Add((dt,temp0.Max()));
                temp2.RemoveAt(0);

                foreach (var t in temp2)
                {
                    if(!_timeLine.ContainsKey(t.Item1)) _timeLine.Add(t.Item1, new List<(int id, TradingRestriction tr)>());
                    _timeLine[t.Item1].Add((id,t.Item2));
                }
            }
        }

        private DateTime _lastUsedDateTime = DateTime.MinValue;
        public List<ICommand> GetCommands(DateTime utcNow)
        {
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
