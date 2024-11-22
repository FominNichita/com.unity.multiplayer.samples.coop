using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAuth : MonoBehaviour
{
    public RelayPongNetworkManager m_NetworkManager;
    // Start is called before the first frame update
    void Start()
    {
        m_NetworkManager.UnityLogin();
    }


}
