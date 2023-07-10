using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public struct Segment{
    public Vector2 start, end;
    public float length, angleSin, angleCos;
    public List<Vehicle> vehicles;
    public TrafficSignal signal;
    public int group;

    public Segment(Vector2 s, Vector2 e, TrafficSignal ts, int g){
        start = s;
        end = e;
        length = Vector2.Distance(end, start);
        angleSin = (end.y - start.y) / length;
        angleCos = (end.x - start.x) / length;
        vehicles = new List<Vehicle>();
        signal = ts;
        group = g;
    }
}

public class Simulation : MonoBehaviour
{
    float t;
    int frameCount;

    public List<Segment> roads;
    public List<VehicleGenerator> vehicleGenerators;
    public GameObject vehiclePrefab;
    public TrafficSignal ts;
    
    // Start is called before the first frame update
    void Start()
    {
        t = 0;
        frameCount = 0;

        List<bool[]> cycles = new List<bool[]>(); 
        cycles.Add(new bool[] {true, false});
        cycles.Add(new bool[] {false, true});
        ts = new TrafficSignal(cycles, 100f);
        
        roads = new List<Segment>();
        //vertical
        CreateRoad(new Vector2(-7f, 200f), new Vector2(-7f, 15f), ts, 0);
        CreateRoad(new Vector2(-7f, 15f), new Vector2(-7f, -200f), ts, 0);

        CreateRoad(new Vector2(7f, -200f), new Vector2(7f, -15f), ts, 0);
        CreateRoad(new Vector2(7f, -15f), new Vector2(7f, 200f), ts, 0);

        //horizontal
        CreateRoad(new Vector2(200f, 7f), new Vector2(15f, 7f), ts, 1);
        CreateRoad(new Vector2(15f, 7f), new Vector2(-200f, 7f), ts, 1);

        CreateRoad(new Vector2(-200f, -7f), new Vector2(-15f, -7f), ts, 1);
        CreateRoad(new Vector2(-15f, -7f), new Vector2(200f, -7f), ts, 1);

        vehicleGenerators = new List<VehicleGenerator>();
        List<(int Weight, VehicleConfig Config)> vehs = new List<(int Weight, VehicleConfig Config)>();
        vehs.Add((1, new VehicleConfig(8, 4f, 20f, 10f, 50f, 25f, new int[] {0, 1})));
        CreateVehicleGenerator(vehs, 20);

        vehs.RemoveAt(0);
        vehs.Add((1, new VehicleConfig(8, 4f, 20f, 10f, 50f, 25f, new int[] {2, 3})));
        CreateVehicleGenerator(vehs, 20);

        vehs.RemoveAt(0);
        vehs.Add((1, new VehicleConfig(8, 4f, 20f, 10f, 50f, 25f, new int[] {4, 5})));
        CreateVehicleGenerator(vehs, 20);

        vehs.RemoveAt(0);
        vehs.Add((1, new VehicleConfig(8, 4f, 20f, 10f, 50f, 25f, new int[] {6, 7})));
        CreateVehicleGenerator(vehs, 20);
    }

    // Update is called once per frame
    public void Update()
    {
        foreach(Segment road in roads){
            if(road.vehicles.Count == 0) continue;

            //update all vehicles
            Vehicle vehicle = road.vehicles[0];
            vehicle.UpdateVehicle(null);

            if(road.vehicles.Count > 1){
                for(int i = 1; i < road.vehicles.Count; i++){
                    road.vehicles[i].UpdateVehicle(road.vehicles[i - 1]);
                }
            }
            
            //if furthest ahead vehicle has reached the end of the road
            if(vehicle.position >= road.length){
                if(vehicle.roadIndex + 1 < vehicle.path.Length){
                    //add to next road
                    vehicle.roadIndex++;

                    int nextIndex = vehicle.path[vehicle.roadIndex];
                    roads[nextIndex].vehicles.Add(vehicle);
                }else{
                    vehicle.Despawn();
                }
                vehicle.position = 0;
                //remove from current road
                road.vehicles.RemoveAt(0);
            }

            //check for traffic lights
            //let vehicles go if traffic light is green
            if(ts.CurrentCycle()[road.group]){
                vehicle.Go();
            }else{
                if(vehicle.position >= road.length - ts.stopDistance && 
                vehicle.position <= road.length - ts.stopDistance / 2){
                    vehicle.Stop();
                }
            }
        } 

        if(vehicleGenerators.Count > 0){
            foreach(VehicleGenerator vg in vehicleGenerators){
                vg.UpdateGenerator();
            }
        }

        ts.UpdateSignal();
        
        t += Time.deltaTime;
        frameCount++; 
    }

    public void OnDrawGizmos(){
        if(EditorApplication.isPlaying && !EditorApplication.isPaused){
            for(int i = 0; i < roads.Count; i++){
                Gizmos.DrawLine(new Vector3(roads[i].start.x, 0, roads[i].start.y), new Vector3(roads[i].end.x, 0, roads[i].end.y));
            }
        }
    }

    public void CreateVehicle(VehicleConfig config){
        Segment road = roads[config.path[0]];
        float rotationAngle = Vector2.SignedAngle(road.end - road.start, Vector2.right);
        
        GameObject test = Instantiate(vehiclePrefab, new Vector3(road.start.x, 2, road.start.y), Quaternion.Euler(new Vector3(0, rotationAngle - 90, 0)));
        
        Vehicle vehicle = test.AddComponent<Vehicle>();

        vehicle.SetProperties(config);
        vehicle.SetSimulation(this);

        road.vehicles.Add(vehicle);
    }

    public void CreateRoad(Vector2 start, Vector2 end, TrafficSignal ts, int g){
        Segment road = new Segment(start, end, ts, g);
        roads.Add(road);
    }

    public void CreateRoads(List<Segment> roadList, TrafficSignal ts, int g){
        foreach(Segment r in roadList){
            CreateRoad(r.start, r.end, ts, g);
        }
    }

    public void CreateVehicleGenerator(List<(int Weight, VehicleConfig Config)> vehicles, int r = 20)
    {
        VehicleGenerator vg = new VehicleGenerator(vehicles, this, r);
        vehicleGenerators.Add(vg);
    }
}
