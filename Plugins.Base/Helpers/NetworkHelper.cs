using System.Net.NetworkInformation;

namespace Plugins.Base.Helpers
{
    public class NetworkHelper
    {
        public static string GetEthernetIpAddress()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                // 查找物理网络接口（例如以太网接口）
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet && ni.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        // 确保是IPv4地址
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            return ip.Address.ToString();
                        }
                    }
                }
            }
            return null; // 未找到适配器或IP地址
        }
    }
}
