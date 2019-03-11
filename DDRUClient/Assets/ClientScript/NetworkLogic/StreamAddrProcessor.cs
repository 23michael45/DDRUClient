using DDRCommProto;
using Google.Protobuf;
using UnityEngine;

public class StreamAddrProcessor : BaseProcessor {

    public override void Process(MessageSerializer serializer, IMessage msg)
    {
        rspStreamAddr rsp = (rspStreamAddr)msg;


        foreach(var channel in  rsp.Channels)
        {
            Debug.Log(channel.Srcname);
        }

        Debug.Log("StreamAddrProcessor Process");
    }
}
