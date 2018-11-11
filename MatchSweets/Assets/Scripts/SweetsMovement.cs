using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweetsMovement : MonoBehaviour {

    private SweetsController sweet;

    private void Awake()
    {
        sweet = GetComponent<SweetsController>();
    }

    public void Move(int aimX, int aimY)
    {
        sweet.X = aimX;
        sweet.Y = aimY;
        sweet.transform.localPosition = sweet.gameManager.CalibratePosition(aimX,aimY);
    }
}
