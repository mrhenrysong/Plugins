using Plugins.Base.Helpers;
using Plugins.Communication;
using System;

namespace Plugins.Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var localIp = NetworkHelper.GetEthernetIpAddress();
            Console.WriteLine(localIp);
            var instance = FindDeviceService.GetInstance();
            instance.InitDriveIO();
            int localPort = 1212;
            instance.ScanLANInstrucments(localPort, localIp);
            var lanInstrs = instance.LANInstrucments;
            foreach (var lanInstr in lanInstrs)
            {
                Console.WriteLine($"{lanInstr.Address}  {lanInstr.Description}");
            }

            //instance.ScanUSBInstrucments();
            //var usbInstrs = instance.USBInstrucments;

            //instance.ScanCOMInstrucments();
            //var comInstrs = instance.COMInstrucments;

            //var allInstrs = instance.AllInstrucments;

            Console.ReadKey();
        }

    }
}
