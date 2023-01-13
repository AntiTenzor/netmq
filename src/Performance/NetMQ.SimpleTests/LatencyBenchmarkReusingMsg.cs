using System;
using System.Threading;
using System.Diagnostics;

using NetMQ.Sockets;

namespace NetMQ.SimpleTests
{
    internal class LatencyBenchmarkReusingMsg : LatencyBenchmarkBase
    {
        public LatencyBenchmarkReusingMsg()
        {
            TestName = "Req/Rep Latency Benchmark (reusing Msg)";
        }

        protected override long DoClient(NetMQSocket socket, int messageSize)
        {
            var msg = new Msg();
            var watch = Stopwatch.StartNew();

            for (int i = 0; i < Iterations; i++)
            {
                msg.InitGC(new byte[messageSize], messageSize);
                socket.Send(ref msg, more: false);
                socket.Receive(ref msg);
                msg.Close();
            }

            return watch.ElapsedTicks;
        }

        protected override void DoServer(NetMQSocket socket, int messageSize)
        {
            var msg = new Msg();
            msg.InitEmpty();

            for (int i = 0; i < Iterations; i++)
            {
                socket.Receive(ref msg);

                socket.Send(ref msg, more: false);
            }
        }

        protected override NetMQSocket CreateClientSocket()
        {
            return new RequestSocket();
        }

        protected override NetMQSocket CreateServerSocket()
        {
            return new ResponseSocket();
        }
    }
}
