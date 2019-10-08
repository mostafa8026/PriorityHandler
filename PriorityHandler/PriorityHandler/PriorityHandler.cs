//MIT License

//Copyright(c) 2018 Mostafa

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriorityHandler
{
    /// <summary>
    /// This namespace is used with classes used PriorityHandler functions.
    /// </summary>
    public interface IPriorityItem
    {
        /// <summary>
        /// priority number as int, smaller number means items with more priority.
        /// </summary>
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

        public PriorityItem(T _item, int _priority)
        {
            this.Item = _item;
            this.priority = _priority;
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

        public void MoveUp(BindingList<T> t, int index)
        {
            var uppert = t.Where(x => x.Priority < t[index].Priority).OrderByDescending(x=>x.Priority).FirstOrDefault<T>();
            if(uppert != null)
            {
                UpdatePriority(t, t.IndexOf(uppert), index);
            }
        }

        public void MoveDown(BindingList<T> t, int index)
        {
            var lowert = t.Where(x => x.Priority > t[index].Priority).OrderBy(x => x.Priority).FirstOrDefault<T>();
            if (lowert != null)
            {
                UpdatePriority(t, t.IndexOf(lowert), index);
            }
        }

        public static List<T> UpdatePriority(BindingList<T> list, int index, int hidden_index = -1)
        {
            var clonedList = list.ToList().Select(x => x.Clone()).OfType<T>().ToList();
            T temph = default(T);
            if (hidden_index >= 0)
                temph = clonedList[hidden_index];
            var tempi = clonedList[index];
            int previ = index;
            int prevh = hidden_index;
            clonedList = clonedList.OrderBy(x => x.Priority).ToList();
            if (hidden_index >= 0)
                hidden_index = clonedList.IndexOf(temph);
            index = clonedList.IndexOf(tempi);
            if (hidden_index >= 0)
            {
                clonedList.Remove(temph);
                clonedList.Insert(index, temph);
                
            }

            List<T> lst_ChangeItem = new List<T>();
            if (clonedList.Count == 1)
            {
                var item = clonedList[0];
                if (hidden_index >= 0)
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
                //if (index > hidden_index && hidden_index >= 0) index--;
                lst_ChangeItem.Add(item);
            }
            else
            {
                //if (index > hidden_index && hidden_index>=0) index--;
                int distance = 1;
                while (true)
                {
                    T prev = default(T);
                    T next = default(T);
                    int indexPrev = index - distance, indexNext = clonedList.Count + (distance - 1);
                    if (index - distance < 0)
                    {
                        indexNext = index + distance;
                        if(indexNext >= clonedList.Count)
                        {
                            UpdateAllPriorities(clonedList, lst_ChangeItem);
                            break;
                        }
                        next = clonedList[indexNext];
                    }
                    else if (index + distance >= clonedList.Count)
                    {
                        indexPrev = index - distance;
                        if (indexPrev < 0)
                        {
                            UpdateAllPriorities(clonedList, lst_ChangeItem);
                            break;
                        }
                        prev = clonedList[indexPrev];
                    }
                    else
                    {
                        indexPrev = index - distance;
                        indexNext = index + distance;
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

            if (hidden_index >= 0)
                list[prevh].Priority = clonedList[index].Priority;
            else
                list[previ].Priority = clonedList[index].Priority;

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

        public static void UpdatePriority(List<T> t, int index)
        {
            PriorityManager<T>.UpdatePriority(new BindingList<T>(t), index);
        }
        #endregion end of functions
    }

    public class PriorityTableHelper
    {
        #region fields
        private SqlConnection connection = new SqlConnection();
        private string tableName = "";
        private string primaryKeyColumn = "id";
        private string priorityColumn = "priority";
        #endregion End of fields

        #region Propertis
        public string TableName { get => tableName; set => tableName = value; }
        public string PrimaryKeyColumn { get => primaryKeyColumn; set => primaryKeyColumn = value; }
        public string PriorityColumn { get => priorityColumn; set => priorityColumn = value; }
        #endregion End of Properties

        #region constructor
        public PriorityTableHelper(SqlConnection _connection, string _tableName, string _primaryKeyColumn, string _priorityColumn)
        {
            connection = _connection;
            TableName = _tableName;
            PrimaryKeyColumn = _primaryKeyColumn;
            PriorityColumn = _priorityColumn;
        }
        #endregion end of constructor

        #region functions
        public Tuple<int, int, int, int> GetBetweenPriority(int afterPriority, int beforePriority, int distance, string additionalWhereCondition = "1=1")
        {
            try
            {
                string cmd = $@"
with ta as
(
select * ,
ROW_NUMBER() over(order by {PriorityColumn} desc) as rn
from {TableName} where {PriorityColumn} < @afterPriority AND {additionalWhereCondition} 
), td as
(
select*,
ROW_NUMBER() over(order by {PriorityColumn} asc) as rn
from {TableName} where {PriorityColumn} > @beforePriority AND {additionalWhereCondition} 
) 
select isnull((select {PriorityColumn} from ta where rn = @distance), 0), 
       isnull((select {PriorityColumn} from td where rn = @distance), 2000000000),
       isnull((select {PrimaryKeyColumn} from ta where rn = @distance), -1), 
       isnull((select {PrimaryKeyColumn} from td where rn = @distance), -1)
";
                SqlCommand command = new SqlCommand(cmd, connection);
                command.Parameters.AddWithValue("@beforePriority", afterPriority);
                command.Parameters.AddWithValue("@afterPriority", beforePriority);
                command.Parameters.AddWithValue("@distance", distance);

                connection.Open();
                var reader = command.ExecuteReader();
                int afterID = -1, beforeID = -1;
                if (reader.HasRows)
                {
                    reader.Read();
                    afterPriority = reader.GetInt32(0);
                    beforePriority = reader.GetInt32(1);
                    afterID = reader.GetInt32(2);
                    beforeID = reader.GetInt32(3);
                }

                return new Tuple<int, int, int, int>(afterPriority, beforePriority, afterID, beforeID);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        public int? GetPriority(int id)
        {
            try
            {
                string cmd = $@"SELECT {priorityColumn} FROM {TableName} WHERE {PrimaryKeyColumn}=@id";
                SqlCommand command = new SqlCommand(cmd, connection);
                command.Parameters.AddWithValue("@id", id);

                connection.Open();
                var reader = command.ExecuteReader();
                int priority = 0;
                if (reader.HasRows)
                {
                    reader.Read();
                    priority = reader.GetInt32(0);
                }
                else return null;

                return priority;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        public int? GetIDWithDirection(int id, DirectionEnum direction, string additionalWhereCondition = "1=1")
        {
            try
            {
                string cmd = $@"SELECT TOP 1 ISNULL({PrimaryKeyColumn}, -1) FROM {TableName} WHERE {PriorityColumn} {(direction == DirectionEnum.After ? ">" : "<")} ISNULL((SELECT ISNULL({PriorityColumn},-1) FROM {TableName} WHERE {PrimaryKeyColumn}=@id),-1) AND {additionalWhereCondition} ORDER BY priority {(direction == DirectionEnum.After ? " ASC " : " DESC ")}";
                SqlCommand command = new SqlCommand(cmd, connection);
                command.Parameters.AddWithValue("@id", id);

                connection.Open();
                var reader = command.ExecuteReader();
                int retID = 0;
                if (reader.HasRows)
                {
                    reader.Read();
                    retID = reader.GetInt32(0);
                }
                else return null;

                return retID;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        public void UpdatePriority(int id, int priority)
        {
            try
            {
                string cmd = $@"UPDATE {TableName} SET {priorityColumn}=@priority WHERE {PrimaryKeyColumn}=@id";
                SqlCommand command = new SqlCommand(cmd, connection);
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@priority", priority);

                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        public SqlDataReader SelectBetweenPriority(int beforePriority, int afterPriority)
        {
            try
            {
                string cmd = $"SELECT * FROM {TableName} WHERE {PriorityColumn}<@afterPriority AND {PriorityColumn}>@beforePriority";
                SqlCommand command = new SqlCommand(cmd, connection);
                command.Parameters.AddWithValue("@beforePriority", beforePriority);
                command.Parameters.AddWithValue("@afterPriority", afterPriority);

                var reader = command.ExecuteReader();
                return reader;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Tuple<int, bool> MovePriority(int _id, int _afterID, int _beforeID, string additionalWhereCondition = "1=1")
        {
            int afterPriority = GetPriority(_afterID) ?? 0;
            int beforePriority = GetPriority(_beforeID) ?? 2000000000;

            int distance = 0;
            List<PriorityItem<int>> priorityList = new List<PriorityItem<int>>();

            priorityList.Add(new PriorityItem<int>(_id, -1));

            bool refreshAll = false;
            while (beforePriority - afterPriority <= (++distance * 2) - 1)
            {
                refreshAll = true;
                if (_afterID != -1) priorityList.Insert(0, new PriorityItem<int>(_afterID, afterPriority));
                if (_beforeID != -1) priorityList.Insert(priorityList.Count, new PriorityItem<int>(_beforeID, beforePriority));
                //distance++;
                var newTuple = GetBetweenPriority(beforePriority, afterPriority, 1, additionalWhereCondition + $" AND {PrimaryKeyColumn}<>{_id} ");
                afterPriority = newTuple.Item1;
                beforePriority = newTuple.Item2;
                _afterID = newTuple.Item3;
                _beforeID = newTuple.Item4;
                //if (newTuple.Item3 != -1) priorityList.Insert(0, new PriorityItem<int>(newTuple.Item3, beforePriority));
                //if (newTuple.Item4 != -1) priorityList.Insert(priorityList.Count, new PriorityItem<int>(newTuple.Item4, afterPriority));
            }

            //int mainPriority = afterPriority + ((beforePriority - afterPriority) / (distance * 2)) * distance;
            int mainPriority = -1;
            int V = ((beforePriority - afterPriority) / (distance * 2));

            for (int i = 0; i < priorityList.Count; i++)
            {
                int currentP = afterPriority + V * (i + 1);
                if (priorityList[i].Priority == -1)
                    mainPriority = currentP;
                priorityList[i].Priority = currentP;
                UpdatePriority(priorityList[i].Item, currentP);
            }

            return new Tuple<int, bool>(mainPriority, refreshAll);
        }

        public Tuple<int, bool> MovePriorityAfter(int _id, int _afterID, string additionalWhereCondition = "1=1")
        {
            if (_afterID == 0) _afterID = -1;
            int beforeID = GetIDWithDirection(_afterID, DirectionEnum.After, additionalWhereCondition) ?? -1;
            return MovePriority(_id, _afterID, beforeID, additionalWhereCondition);
        }

        public Tuple<int, bool> MovePriorityBefore(int _id, int _beforeID, string additionalWhereCondition = "1=1")
        {
            if (_beforeID == 0) _beforeID = -1;
            int afterID = GetIDWithDirection(_beforeID, DirectionEnum.Before, additionalWhereCondition) ?? -1;
            return MovePriority(_id, afterID, _beforeID, additionalWhereCondition);
        }
        #endregion end of functions
    }

    public enum DirectionEnum
    {
        After = 1,
        Before = -1
    }
}