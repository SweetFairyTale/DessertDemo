using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    //甜品元素类型枚举
    public enum SweetsType
    {
        EMPTY,
        NORMAL,
        BARRIER,
        COLUMN_CLEAR,
        RAINBOW_CANDY,
        COUNT
    }

    public Dictionary<SweetsType, GameObject> sweetsPrefabDic;
    
    [System.Serializable] //C#序列化
    public struct SweetsPrefab
    {
        public SweetsType type;
        public GameObject prefab;
    }

    public SweetsPrefab[] sweetsPrefabs;

    //Single Instance.
    private GameManager _instance;
    public GameManager Instance
    {
        get
        {
            return _instance;
        }

        set
        {
            _instance = value;
        }
    }

    private int columns = 10;
    private int rows = 10;

    public GameObject gridPrefab;

    //Sweets Array
    private SweetsController[,] sweets;

    private void Awake()
    {
        _instance = this;
    }

    // Use this for initialization
    void Start () {
        //为字典赋值
        sweetsPrefabDic = new Dictionary<SweetsType, GameObject>();
        for(int i = 0; i < sweetsPrefabs.Length; i++)
        {
            if(!sweetsPrefabDic.ContainsKey(sweetsPrefabs[i].type))
            {
                sweetsPrefabDic.Add(sweetsPrefabs[i].type, sweetsPrefabs[i].prefab);
            }
        }

		for(int x = 0; x < columns; x++)
        {
            for(int y = 0; y < rows; y++)
            {
                GameObject chocolate = Instantiate(gridPrefab, CalibratePosition(x, y), Quaternion.identity);
                chocolate.transform.SetParent(transform);
            }
        }

        sweets = new SweetsController[columns, rows];
        for(int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                CreateNewSweet(x, y, SweetsType.EMPTY);
                //GameObject newSweet = Instantiate(sweetsPrefabDic[SweetsType.NORMAL], CalibratePosition(x, y), Quaternion.identity);
                //newSweet.transform.SetParent(transform);

                //sweets[x, y] = newSweet.GetComponent<SweetsController>();
                //sweets[x, y].Init(x, y, this, SweetsType.NORMAL);

                //if (sweets[x, y].Movable())
                //{
                //    sweets[x, y].MovedComponent.Move(x, y);  //test(new Vector3.zero)
                //}

                //if (sweets[x, y].ColorAble())
                //{
                //    sweets[x, y].ColoredComponent.SetThisType(
                //        (SweetsColorType.ColorType)Random.Range(0, sweets[x, y].ColoredComponent.MaxColorsNum));
                //}
            }
        }
	}
	

    public Vector3 CalibratePosition(int x, int y)
    {
        return new Vector3(transform.position.x - columns / 2f + x,
                           transform.position.y + rows / 2f - y);
    }

    //Create new Sweets Object.
    public SweetsController CreateNewSweet(int x, int y, SweetsType type)
    {
        GameObject newSweet =
        Instantiate(sweetsPrefabDic[type], CalibratePosition(x, y), Quaternion.identity);
        newSweet.transform.parent = transform;

        sweets[x, y] = newSweet.GetComponent<SweetsController>();
        sweets[x, y].Init(x, y, this, type);
        return sweets[x, y];
    }
}
