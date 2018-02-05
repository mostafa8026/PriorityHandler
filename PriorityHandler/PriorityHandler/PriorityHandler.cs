using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

    public class Prioritylist<T> : List<T> where T : IPriorityItem
    {
        public void PriorityInsert(int index, T item, bool updatePriority = true)
        {
            base.Insert(index, item);
            if (updatePriority)
                UpdatePriority(index);
        }

        public void Swap(int indexA, int indexB)
        {
            var tempB = base[indexB];
            base.RemoveAt(indexB);
            this.PriorityInsert(indexA, tempB, true);
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
                    int indexPrev = index - distance, indexNext = base.Count + (distance-1);
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

        public int? GetIDWithDirection(int id, DirectionEnum direction, string additionalWhereCondition="1=1")
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
            Prioritylist<PriorityItem<int>> priorityList = new Prioritylist<PriorityItem<int>>();

            priorityList.Add(new PriorityItem<int>(_id, -1));

            bool refreshAll = false;
            while (beforePriority - afterPriority <= (++distance * 2) - 1)
            {
                refreshAll = true;
                if (_afterID != -1) priorityList.Insert(0, new PriorityItem<int>(_afterID, afterPriority));
                if (_beforeID != -1) priorityList.Insert(priorityList.Count, new PriorityItem<int>(_beforeID, beforePriority));
                //distance++;
                var newTuple = GetBetweenPriority(beforePriority, afterPriority, 1, additionalWhereCondition  + $" AND {PrimaryKeyColumn}<>{_id} ");
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