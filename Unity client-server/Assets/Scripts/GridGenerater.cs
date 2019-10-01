using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerater : MonoBehaviour
{
    public int cellHeigth = 1;
    public int cellWidth = 1;

    public int gridWidth = 80;
    public int gridHeigth = 200;

    public int[,] grid;

    // Start is called before the first frame update
    void Start()
    {
        Generate();


    }

    public void Generate()
    {
        UnityEngine.Object gridPrefab = Resources.Load("Prefabs/Cell");

        int xOffset = (gridWidth / cellWidth) / 2;
        int yOffset = (gridHeigth / cellHeigth) / 2;

        grid = new int[gridWidth, gridHeigth];
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeigth; y++)
            {
                Instantiate(gridPrefab, new Vector3(x-xOffset, y-yOffset), Quaternion.identity);
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
