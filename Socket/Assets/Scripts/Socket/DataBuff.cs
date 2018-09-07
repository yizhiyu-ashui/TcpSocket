using System.IO;
using System;
using UnityEngine;

public class ConstantsTAG
{
    public static string DataHead_Str = "####";
    public static string DataEnd_Str = "!!!";
}

/// <summary>
/// 常量数据
/// </summary>
public class Constants
{
    //消息：数据总长度(4byte) + 数据类型(2byte) + 数据(N byte)
    public static int HEAD_DATA_LEN = 4;
    public static int HEAD_TYPE_LEN = 2;
    public static int HEAD_LEN//6byte
    {
        get { return HEAD_DATA_LEN + HEAD_TYPE_LEN; }
    }
}

/// <summary>
/// 网络数据结构
/// </summary>
[System.Serializable]
public struct sSocketData
{
    public byte[] _data;
    public eProtocalCommand _protocallType;
    public int _buffLength;
    public int _dataLength;
}

/// <summary>
/// 网络数据缓存器，
/// </summary>
[System.Serializable]
public class DataBuffer
{//自动大小数据缓存器
    private int _minBuffLen;
    private byte[] _buff;
    private int _curBuffPosition;
    private int _buffLength = 0;
    private int _dataLength;
    private UInt16 _protocalType;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="_minBuffLen">最小缓冲区大小</param>
    public DataBuffer(int _minBuffLen = 1024)
    {
        if (_minBuffLen <= 0)
        {
            this._minBuffLen = 1024;
        }
        else
        {
            this._minBuffLen = _minBuffLen;
        }
        _buff = new byte[this._minBuffLen];
    }

    /// <summary>
    /// 添加缓存数据
    /// </summary>
    /// <param name="_data"></param>
    /// <param name="_dataLen"></param>
    public void AddBuffer(byte[] _data, int _dataLen)
    {
        if (_dataLen > _buff.Length - _curBuffPosition)//超过当前缓存
        {
            byte[] _tmpBuff = new byte[_curBuffPosition + _dataLen];
            Array.Copy(_buff, 0, _tmpBuff, 0, _curBuffPosition);
            Array.Copy(_data, 0, _tmpBuff, _curBuffPosition, _dataLen);
            _buff = _tmpBuff;
            _tmpBuff = null;
        }
        else
        {
            Array.Copy(_data, 0, _buff, _curBuffPosition, _dataLen);
        }
        _curBuffPosition += _dataLen;//修改当前数据标记
    }

    /// <summary>
    /// 更新数据长度
    /// </summary>
    public void UpdateDataLength()
    {
        if (_dataLength == 0 && _curBuffPosition >= Constants.HEAD_LEN)
        {
            byte[] tmpDataLen = new byte[Constants.HEAD_DATA_LEN];
            Array.Copy(_buff, 0, tmpDataLen, 0, Constants.HEAD_DATA_LEN);
            _buffLength = BitConverter.ToInt32(tmpDataLen, 0);//返回转换的字节数组中指定位置处的四个字节从 32 位有符号的整数,即包长

            byte[] tmpProtocalType = new byte[Constants.HEAD_TYPE_LEN];
            Array.Copy(_buff, Constants.HEAD_DATA_LEN, tmpProtocalType, 0, Constants.HEAD_TYPE_LEN);
            _protocalType = BitConverter.ToUInt16(tmpProtocalType, 0);//返回从字节数组中指定位置处的两个字节转换的 16 位无符号的整数

            _dataLength = _buffLength - Constants.HEAD_LEN;
        }
    }

    /// <summary>
    /// 获取一条可用数据，返回值标记是否有数据
    /// (根据包长来获取一条完整数据)
    /// </summary>
    /// <param name="_tmpSocketData"></param>
    /// <returns></returns>
    public bool GetData(out sSocketData _tmpSocketData)
    {
        _tmpSocketData = new sSocketData();

        if (_buffLength <= 0)
        {
            UpdateDataLength();
        }

        if (_buffLength > 0 && _curBuffPosition >= _buffLength)
        {
            _tmpSocketData._buffLength = _buffLength;
            _tmpSocketData._dataLength = _dataLength;
            _tmpSocketData._protocallType = (eProtocalCommand)_protocalType;
            _tmpSocketData._data = new byte[_dataLength];
            Array.Copy(_buff, Constants.HEAD_LEN, _tmpSocketData._data, 0, _dataLength);//从_buff数组下标Constants.HEAD_LEN开始复制到_tmpSocketData._data数组中，复制长度为_dataLength
            _curBuffPosition -= _buffLength;//当前剩余长度 -= 刚取出的包长
            byte[] _tmpBuff = new byte[_curBuffPosition < _minBuffLen ? _minBuffLen : _curBuffPosition];
            Array.Copy(_buff, _buffLength, _tmpBuff, 0, _curBuffPosition);
            _buff = _tmpBuff;


            _buffLength = 0;
            _dataLength = 0;
            _protocalType = 0;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 获取一条可用数据，返回值标记是否有数据
    /// （根据包头包尾的特殊字符）
    /// </summary>
    /// <param name="startStr">开始字符串</param>
    /// <param name="endStr">结束字符串</param>
    /// <param name="_tmpSocketData"></param>
    /// <returns></returns>
    public bool GetData(string startStr ,string endStr, out sSocketData _tmpSocketData)
    {
        _tmpSocketData = new sSocketData();
        byte[] startBuff = System.Text.Encoding.Default.GetBytes(startStr);
        byte[] endBuff = System.Text.Encoding.Default.GetBytes(endStr);
        //定义两个位置标识
        int _dataStartPosi = 0;//开始位置，即包头位置
        int _dataEndPosi = startBuff.Length;//结束位置，即包尾位置（结束位置至少在包头之后）
        //定义两个bool变量判断是否找到包头包尾
        bool isHaveHead = false;
        bool isHaveEnd = false;
        byte[] _tmpBuff = null;

        //搜索包头
        while (_dataStartPosi < _curBuffPosition)
        {
            if (_buff[_dataStartPosi] == startBuff[0])
            {
                _tmpBuff = new byte[startBuff.Length];
                Array.Copy(_buff, _dataStartPosi, _tmpBuff, 0, _tmpBuff.Length);
                if (CompareBytes(_tmpBuff, startBuff))
                {
                    _dataStartPosi = startBuff.Length; //修改有用数据的开始位置（找到包头）
                    _dataEndPosi = _dataStartPosi;
                    isHaveHead = true;
                    break; //找到包头就跳出循环
                }
                else
                {
                    _dataStartPosi += 1;//继续往后搜索
                    continue;
                }
            }
            else
            {
                _dataStartPosi += 1;//继续往后搜索
            }
        }
        //搜索包尾
        while (_dataEndPosi < _curBuffPosition)
        {
            if (_buff[_dataEndPosi] == endBuff[0])
            {
                _tmpBuff = new byte[endBuff.Length];
                Array.Copy(_buff, _dataEndPosi, _tmpBuff, 0, _tmpBuff.Length);
                if (CompareBytes(_tmpBuff, endBuff))
                {
                    isHaveEnd = true;
                    break; //找到包尾就退出
                }
                else
                    _dataEndPosi += 1;
            }
            else
                _dataEndPosi += 1;
        }

        if (isHaveHead && isHaveEnd)
        {
            _dataLength = _dataEndPosi - _dataStartPosi;
            _buffLength = _dataEndPosi + endBuff.Length;
            if (_buffLength > 0 && _curBuffPosition >= _buffLength)
            {
                _tmpSocketData._buffLength = _buffLength;
                _tmpSocketData._dataLength = _dataLength;
                _tmpSocketData._protocallType = eProtocalCommand.sc_binary_login;
                _tmpSocketData._data = new byte[_dataLength];
                Array.Copy(_buff, _dataStartPosi, _tmpSocketData._data, 0, _dataLength);
                _curBuffPosition -= _buffLength;//当前剩余长度 -= 刚取出的包长
                _tmpBuff = new byte[_curBuffPosition < _minBuffLen ? _minBuffLen : _curBuffPosition];
                Array.Copy(_buff, _buffLength, _tmpBuff, 0, _curBuffPosition);
                _buff = _tmpBuff;

                _buffLength = 0;
                _dataLength = 0;
                return true;
            }    
        }

        return false;
    }
    /// <summary>
    /// 比较两个byte数组
    /// </summary>
    /// <param name="b1"></param>
    /// <param name="b2"></param>
    /// <returns></returns>
    private bool CompareBytes(byte[] b1,byte[] b2)
    {
        if (b1 == null || b2 == null)
            return false;
        if (b1.Length != b2.Length)
            return false;
        string str1 = System.Text.Encoding.Default.GetString(b1);
        string str2 = System.Text.Encoding.Default.GetString(b2);
        if (str1.Equals(str2))
        {
            return true;
        }
        return false;
    }

}