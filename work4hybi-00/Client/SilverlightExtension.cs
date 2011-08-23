using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace SuperWebSocket.Client
{
    public static class SilverlightExtension
    {
        public static string GetString(this Encoding encoding, byte[] buffer)
        {
            return encoding.GetString(buffer, 0, buffer.Length);
        }
    }

    public static class MD5
    {
        public static HashAlgorithm Create()
        {
            return new MD5Managed();
        }
    }
}
