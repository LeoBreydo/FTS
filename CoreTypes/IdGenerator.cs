using System;
using System.Threading;

namespace CoreTypes
{
    public class IdGenerator
    {
        private int _seed;
        public IdGenerator(int seed)
        {
            _seed = seed;
        }

        public int GetNextId() => Interlocked.Increment(ref _seed);
    }

    public static class IdGenerators
    {
        private static readonly IdGenerator _clOrdreIdGenerator;
        private static readonly IdGenerator _internalIdGenerator;

        static IdGenerators()
        {
            _internalIdGenerator = new(0);
            _clOrdreIdGenerator = new((int) (DateTime.Now - new DateTime(2021, 7, 1)).TotalSeconds / 10);
        }

        public static int GetNextClientOrderId() => _clOrdreIdGenerator.GetNextId();
        public static int GetNextInternalId() => _internalIdGenerator.GetNextId();

    }
}
