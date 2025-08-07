using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    public Simulation simulation;
    public float maxVelocity, effectiveMaxVelocity, maxAccel, smoothing, reactionTime, comfortableAccel, minDistance, length;
    public float velocity, distance, position, freeRoadAccel, interactionAccel, acceleration;
    public bool stopped;
    public int roadIndex;
    public int[] path;
    public int id;
    public bool test;
    public float carYOffset;

    public void SetDefaultProperties()
    {
        effectiveMaxVelocity = maxVelocity;

        smoothing = 4;
        acceleration = 0;

        reactionTime = 0.25f;
        position = 0;

        roadIndex = 0;
        stopped = false;

        carYOffset = 0.5f;
    }

    public void SetProperties(VehicleConfig config){
        length = config.length;
        path = config.path;
        minDistance = config.minDistance;
        maxAccel = config.maxAccel;
        comfortableAccel = config.comfyAccel;
        maxVelocity = config.maxVelocity;
        velocity = config.velocity;
        carYOffset = config.carYOffset;
        SetDefaultProperties();
    }
    
    public void SetSimulation(Simulation sim){
        simulation = sim;
    }

    // Update is called once per frame
    public void UpdateVehicle(Vehicle lead)
    {
        if(transform.position.y < -20){
            Despawn();
        }
        
        //update position and velocity using taylor series expansions
        if(velocity + acceleration * Time.deltaTime < 0){
            position -= Mathf.Pow(velocity, 2) / 2 / acceleration;
            velocity = 0;
        }else{
            velocity += acceleration * Time.deltaTime;
            position += velocity * Time.deltaTime + acceleration * Mathf.Pow(Time.deltaTime, 2) / 2;
        }
        if(test) print(position);
        
        //update acceleration using IDM equation
        freeRoadAccel = 1 - Mathf.Pow(velocity / effectiveMaxVelocity, smoothing);
        
        //only update interaction acceleration if there is a car ahead
        interactionAccel = 0;
        if(lead != null){
            distance = Vector3.Distance(lead.gameObject.transform.position, transform.position) - lead.length;
            interactionAccel = Mathf.Pow(DesiredDistance(velocity, velocity - lead.velocity) / distance, 2);
        }
        
        acceleration = maxAccel * (freeRoadAccel - interactionAccel);

        //stop at traffic light using damping equation
        if(stopped){
            //vf^2 = v0^2 + 2 * a * deltaX
            acceleration = -Mathf.Pow(velocity, 2) / (2 * (simulation.roads[roadIndex].length - position - length / 2));
        }
        
        //update transform.position of vehicle
        Segment currentRoad = simulation.roads[path[roadIndex]];
        float interpolationValue = Mathf.Clamp(position / currentRoad.length, 0f, 1f);
        Vector2 newPos = Vector2.Lerp(currentRoad.start, currentRoad.end, interpolationValue);
        transform.position = new Vector3(newPos.x, carYOffset, newPos.y);
    }

    float DesiredDistance(float velocity, float differenceInVelocity){
        return Mathf.Max(0, minDistance + velocity * reactionTime + (velocity * differenceInVelocity / Mathf.Sqrt(2 * maxAccel * comfortableAccel)));
    }

    public void Stop(){
        stopped = true;
    }

    public void Go(){
        stopped = false;
    }

    public void Despawn(){
        Destroy(gameObject);
    }
}
