using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweetsColorType : MonoBehaviour {

    public enum ColorType
    {
        YELLOW,
        PURPLE,
        RED,
        BULE,
        GREEN,
        PINK,
        UNIVERSAL,
        COUNT
    }

    [System.Serializable]
    public struct ColorSprite
    {
        public ColorType color;
        public Sprite sprite;
    }

    public ColorSprite[] colorSprites;  //为字典赋值.

    private Dictionary<ColorType, Sprite> colorSpirteDict;

    private SpriteRenderer renderer;  //sprite

    public int MaxColorsNum   //后期限制随机数生成.
    {
        get { return colorSprites.Length; }
    }

    public ColorType ThisType
    {
        get
        {
            return thisType;
        }

        set
        {
            thisType = value;
        }

    }

    private ColorType thisType;


    private void Awake()
    {
        renderer = transform.Find("Sweet").GetComponent<SpriteRenderer>();
        colorSpirteDict = new Dictionary<ColorType, Sprite>();

        for(int i = 0; i < colorSprites.Length; i++)
        {
            if(!colorSpirteDict.ContainsKey(colorSprites[i].color))
            {
                colorSpirteDict.Add(colorSprites[i].color, colorSprites[i].sprite);
            }
        }
    }

    //set当前甜品类型的方法及校验
    public void SetThisType(ColorType color)
    {
        thisType = color;
        if(colorSpirteDict.ContainsKey(color))
        {
            renderer.sprite = colorSpirteDict[color];
        }
    }
}
