using System;
using UnityEngine;


    public class MonoSingleton<T> : MonoBehaviour
    where T : Component, new()
    {
        static T mInstance = null;

        static GameObject mInstanceGameObject = null;


        static public T Instance
        {
            get
            {
                if (mInstanceGameObject == null)
                {
                    DestoryInstance();
                    CreateInstance();
                }

                return mInstance;
            }
        }

        static public T CreateInstance()
        {
            if (mInstance == null)
            {
                mInstanceGameObject = new GameObject(typeof(T).Name);
                DontDestroyOnLoad(mInstanceGameObject);
                var _Instance = mInstanceGameObject.AddComponent<T>();
                mInstance = _Instance;
            }
            return mInstance;
        }

        static public void DestoryInstance()
        {
            if (mInstance != null)
                mInstance = null;

            if (mInstanceGameObject != null)
                Destroy(mInstanceGameObject);
        }
    }


