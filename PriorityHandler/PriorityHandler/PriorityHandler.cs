using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriorityHandler
{
    public interface IPriorityItem
    {
        int Priority { get; set; }
    }

    public class PriorityItem<T> : IPriorityItem, IComparable
    {
        #region fields
        private int priority = 0;
        private T item;
        #endregion end of fields

        #region Property
        public int Priority { get => priority; set => priority = value; }
        public T Item { get => item; set => item = value; }
        #endregion End of Property

        #region Constructors
        public PriorityItem() { }

        public PriorityItem(T _item)
        {
            this.Item = _item;
        }
        #endregion End of Constructors

        #region function
        public override string ToString()
        {
            return Item.ToString() + $" - Priority: {Priority}";
        }

        public int CompareTo(T other)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(object obj)
        {
            if (this.priority > ((IPriorityItem)obj).Priority)
                return 1;
            else
                return -1;
        }
        #endregion end of function
    }

    public class Prioritylist<T> : List<T> where T : IPriorityItem
    {
        public void Insert(int index, T item, bool updatePriority = true)
        {
            base.Insert(index, item);
            if (updatePriority)
                UpdatePriority(index);
        }

        private void UpdatePriority(int index)
        {
            var item = base[index];
            if (base.Count == 1)
            {
                //Log 1396/11/06 19:28:51 by [M#] - At first, It seems that 1 billion make it easier to handle future inserts.
                item.Priority = 1000000000;
            }
            else
            {
                int distance = 1;
                while (true)
                {
                    T prev = default(T);
                    T next = default(T);
                    int indexPrev = -1 * (distance - 1), indexNext = base.Count + (distance - 1);
                    if (index - distance < 0)
                    {
                        indexNext = index + distance;
                        next = base[indexNext];
                    }
                    else if (index + distance >= base.Count)
                    {
                        indexPrev = index - distance;
                        prev = base[indexPrev];
                    }
                    else
                    {
                        indexPrev = index - distance;
                        indexNext = index + distance;
                        next = base[indexNext];
                        prev = base[indexPrev];
                    }

                    int priorityPrev = 0, priorityNext = 2000000000;

                    if (prev != null)
                    {
                        priorityPrev = prev.Priority;
                    }
                    if (next != null)
                    {
                        priorityNext = next.Priority;
                    }

                    if (priorityNext - priorityPrev > (distance * 2) - 1)
                    {
                        int j = 1;
                        for (int i = indexPrev + 1; i <= indexNext - 1; i++)
                        {
                            if (i < 0) continue;
                            if (i >= base.Count) continue;
                            base[i].Priority = priorityPrev + ((priorityNext - priorityPrev) / (distance * 2)) * j;
                            j++;
                        }
                        break;
                    }

                    distance++;
                }
            }
        }
    }
}