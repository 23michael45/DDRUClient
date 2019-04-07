using DDRCommProto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimulationLogic : MonoSingleton<SimulationLogic>
{

    public Button m_ConnectBtn;


    public InputField m_IPInput;
    public InputField m_PortInput;

    public DifferentialDriveController m_DifferentialDriveController;


    AsyncTcpClient m_TcpClient;

    [HideInInspector]
    public MainUILogic m_ParentUILogic;
    // Start is called before the first frame update
    void Start()
    {
        m_IPInput.text = "127.0.0.1";
        m_PortInput.text = "99";
        
        m_ConnectBtn.onClick.AddListener(OnConnectBtn);




    }

    public void OnConnectBtn()
    {
        Thread t = new Thread(() =>
        {
            m_TcpClient = new AsyncTcpClient(new SimulationMessageDispatcher());
            m_TcpClient.OnDisconnected += OnDisconnect;
            m_TcpClient.Connect(m_IPInput.text, Convert.ToInt32(m_PortInput.text));

        });
        t.Start();

    }
    public void OnDisconnect(object sender, EventArgs e)
    {
        m_TcpClient.Close();

    }


    private void OnDestroy()
    {
        m_ConnectBtn.onClick.RemoveListener(OnConnectBtn);

    }

    // Update is called once per frame
    void Update()
    {
        if (m_TcpClient != null)
        {
            m_TcpClient.GetMessageSerializer().Update();
        }
    }

    public void OnNotifyDifferentialDrive(notifyDifferentialDrive notify)
    {
        if (m_DifferentialDriveController)
        {
            m_DifferentialDriveController.SetMotorTorque(notify.LeftRPM, notify.RightRPM);
        }
    }
}
