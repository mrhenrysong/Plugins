using Plugins.Communication;
using System;

namespace Plugins.Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var instance = FindDeviceService.GetInstance();
            int localPort = 1212;
            instance.ScanLANInstrucments(localPort);
            var lanInstrs = instance.LANInstrucments;

            instance.ScanUSBInstrucments();
            var usbInstrs = instance.USBInstrucments;

            instance.ScanCOMInstrucments();
            var comInstrs = instance.COMInstrucments;

            var allInstrs = instance.AllInstrucments;

            Console.ReadKey();
        }

    }
}
