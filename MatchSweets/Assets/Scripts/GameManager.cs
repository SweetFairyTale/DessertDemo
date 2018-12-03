using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

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

    private float fillTime = 0.1f;

    public GameObject gridPrefab;

    //Sweets Array
    private SweetsController[,] sweets;

    //Two sweets (refer to the mouse action) that waiting to be exchanged.
    private SweetsController currentSweet;
    private SweetsController targetSweet;

    private void Awake()
    {
        _instance = this;
    }

    // Use this for initialization
    void Start()
    {
        //为字典赋值
        sweetsPrefabDic = new Dictionary<SweetsType, GameObject>();
        for (int i = 0; i < sweetsPrefabs.Length; i++)
        {
            if (!sweetsPrefabDic.ContainsKey(sweetsPrefabs[i].type))
            {
                sweetsPrefabDic.Add(sweetsPrefabs[i].type, sweetsPrefabs[i].prefab);
            }
        }

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                GameObject chocolate = Instantiate(gridPrefab, CalibratePosition(x, y), Quaternion.identity);
                chocolate.transform.SetParent(transform);
            }
        }

        sweets = new SweetsController[columns, rows];
        for (int x = 0; x < columns; x++)
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

        //!--test
        Destroy(sweets[5, 5].gameObject);
        CreateNewSweet(5, 5, SweetsType.BARRIER);

        //FillAll();
        StartCoroutine(FillAll());
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

    //Fill all the grids with sweets.
    public IEnumerator FillAll()
    {
        while (Fill())
        {
            yield return new WaitForSeconds(fillTime);
        }
    }

    //Fill the grids one by one.
    public bool Fill()
    {
        bool filledNotFinished = false;

        //From bottom to top.
        for (int y = rows - 2; y >= 0; y--)
        {
            for (int x = 0; x < columns; x++)
            {
                SweetsController sweet = sweets[x, y];

                if (sweet.Movable())  //无法移动则无法向下填充(垂直填充).
                {
                    SweetsController sweetBelow = sweets[x, y + 1];

                    if (sweetBelow.Type == SweetsType.EMPTY) //垂直填充.
                    {
                        Destroy(sweetBelow.gameObject);  //!?
                        sweet.MovedComponent.Move(x, y + 1, fillTime);
                        sweets[x, y + 1] = sweet;
                        CreateNewSweet(x, y, SweetsType.EMPTY);
                        filledNotFinished = true;
                    }
                    else //向左右偏移填充
                    {
                        for (int d = -1; d <= 1; d++)
                        {
                            if (d != 0)  //排除正下方.
                            {
                                int down = x + d;

                                if (down >= 0 && down < columns)
                                {
                                    SweetsController downSweet = sweets[down, y + 1];
                                    if (downSweet.Type == SweetsType.EMPTY)
                                    {
                                        bool vertical_fillable = true;  //用于判断垂直填充是否能满足向下填充.
                                        for (int above = y; above >= 0; above--)
                                        {
                                            SweetsController aboveSweet = sweets[down, above];
                                            if (aboveSweet.Movable())
                                            {
                                                //左下方空白的正上方有可向下填充的物体，则不用向侧下填充.
                                                break;
                                            }
                                            else if (!aboveSweet.Movable() && aboveSweet.Type != SweetsType.EMPTY)
                                            {
                                                vertical_fillable = false;
                                                break;
                                            }
                                        }

                                        if (!vertical_fillable)
                                        {
                                            Destroy(downSweet.gameObject);
                                            sweet.MovedComponent.Move(down, y + 1, fillTime);
                                            sweets[down, y + 1] = sweet;
                                            CreateNewSweet(x, y, SweetsType.EMPTY);
                                            filledNotFinished = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            }
        }

        //Exceptional case: the top row
        for (int x = 0; x < rows; x++)
        {
            SweetsController sweet = sweets[x, 0];
            if (sweet.Type == SweetsType.EMPTY)
            {
                GameObject newSweet =
                Instantiate(sweetsPrefabDic[SweetsType.NORMAL], CalibratePosition(x, -1), Quaternion.identity);
                newSweet.transform.parent = transform;

                sweets[x, 0] = newSweet.GetComponent<SweetsController>();
                sweets[x, 0].Init(x, -1, this, SweetsType.NORMAL);
                sweets[x, 0].MovedComponent.Move(x, 0, fillTime);
                sweets[x, 0].ColoredComponent.SetThisType(  //随机生成一种类型的甜品 **枚举为特殊的int**
                    (SweetsColorType.ColorType)Random.Range(0, sweets[x, 0].ColoredComponent.MaxColorsNum));
                filledNotFinished = true;
            }
        }
        //Filling finished or not.
        return filledNotFinished;
    }

    //Judge if two sweets are adjacent. 
    private bool IsAdjacent(SweetsController sweet1, SweetsController sweet2)
    {
        return (sweet1.X == sweet2.X && Mathf.Abs(sweet1.Y - sweet2.Y) == 1) ||
               (sweet1.Y == sweet2.Y && Mathf.Abs(sweet1.X - sweet2.X) == 1);
    }

    //Exchange the positions of two sweets.
    private void ExchangeSweets(SweetsController sweet1, SweetsController sweet2)
    {
        if(sweet1.Movable()&&sweet2.Movable())
        {          
            sweets[sweet1.X, sweet1.Y] = sweet2;
            sweets[sweet2.X, sweet2.Y] = sweet1;

            int tempX = sweet1.X, tempY = sweet1.Y;
            sweet1.MovedComponent.Move(sweet2.X, sweet2.Y, fillTime);
            sweet2.MovedComponent.Move(tempX, tempY, fillTime);
        }
    }

    public void PressCurrentSweet(SweetsController sweet)
    {
        currentSweet = sweet;
    }

    public void EnterTargetSweet(SweetsController sweet)
    {
        targetSweet = sweet;
    }

    public void ReleaseCurrentSweet()
    {
        if(IsAdjacent(currentSweet, targetSweet))
        {
            ExchangeSweets(currentSweet, targetSweet);
        }
    }
}
