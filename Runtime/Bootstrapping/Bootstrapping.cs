using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace JS
{
    public static class Bootstrapping
    {
        public static void Resolve(MonoBehaviour root,out List<MonoBehaviour> sortedBehaviours)
        {
            var cache = GetAllFieldsWithAttribute<InjectAttribute>(root.AllChildren());
            var graph = ToDependencyGraph(cache);
            sortedBehaviours = SortTopologically(graph,out var circular);
            if (circular) return;
            ResolveDependencies(sortedBehaviours, graph,cache);
        }

        #region Plumbing

        public static MonoBehaviour[] AllChildren(this MonoBehaviour root) => root.GetComponentsInChildren<MonoBehaviour>().Where(c => c != root).ToArray();
        
        static Dictionary<MonoBehaviour, HashSet<Type>> ToDependencyGraph(Dictionary<MonoBehaviour, FieldInfo[]> dict)
        {
            var graph = new Dictionary<MonoBehaviour, HashSet<Type>>();
            foreach (var pair in dict) graph[pair.Key] = new HashSet<Type>(pair.Value.Select(f => f.FieldType));
            return graph;
        }
        
        static Dictionary<MonoBehaviour, FieldInfo[]> GetAllFieldsWithAttribute<T>(MonoBehaviour[] all) where T : Attribute => 
            all.ToDictionary(b => b, b => GetFieldsWith<T>(b.GetType()));

        static void ResolveDependencies(List<MonoBehaviour> sorted,
            Dictionary<MonoBehaviour, HashSet<Type>> graph,
            Dictionary<MonoBehaviour, FieldInfo[]> cache)
        {
            foreach (var behavior in sorted) foreach (var field in cache[behavior]) 
                ResolveDependenciesInBehaviour(graph, field, behavior);
        }

        static void ResolveDependenciesInBehaviour(Dictionary<MonoBehaviour, HashSet<Type>> graph, FieldInfo field, MonoBehaviour behavior)
        {
            var dependencyType = field.FieldType;
            var dependencyBehavior = graph.Keys.FirstOrDefault(b => dependencyType.IsInstanceOfType(b));
            if (dependencyBehavior == null)
            {
                Debug.LogWarning($"Dependency not found for {behavior.name}: {dependencyType.Name}");
                return;
            }
            field.SetValue(behavior, dependencyBehavior);
        }
        
        static List<MonoBehaviour> SortTopologically(Dictionary<MonoBehaviour, HashSet<Type>> graph,out bool circular)
        {
            var sorted = new List<MonoBehaviour>();
            var visited = new HashSet<MonoBehaviour>();
            var recursionStack = new HashSet<MonoBehaviour>();

            if (graph.Keys.Any(behavior => DFS(behavior, visited, recursionStack, sorted, graph)))
            {
                var circularBehaviors = visited.Except(sorted);
                Debug.LogWarning("Circular dependency detected: " + string.Join(", ", circularBehaviors.Select(b => b.name)));
                circular = true;
                return new List<MonoBehaviour>();
            }
            sorted.Reverse();
            circular = false;
            return sorted;
        }

        static FieldInfo[] GetFieldsWith<T>(Type type) where T : Attribute =>
            type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => f.GetCustomAttribute<T>() != null)
                .ToArray();

        static bool DFS(MonoBehaviour behavior, HashSet<MonoBehaviour> visited, HashSet<MonoBehaviour> recursionStack, List<MonoBehaviour> sorted, Dictionary<MonoBehaviour, HashSet<Type>> graph)
        {
            if (recursionStack.Contains(behavior))
                return true;

            if (!visited.Add(behavior))
                return false;

            recursionStack.Add(behavior);

            foreach (var dependencyType in graph[behavior])
            {
                var dependencyBehavior = graph.Keys.FirstOrDefault(b => dependencyType.IsInstanceOfType(b));
                if (dependencyBehavior != null && DFS(dependencyBehavior, visited, recursionStack, sorted, graph))
                    return true;
            }
            recursionStack.Remove(behavior);
            sorted.Add(behavior);
            return false;
        }
        
        #endregion
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class InjectAttribute : Attribute { }
}