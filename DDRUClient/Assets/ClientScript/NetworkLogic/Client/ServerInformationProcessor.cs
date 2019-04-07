using DDRCommProto;
using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ServerInformationProcessor : BaseProcessor
{

    public override void Process(MessageSerializer serializer, IMessage msg)
    {
        bcLSAddr bcmsg = (bcLSAddr)msg;

        string s = "";
        foreach(var info in bcmsg.LSInfos)
        {
            foreach(var remoteip in info.Ips)
            {

                s += " : " + remoteip;

                string hostName = Dns.GetHostName();
                foreach(var localip in Dns.GetHostEntry(hostName).AddressList)
                {
                    if(localip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        byte[] localbytes = localip.GetAddressBytes();

                        IPAddress remoteipaddr;
                        if (IPAddress.TryParse(remoteip, out remoteipaddr))
                        {
                            byte[] remotebytes = remoteipaddr.GetAddressBytes();

                            localbytes[3] = 0;
                            remotebytes[3] = 0;
                            if ((localbytes[0]  == remotebytes[0] )&&
                                (localbytes[1] == remotebytes[1]) &&
                                (localbytes[2] == remotebytes[2]) )
                            {
                                if(!remoteip.Contains("183"))
                                {
                                    break;
                                }
                                MainUILogic.mInstance.m_TcpIP = remoteip;
                                MainUILogic.mInstance.m_TcpPort = info.Port;
                                break;
                            }

                        }
                        
                    }

                }

            }
            Debug.Log(info.Name + s);
        }
        //bcmsg.ServerName ="ClientName 127";
        //AsyncUdpClient.getInstance().SendTo("127.0.0.1", bcmsg);
        //bcmsg.ServerName = "ClientName 183";
        //AsyncUdpClient.getInstance().SendTo("192.168.1.183", bcmsg);

    }

}