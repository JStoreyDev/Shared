using UnityEngine;
using static UnityEngine.Object;
using static UnityEngine.Resources;

namespace JS
{
    public class _init
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Main()
        {
            var t = Load("Game");
            if (!t)
            {
                Debug.LogWarning(
                    "No <color=yellow>Game</color> object found in Resources folder. No managers will be created.");
                return;
            }

            var g = Instantiate(t);
            g.name = t.name;
            DontDestroyOnLoad(g);
        }
    }
}