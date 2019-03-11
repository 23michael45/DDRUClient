using DDRCommProto;

public class BroadcastDispatcher :  BaseMessageDispatcher {


    public BroadcastDispatcher()
    {
        bcLSAddr bcSeverInfo = new bcLSAddr();
        m_ProcessorMap.Add(bcSeverInfo.GetType().ToString(), new ServerInformationProcessor());

        
    }

}
