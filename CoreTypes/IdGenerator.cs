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

    public static class ClientOIdProvider
    {
        private static readonly IdGenerator _clOrderIdGenerator;

        static ClientOIdProvider()
        {
            _clOrderIdGenerator = new((int) ((DateTime.Now - new DateTime(2021, 7, 1)).TotalSeconds / 10));
        }

        public static int GetNextClientOrderId() => _clOrderIdGenerator.GetNextId();
    }
}
