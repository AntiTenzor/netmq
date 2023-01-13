using System;
using System.Threading;
using System.Diagnostics;

using NetMQ.Sockets;

namespace NetMQ.SimpleTests
{
    internal abstract class LatencyBenchmarkBase : ITest
    {
        protected const int Iterations = 20_000;

        private static readonly int[] s_messageSizes = { 8, 64, 512, 4096, 8192, 16384, 32768 };

        public string TestName { get; protected set; }

        public void RunTest()
        {
            Console.Out.WriteLine(" Iterations: {0:#,##0}", Iterations);
            Console.Out.WriteLine();
            Console.Out.WriteLine(" {0,-6} {1}", "Size", "Latency (µs)");
            Console.Out.WriteLine("---------------------");

            var client = new Thread(ClientThread) { Name = "Client" };
            var server = new Thread(ServerThread) { Name = "Server" };

            server.Start();
            client.Start();

            server.Join();
            client.Join();
        }

        private void ClientThread()
        {
            using (var socket = CreateClientSocket())
            {
                socket.Connect("tcp://127.0.0.1:9000");

                foreach (int messageSize in s_messageSizes)
                {
                    var ticks = DoClient(socket, messageSize);

                    const long tripCount = Iterations * 2;
                    double seconds = (double)ticks / Stopwatch.Frequency;
                    double microsecond = seconds * 1000000.0;
                    double microsecondsPerTrip = microsecond / tripCount;

                    Console.Out.WriteLine(" {0,-7} {1,6:0.0}", messageSize, microsecondsPerTrip);
                }
            }
        }

        private void ServerThread()
        {
            using (var socket = CreateServerSocket())
            {
                socket.Bind("tcp://*:9000");

                foreach (int messageSize in s_messageSizes)
                {
                    DoServer(socket, messageSize);
                }
            }
        }

        protected abstract NetMQSocket CreateClientSocket();
        protected abstract NetMQSocket CreateServerSocket();

        protected abstract long DoClient(NetMQSocket socket, int messageSize);
        protected abstract void DoServer(NetMQSocket socket, int messageSize);
    }
}
