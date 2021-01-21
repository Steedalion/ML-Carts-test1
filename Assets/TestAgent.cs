using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
public class TestAgent : Agent
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
	public override void Heuristic(float[] actionsOut)
	{
		Debug.Log("Entered Heuristic");
		base.Heuristic(actionsOut);
	}
}
