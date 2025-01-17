﻿using System;
using System.Threading;
using System.Diagnostics;

using NetMQ.Sockets;

namespace NetMQ.SimpleTests
{
    internal class LatencyBenchmark : LatencyBenchmarkBase
    {
        public LatencyBenchmark()
        {
            TestName = "Req/Rep Latency Benchmark";
        }

        protected override long DoClient(NetMQSocket socket, int messageSize)
        {
            var msg = new byte[messageSize];

            var watch = Stopwatch.StartNew();

            for (int i = 0; i < Iterations; i++)
            {
                socket.SendFrame(msg);
                socket.SkipFrame(); // ignore response
            }

            return watch.ElapsedTicks;
        }

        protected override void DoServer(NetMQSocket socket, int messageSize)
        {
            for (int i = 0; i < Iterations; i++)
            {
                byte[] message = socket.ReceiveFrameBytes();
                socket.SendFrame(message);
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
