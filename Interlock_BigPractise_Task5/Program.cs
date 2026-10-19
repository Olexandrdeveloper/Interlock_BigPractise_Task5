using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Interlock_BigPractise_Task5
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Warehouse w = new Warehouse();
            Random rnd = new Random();
            bool running = true;

            Thread supplier = new Thread(() =>
            {
                while (running)
                {
                    Thread.Sleep(700);
                    int item = rnd.Next(0, 5);
                    int count = rnd.Next(10, 16);
                    w.AddGoods(item, count);
                    Console.WriteLine("Постачальник додав " + count + " од. до товару #" + item);
                }
            });

            Thread analyst = new Thread(() =>
            {
                while (running)
                {
                    Thread.Sleep(500);
                    w.ShowReport();
                }
            });

            Thread[] buyers = new Thread[8];
            for (int i = 0; i < buyers.Length; i++)
            {
                int bId = i + 1;
                buyers[i] = new Thread(() =>
                {
                    for (int j = 0; j < 5; j++)
                    {
                        Thread.Sleep(rnd.Next(200, 600));

                        Dictionary<int, int> order = new Dictionary<int, int>();
                        int countTypes = rnd.Next(1, 4);

                        while (order.Count < countTypes)
                        {
                            int item = rnd.Next(0, 5);
                            int qty = rnd.Next(1, 4);
                            order[item] = qty;
                        }

                        bool res = w.Buy(order);
                        if (res)
                            Console.WriteLine("Покупець #" + bId + " купив товар");
                        else
                            Console.WriteLine("Покупець #" + bId + " не дочекався і пішов");
                    }
                });
            }

            supplier.Start();
            analyst.Start();

            foreach (var t in buyers) t.Start();
            foreach (var t in buyers) t.Join();

            running = false;
            Thread.Sleep(600);

            w.Check();
        }
    }
}
