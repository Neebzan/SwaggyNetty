using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject Occupant;
    //Get the Renderer component from the new cube
    Renderer renderer;
    public Color color = Color.white;
    public int X;
    public int Y;

    private void Awake()
    {
        renderer = gameObject.GetComponent<Renderer>();
    }

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
        color = Color.green;
        //Call SetColor using the shader property name "_Color" and setting the color to red
        renderer.material.SetColor("_Color", color);
    }

    public void MarkCell()
    {
        //Call SetColor using the shader property name "_Color" and setting the color to red

        color = Color.blue;
        renderer.material.SetColor("_Color", color);

    }

    public void UnmarkCell()
    {
        //Call SetColor using the shader property name "_Color" and setting the color to red
        color = Color.white;
        renderer.material.SetColor("_Color", color);
    }

    public void SetColor(Color col)
    {
        renderer.material.SetColor("_Color", col);
    }

    public void UnoccupyCell()
    {
        Occupant = null;

        //Call SetColor using the shader property name "_Color" and setting the color to red
        renderer.material.SetColor("_Color", Color.white);
    }

}
