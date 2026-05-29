using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
[CreateAssetMenu(fileName = "Item", menuName = "CreateItem")]

public class Item : ScriptableObject
{
  public enum Type
    {
        RandomBox,
        BigBox,
        SmallBox,
        ReverseBox,
        PaintBox,
    }

    public Type type;      //効果内容
    //public Sprite icon;    //持続中の表示アイコン（仮）
    public String itemName;//名前
    //public GameObject Obj; //オブジェクトの見た目
    public float duration; //持続時間
    public int effectValue;//効果値

/*    public Item(Item item)
    {
        this.type = item.type;
        this.icon = item.icon;
        this.itemName = item.itemName;
        this.Obj = item.Obj;
        this.duration = item.duration;
        this.effectValue = item.effectValue;
    }*/

}
