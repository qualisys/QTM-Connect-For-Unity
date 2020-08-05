using UnityEngine;
using UnityEngine.Assertions;

namespace QualisysRealTime.Unity
{
    public class RTClientUpdater : MonoBehaviour 
    {
        public static void AssertExistence() 
        {
            if (Application.isPlaying == false)
            { 
                return;
            }

            if (instance == null) 
            {
                var o = new GameObject("RTClientUpdater", typeof(RTClientUpdater));
                Assert.IsNotNull(instance);
            }
        }
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        static RTClientUpdater instance;
        private void FixedUpdate()
        {
            if (instance == null) 
            {
                instance = this;
            }

            if (instance != this) 
            {
                return;
            }

            RTClient.GetInstance().Update();
        }
    }
}
