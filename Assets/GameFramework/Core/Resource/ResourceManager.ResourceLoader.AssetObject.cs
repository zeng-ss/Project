//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework.ObjectPool;
using System.Collections.Generic;

namespace GameFramework.Resource
{
    internal sealed partial class ResourceManager : GameFrameworkModule, IResourceManager
    {
        private sealed partial class ResourceLoader
        {
            /// <summary>
            /// 资源对象。
            /// </summary>
            private sealed class AssetObject : ObjectBase
            {
                private object m_Resource;
                private IResourceHelper m_ResourceHelper;
                private ResourceLoader m_ResourceLoader;
                private string m_name;

                public AssetObject()
                {
                    m_Resource = null;
                    m_ResourceHelper = null;
                    m_ResourceLoader = null;
                }

                public static AssetObject Create(string name, object target, object resource, IResourceHelper resourceHelper, ResourceLoader resourceLoader)
                {
                    if (resource == null)
                    {
                        throw new GameFrameworkException("Resource is invalid.");
                    }

                    if (resourceHelper == null)
                    {
                        throw new GameFrameworkException("Resource helper is invalid.");
                    }

                    if (resourceLoader == null)
                    {
                        throw new GameFrameworkException("Resource loader is invalid.");
                    }

                    AssetObject assetObject = ReferencePool.Acquire<AssetObject>();
                    assetObject.Initialize(name, target);
                    assetObject.m_Resource = resource;
                    assetObject.m_ResourceHelper = resourceHelper;
                    assetObject.m_ResourceLoader = resourceLoader;
                    assetObject.m_name = name;

                    return assetObject;
                }

                public override void Clear()
                {
                    base.Clear();
                    m_Resource = null;
                    m_ResourceHelper = null;
                    m_ResourceLoader = null;
                }

                protected internal override void OnUnspawn()
                {
                    base.OnUnspawn();
                }

                protected internal override void Release(bool isShutdown)
                {
                    m_ResourceLoader.m_AssetToResourceMap.Remove(Target);
                    m_ResourceHelper.Release(Target);
                    ReleaseDenpendencies(m_name);
                }
                
                private void ReleaseDenpendencies(string name)
                {
                    m_ResourceLoader.CheckAsset(name, out ResourceInfo resourceInfo, out string[] dependcencyNames);
                    if (dependcencyNames.Length > 0)
                    {
                        foreach (var item in dependcencyNames)
                        {
                            ReleaseDenpendencies(item);
                        }
                    }
                    m_ResourceLoader.m_ResourcePool.GetAssetAndUnSpawn(resourceInfo.ResourceName.Name);
                }
            }
        }
    }
}
