using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperWebSocket.Protocol
{
    interface IDataFramePartReader
    {
        int Process(int lastLength, WebSocketDataFrame frame, out IDataFramePartReader nextPartReader);
    }

    abstract class DataFramePartReader : IDataFramePartReader
    {
        static DataFramePartReader()
        {
            FixPartReader = new FixPartReader();
            ExtendedLenghtReader = new ExtendedLenghtReader();
            MaskKeyReader = new MaskKeyReader();
            PayloadDataReader = new PayloadDataReader();
        }

        public abstract int Process(int lastLength, WebSocketDataFrame frame, out IDataFramePartReader nextPartReader);

        public static IDataFramePartReader NewReader
        {
            get { return FixPartReader; }
        }

        protected static IDataFramePartReader FixPartReader { get; private set; }

        protected static IDataFramePartReader ExtendedLenghtReader { get; private set; }

        protected static IDataFramePartReader MaskKeyReader { get; private set; }

        protected static IDataFramePartReader PayloadDataReader { get; private set; }
    }

    class FixPartReader : DataFramePartReader
    {
        public override int Process(int lastLength, WebSocketDataFrame frame, out IDataFramePartReader nextPartReader)
        {
            if (frame.Length < 2)
            {
                nextPartReader = this;
                return -1;
            }

            if (frame.PayloadLenght < 126)
            {
                if (frame.HasMask)
                    nextPartReader = MaskKeyReader;
                else
                    nextPartReader = PayloadDataReader;
            }
            else
            {
                nextPartReader = ExtendedLenghtReader;
            }

            if (frame.Length > 2)
                return nextPartReader.Process(2, frame, out nextPartReader);

            return 0;
        }
    }

    class ExtendedLenghtReader : DataFramePartReader
    {
        public override int Process(int lastLength, WebSocketDataFrame frame, out IDataFramePartReader nextPartReader)
        {
            int required = 2;

            if (frame.PayloadLenght == 126)
                required += 2;
            else
                required += 8;

            if (frame.Length < required)
            {
                nextPartReader = this;
                return -1;
            }

            if (frame.HasMask)
                nextPartReader = MaskKeyReader;
            else
                nextPartReader = PayloadDataReader;

            if (frame.Length > required)
                return nextPartReader.Process(required, frame, out nextPartReader);

            return 0;
        }
    }

    class MaskKeyReader : DataFramePartReader
    {
        public override int Process(int lastLength, WebSocketDataFrame frame, out IDataFramePartReader nextPartReader)
        {
            int required = lastLength + 4;

            if (frame.Length < required)
            {
                nextPartReader = this;
                return -1;
            }

            nextPartReader = new PayloadDataReader();

            if (frame.Length > required)
                return nextPartReader.Process(required, frame, out nextPartReader);

            return 0;
        }
    }

    class PayloadDataReader : DataFramePartReader
    {
        public override int Process(int lastLength, WebSocketDataFrame frame, out IDataFramePartReader nextPartReader)
        {
            long required = lastLength + frame.ActualPayloadLength;

            if (frame.Length < required)
            {
                nextPartReader = this;
                return -1;
            }

            nextPartReader = null;

            return (int)((long)frame.Length - required);
        }
    }
}
