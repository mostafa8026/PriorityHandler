using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PriorityHandler.Samples
{
    public class PriorityItemSample
    {
        public void Run()
        {
            var list = GetSampleData();

            PriorityManager<PriorityItem<string>> priorityManager = new PriorityManager<PriorityItem<string>>();
            var changedItems = priorityManager.MoveUp(list, 3);

            PrintList(changedItems, "Changed Items are:");

            PrintList(list.ToList(), "New List:");
        }

        private static void PrintList(List<PriorityItem<string>> changedItems, string title)
        {
            Console.WriteLine(title);
            foreach (var v in changedItems)
            {
                Console.WriteLine($"Item: {v.Item}, priority: {v.Priority}");
            }
        }

        private static BindingList<PriorityItem<string>> GetSampleData()
        {
            BindingList<PriorityItem<string>> list = new BindingList<PriorityItem<string>>();
            list.Add(new PriorityItem<string>("Item 1"));
            list.Add(new PriorityItem<string>("Item 2"));
            list.Add(new PriorityItem<string>("Item 3"));
            list.Add(new PriorityItem<string>("Item 4"));
            return list;
        }
    }
}
