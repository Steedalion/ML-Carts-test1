using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;


public class DriverAgent : Agent
{
	public float moveSpeed = 10f;
	public float turnSpeed = 100f;
	
	public float fallingReward = -100f;
	private float speed;
	private float delaySpeedCheck = 0;
	
	public Transform[] CheckPoints;
	
	private int currentCheckpoint=0;
	
	
	
	
	
	/// <summary>
	/// Initialize ML agent
	/// </summary>
	public override void Initialize()
	{
		Debug.Log("Initializing agent");
		currentCheckpoint = 0;
	}
	
	public override void OnEpisodeBegin()
	{
		Debug.Log("Episode started agent");
		float x = UnityEngine.Random.Range(0f,3f);
		float y = UnityEngine.Random.Range(0.5f,0.8f);
		float z = UnityEngine.Random.Range(-20f,-30f);
		transform.position = new Vector3(x,y,z);
		base.OnEpisodeBegin();
	}
	
	
	/// <summary>
	/// When action is recieved the agent will act.
	/// vectorAction[i]
	/// 1: Steer= left,straight,right [-1,0,1]
	/// 2; Drive = reverse,coast,accelerate [-1,0,1];
	/// </summary>
	/// <param name="vectorAction">The actions taken.</param>
	public override void OnActionReceived(float[] vectorAction)
	{
		float accelerate = vectorAction[0]*Time.deltaTime;
		transform.Translate(0f ,0f , accelerate*moveSpeed);
		
		float steering = vectorAction[1]*Time.deltaTime;
		transform.Rotate(0f, steering*turnSpeed, 0f);
		
		
		base.OnActionReceived(vectorAction);
	}
	/// <summary>
	/// Using heuristic only will return the actions from this method 
	/// <see cref = "OnActionReceived(float[])" /> instead of using the neural network.
	/// </summary>
	/// <param name="actionsOut"></param>
	public override void Heuristic(float[] actionsOut)
	{
		float accelerate = Input.GetAxis("Vertical");
		float steering = Input.GetAxis("Horizontal");
		actionsOut[0] = accelerate;
		actionsOut[1] = steering;
	}
	
	
    // Start is called before the first frame update
    void Start()
    {
	    StartCoroutine(SpeedReckoner());
    }

	// Update is called every frame, if the MonoBehaviour is enabled.
	// Update is called every frame, if the MonoBehaviour is enabled.
	protected void Update()
	{
		if(transform.position.y < -50)
		{//fall;
			AddReward(fallingReward);
			EndEpisode();
		}
	}
	/// <summary>
	/// Collect observations from the environment.
	/// Speed (normalized)
	/// Velocity (normalized)
	/// Steering (Left or right)
	/// Distance to checkpoint.
	/// </summary>
	/// <param name="sensor"></param>
	public override void CollectObservations(VectorSensor sensor)
	{
		base.CollectObservations(sensor);
		//direction 1, continous normalized
		float direction =transform.rotation.y;
		direction = Mathf.Abs(direction);
		sensor.AddObservation(direction);
		
		//speed 1, continous normalized
		sensor.AddObservation(speed/moveSpeed);
		AddReward(speed/moveSpeed*0.001f);
		//distance to checkpoint, continous 
		if(nextCheckpoint() != null)
		{
		Vector3 toCheckpoint = (nextCheckpoint().position - transform.position).normalized;
			float distanceCheckpoint = toCheckpoint.magnitude;
		
			sensor.AddObservation(distanceCheckpoint);
			float dotCheckpoint = Vector3.Dot(toCheckpoint, nextCheckpoint().forward);
			sensor.AddObservation(dotCheckpoint);
		}else
		{
			sensor.AddObservation(0);
			sensor.AddObservation(0);
		}
		
		//dot checkpoint direction player direction, continous normalized
		//1 if same direction
		//0 if perpindicular
		//-1 if opposite direction
		
		
		
	}
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	private Transform nextCheckpoint()
	{
		if(currentCheckpoint >= CheckPoints.Length)
		{
			//Complete
			Debug.Log("Complete");
			currentCheckpoint=0;
			AddReward(5);
			
		}
		
		return CheckPoints[currentCheckpoint];
	}
	
	

	
	// OnTriggerEnter is called when the Collider other enters the trigger.
	protected void OnTriggerEnter(Collider other)
	{
		if((other.CompareTag("Checkpoint") && (other.name.Equals(nextCheckpoint().name
		))))
		{
			Debug.Log("Pass checkpoint: "+other.name);
			AddReward(1);
			currentCheckpoint++;
		}
	}
	
	
	private IEnumerator SpeedReckoner()
	{
		YieldInstruction timedWait = new WaitForSeconds(delaySpeedCheck);
		Vector3 lastPosition = transform.position;
		float lastTimestamp = Time.time;
		
		while(enabled)
		{
			yield return timedWait;
			
			float deltaPosition = (transform.position - lastPosition).magnitude;
			float deltaTime = Time.time - lastTimestamp;
			
			if(Mathf.Approximately(deltaPosition,0))
			{
				//deltaPosition = 0;
			}
			speed = deltaPosition/deltaTime;
			
			lastPosition = transform.position;
			lastTimestamp = Time.time;
		}
	}
	
}
