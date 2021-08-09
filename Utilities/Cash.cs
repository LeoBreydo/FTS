using System;
using System.Collections.Generic;

namespace Utilities
{
    /// <summary>
    /// Кэш
    /// </summary>
    /// <remarks>
    /// Аналог мапа, но с фиксированным capacity. При достижении count=capacity при добавлении нового элемента забывает про элемент, который не спрашивали дольше всего.
    /// Не потокобезопасный
    /// </remarks>
    /// <typeparam name="TKey">Ключ</typeparam>
    /// <typeparam name="TValue">Значение</typeparam>
    public class Cash<TKey,TValue>
    {
        class Item
        {
            public TKey Key;
            public TValue Value;
            public DateTime utcLastGetTime;
        }

        private readonly int _capacity;
        private readonly Dictionary<TKey, Item> Items = new();

        public Cash(int capacity=100)
        {
            if (capacity<=0)throw new Exception();
            _capacity = capacity;
        }
        public TValue Get(TKey key)
        {
            if(!Items.TryGetValue(key, out var item))
                return default;

            item.utcLastGetTime = DateTime.UtcNow;
            return item.Value;
        }
        public void Remove(TKey key)
        {
            Items.Remove(key);
        }
        public void Set(TKey key,TValue value)
        {
            if (!Items.ContainsKey(key) &&Items.Count==_capacity)
            {
                // удалить элемент,который дольше всех не использовался
                Item itemToRemove = null;
                foreach(Item item in Items.Values)
                {
                    if(itemToRemove==null || itemToRemove.utcLastGetTime>item.utcLastGetTime)
                        itemToRemove = item;
                }
                if (itemToRemove!=null)
                    Items.Remove(itemToRemove.Key);
            }
            
            Items[key] = new Item {Key = key, Value = value, utcLastGetTime = DateTime.UtcNow};
        }
    }
}
