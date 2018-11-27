using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweetsMovement : MonoBehaviour
{

    private SweetsController sweet;

    private IEnumerator moveCoroutine; //结束协程句柄.

    private void Awake()
    {
        sweet = GetComponent<SweetsController>();
    }

    //Start or End a IEnumerator.
    //在每次遍历调用Move方法时，首先关闭前一个协程，然后再开启后一个确保不冲突.
    public void Move(int aimX, int aimY,float filltime)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        /*
         * sweet.X = aimX;
         * sweet.Y = aimY;
         * sweet.transform.position = sweet.gameManager.CalibratePosition(aimX,aimY);
        */
        moveCoroutine = MoveCoroutine(aimX, aimY, filltime);
        StartCoroutine(moveCoroutine);
    }

    private IEnumerator MoveCoroutine(int aimX, int aimY, float filltime)
    {
        sweet.X = aimX;
        sweet.Y = aimY;

        Vector3 startPos = transform.position;
        Vector3 endPos = sweet.gameManager.CalibratePosition(aimX, aimY);

        for (float t = 0; t < filltime; t += Time.deltaTime)
        {
            sweet.transform.position = Vector3.Lerp(startPos, endPos, t / filltime);
            yield return 0;  //wait for 1 frame to make the movement smoothly
        }

        sweet.transform.position = endPos;
    }
}
