﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDatabaseLoadbalancer
{
    class Program
    {
        static void Main(string[] args)
        {
            LoadBalancer loadBalancer = new LoadBalancer();

            Console.ReadKey();
        }
    }
}
