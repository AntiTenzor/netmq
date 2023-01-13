using System;
using System.Threading;
using System.Diagnostics;

using NetMQ.Sockets;

namespace NetMQ.SimpleTests
{
    internal abstract class ThroughputBenchmarkBase : ITest
    {
        private static readonly int[] s_messageSizes = { 8, 64, 256, 1024, 4096, 8192 };

        protected const int MsgCount = 100_000;

        public string TestName { get; protected set; }

        public void RunTest()
        {
            Console.Out.WriteLine(" Messages: {0:#,##0}", MsgCount);
            Console.Out.WriteLine();
            Console.Out.WriteLine(" {0,-6} {1,10} {2,8}", "Size", "Msgs/sec", "MegaBYTES/s");
            Console.Out.WriteLine("----------------------------");

            var consumer = new Thread(ConsumerThread) { Name = "Consumer" };
            var producer = new Thread(ProducerThread) { Name = "Producer" };

            consumer.Start();
            producer.Start();

            producer.Join();
            consumer.Join();
        }

        private void ConsumerThread()
        {
            using (var socket = CreateConsumerSocket())
            {
                socket.Bind("tcp://*:9091");

                foreach (var messageSize in s_messageSizes)
                {
                    var watch = Stopwatch.StartNew();

                    Consume(socket, messageSize);

                    long ticks = watch.ElapsedTicks;
                    double seconds = (double)ticks / Stopwatch.Frequency;
                    double msgsPerSec = MsgCount / seconds;
                    double megaBYTESPerSec = msgsPerSec * messageSize /* * 8 */ / 1024.0 / 1024.0;

                    Console.Out.WriteLine(" {0,-6} {1,10:0.0} {2,8:0.00}", messageSize, msgsPerSec, megaBYTESPerSec);
                }
            }
        }

        private void ProducerThread()
        {
            using (var socket = CreateProducerSocket())
            {
                socket.Connect("tcp://127.0.0.1:9091");

                foreach (var messageSize in s_messageSizes)
                    Produce(socket, messageSize);
            }
        }

        protected abstract PushSocket CreateProducerSocket();
        protected abstract PullSocket CreateConsumerSocket();

        protected abstract void Produce(PushSocket socket, int messageSize);
        protected abstract void Consume(PullSocket socket, int messageSize);
    }
}
