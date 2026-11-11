using UnityEngine;


namespace Tutorial
{
    public enum eGameBitwise
    {
        None = 1 << 0,
        //新手引导
        Prop1 = 1 << 1,
        Prop2 = 1 << 2,
        Prop3 = 1 << 3,
        Prop4 = 1 << 4,
        Prop5 = 1 << 5,

        //
        Collection = 1 << 6,
    }
}
