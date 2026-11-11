//------------------------------------------------------------
// File : IDependenciesEditor.cs
// Email: mailto:zhiqiang.yang@kingboat.io
// Desc : 
//------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
namespace UnityGameFramework.Editor.ResourceTools
{
    public interface IDependenciesEditor
    {
        void ReloadDependencies(params string[] dependencies);
    }
}

