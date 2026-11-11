//------------------------------------------------------------
// File : IUGUIChild.cs
// Email: zewei.zhuang@kingboat.io
// Desc : 
//------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CasualGame
{
    public interface IUGUIChild
    {
        public void OnInit();

        public void OnOpen();

        public void OnUpdate(float elapseSeconds, float realElapseSeconds);

        public void OnClose();
    }
}

