using DDRCommProto;
using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DifferentialDriveProcessor : BaseProcessor
{


    public override void Process(MessageSerializer serializer, IMessage msg)
    {
        notifyDifferentialDrive rsp = (notifyDifferentialDrive)msg;

        Debug.Log(rsp.LeftRPM + ":" + rsp.RightRPM);


        SimulationLogic.Instance.OnNotifyDifferentialDrive(rsp);
    }
}