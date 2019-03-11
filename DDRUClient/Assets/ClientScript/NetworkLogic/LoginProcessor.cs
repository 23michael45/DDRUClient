using DDRCommProto;
using Google.Protobuf;
using UnityEngine;

public class LoginProcessor :  BaseProcessor {


    public override void Process(MessageSerializer serializer, IMessage msg)
    {
        rspLogin rsp = (rspLogin)msg;


        Debug.Log("SigninProcessor:" + rsp.Retcode);

        //do send message
        //Intent i = new Intent(BaseProcessor.MessageFilter);
        //i.putExtra(Intent.EXTRA_TEXT,rsp.getName() + " " + rsp.getSucc());

        ////i.setData()
        //MainActivity.Instance.sendBroadcast(i);
    }
}
