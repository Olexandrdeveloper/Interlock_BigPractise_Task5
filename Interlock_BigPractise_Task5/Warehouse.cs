using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Interlock_BigPractise_Task5
{
    internal class Warehouse
    {
        private object locker = new object();
        private int[] stock = { 20, 20, 20, 20, 20 };
        private int[] startStock = { 20, 20, 20, 20, 20 };
        private int[] added = new int[5];
        private int[] sold = new int[5];
        private long revenue = 0;
        private int okOrders = 0;
        private int badOrders = 0;

        private long[] prices = { 1000, 2500, 500, 1500, 3000 };

        public bool Buy(Dictionary<int, int> cart)
        {
            lock (locker)
            {
                DateTime limit = DateTime.Now.AddMilliseconds(2000);

                while (true)
                {
                    bool canBuy = true;
                    foreach (var item in cart)
                    {
                        if (stock[item.Key] < item.Value)
                        {
                            canBuy = false;
                            break;
                        }
                    }

                    if (canBuy)
                    {
                        long sum = 0;
                        foreach (var item in cart)
                        {
                            stock[item.Key] -= item.Value;
                            sold[item.Key] += item.Value;
                            sum += item.Value * prices[item.Key];
                        }

                        Interlocked.Add(ref revenue, sum);
                        Interlocked.Increment(ref okOrders);
                        return true;
                    }

                    int leftTime = (int)(limit - DateTime.Now).TotalMilliseconds;
                    if (leftTime <= 0)
                    {
                        Interlocked.Increment(ref badOrders);
                        return false;
                    }

                    Monitor.Wait(locker, leftTime);
                }
            }
        }

        public void AddGoods(int index, int count)
        {
            lock (locker)
            {
                stock[index] += count;
                added[index] += count;
                Monitor.PulseAll(locker);
            }
        }

        public void ShowReport()
        {
            lock (locker)
            {
                long rev = Interlocked.Read(ref revenue);
                Console.WriteLine("\n--- ЗВІТ ---");
                Console.WriteLine("Залишки: " + string.Join(", ", stock));
                Console.WriteLine("Виручка: " + (rev / 100.0) + " грн");
                Console.WriteLine("Успішних: " + okOrders + " | Скасованих: " + badOrders);
                Console.WriteLine("------------\n");
            }
        }

        public void Check()
        {
            lock (locker)
            {
                Console.WriteLine("\n================ ПЕРЕВІРКА ================");
                for (int i = 0; i < 5; i++)
                {
                    int calc = startStock[i] + added[i] - sold[i];
                    Console.WriteLine("Товар " + i + ": поч=" + startStock[i] + " + дод=" + added[i] + " - прож=" + sold[i] + " = " + calc + " (факт: " + stock[i] + ")");
                }
            }
        }
    }
}
