using DDRCommProto;
using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class AsyncUdpClient : BaseSocketConnection
{
    BaseMessageDispatcher m_Dispatcher;

    static AsyncUdpClient mInstance;
    public static AsyncUdpClient getInstance()
    {
        if (mInstance == null)
            mInstance = new AsyncUdpClient();
        return mInstance;
    }

    UdpClient m_Client;


    AsyncUdpClient()
    {
        mInstance = this;
    }
    public override void Close()
    {
        m_Client.Close();
        base.Close();
    }
    public void SetDispatcher(BaseMessageDispatcher dispatcher)
    {
        m_Dispatcher = dispatcher;
    }

    // Use this for initialization
    public void Start()
    {

        Task.Factory.StartNew(() =>
        {
            m_Client = new UdpClient(28888);

            while (true)
            {
                var groupEP = new IPEndPoint(IPAddress.Any, 28888); // listen on any port
                var data = m_Client.Receive(ref groupEP);



                var msg = MessageSerializer.Parse(data);
                if(m_Dispatcher != null)
                {
                    m_Dispatcher.Dispatcher(null, msg.GetType().ToString(), (IMessage)msg);


                }

                //udpServer.Send(new byte[] { 1 }, 1); // if data is received reply letting the client know that we got his data          
            }
        });
    }

    public void SendTo(string ip,IMessage msg)
    {
        var ms = MessageSerializer.Serialize(msg);

        IPAddress serverAddr = IPAddress.Parse(ip);
        IPEndPoint endPoint = new IPEndPoint(serverAddr, 28888);

        m_Client.Connect(endPoint);
        m_Client.Send(ms.GetBuffer(), (int)ms.Length);
    }

}
