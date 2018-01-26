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
        static PriorityHandler.Prioritylist<PriorityItem<string>> myList = new Prioritylist<PriorityHandler.PriorityItem<string>>();
        static int count = 0;
        static void Main(string[] args)
        {
            int line = 0;
            while (line != -1)
            {
                string lineStr = Console.ReadLine();
                if (!string.IsNullOrEmpty(lineStr))
                {
                    try
                    {
                        myList.Insert(0, new PriorityItem<string>(count.ToString()));
                        Random rand = new Random();
                        for (int i = 0; i < 1000000; i++)
                        {
                            line = Convert.ToInt32(lineStr);
                            myList.Insert(rand.Next(0,myList.Count-1), new PriorityItem<string>(count.ToString()));
                            count++;
                            Console.WriteLine(i);
                        }
                        Console.WriteLine($"------- {count} -------");
                        PrintList();
                        Console.WriteLine($"------- {count} -------");
                        count++;
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
            myList.Sort();
            foreach (var v in myList)
            {
                Console.WriteLine(i.ToString()+ " --> "+ v.ToString());
                i++;
            }
        }
    }
}
