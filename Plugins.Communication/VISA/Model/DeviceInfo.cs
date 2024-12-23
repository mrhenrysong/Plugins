namespace Plugins.Communication.VISA.Model
{
    public enum COMType
    {
        VISA = 0
    }

    public enum InstrucmentInterface
    {
        USB = 0,
        LAN = 1,
        COM = 2
    }

    public class InstrucmentInfo
    {
        public string Address { get; set; }
        public COMType ComType { get; set; }
        public InstrucmentInterface Interface { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
    }
}
