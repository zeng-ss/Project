//------------------------------------------------------------
// File : AutoDestroy.cs
// Email: mailto:zhiqiang.yang
// Desc : 
//------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CasualGame;

public class AutoDestroy : MonoBehaviour
{
    public float delayTime = 1;

    private float time = 0;

    private BaseUnitObject baseUnitObject;

    void Start()
    {
        baseUnitObject = gameObject.GetComponent<BaseUnitObject>();
    }

    void Update()
    {
        time += Time.deltaTime;
        if (time >= delayTime)
        {
            _Destroy();
        }
    }

    public void Reset()
    {
        time = 0;
    }

    private void _Destroy()
    {
        if (baseUnitObject != null)
        {
            time = 0;
            PrefabPool.Instance.DeSpawn(baseUnitObject);
        }
    }

}