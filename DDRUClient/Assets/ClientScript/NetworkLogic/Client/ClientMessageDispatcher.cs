using DDRCommProto;

public class ClientMessageDispatcher :  BaseMessageDispatcher {


    public  ClientMessageDispatcher()
    {
        rspLogin rspLogin = new rspLogin();
        m_ProcessorMap.Add(rspLogin.GetType().ToString(), new LoginProcessor());


        rspStreamAddr rspStreamAddr = new rspStreamAddr();
        m_ProcessorMap.Add(rspStreamAddr.GetType().ToString(), new StreamAddrProcessor());


    }

}
