﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerater : MonoBehaviour
{
    public int cellHeigth = 1;
    public int cellWidth = 1;
    public bool clientSide = true;

    public int gridWidth = 10;
    public int gridHeigth = 10;

    public GameObject[,] grid;


    // Start is called before the first frame update
    void Start()
    {
        if (!clientSide)
            Generate();
    }

    public void Generate(int width, int heigth)
    {
        GameObject gridPrefab = (GameObject)Resources.Load("Prefabs/Cell");

        int xOffset = (width / cellWidth) / 2;
        int yOffset = (heigth / cellHeigth) / 2;

        grid = new GameObject[width, heigth];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < heigth; y++)
            {
                grid[x, y] = Instantiate<GameObject>(gridPrefab, new Vector3(x - xOffset, y - yOffset), Quaternion.identity);
                grid[x, y].gameObject.transform.parent = this.transform;
                grid[x, y].GetComponent<Cell>().X = x;
                grid[x, y].GetComponent<Cell>().Y = y;


            }
        }
    }

    public void Generate()
    {
        GameObject gridPrefab = (GameObject)Resources.Load("Prefabs/Cell");

        int xOffset = (gridWidth / cellWidth) / 2;
        int yOffset = (gridHeigth / cellHeigth) / 2;

        grid = new GameObject[gridWidth, gridHeigth];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeigth; y++)
            {
                grid[x, y] = Instantiate<GameObject>(gridPrefab, new Vector3(x - xOffset, y - yOffset), Quaternion.identity);
                grid[x, y].gameObject.transform.parent = this.transform;
                grid[x, y].GetComponent<Cell>().X = x;
                grid[x, y].GetComponent<Cell>().Y = y;
            }
        }
    }

    public Vector2 GetCellPosition(Vector2 index)
    {
        return grid[(int)index.x, (int)index.y].gameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
