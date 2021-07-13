using System.Collections.Generic;

namespace CommonStructures
{
    public interface IDisabledStrategies
    {
        void RemoveAllRestrictions();
        void SetDisabled(bool isDisabled, params long[] strategyIds);
        bool IsStrategyDisabled(long strategyId);
        void Save();
        void ExcludeObsoletteStrategies(IEnumerable<long> aliveStrategies);
    }
}
