using System;
using System.Diagnostics;

using NetMQ;
using NetMQ.Sockets;

namespace local_thr
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("usage: local_thr <bind-to> <message-size> <message-count>");
                return 1;
            }

            Console.WriteLine("Args:   {0}", String.Join("   ", args));



            string bindTo = args[0];
            int messageSize = int.Parse(args[1]);
            int messageCount = int.Parse(args[2]);

            using (var pullSocket = new PullSocket())
            {
                pullSocket.Bind(bindTo);

                var msg = new Msg();
                msg.InitEmpty();

                pullSocket.Receive(ref msg);

                var stopWatch = Stopwatch.StartNew();
                for (int i = 0; i < messageCount - 2; i++)
                {
                    if ((i > messageCount - 1100) && (i % 1 == 0))
                        Console.WriteLine("    Receiving message number i: {0:# ### ##0}", i);

                    pullSocket.Receive(ref msg);
                    if (msg.Size != messageSize)
                    {
                        Console.WriteLine("message of incorrect size received. Received: " + msg.Size + " Expected: " + messageSize);
                        return -1;
                    }
                }
                stopWatch.Stop();

                double secondsElapsed = stopWatch.Elapsed.TotalSeconds;
                //if (millisecondsElapsed == 0)
                //    millisecondsElapsed = 1;

                msg.Close();

                double messagesPerSecond = (double)messageCount / secondsElapsed;
                double megaBYTES = messagesPerSecond * messageSize /* *8 */ / 1024.0 / 1024.0;

                Console.WriteLine("message size: {0} [Bytes]", messageSize);
                Console.WriteLine("message count: {0}", messageCount);
                Console.WriteLine("mean throughput: {0:0.000} [msg/s]", messagesPerSecond);
                Console.WriteLine("mean throughput: {0:0.000} [MegaBYTES/s]", megaBYTES);
            }

            return 0;
        }
    }
}