using System;

namespace MemoScope.Core.Dac
{
    public static class DacFinderFactory
    {
        public static AbstractDacFinder CreateDactFinder(string localCache) => Environment.Is64BitProcess ? new DacFinder64(localCache) : (AbstractDacFinder)new DacFinder32(localCache);
    }
}