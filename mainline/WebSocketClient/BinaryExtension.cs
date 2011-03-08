using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperWebSocket.WebSocketClient
{
    public static class BinaryExtension
    {
        public static int IndexOf<T>(this IList<T> source, T target, int pos, int length)
        {
            for (int i = pos; i < pos + length; i++)
            {
                if (source[i].Equals(target))
                    return i;
            }

            return -1;
        }

        public static byte[] Combile(this List<ArraySegment<byte>> source)
        {
            List<byte> data = new List<byte>();

            for (int i = 0; i < source.Count; i++)
            {
                var item = source[i];
                byte[] newArray = new byte[item.Count];
                Array.Copy(item.Array, item.Offset, newArray, 0, item.Count);
                data.AddRange(newArray);
            }

            return data.ToArray();
        }

        public static int? SearchMark<T>(this IList<T> source, T[] mark)
        {
            return SearchMark(source, 0, source.Count, mark);
        }

        public static int? SearchMark<T>(this IList<T> source, int offset, int length, T[] mark)
        {
            int pos = offset;
            int endOffset = offset + length - 1;
            int matchCount = 0;

            while (true)
            {
                pos = source.IndexOf(mark[0], pos, length - pos + offset);

                if (pos < 0)
                    return null;

                matchCount = 1;

                for (int i = 1; i < mark.Length; i++)
                {
                    int checkPos = pos + i;

                    if (checkPos > endOffset)
                    {
                        //found end, return matched chars count
                        return (0 - i);
                    }

                    if (!source[checkPos].Equals(mark[i]))
                        break;

                    matchCount++;
                }

                if (matchCount == mark.Length)
                    return pos;

                //Reset next round read pos
                pos += matchCount;
                //clear matched chars count
                matchCount = 0;
            }
        }
    }
}
