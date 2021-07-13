using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CommonStructures
{
    public class DisabledStrategies : IDisabledStrategies
    {
        private readonly string _disabledStrategiesFileName;
        private readonly bool _saveWhenDisabledStateChanged;
        private readonly List<long> listDisabledStrategies = new List<long>();
        public DisabledStrategies(string disabledStrategiesFileName = null, bool saveWhenDisabledStateChanged = true)
        {
            _disabledStrategiesFileName = disabledStrategiesFileName;
            _saveWhenDisabledStateChanged = saveWhenDisabledStateChanged;

            if (_disabledStrategiesFileName != null)
            {
                if (!File.Exists(disabledStrategiesFileName))
                {
                    MakeSureDirectoryExists(Path.GetDirectoryName(_disabledStrategiesFileName));
                    File.WriteAllText(_disabledStrategiesFileName, ""); // save empty content to the file to make sure that the write operation is not locked by user credentials

                }
                else
                {
                    foreach (string row in File.ReadAllLines(_disabledStrategiesFileName))
                    {
                        if (string.IsNullOrEmpty(row)) continue;
                        if (long.TryParse(row, out var strategyId))
                        {
                            if (!listDisabledStrategies.Contains(strategyId))
                                listDisabledStrategies.Add(strategyId);
                        }
                    }
                }
            }
        }

        private static void MakeSureDirectoryExists(string DirName)
        {
            if (!Directory.Exists(DirName))
                Directory.CreateDirectory(DirName);
        }

        public void RemoveAllRestrictions()
        {
            int L = listDisabledStrategies.Count;
            listDisabledStrategies.Clear();
            if (_saveWhenDisabledStateChanged && L>0)
                Save();
        }
        public void SetDisabled(bool isDisabled, params long[] strategyIds)
        {
            bool updated = false;
            if (isDisabled)
            {
                foreach (long strategyId in strategyIds)
                    if (!listDisabledStrategies.Contains(strategyId))
                    {
                        listDisabledStrategies.Add(strategyId);
                        updated = true;
                    }
            }
            else
            {
                int L = listDisabledStrategies.Count;
                listDisabledStrategies.RemoveAll(strategyId => strategyIds.Any(id => id == strategyId));
                updated = listDisabledStrategies.Count != L;
            }
            if (_saveWhenDisabledStateChanged && updated)
                Save();
        }

        public void Save()
        {
            if (_disabledStrategiesFileName != null)
                File.WriteAllText(_disabledStrategiesFileName, string.Join("\n", listDisabledStrategies.Select(id => id.ToString())));
        }
        public bool IsStrategyDisabled(long strategyId)
        {
            return listDisabledStrategies.Contains(strategyId);
        }

        public void ExcludeObsoletteStrategies(IEnumerable<long> aliveStrategies)
        {
            var listAlive = aliveStrategies.ToList();
            int countBeforeClean = listDisabledStrategies.Count;
            if (countBeforeClean == 0) return;
            listDisabledStrategies.RemoveAll(id => !listAlive.Contains(id));

            if (listDisabledStrategies.Count != countBeforeClean)
                Save();
        }
    }
}