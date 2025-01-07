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

            instance.ScanUSBInstrucments();
            var usbInstrs = instance.USBInstrucments;

            //instance.ScanCOMInstrucments();
            //var comInstrs = instance.COMInstrucments;

            var allInstrs = instance.AllInstrucments;
            instance.CloseDriveIO();

            foreach (var Instr in allInstrs)
            {
                Console.WriteLine($"{Instr.Address}  {Instr.Description}");
            }

            Console.ReadKey();
        }

    }
}
