using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliminateAnySweets : Eliminate {

    private SweetsColorType.ColorType clearColor;

    public SweetsColorType.ColorType ClearColor
    {
        get
        {
            return clearColor;
        }

        set
        {
            clearColor = value;
        }
    }

    public override void Elimi()
    {
        base.Elimi();
        sweet.gameManager.ClearColorType(clearColor);
    }
}
