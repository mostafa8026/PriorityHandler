using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;

namespace PriorityHandler
{
    public static class DataTableExtension
    {
        public static BindingList<PriorityItem<DataRow>> GetPriorityItemList(this DataTable dataTable, string priorityColumnName)
        {
            var retList = new BindingList<PriorityItem<DataRow>>();

            foreach (DataRow row in dataTable.Rows)
            {
                var priority = (int) row[priorityColumnName];
                retList.Add(new PriorityItem<DataRow>(row, priority));
            }

            return retList;
        }
    }
}
