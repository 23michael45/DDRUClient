using UnityEngine;
using System.Collections;

public class ISMObject<T> : MonoBehaviour where T:MonoBehaviour{

    public ISMStateMachine<T> mISM;
	// Use this for initialization
	protected void Start () {
	}
	
	// Update is called once per frame
    protected void Update()
    {

        if (mISM != null)
        {
            mISM.Update();
        }
        
	}

    protected void LateUpdate()
    {
        if (mISM != null)
        {
            mISM.LateUpdate();
        }

    }

    protected void FixedUpdate()
    {
        if (mISM != null)
        {
            mISM.FixedUpdate();
        }

    }

    protected void OnDestroy()
    {
        if (mISM != null)
        {
            mISM.Exit();
        }
    }



    protected virtual void MakeISM()
    {

    }

}
