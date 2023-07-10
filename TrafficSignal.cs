using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficSignal
{
    List<Light[]> lights;
    List<bool[]> cycle;
    public float stopDistance;
    int cycleIndex = 0;
    float cycleLength = 10f;
    float timeSinceLastCycle = 0f;
    float t;
    int i = 0;
    // Start is called before the first frame update
    public TrafficSignal(List<bool[]> c, float stopDist)
    {
        cycle = c;

        t = 0;
        stopDistance = stopDist;

        lights = new List<Light[]>();
        lights.Add(new Light[2]);
        lights.Add(new Light[2]);

        foreach(Transform child in GameObject.Find("VerticalTrafficLight").transform){
            lights[0][i] = child.GetComponent<Light>();
            i++;
        }

        i = 0;
        foreach(Transform child in GameObject.Find("HorizontalTrafficLight").transform){
            lights[1][i] = child.GetComponent<Light>();
            i++;
        }
    }

    // Update is called once per frame
    public void UpdateSignal()
    {
        //works
        t += Time.deltaTime;
        if(t >= cycleLength){
            cycleIndex = (cycleIndex + 1) % cycle.Count;
            t = 0;
        }

        for(int i = 0; i < lights.Count; i++){
            if(CurrentCycle()[i]){
                foreach(Light l in lights[i]){
                    l.color = Color.green;
                }
            }else{
                foreach(Light l in lights[i]){
                    l.color = Color.red;
                }
            }
        }
    }

    public bool[] CurrentCycle(){
        return cycle[cycleIndex];
    }
}
