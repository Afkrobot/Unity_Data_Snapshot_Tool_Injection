using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Unity_Data_Snapshot_Tool_Injection
{
    /// <summary>
    /// The Loader class handeling the injection of our game object into the scene.
    /// </summary>
    public class Loader
    {
        public static void Init()
        {
            Loader.Load = new GameObject();
            Loader.Load.AddComponent<Injection>();
            UnityEngine.Object.DontDestroyOnLoad(Loader.Load);
        }

        public static void Unload()
        {
            GameObject.Destroy(Load);
        }

        private static GameObject Load;
    }
}
