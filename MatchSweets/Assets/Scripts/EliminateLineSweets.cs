using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliminateLineSweets : Eliminate {

    public bool isRow;

    public override void Elimi()
    {
        base.Elimi();
        if(isRow)
        {
            sweet.gameManager.ClearRow(sweet.Y);
        }
        else
        {
            sweet.gameManager.ClearColumn(sweet.X);
        }
    }
}
