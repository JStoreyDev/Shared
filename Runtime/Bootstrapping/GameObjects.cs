using UnityEngine;    
namespace JS
{
    [CreateAssetMenu(fileName = "GameObjects", menuName = "JS/GameObjects", order = 0)]
    public class GameObjects : ScriptableObject
    {
        [SerializeField]
        GameObject[] _gameObjects;

        public void InstantiateAsChildren(Transform parent)
        {
            foreach (var go in _gameObjects)
            {
                var instance = Instantiate(go, parent);
                instance.name = go.name;
            }
        }
    }
}