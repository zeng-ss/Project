using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObjectPrefab
{
    public int Index { get; set; }

    public void Spawn();

    public void DeSpawn();
}

/*
 * AttachParentPrefabPool 版本: V2.0.0，设计思路：
 * 1. 依附父级生命周期的小型对象池
 * 2. IObjectPrefab接口去除 GetComponent 方法，靠 Unity 的 GetComponent 实现对应逻辑 --- v1.1.0
 * 3. 移除所有 GetComponent 逻辑，靠类型判断实现 --- v1.2.0
 * 4. 支持非IObjectPrefab的对象 --- V2.0.0
 * 5. 不再强制设置 克隆 GameObject 的 Active属性 --- v2.0.0
 */
public class AttachParentPrefabPool : MonoBehaviour
{
    private class ObjectPrefabStruct
    {
        public int Index;
        public GameObject ObjectPrefab;
        public Component ObjectPrefabComponent;

        public static ObjectPrefabStruct Create(int index, GameObject objectPrefab)
        {
            return new ObjectPrefabStruct()
            {
                Index = index,
                ObjectPrefab = objectPrefab,
            };
        }
    }

    [SerializeField] private List<GameObject> prefabGameObjectList = new List<GameObject>();

    private List<ObjectPrefabStruct> _objectPrefabList = new List<ObjectPrefabStruct>();

    private Dictionary<int, List<Component>> _objectPrefabPoolDictionary = new Dictionary<int, List<Component>>();

    private Transform ParentTransform => this.transform;

    private bool isInit = false;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (isInit) return;

        _objectPrefabPoolDictionary.Clear();

        for (var index = 0; index < prefabGameObjectList.Count; index++)
        {
            GameObject prefabGameObject = prefabGameObjectList[index];
            if (prefabGameObject == null)
            {
                Debug.LogError($"AttachParentPrefabPool Init: prefabGameObjectList[{index}] is missing.", this);
                continue;
            }

            // 场景实例挂到池节点下；Project 预制体资产不能 SetParent，仅作 Instantiate 模板。
            if (prefabGameObject.scene.IsValid())
            {
                prefabGameObject.transform.SetParent(ParentTransform);
                prefabGameObject.transform.localPosition = Vector3.zero;
            }

            _objectPrefabList.Add(ObjectPrefabStruct.Create(index, prefabGameObject));
            _objectPrefabPoolDictionary[index] = new List<Component>();
        }

        gameObject.SetActive(false);
        isInit = true;
    }

    private Component Spawn(int index, Transform parent, Type type)
    {
        Component tempComponent = default;

        if (_objectPrefabPoolDictionary.TryGetValue(index, out List<Component> objectPrefabs))
        {
            if (objectPrefabs.Count > 0)
            {
                Component objectPrefab = objectPrefabs[0];
                objectPrefabs.RemoveAt(0);

                tempComponent = objectPrefab;
            }
            else
            {
                Component objectPrefabComponent = GetObjectPrefabComponent(index, type);

                if (objectPrefabComponent != null)
                {
                    tempComponent = Instantiate(objectPrefabComponent);
                }
                else
                {
                    GameObject tempGameObject = GetObjectPrefabGameObject(index);
                    if (tempGameObject == null)
                    {
                        Debug.LogError($"AttachParentPrefabPool Spawn: index {index} has no template.", this);
                    }
                    else
                    {
                        GameObject instance = Instantiate(tempGameObject);
                        tempComponent = instance.GetComponent(type) ??
                                        instance.GetComponentInChildren(type, true);
                        if (tempComponent == null)
                        {
                            Debug.LogError(
                                $"AttachParentPrefabPool Spawn: {tempGameObject.name} has no {type.Name}.",
                                this);
                            Destroy(instance);
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogError($"AttachParentPrefabPool Spawn: index {index} is not registered.", this);
        }

        if (tempComponent != null)
        {
            IObjectPrefab objectPrefab = tempComponent as IObjectPrefab;
            if (objectPrefab != null)
            {
                objectPrefab.Spawn();
                objectPrefab.Index = index;
            }

            Transform componentTransform = tempComponent.transform;
            componentTransform.SetParent(parent);
            componentTransform.localPosition = Vector3.zero;
            componentTransform.localEulerAngles = Vector3.zero;
            componentTransform.localScale = Vector3.one;
        }
        else
        {
            return null;
        }

        return tempComponent;
    }

    private void DeSpawn<T>(int index, T monoBehaviour) where T : Component, IObjectPrefab
    {
        if (_objectPrefabPoolDictionary.TryGetValue(index, out List<Component> objectPrefabs))
        {
            monoBehaviour.transform.SetParent(ParentTransform);
            monoBehaviour.transform.localPosition = Vector3.zero;

            monoBehaviour.DeSpawn();

            objectPrefabs.Add(monoBehaviour);
        }
        else
        {
            Debug.LogError($"AttachParentPrefabPool DeSpawn error: {index}");
        }
    }

    private Component GetObjectPrefabComponent(int index, Type type)
    {
        foreach (var objectPrefabStruct in _objectPrefabList)
        {
            if (objectPrefabStruct.Index != index) continue;

            if (objectPrefabStruct.ObjectPrefabComponent == null)
            {
                objectPrefabStruct.ObjectPrefabComponent =
                    objectPrefabStruct.ObjectPrefab.GetComponent(type) ??
                    objectPrefabStruct.ObjectPrefab.GetComponentInChildren(type, true);
            }

            return objectPrefabStruct.ObjectPrefabComponent;
        }

        return null;
    }

    public T Spawn<T>(int index, Transform parent) where T : Component, IObjectPrefab
    {
        if (!isInit)
            Init();

        return Spawn(index, parent, typeof(T)) as T;
    }

    public void DeSpawn<T>(T monoBehaviour) where T : Component, IObjectPrefab
    {
        DeSpawn(monoBehaviour.Index, monoBehaviour);
    }

    public GameObject GetObjectPrefabGameObject(int index)
    {
        if (!isInit)
            Init();

        foreach (var objectPrefabStruct in _objectPrefabList)
        {
            if (objectPrefabStruct.Index != index) continue;

            return objectPrefabStruct.ObjectPrefab;
        }

        return null;
    }
}