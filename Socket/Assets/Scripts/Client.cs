using UnityEngine;
using System.Collections;
using System.Text;
using System;
using ProtoBuf;
using UnityEngine.UI;


public class Client : MonoBehaviour {

    public Transform BtnRoot;

    void Start()
    {
        RegeditControl();
    }
    

    void OnEnable()
    {
        MessageCenter.Instance.AddEventListener(eGameLogicEventType.NoticeInfo, CallBack_PoseEvent);
        MessageCenter.Instance.addObsever(eProtocalCommand.sc_protobuf_login, CallBack_ProtoBuff_LoginServer);
        MessageCenter.Instance.addObsever(eProtocalCommand.sc_binary_login, CallBack_Binary_LoginServer);
    }

    void OnDisable()
    {
        MessageCenter.Instance.RemoveEventListener(eGameLogicEventType.NoticeInfo, CallBack_PoseEvent);
        MessageCenter.Instance.removeObserver(eProtocalCommand.sc_protobuf_login, CallBack_ProtoBuff_LoginServer);
        MessageCenter.Instance.removeObserver(eProtocalCommand.sc_binary_login, CallBack_Binary_LoginServer);
    }

    void OnApplicationQuit()
    {
        SocketManager.Instance.Close();
    }

    private void RegeditControl()
    {
        BtnRoot.Find("Btn_Connect").GetComponent<Button>().onClick.AddListener(OnButton_Connect);
        BtnRoot.Find("Btn_DisConnect").GetComponent<Button>().onClick.AddListener(OnButton_DisConnect);
        BtnRoot.Find("Btn_PostEvent_NoticeInfo").GetComponent<Button>().onClick.AddListener(OnButton_PostEvent);
        BtnRoot.Find("Btn_SendMsg_Protobuf").GetComponent<Button>().onClick.AddListener(OnButton_ProtoBuff_SendMsg);
        BtnRoot.Find("Btn_SendMsg_Binary").GetComponent<Button>().onClick.AddListener(OnButton_Binary_SendMsg);
    }


    private void OnButton_Connect()
    {
        SocketManager.Instance.Connect(GameConst.IP, GameConst.Port);
        //SocketManager.Instance.Connect("192.168.0.112",55555);
    }

    private void OnButton_DisConnect()
    {
        SocketManager.Instance.Close();
    }

    private void OnButton_PostEvent()
    {
        string _content = "GameLogicEvent";
        MessageCenter.Instance.PostEvent(eGameLogicEventType.NoticeInfo, _content);
    }

    private void OnButton_ProtoBuff_SendMsg()//protonBuf方式发送
    {
        gprotocol.CS_LOGINSERVER _cs_loginServer = new gprotocol.CS_LOGINSERVER();
        _cs_loginServer.account = "ProtoBuf_LogicData";
        _cs_loginServer.password = "ProtoBuf_123456";
        SocketManager.Instance.SendMsg(eProtocalCommand.sc_protobuf_login, _cs_loginServer);
    }
    
    
    private void OnButton_Binary_SendMsg()//二进制方式发送
    {
        ByteStreamBuff _tmpbuff = new ByteStreamBuff();
        //_tmpbuff.Write_Int(1314);
        //_tmpbuff.Write_Float(99.99f);
        //_tmpbuff.Write_UniCodeString("Claine");
        //_tmpbuff.Write_UniCodeString("123456");
        string str = "###122222!!!";
        byte[] buff = System.Text.Encoding.Default.GetBytes(str);
        for (int i = 0; i < 100; i++)
        {
            _tmpbuff.Write_Bytes(buff);
        }
       
        SocketManager.Instance.SendMsg(eProtocalCommand.sc_binary_login, _tmpbuff);
    }



    private void CallBack_PoseEvent(object _eventParam)
    {
        string _content = (string)_eventParam;
        Debug.Log(_content);
    }

    private void CallBack_ProtoBuff_LoginServer(byte[] _msgData)
    {
        gprotocol.CS_LOGINSERVER _tmpLoginServer = SocketManager.ProtoBuf_Deserialize<gprotocol.CS_LOGINSERVER>(_msgData);
        Debug.Log(_tmpLoginServer.account);
        Debug.Log(_tmpLoginServer.password);
    }

    private void CallBack_Binary_LoginServer(byte[] _msgData)
    {
        //ByteStreamBuff _tmpbuff = new ByteStreamBuff(_msgData);//将收到的byte数组数据转换成二进制流来读取
        //Debug.Log(_tmpbuff.Read_Int());
        //Debug.Log(_tmpbuff.Read_Float());
        //Debug.Log(_tmpbuff.Read_UniCodeString());
        //Debug.Log(_tmpbuff.Read_UniCodeString());
        string str = string.Empty;
        str = System.Text.Encoding.Default.GetString(_msgData);
        Debug.Log(str);

        //_tmpbuff.Close();
        //_tmpbuff = null;
    }
}
