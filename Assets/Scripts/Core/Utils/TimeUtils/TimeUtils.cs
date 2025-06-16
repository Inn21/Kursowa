#region

using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

#endregion

namespace Engine.Core.Utils.TimeUtils
{
    public static class TimeUtils
    {
        public static long GenerateRelativeTimestamp(int addHours, int addMinutes, int addSeconds)
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = DateTime.UtcNow.AddSeconds(addSeconds).AddMinutes(addMinutes).AddHours(addHours);
            var span = end - start;
            return (long)span.TotalMilliseconds;
        }

        public static long GenerateRelativeTimestamp()
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = DateTime.UtcNow;
            var span = end - start;
            return (long)span.TotalMilliseconds;
        }

        public static DateTime LongToDateTime(long time)
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var dateTime = start.AddMilliseconds(time);
            return dateTime;
        }

        public static TimeSpan TimeLeft(long endsAt)
        {
            var endDateTime = LongToDateTime(endsAt);
            var timeLeft = endDateTime.Subtract(DateTime.UtcNow);
            return timeLeft;
        }

        public static bool IsTimeSpanCurrentlyActive(long startAt, long endsAt)
        {
            if (startAt != 0 && TimeLeft(startAt).Milliseconds > 0)
                return false;
            if (endsAt != 0 && TimeLeft(endsAt).Milliseconds <= 0)
                return false;
            return true;
        }

        public static void GetNetworkUtc(Action<DateTime> onComplete)
        {
            
            var ntpServerIpAddress = "time.windows.com";

            
            var ntpData = new byte[48];
            ntpData[0] = 0x1B;

            try
            {
                
                using (var udpClient = new UdpClient(ntpServerIpAddress, 123))
                {
                    udpClient.Send(ntpData, ntpData.Length);
                    IPEndPoint remoteEndPoint = null;
                    var receivedData = udpClient.Receive(ref remoteEndPoint);
                    var ntpReceiveTime = GetNtpReceiveTimestamp(receivedData);
                    onComplete(ntpReceiveTime);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError(exception.Message);
                onComplete(DateTime.UtcNow);
            }
        }

       
        private static DateTime GetNtpReceiveTimestamp(byte[] ntpData)
        {
            const int ntpDataOffset = 40;
            var ntpTimestamp = ((long)ntpData[ntpDataOffset] << 24) | ((long)ntpData[ntpDataOffset + 1] << 16) |
                               ((long)ntpData[ntpDataOffset + 2] << 8) | ntpData[ntpDataOffset + 3];
            return new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(ntpTimestamp);
        }
    }
}