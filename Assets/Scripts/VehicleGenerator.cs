using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct VehicleConfig
{   
    public int length;
    public float minDistance;
    public float maxAccel;
    public float comfyAccel;
    public float maxVelocity;
    public float velocity;
    public float carYOffset;
    public int[] path;

    public VehicleConfig(int l, float minDist, float maxA, float comfyA, float maxV, float v, float offset, int[] p)
    {
        length = l;
        minDistance = minDist;
        maxAccel = maxA;
        comfyAccel = comfyA;
        maxVelocity = maxV;
        velocity = v;
        carYOffset = offset;
        path = p;
    }

    public VehicleConfig(int[] p)
    {
        length = 8;
        minDistance = 5f;
        maxAccel = 15f;
        comfyAccel = 7f;
        maxVelocity = 60f;
        velocity = 30f;
        path = p;
        carYOffset = 0.5f;
    }
}

public class VehicleGenerator
{
    public Simulation simulation;
    int rate;
    float timeSinceLastGen;
    float t;

    List<(int Weight, VehicleConfig Config)> vehicles;
    VehicleConfig upcomingVehicle;
    Vector2 spawnPoint;
    int roadIndex;
    float randomBuffer;
    
    public VehicleGenerator(List<(int Weight, VehicleConfig config)> vehs, Simulation sim, int r = 20){
        vehicles = new List<(int, VehicleConfig)>();
        foreach((int weight, VehicleConfig config) i in vehs){
            vehicles.Add(i);
        }
        //roadIndex = startingRoadIndex;

        upcomingVehicle = GenerateVehicle();

        rate = r;
        timeSinceLastGen = 0;
        t = 0;
        simulation = sim;
        randomBuffer = UnityEngine.Random.value * 2;
    }

    // Update is called once per frame
    public void UpdateGenerator()
    {
        t += Time.deltaTime;
        if(t - timeSinceLastGen - randomBuffer >= 60f / rate){  
            Segment segment = simulation.roads[upcomingVehicle.path[0]];
            //if there is space for the vehicle to spawn
            if(segment.vehicles.Count == 0 || segment.vehicles[segment.vehicles.Count - 1].position > upcomingVehicle.minDistance + upcomingVehicle.length){
                simulation.CreateVehicle(upcomingVehicle);
                timeSinceLastGen = t;
            }
            upcomingVehicle = GenerateVehicle();
            randomBuffer = UnityEngine.Random.value * 2;
        }
    }

    public VehicleConfig GenerateVehicle(){
        int total = 0;
        for(int i = 0; i < vehicles.Count; i++){
            total += vehicles[i].Weight;
        }
        
        //random value from 1 to sum of weights
        int r = (int)Mathf.Floor(UnityEngine.Random.value * total + 1);

        //subtract weights from total
        int count = -1;
        while(r > 0){
            count++;
            r -= vehicles[count % vehicles.Count].Weight;
        }

        //use config linked with current weight
        VehicleConfig vehicle = vehicles[count].Config;
        return vehicle;
    }
}
