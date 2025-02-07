

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Vulture : MonoBehaviour
{
    [Header("Vulture general movement settings.")]
    [Tooltip("Speed of the vulture.")] public float speed = 3.0f;
    [Tooltip("Maximum banking angle.")] public float maxBankingAngle = 45.0f;
    [Tooltip("Turn speed")] public float turnSpeed = 30.0f;

    [Header("Flapping")]
    [Tooltip("Flap force power")] public float flapForce;
    [Tooltip("How often to flap.")] public float flapFrequency = 3.0f;
    [Tooltip("How long to flap for.")] public float flapTime = 1.0f;

    [Header("Vulture wandering settings")]
    public bool enableWandering = true;
    [Tooltip("How far from starting Pos to wander off to")] public float wanderRange = 50.0f;
    [Tooltip("How far much to offset in height")] public float wanderHeightOffset = 10.0f;
    [Tooltip("How many wander points")] [Range(2, 10)] public int numberOfWanderPoints = 4;
    [Tooltip("How near to get to wander point")] public float wanderPointProximity = 20.0f;
    [Tooltip("Preferred orbit distance")] [Range(0f, 1f)] public float orbitDistance = 1.0f;
    [Tooltip("Deadzone radius")] public float deadzoneRadius = 1.0f;
    [Tooltip("Percent chance to keep circling before moving on")] [Range(0f, 1f)] public float chanceToMoveOn = 0.002f;

    [Header("Vulture Head look settings")]
    [Tooltip("How often to change look")] public float changelookEveryX = 2.0f;

    public bool debugMode;
    private int lookpose = 0; //Holds the look position until its changed again

    float timeSinceLastFlap;
    float timeSpentFlapping;
    bool flap;
    Vector3[] wanderPoints;
    int wanderIndex;
    float timeLastLookChanged;
    public Rigidbody rigidBody;
    public Animator anim;
    public Transform banker;

    void Start()
    {
        GenerateWanderPoints();
        flapFrequency = flapFrequency + (Random.Range(0, 100) * 0.02f);
    }

    void GenerateWanderPoints()
    {
        if (!enableWandering)
            return;
        wanderPoints = new Vector3[numberOfWanderPoints];
        for (int i = 0; i < numberOfWanderPoints; i++)
        {
            Vector2 circle = Random.insideUnitCircle * wanderRange;
            float circleHeight = Random.Range(0, wanderHeightOffset);
            Vector3 newPos = new Vector3(circle.x, circleHeight, circle.y) + transform.position;
            wanderPoints[i] = newPos;
        }
    }

    void Update()
    {
        rigidBody.AddForce((transform.forward * (speed * 100)) * Time.deltaTime, ForceMode.VelocityChange);

        HandleWandering();

        timeSinceLastFlap += Time.deltaTime;

        if (timeSinceLastFlap > flapFrequency)
            Flap();

        if (flap)
        {
            timeSpentFlapping += Time.deltaTime;
            if (timeSpentFlapping > flapTime)
            {
                flap = false;
                anim.SetBool("Flap", false);
                if (debugMode)
                    Debug.Log("Vulture exiting flap");
            }
        }

        timeLastLookChanged += Time.deltaTime;
        if (timeLastLookChanged > changelookEveryX)
        {
            timeLastLookChanged = 0;
            lookpose = Random.Range(0, 4);
        }
        anim.SetInteger("Look", lookpose);
    }

    void HandleWandering()
    {
        // Turn to face our wander index
        // Transform.LookAt(wanderPoints[wanderIndex]);
        Vector3 point = wanderPoints[wanderIndex];
        Vector3 lookTowardWanderIndex = point - transform.position;

        Vector3 lookToward = Quaternion.LookRotation(lookTowardWanderIndex, Vector3.up) * Vector3.forward;
        Vector3 cross = -Vector3.Cross(lookTowardWanderIndex - lookToward, Vector3.up).normalized * (wanderPointProximity * orbitDistance);

        if (debugMode)
            Debug.DrawRay(point, cross);

        float bankingAngle = maxBankingAngle * -Vector3.Dot(transform.right, lookTowardWanderIndex.normalized);
        Quaternion lastRollRot = banker.localRotation;
        float turnStep = Time.deltaTime * turnSpeed;
        banker.localRotation = Quaternion.RotateTowards(lastRollRot, Quaternion.AngleAxis(bankingAngle, Vector3.forward), turnStep);

        if (Vector3.Distance(transform.position, point) > deadzoneRadius)
        {
            //transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookTowardWanderIndex), turnStep);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookTowardWanderIndex + cross), turnStep);
        }

        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0f);


        // If within x of our wander index move to next wander index
        if (
            Vector3.Distance(transform.position, point) < wanderPointProximity &&
            Random.value < chanceToMoveOn
        )
        {
            if (wanderIndex < numberOfWanderPoints - 1)
                wanderIndex++;
            else
                wanderIndex = 0;
        }
    }


    void Flap()
    {
        timeSinceLastFlap = 0;
        timeSpentFlapping = 0;
        rigidBody.AddForce(transform.up * flapForce, ForceMode.Impulse);
        flap = true;
        anim.SetBool("Flap", true);
        if (debugMode)
            Debug.Log("Vulture entering flap");
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !enableWandering || !debugMode)
            return;

        for (int i = 0; i < numberOfWanderPoints; i++)
        {
            Vector3 wanderPoint = wanderPoints[i];
            bool inPoint = Vector3.Distance(transform.position, wanderPoint) < wanderPointProximity;
            if (i == wanderIndex)
            {
                Gizmos.color = Color.yellow;
                if (inPoint)
                    Gizmos.color = Color.green;
            }
            else if (inPoint)
            {
                Gizmos.color = Color.blue;
            }
            else
            {
                Gizmos.color = Color.red;
            }

            Gizmos.DrawSphere(wanderPoint, 0.3f);
            Gizmos.DrawWireSphere(wanderPoint, wanderPointProximity);
        }

        Gizmos.color = Color.red;
        for (int i = 0; i < numberOfWanderPoints - 1; i++)
        {
            Gizmos.DrawLine(wanderPoints[i], wanderPoints[i + 1]);
        }

        Gizmos.DrawLine(wanderPoints[numberOfWanderPoints - 1], wanderPoints[0]);
    }
}

