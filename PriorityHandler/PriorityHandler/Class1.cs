using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriorityHandler
{
    public class PriorityItem<T>
    {
        #region fields
        private int priority = 0;
        private T item;
        #endregion end of fields

        #region Property
        public int Priority { get => priority; set => priority = value; }
        public T Item { get => item; set => item = value; }
        #endregion End of Property
    }
}
