using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweetsController : MonoBehaviour
{

    private int x;
    private int y;
    private GameManager.SweetsType type;
    private SweetsMovement movedComponent;
    public int X
    {
        get
        {
            return x;
        }

        set
        {
            if (Movable())
                x = value;
        }
    }
    public int Y
    {
        get
        {
            return y;
        }

        set
        {
            if (Movable())
                y = value;
        }
    }  
    public GameManager.SweetsType Type
    {
        get
        {
            return type;
        }
    }
    public SweetsMovement MovedComponent
    {
        get
        {
            return movedComponent;
        }
    }

    public SweetsColorType ColoredComponent
    {
        get
        {
            return coloredComponent;
        }
    }
    private SweetsColorType coloredComponent;
   
    [HideInInspector]
    public GameManager gameManager;

    private void Awake()
    {
        movedComponent = GetComponent<SweetsMovement>();
        coloredComponent = GetComponent<SweetsColorType>();
    }

    public void Init(int _x, int _y, GameManager _gameManager, GameManager.SweetsType _type)
    {
        x = _x;
        y = _y;
        gameManager = _gameManager;
        type = _type;
    }

    public bool Movable()
    {
        //call in (XY)Setter.
        return movedComponent != null;
    }

    public bool ColorAble()
    {
        return coloredComponent != null;
    }
}
