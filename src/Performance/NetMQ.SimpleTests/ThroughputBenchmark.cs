﻿using System;
using System.Threading;
using System.Diagnostics;

using NetMQ.Sockets;

namespace NetMQ.SimpleTests
{
    internal class ThroughputBenchmark : ThroughputBenchmarkBase
    {
        public ThroughputBenchmark()
        {
            TestName = "Push/Pull Throughput Benchmark";
        }

        protected override PushSocket CreateProducerSocket()
        {
            return new PushSocket();
        }

        protected override PullSocket CreateConsumerSocket()
        {
            return new PullSocket();
        }

        protected override void Produce(PushSocket socket, int messageSize)
        {
            var msg = new byte[messageSize];
            msg[messageSize / 2] = 0x42;

            for (int i = 0; i < MsgCount; i++)
            {
                if (i > MsgCount - 3)
                    Console.WriteLine("    Sending message number i: {0:# ### ##0}", i);

                socket.SendFrame(msg);

                if (i > MsgCount - 3)
                    Console.WriteLine("    Message number i: {0:# ### ##0} was sent", i);
            }
        }

        protected override void Consume(PullSocket socket, int messageSize)
        {
            for (int i = 0; i < MsgCount; i++)
            {
                byte[] message = socket.ReceiveFrameBytes();

                Debug.Assert(message.Length == messageSize, "Message length was different from expected size.");
                Debug.Assert(message[messageSize/2] == 0x42, "Message does not contain verification data.");

                if (message.Length != messageSize)
                    throw new InvalidOperationException("Message length was different from expected size.");

                if (message[message.Length / 2] != 0x42)
                    throw new InvalidOperationException("Message does not contain verification data.");
            }
        }
    }

}