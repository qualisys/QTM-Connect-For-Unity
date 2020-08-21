using UnityEngine;
using UnityEngine.Assertions;

namespace QualisysRealTime.Unity
{
    public class RTClientUpdater : MonoBehaviour
    {
        static bool quitting = false;
        static RTClientUpdater instance;
        public static void AssertExistence()
        {
            if (Application.isPlaying == false || quitting)
            {
                return;
            }

            if (instance == null)
            {
                var _ = new GameObject("RTClientUpdater", typeof(RTClientUpdater));
                Assert.IsNotNull(instance);
            }
        }
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        void FixedUpdate()
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

        void OnDestroy()
        {
            if (instance != this)
            {
                return;
            }
            RTClient.GetInstance().Dispose();
            instance = null;
        }

        void OnApplicationQuit()
        {
            quitting = true;
        }
    }
}
