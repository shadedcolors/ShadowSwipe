using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Keep track of all the light sources in the scene
    public List<GameObject> lightSources;

    private void Awake()
    {
        //Get all light sources in the scene
        GameObject[] lightSourceArray = GameObject.FindGameObjectsWithTag("LightSources");
        lightSources = new List<GameObject>(lightSourceArray);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
