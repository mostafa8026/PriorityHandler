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

namespace PriorityHandler
{
    public class PriorityItem<T> : IPriorityItem, IComparable, ICloneable
    {
        #region Constructors

        public PriorityItem()
        {
        }

        public PriorityItem(T _item)
        {
            Item = _item;
        }

        public PriorityItem(T _item, int _priority)
        {
            Item = _item;
            Priority = _priority;
        }

        #endregion

        #region Properties
        
        public int Priority { get; set; }
        public T Item { get; set; }

        #endregion

        #region Interface Implementations

        public int CompareTo(object obj)
        {
            if (Priority > ((IPriorityItem) obj).Priority)
                return 1;
            return -1;
        }

        #endregion


        public override string ToString()
        {
            return Item + $" - Priority: {Priority}";
        }

        public object Clone()
        {
            var retClone = this.MemberwiseClone();
            return retClone;
        }

        public int CompareTo(T other)
        {
            throw new NotImplementedException();
        }
    }
}