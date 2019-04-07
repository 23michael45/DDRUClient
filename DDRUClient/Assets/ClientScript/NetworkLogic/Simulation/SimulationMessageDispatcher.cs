using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DDRCommProto;

public class SimulationMessageDispatcher : BaseMessageDispatcher
{
    public SimulationMessageDispatcher()
    {
        notifyDifferentialDrive notifyDifferentialDrive = new notifyDifferentialDrive();
        m_ProcessorMap.Add(notifyDifferentialDrive.GetType().ToString(), new DifferentialDriveProcessor());





    }
}
