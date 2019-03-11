using DDRCommProto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TcpConnectionLogic : MonoBehaviour
{


    public Button m_LoginBtn;
    public Button m_CmdBtn;
    public Button m_StartHeartbeatBtn;
    public Button m_StopHeartbeatBtn;
    public Button m_CloseBtn;

    public Text m_NameTxt;
    public TextMeshProUGUI m_MsgTxt;


    Coroutine m_HeartBeadCoroutine;
    bool m_HeartBeat;

    AsyncTcpClient m_TcpClient;

    [HideInInspector]
    public MainUILogic m_ParentUILogic;
    // Start is called before the first frame update
    void Start()
    {
        m_NameTxt.text = "Connection:" + DateTime.Now.ToLongTimeString();

        m_HeartBeat = false;

        m_LoginBtn.onClick.AddListener(OnLogin);
        m_CmdBtn.onClick.AddListener(OnCmd);
        m_StartHeartbeatBtn.onClick.AddListener(OnStartHeartbeat);
        m_StopHeartbeatBtn.onClick.AddListener(OnStopHeartbeat);
        m_CloseBtn.onClick.AddListener(OnClose);


        Thread t = new Thread(() =>
        {
            m_TcpClient = new AsyncTcpClient();
            m_TcpClient.OnDisconnected += OnDisconnect;
            //m_TcpClient.Connect("127.0.0.1", 88);
            //m_TcpClient.Connect("192.168.1.137", 88);
            m_TcpClient.Connect(m_ParentUILogic.m_TcpIP,m_ParentUILogic.m_TcpPort);

        });
        t.Start();

    }

    public void OnConnect()
    {
    }
    public void OnDisconnect(object sender, EventArgs e)
    {
        m_TcpClient.Close();
        if (m_HeartBeadCoroutine != null)
        {
            m_HeartBeat = false;
        }

    }
    

    private void OnDestroy()
    {
        m_LoginBtn.onClick.RemoveListener(OnLogin);
        m_StartHeartbeatBtn.onClick.RemoveListener(OnStartHeartbeat);
        m_StopHeartbeatBtn.onClick.RemoveListener(OnStopHeartbeat);
        m_CloseBtn.onClick.RemoveListener(OnClose);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_TcpClient != null)
        {
            m_TcpClient.GetMessageSerializer().Update();
        }


        if (m_TcpClient.IsConnected == false)
        {

            m_ParentUILogic.OnConnectionClose(this);
        }
        else
        {
            
            m_CloseBtn.gameObject.SetActive(true);
        }



    }

    public void OnLogin()
    {
        if (m_TcpClient.IsConnected)
        {
            reqLogin req = new reqLogin();
            req.Username = "admin";
            req.Userpwd = "admin";
            req.Type = eCltType.ELocalPcclient;

            m_TcpClient.Send(req);


        }
    }
    public void OnCmd()
    {
        if (m_TcpClient.IsConnected)
        {
            reqStreamAddr req = new reqStreamAddr();
            req.NetworkType = ChannelNetworkType.Local;

            m_TcpClient.Send(req);


        }

    }
    public void OnStartHeartbeat()
    {
        m_HeartBeadCoroutine = StartCoroutine(HeartBeat());

    }
    public void OnStopHeartbeat()
    {

        m_HeartBeat = false;
    }
    IEnumerator HeartBeat()
    {
        m_HeartBeat = true;
        HeartBeat hb = new HeartBeat();
        hb.Whatever = "hb";
        while (m_HeartBeat)
        {
            yield return new WaitForSeconds(1);


            m_TcpClient.Send(hb);

        }

        yield return 0;

    }
    public void OnClose()
    {
        m_TcpClient.Close();
        if (m_HeartBeadCoroutine != null)
        {
            m_HeartBeat = false;
        }


        m_ParentUILogic.OnConnectionClose(this);
    }
}
