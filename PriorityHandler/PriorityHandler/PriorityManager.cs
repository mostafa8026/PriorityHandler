using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PriorityHandler
{
    public class PriorityManager<T> where T : IPriorityItem, ICloneable
    {
        #region functions
        public void PriorityInsert(BindingList<T> t, int index, T item, bool updatePriority = true)
        {
            t.Insert(index, item);
            if (updatePriority)
                UpdatePriority(t, index);
        }

        public void Swap(BindingList<T> t, int indexA, int indexB)
        {
            UpdatePriority(t, indexA, indexB);
            UpdatePriority(t, indexB, indexA);
        }

        public List<T> MoveUp(BindingList<T> t, int index)
        {
            var uppert = t.Where(x => x.Priority < t[index].Priority).OrderByDescending(x=>x.Priority).FirstOrDefault<T>();
            if(uppert != null)
            {
                return UpdatePriority(t, t.IndexOf(uppert), index);
            }

            return new List<T>();
        }

        public List<T> GoToFirst(BindingList<T> t, int index)
        {
            var firstIndex = t.OrderBy(x => x.Priority).FirstOrDefault<T>();
            if(firstIndex != null)
            {
                return UpdatePriority(t, t.IndexOf(firstIndex), index);
            }

            return new List<T>();
        }

        public List<T> MoveDown(BindingList<T> t, int index)
        {
            var lowert = t.Where(x => x.Priority > t[index].Priority).OrderBy(x => x.Priority).FirstOrDefault<T>();
            if (lowert != null)
            {
                return UpdatePriority(t, t.IndexOf(lowert), index);
            }

            return new List<T>();
        }

        public List<T> GoToLast(BindingList<T> t, int index)
        {
            var lastIndex = t.OrderByDescending(x => x.Priority).FirstOrDefault<T>();
            if(lastIndex != null)
            {
                return UpdatePriority(t, t.IndexOf(lastIndex), index);
            }

            return new List<T>();
        }

        /// <summary>
        /// Update the priority of the source index based on the destination
        /// </summary>
        /// <param name="list">your list</param>
        /// <param name="toIndex">index of the item that your destination (the index of desired priority)</param>
        /// <param name="fromIndex">index of the source item (the item you want to change its priority). if not defined (-1), then the item of sourceIndex updated in-place (the algorithm supposes that the source item is in its place and update the upper and lower items, and change its priority to something correct)</param>
        /// <returns>changed items (the items that their priority was changed)</returns>
        public static List<T> UpdatePriority(BindingList<T> list, int toIndex, int fromIndex = -1)
        {
            var clonedList = list.ToList().Select(x => x.Clone()).OfType<T>().ToList();
            T temph = default(T);
            if (fromIndex >= 0)
                temph = clonedList[fromIndex];
            var tempi = clonedList[toIndex];
            int previ = toIndex;
            int prevh = fromIndex;
            if(fromIndex != -1)
                clonedList = clonedList.OrderBy(x => x.Priority).ToList();
            if (fromIndex >= 0)
                fromIndex = clonedList.IndexOf(temph);
            toIndex = clonedList.IndexOf(tempi);
            if (fromIndex >= 0)
            {
                clonedList.Remove(temph);
                clonedList.Insert(toIndex, temph);
                
            }

            List<T> lst_ChangeItem = new List<T>();
            if (clonedList.Count == 1)
            {
                var item = clonedList[0];
                if (fromIndex >= 0)
                {
                    if (list[prevh].Priority > list[previ].Priority)
                        clonedList[0].Priority = list[previ].Priority / 2;
                    else
                        clonedList[0].Priority = list[previ].Priority + (2000000000 - list[previ].Priority) / 2;
                }
                else
                {
                    //Log 1396/11/06 19:28:51 by [M#] - At first, It seems that 1 billion make it easier to handle future inserts.
                    item.Priority = 1000000000;
                }
                //if (sourceIndex > destinationIndex && destinationIndex >= 0) sourceIndex--;
                lst_ChangeItem.Add(item);
            }
            else
            {
                //if (sourceIndex > destinationIndex && destinationIndex>=0) sourceIndex--;
                int distance = 1;
                while (true)
                {
                    T prev = default(T);
                    T next = default(T);
                    int indexPrev = toIndex - distance, indexNext = clonedList.Count + (distance - 1);
                    if (toIndex - distance < 0)
                    {
                        indexNext = toIndex + distance;
                        if(indexNext >= clonedList.Count)
                        {
                            UpdateAllPriorities(clonedList, lst_ChangeItem);
                            break;
                        }
                        next = clonedList[indexNext];
                    }
                    else if (toIndex + distance >= clonedList.Count)
                    {
                        indexPrev = toIndex - distance;
                        if (indexPrev < 0)
                        {
                            UpdateAllPriorities(clonedList, lst_ChangeItem);
                            break;
                        }
                        prev = clonedList[indexPrev];
                    }
                    else
                    {
                        indexPrev = toIndex - distance;
                        indexNext = toIndex + distance;
                        next = clonedList[indexNext];
                        prev = clonedList[indexPrev];
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
                            if (i >= clonedList.Count) continue;
                            int oldpriority = clonedList[i].Priority;
                            clonedList[i].Priority = priorityPrev + ((priorityNext - priorityPrev) / (distance * 2)) * j;
                            //if (oldpriority != clonedList[i].Priority)
                            lst_ChangeItem.Add(clonedList[i]);
                            j++;
                        }
                        break;
                    }

                    distance++;
                }
            }

            if (fromIndex >= 0)
                list[prevh].Priority = clonedList[toIndex].Priority;
            else
                list[previ].Priority = clonedList[toIndex].Priority;

            return lst_ChangeItem;
        }

        private static void UpdateAllPriorities(List<T> clonedList, List<T> lst_ChangeItem)
        {
            int p = 1;
            int step = 2000000000 / clonedList.Count;
            foreach (var ii in clonedList)
            {
                ii.Priority = p;
                lst_ChangeItem.Add(ii);
                p += step;
            }
        }

        public static void UpdatePriority(List<T> t, int sourceIndex)
        {
            PriorityManager<T>.UpdatePriority(new BindingList<T>(t), sourceIndex);
        }
        #endregion end of functions
    }
}