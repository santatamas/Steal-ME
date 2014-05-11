using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using StealME.Networking.Protocol;
using StealME.Server.Messaging;
using StealME.Server.Messaging.Requests;
using StealME.Server.Messaging.Responses;

namespace StealME.Server.Networking.Client
{
    class Program
    {
        static MsgPackSerializer serializer = new MsgPackSerializer(new TypeResolver());

        static void Main(string[] args)
        {
            string serverIP = "127.0.0.1";
            int connCount = 1;
            int messageCount = 10;

            if (args.Length == 0)
            {
                Console.WriteLine("No argument provided. See --help for details.");
                return;
            }
            else
            {
                if (args[0] == "--help")
                {
                    PrintHelp();
                    return;
                }
                else
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                serverIP = args[i];
                                break;
                            case 1:
                                connCount = int.Parse(args[i]);
                                break;
                            case 2:
                                messageCount = int.Parse(args[i]);
                                break;
                        }
                    }
                }
            }

            var rnd = new Random();
            var nums = Enumerable.Range(0, 2).ToArray();
            var listeners = new List<TcpClient>();
            var start = DateTime.Now;

            for (int i = 0; i < connCount; i++)
            {
                TcpClient tcpclnt = new TcpClient();
                listeners.Add(tcpclnt);

                Console.WriteLine("Connecting.....");
                tcpclnt.Connect(serverIP, 4444);

                Thread listenerThread = new Thread(new ParameterizedThreadStart(ListenAndEchoToConsole));
                listenerThread.Start(tcpclnt.GetStream());
            }

            int totalMSG = 0;

            try
            {
                for (int i = 0; i < messageCount; i++)
                {
                    Stream stm = listeners[rnd.Next(listeners.Count)].GetStream();

                    TestRequest demoRequest = new TestRequest {Id=0, Message = "Hello World!", Number = i };

                    byte[] msg = serializer.Serialize(demoRequest);
                    byte[] prefix = BitConverter.GetBytes(msg.Length);
                    var merged = new byte[prefix.Length + msg.Length];
                    prefix.CopyTo(merged, 0);
                    msg.CopyTo(merged, prefix.Length);

                    //Console.WriteLine(str);

                    stm.Write(merged, 0, merged.Length);
                    stm.Flush();
                    Interlocked.Increment(ref totalMSG);
                    //Thread.Sleep(50);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                foreach (var tcpClient in listeners)
                {
                    tcpClient.Close();
                }
            }

            var total = DateTime.Now - start;

            Console.WriteLine("finished: " + total.TotalMilliseconds + "ms, messages: " + totalMSG);
            Console.ReadKey();
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Usage: StealME.Server.Networking.Host.exe [serverIP] [connCount] [messageCount]");
            Console.WriteLine("[serverIP]: IP of the server (thanks cpt. obvious!)");
            Console.WriteLine("[connCount]: Number of concurrent TCP connections");
            Console.WriteLine("[messageCount]: Number of messages to send (a connection is picked up randomly for each)");
        }

        static object lockObject = new object();
        static void ListenAndEchoToConsole(object arg)
        {
            lock (lockObject)
            {
                Stream tcpStream = (Stream)arg;
                byte[] buffer = new byte[50];
                PacketReceiverLogic receiverLogic = new PacketReceiverLogic(50, 4);
                IMessageSerializer serializer = new MsgPackSerializer(new TypeResolver());
                while (true)
                {
                    try
                    {
                        int readBytes = tcpStream.Read(buffer, 0, 50);
                        lock (lockObject)
                        {
                            Buffer.BlockCopy(buffer, 0, receiverLogic.IncomingDataBuffer, 0, readBytes);
                            receiverLogic.TransferredBytesCount = readBytes;
                            receiverLogic.Process();
                        }
                        foreach (var output in receiverLogic.Output)
                        {
                            if (output.Length > 0)
                            {
                                //TestResponse message = (TestResponse)serializer.Deserialize(output);
                                //Console.WriteLine("Response: " + message.MessageType + " " + message.Number);
                            }
                        }
                    }
                    catch (ObjectDisposedException ex){}
                    catch (IOException ex){}
                    catch (InvalidOperationException ex){}
                    catch (Exception ex)
                    {
                        Console.WriteLine("OOPS! An error happened!");
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
        }
    }
}
