using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Sockets;

namespace VNPAY.NET.Utilities
{
    public class NetworkHelper
    {
        /// <summary>
        /// Lấy địa chỉ IP từ HttpContext của API Controller.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetIpAddress(HttpContext context)
        {
            var remoteIpAddress = context.Connection.RemoteIpAddress;

            if (remoteIpAddress != null)
            {
                return remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6
                    ? Dns.GetHostEntry(remoteIpAddress).AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork).ToString()
                    : remoteIpAddress.ToString();
            }

            throw new NullReferenceException("Không tìm thấy địa chỉ IP");
        }
    }
}
