using Plugins.Communication.Extension.Helpers;

namespace Plugins.Communication.VISA.Model
{
    /// <summary>
    /// 使用枚举类型描述符属性，让枚举变量和字符串一一对应
    /// </summary>
    public enum FilterDescriptionType
    {
        [EnumDescription("GPIB[0-9]*::?*INSTR")]
        GPIB,
        [EnumDescription("VXI?*INSTR")]
        VXI,
        [EnumDescription("GPIB-VXI?*INSTR")]
        GPIBVXI,
        [EnumDescription("ASRL[0-9]*::?*INSTR")]
        Serial,
        [EnumDescription("PXI?*INSTR")]
        PXI,
        [EnumDescription("USB?*")]
        USB,
        [EnumDescription("TCPIP?*")]
        LAN,
        [EnumDescription("?*INSTR")]
        ALLINSTR,
        [EnumDescription("?*")]
        ALL
    };
}
