/// <summary>
/// 网络事件协议ID
/// </summary>
public enum eProtocalCommand
{
    /// <summary>
    /// 二进制形式收发
    /// </summary>
    sc_binary_login = 0x1000,
    /// <summary>
    /// ProtonBuf形式收发
    /// </summary>
    sc_protobuf_login = 0x2000,
}