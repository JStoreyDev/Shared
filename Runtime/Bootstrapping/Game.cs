using System.Linq;
using UnityEngine;
namespace JS
{
    public class Game : MonoBehaviour
    {
        [SerializeField] GameObjects _services;

        public static T Get<T>()
        {
            var service = _instance.GetComponentInChildren<T>();
            if (service == null) Debug.LogWarning($"Service not found: {typeof(T).Name}");
            return service;
        }
        
        void Awake()
        {
            _instance = this;
            if (_services) _services.InstantiateAsChildren(transform);
            else Debug.LogWarning($"<color=yellow>Services</color> collection not found. No additional services will be loaded.");
            Bootstrapping.Resolve(this,out var behaviours);
        }

        static Game _instance;
    }
}