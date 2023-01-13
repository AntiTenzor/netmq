using System;
using System.Threading;
using System.Diagnostics;

using NetMQ.Sockets;

namespace NetMQ.SimpleTests
{
    internal class ThroughputBenchmarkReusingMsg : ThroughputBenchmarkBase
    {
        public ThroughputBenchmarkReusingMsg()
        {
            TestName = "Push/Pull Throughput Benchmark (reusing Msg)";
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
            var msg = new Msg();
            msg.InitGC(new byte[messageSize], messageSize);
            msg.Slice()[messageSize / 2] = 0x42;

            for (int i = 0; i < MsgCount; i++)
            {
                socket.Send(ref msg, more: false);
            }

            msg.Close();
        }

        protected override void Consume(PullSocket socket, int messageSize)
        {
            var msg = new Msg();
            msg.InitEmpty();

            for (int i = 0; i < MsgCount; i++)
            {
                socket.Receive(ref msg);

                Debug.Assert(msg.Slice().Length == messageSize, "Message length was different from expected size.");
                Debug.Assert(msg.Slice()[msg.Size / 2] == 0x42, "Message does not contain verification data.");

                if (msg.Data.Length != messageSize)
                    throw new InvalidOperationException("Message length was different from expected size.");

                if (msg.Data[msg.Size / 2] != 0x42)
                    throw new InvalidOperationException("Message does not contain verification data.");
            }
        }
    }
}
