using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MainUILogic : MonoBehaviour
{
    public static MainUILogic mInstance;
    public Button m_CreatConnectionBtn;

    public GameObject m_ConnectionItemPrefab;
    public GameObject m_Content;


    [HideInInspector]
    public string m_TcpIP;
    [HideInInspector]
    public int m_TcpPort;

    private void Awake()
    {
        mInstance = this;
    }


    List<TcpConnectionLogic> m_TcpConnections = new List<TcpConnectionLogic>();
    // Start is called before the first frame update
    void Start()
    {
        m_CreatConnectionBtn.onClick.AddListener(OnCreateConnection);


        AsyncUdpClient.getInstance().SetDispatcher(new BroadcastDispatcher());
        AsyncUdpClient.getInstance().Start();
    }

    private void OnDestroy()
    {
        m_CreatConnectionBtn.onClick.RemoveListener(OnCreateConnection);

        
        AsyncUdpClient.getInstance().Close();

        foreach(var item in m_TcpConnections)
        {

        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCreateConnection()
    {
        GameObject gonew = GameObject.Instantiate(m_ConnectionItemPrefab);
        gonew.transform.parent = m_Content.transform;

        var connection = gonew.GetComponent<TcpConnectionLogic>();
        connection.m_ParentUILogic = this;
        gonew.SetActive(true);
        m_TcpConnections.Add(connection);


    }

    public void OnConnectionClose(TcpConnectionLogic connection)
    {

        m_TcpConnections.Remove(connection);
        GameObject.Destroy(connection.gameObject);
    }
}
