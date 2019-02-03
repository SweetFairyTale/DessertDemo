using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //甜品元素类型枚举
    public enum SweetsType
    {
        EMPTY,
        NORMAL,
        BARRIER,
        ROW_CLEAR,
        COLUMN_CLEAR,
        RAINBOW_CANDY,
        COUNT
    }

    public Dictionary<SweetsType, GameObject> sweetsPrefabDic;

    [System.Serializable] // C# serializing
    public struct SweetsPrefab
    {
        public SweetsType type;
        public GameObject prefab;
    }

    public SweetsPrefab[] sweetsPrefabs;

    //Single Instance.
    private static GameManager _instance;
    public static GameManager Instance
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
    private int rows = 9;

    private float fillTime = 0.1f;

    public GameObject gridPrefab;

    //Sweets Array
    private SweetsController[,] sweets;

    //Two sweets (refer to the mouse event) that waiting to be exchanged.
    private SweetsController currentSweet;
    private SweetsController targetSweet;

    public Text timeText;
    private float gameTime = 60f;
    private bool gameOver = false;

    public int playerScore;
    public Text scoreText;

    private float intervalTime;
    private float currentScore;

    public GameObject gameOverPanel;

    public Text finalScoreText;

    private void Awake()
    {
        _instance = this;
    }

    // Use this for initialization
    void Start()
    {
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

        Destroy(sweets[5, 5].gameObject);
        CreateNewSweet(5, 5, SweetsType.BARRIER);

        //FillAll();
        StartCoroutine(FillAll());
    }

    void Update()
    {
        gameTime -= Time.deltaTime;
        if (gameTime <= 0)
        {
            gameTime = 0;
            //...
            gameOverPanel.SetActive(true);
            finalScoreText.text = scoreText.text;
            gameOver = true;
            //return;
        }
        timeText.text = gameTime.ToString("0");  //"0"-Rounding ("0.0","0.00"...)

        if (intervalTime <= 0.05f)
        {
            intervalTime += Time.deltaTime;
        }
        else
        {
            if (currentScore < playerScore)
            {
                currentScore++;
                scoreText.text = currentScore.ToString();
                intervalTime = 0;
            }
        }
        //scoreText.text = playerScore.ToString();
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
        bool needFill = true;
        while (needFill)
        {
            yield return new WaitForSeconds(fillTime);
            while (Fill())
            {
                yield return new WaitForSeconds(fillTime);
            }

            needFill = EliminateAllMatchedSweets();
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
                    SweetsController sweetBelow = sweets[x, y + 1];  //得到当前位置下方的实体.

                    if (sweetBelow.Type == SweetsType.EMPTY) //下方当前为空物体，允许垂直填充.
                    {
                        Destroy(sweetBelow.gameObject);
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
        for (int x = 0; x < columns; x++)
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
        if (sweet1.Movable() && sweet2.Movable())
        {
            sweets[sweet1.X, sweet1.Y] = sweet2;
            sweets[sweet2.X, sweet2.Y] = sweet1;

            if (MatchSweets(sweet1, sweet2.X, sweet2.Y) != null || MatchSweets(sweet2, sweet1.X, sweet1.Y) != null)
            {
                int tempX = sweet1.X, tempY = sweet1.Y;
                sweet1.MovedComponent.Move(sweet2.X, sweet2.Y, fillTime);
                sweet2.MovedComponent.Move(tempX, tempY, fillTime);
                EliminateAllMatchedSweets();
                StartCoroutine(FillAll());
            }
            else
            {
                sweets[sweet1.X, sweet1.Y] = sweet1;
                sweets[sweet2.X, sweet2.Y] = sweet2;
            }
        }
    }

    /// <summary>
    /// Player Event.
    /// </summary>
    #region
    public void PressCurrentSweet(SweetsController sweet)
    {
        if (!gameOver)
            currentSweet = sweet;
    }

    public void EnterTargetSweet(SweetsController sweet)
    {
        if (!gameOver)
            targetSweet = sweet;
    }

    public void ReleaseCurrentSweet()
    {
        if (!gameOver)
            if (IsAdjacent(currentSweet, targetSweet))
            {
                ExchangeSweets(currentSweet, targetSweet);
            }
    }
    #endregion

    public List<SweetsController> MatchSweets(SweetsController sweet, int newX, int newY)
    {
        if (sweet.ColorAble())
        {
            SweetsColorType.ColorType color = sweet.ColoredComponent.ThisType;
            List<SweetsController> matchSweetsInSameRow = new List<SweetsController>();
            List<SweetsController> matchSweetsInSameCol = new List<SweetsController>();
            List<SweetsController> matchingResult = new List<SweetsController>();

            //row maching
            matchSweetsInSameRow.Add(sweet);
            //match the current row.(0->left, 1->right)
            for (int i = 0; i <= 1; i++)
            {
                for (int xDis = 1; xDis < columns; xDis++)
                {
                    int x = newX;
                    if (i == 0)  //traverse the left side.
                    {
                        x -= xDis;
                    }
                    if (i == 1)  //traverse the right side.
                    {
                        x += xDis;
                    }
                    if (x < 0 || x >= columns)  //encounter the border.
                    {
                        break;
                    }

                    if (sweets[x, newY].ColorAble() && sweets[x, newY].ColoredComponent.ThisType == color)
                    {
                        matchSweetsInSameRow.Add(sweets[x, newY]);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            //"L","T" shape matching.
            if (matchSweetsInSameRow.Count >= 3)
            {
                for (int i = 0; i < matchSweetsInSameRow.Count; i++)
                {
                    matchingResult.Add(matchSweetsInSameRow[i]);
                }
                for (int j = 0; j <= 1; j++)  //只需扫描被调换元素的列而无需所有行匹配成功元素？
                {
                    //apply col-matching to the first element.
                    for (int yDis = 1; yDis <= rows; yDis++)
                    {
                        int y = newY;
                        if (j == 0)
                        {
                            y -= yDis;
                        }
                        if (j == 1)
                        {
                            y += yDis;
                        }
                        if (y < 0 || y >= rows)
                        {
                            break;
                        }

                        if (sweets[matchSweetsInSameRow[0].X, y].ColorAble() && sweets[matchSweetsInSameRow[0].X, y].ColoredComponent.ThisType == color)
                        {
                            matchSweetsInSameCol.Add(sweets[matchSweetsInSameRow[0].X, y]);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                if (matchSweetsInSameCol.Count < 2)
                {
                    matchSweetsInSameCol.Clear();
                }
                else
                {
                    for (int k = 0; k < matchSweetsInSameCol.Count; k++)
                    {
                        matchingResult.Add(matchSweetsInSameCol[k]);
                    }
                }
            }

            if (matchingResult.Count >= 3)
            {
                //If successed in matching sweets in one row, then return. 
                return matchingResult;
            }
            else
            {
                matchSweetsInSameRow.Clear();
                matchSweetsInSameCol.Clear();
            }

            //columns matching
            matchSweetsInSameCol.Add(sweet);  //basic element
            //match the current row to the left and to the right.
            for (int i = 0; i <= 1; i++)
            {
                for (int yDis = 1; yDis < rows; yDis++)
                {
                    int y = newY;
                    if (i == 0)  //traverse the up side.
                    {
                        y -= yDis;
                    }
                    if (i == 1)  //traverse the down side.
                    {
                        y += yDis;
                    }
                    if (y < 0 || y >= rows)  //encounter the border.
                    {
                        break;
                    }

                    if (sweets[newX, y].ColorAble() && sweets[newX, y].ColoredComponent.ThisType == color)
                    {
                        matchSweetsInSameCol.Add(sweets[newX, y]);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (matchSweetsInSameCol.Count >= 3)
            {
                for (int i = 0; i < matchSweetsInSameCol.Count; i++)
                {
                    matchingResult.Add(matchSweetsInSameCol[i]);
                }
                for (int j = 0; j <= 1; j++)
                {
                    for (int xDis = 1; xDis < columns; xDis++)
                    {
                        int x = newX;
                        if (j == 0)
                        {
                            x -= xDis;
                        }
                        if (j == 1)
                        {
                            x += xDis;
                        }
                        if (x < 0 || x >= columns)
                        {
                            break;
                        }

                        if (sweets[x, matchSweetsInSameCol[0].Y].ColorAble() && sweets[x, matchSweetsInSameCol[0].Y].ColoredComponent.ThisType == color)
                        {
                            matchSweetsInSameRow.Add(sweets[x, matchSweetsInSameCol[0].Y]);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                if (matchSweetsInSameRow.Count < 2)
                {
                    matchSweetsInSameRow.Clear();
                }
                else
                {
                    for (int k = 0; k < matchSweetsInSameRow.Count; k++)
                    {
                        matchingResult.Add(matchSweetsInSameRow[k]);
                    }
                }
            }

            if (matchingResult.Count >= 3)
            {
                //If successed in matching sweets in one column. 
                return matchingResult;
            }
        }

        return null;
    }

    //Eliminate Function
    public bool EliminateSweet(int x, int y)
    {
        if (sweets[x, y].Eliminable() && !sweets[x, y].ElimiComponent.Eliminating)
        {
            sweets[x, y].ElimiComponent.Elimi();
            CreateNewSweet(x, y, SweetsType.EMPTY);
            ClearBarrier(x, y);
            return true;
        }
        return false;
    }

    //Eliminate sweets in all range.
    private bool EliminateAllMatchedSweets()
    {
        bool needFill = false;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (sweets[x, y].Eliminable())
                {
                    List<SweetsController> matchList = MatchSweets(sweets[x, y], x, y);

                    if (matchList != null)
                    {
                        SweetsType superSweets = SweetsType.COUNT;
                        SweetsController superSweetsPos = matchList[Random.Range(0, matchList.Count)];

                        if (matchList.Count == 4)
                        {
                            superSweets = (SweetsType)Random.Range((int)SweetsType.ROW_CLEAR, (int)SweetsType.COLUMN_CLEAR);
                        }

                        //matchList.Count == 5 ... Rainbow Sweets

                        for (int i = 0; i < matchList.Count; i++)
                        {
                            if (EliminateSweet(matchList[i].X, matchList[i].Y))
                                needFill = true;
                        }

                        //Generate super sweets.
                        if (superSweets != SweetsType.COUNT)
                        {
                            Destroy(sweets[superSweetsPos.X, superSweetsPos.Y]);
                            SweetsController newSweets = CreateNewSweet(superSweetsPos.X, superSweetsPos.Y, superSweets);

                            if (matchList.Count == 4 && newSweets.ColorAble() && matchList[0].ColorAble())
                            {
                                newSweets.ColoredComponent.SetThisType(matchList[0].ColoredComponent.ThisType);
                            }
                            else
                            {
                                //Generate Rainbow Sweets
                            }

                        }
                    }
                }
            }
        }
        return needFill;
    }

    //Eliminate biscuit barriers.
    private void ClearBarrier(int x, int y)
    {
        for (int nearbyX = x - 1; nearbyX <= x + 1; nearbyX++)
        {
            if (nearbyX != x && nearbyX >= 0 && nearbyX < columns)
            {
                if (sweets[nearbyX, y].Type == SweetsType.BARRIER && sweets[nearbyX, y].Eliminable())
                {
                    sweets[nearbyX, y].ElimiComponent.Elimi();
                    CreateNewSweet(nearbyX, y, SweetsType.EMPTY);
                }
            }
        }

        for (int nearbyY = y - 1; nearbyY <= y + 1; nearbyY++)
        {
            if (nearbyY != y && nearbyY >= 0 && nearbyY < rows)
            {
                if (sweets[x, nearbyY].Type == SweetsType.BARRIER && sweets[x, nearbyY].Eliminable())
                {
                    sweets[x, nearbyY].ElimiComponent.Elimi();
                    CreateNewSweet(x, nearbyY, SweetsType.EMPTY);
                }
            }
        }
    }

    public void BackToMainScene()
    {
        SceneManager.LoadScene(0);
    }

    public void Replay()
    {
        SceneManager.LoadScene(1);
    }

    //Row elimination
    public void ClearRow(int row)
    {
        for(int x = 0; x < columns; x++)
        {
            EliminateSweet(x, row);
        }
    }

    //Column elimination
    public void ClearColumn(int column)
    {
        for(int y = 0; y < rows; y++)
        {
            EliminateSweet(column, y);
        }
    }
}
