using System;
using System.Collections.Specialized;
using StealME.Server.Core.Communication;

namespace StealME.Server.Networking.Host
{
    class Program
    {
        static void Main(String[] args)
        {
            Node smNode = new Node();
            smNode.Start();

            Console.ReadKey();
        }
    }
}
