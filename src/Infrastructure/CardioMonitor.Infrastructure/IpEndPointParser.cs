using System;
using System.Globalization;
using System.Net;

namespace CardioMonitor.Infrastructure
{
    public static class IpEndPointParser
    {
        public static IPEndPoint Parse(string endPoint)
        {
            var ep = endPoint.Split(':');
            if (ep.Length < 2) throw new FormatException("Invalid endpoint format");
            IPAddress ip;
            if (ep.Length > 2 )
            {
                if (!IPAddress.TryParse(string.Join(":", ep, 0, ep.Length - 1), out ip))
                {
                    throw new FormatException("Invalid ip-adress");
                }
            }
            else
            {
                if (!IPAddress.TryParse(ep[0], out ip))
                {
                    throw new FormatException("Invalid ip-adress");
                }
            }

            if (!int.TryParse(ep[ep.Length - 1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out var port))
            {
                throw new FormatException("Invalid port");
            }
            return new IPEndPoint(ip, port);
        }

        public static bool TryParse(string endPoint, out IPEndPoint result)
        {
            try
            {
                result = Parse(endPoint);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}