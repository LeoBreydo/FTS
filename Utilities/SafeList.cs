using System.Collections.Generic;

namespace Utilities
{
    /// <summary>
    /// The thread save set of unduplicated items 
    /// </summary>
    /// <remarks>
    /// Designed to use in operations like registration/unregistration.
    /// An element addition is ignored if the list already contains the adding element.
    /// Also the implementation of the class takes into consideration that the adding/removing of items 
    /// has to be called much less frequently than the members enumeration:
    /// Uses lock in Add/Remove operations; GetItems() is non-locking operation.
    /// </remarks>
    public class SafeList<T>
    {
        /// <summary>
        /// For XmlSerialization (list of items)
        /// </summary>
        public T[] XmlSavedState
        {
            get { return GetItems(); }
            set { AddRange(value); }
        }

        /// <summary>
        /// list to manage add/remove
        /// </summary>
        private readonly List<T> mList = new List<T>();
        /// <summary>
        /// array to enumerate added items
        /// </summary>
        private T[] mItems = new T[0];
        /// <summary>
        /// Get container items
        /// </summary>
        /// <remarks>
        /// non-blocking operation
        /// </remarks>
        public T[] GetItems() { return mItems; }
        /// <summary>
        /// default ctor
        /// </summary>
        public SafeList()
        {

        }
        /// <summary>
        /// ctor with initial values to add
        /// </summary>
        public SafeList(IEnumerable<T> initialValues)
        {
            foreach (T item in initialValues)
            {
                if (!mList.Contains(item))
                    mList.Add(item);
            } mItems = mList.ToArray();
        }
        /// <summary>
        /// Returns true if item is included to container
        /// </summary>
        /// <remarks>
        /// blocking operation
        /// </remarks>
        public bool Contains(T item)
        {
            lock (mList)
            {
                return mList.Contains(item);
            }
        }
        /// <summary>
        /// Add item to container
        /// </summary>
        /// <remarks>
        /// blocking operation
        /// </remarks>
        public void Add(T item)
        {
            lock (mList)
            {
                if (mList.Contains(item)) return;

                mList.Add(item);
                mItems = mList.ToArray();
            }
        }
        /// <summary>
        /// Add items to container
        /// </summary>
        /// <remarks>
        /// blocking operation
        /// </remarks>
        public void AddRange(T[] items)
        {
            if (items.Length == 0) return;
            lock (mList)
            {
                foreach (T item in items)
                {
                    if (!mList.Contains(item))
                        mList.Add(item);
                }
                mItems = mList.ToArray();
            }
        }

        /// <summary>
        /// Remove item from container
        /// </summary>
        /// <remarks>
        /// blocking operation
        /// </remarks>
        public void Remove(T item)
        {
            lock (mList)
                if (mList.Contains(item))
                {
                    mList.Remove(item);
                    mItems = mList.ToArray();
                }
        }
        /// <summary>
        /// Remove all items from container
        /// </summary>
        /// <remarks>
        /// blocking operation
        /// </remarks>
        public void Clear()
        {
            lock (mList)
            {
                mList.Clear();
                mItems = new T[0];
            }
        }
    }
}
