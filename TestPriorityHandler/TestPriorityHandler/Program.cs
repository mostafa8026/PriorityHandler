using PriorityHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPriorityHandler
{
    class Program
    {
        static List<PriorityItem<string>> priority_list = new List<PriorityItem<string>>();
        static PriorityHandler.PriorityManager<PriorityItem<string>> priority_manager = new PriorityManager<PriorityHandler.PriorityItem<string>>();
        static int count = -1;
        static void Main(string[] args)
        {
            RunTableSample();
        }

        private static void RunTableSample()
        {
            int line = 0;
            while (line != -1)
            {
                string lineStr = Console.ReadLine();
                if (!string.IsNullOrEmpty(lineStr))
                {
                    try
                    {
                        //myList.Insert(0, new PriorityItem<string>(count.ToString()));
                        //Random rand = new Random();
                        //for (int i = 0; i < 1000000; i++)
                        //{
                        //    myList.Insert(rand.Next(0,myList.Count-1), new PriorityItem<string>(count.ToString()));
                        //    count++;
                        //    Console.WriteLine(i);
                        //}
                        if (lineStr.StartsWith("swap"))
                        {
                            string[] strs = lineStr.Split(' ');
                            int indexA = Convert.ToInt32(strs[1]);
                            int indexB = Convert.ToInt32(strs[2]);
                            priority_manager.Swap(priority_list, indexA, indexB);
                        }
                        else if (lineStr.StartsWith("table"))
                        {
                            PriorityTableHelper pth = new PriorityTableHelper(new System.Data.SqlClient.SqlConnection(@"server=.\chapyarmsql;dataBase=chapyar;User ID=user;Password="), "processes", "id", "priority");
                            string[] strs = lineStr.Split(' ');
                            int id = Convert.ToInt32(strs[1]);
                            int after = Convert.ToInt32(strs[2]);
                            int before = Convert.ToInt32(strs[3]);
                            pth.MovePriority(id, after, before);
                        }
                        else
                        {
                            line = Convert.ToInt32(lineStr);
                            priority_manager.PriorityInsert(priority_list, line, new PriorityItem<string>((++count).ToString()));
                        }
                        Console.WriteLine($"------- {count} -------");
                        PrintList();
                        Console.WriteLine($"------- {count} -------");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        static void PrintList()
        {
            int i = 0;
            priority_list.Sort();
            foreach (var v in priority_list)
            {
                Console.WriteLine(i.ToString()+ " --> "+ v.ToString());
                i++;
            }
        }
    }
}
