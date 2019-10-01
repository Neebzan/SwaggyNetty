using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject Occupant;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void OccupyCell(GameObject obj)
    {
        Occupant = obj;
        obj.transform.position = gameObject.transform.position;
        //Get the Renderer component from the new cube
        var cubeRenderer = gameObject.GetComponent<Renderer>();

        //Call SetColor using the shader property name "_Color" and setting the color to red
        cubeRenderer.material.SetColor("_Color", Color.green);
    }

    public void UnoccupyCell()
    {
        Occupant = null;
        //Get the Renderer component from the new cube
        var cubeRenderer = gameObject.GetComponent<Renderer>();

        //Call SetColor using the shader property name "_Color" and setting the color to red
        cubeRenderer.material.SetColor("_Color", Color.red);
    }

}
