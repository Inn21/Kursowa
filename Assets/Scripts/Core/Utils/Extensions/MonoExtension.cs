#region

using System.Collections.Generic;
using System.Text;
using UnityEngine;

#endregion

namespace Engine.Core.Utils.Extensions
{
    public static class MonoExtension
    {
        public static void SetObjectNameInEditor(this Component component, string name)
        {
            if (component != null) SetObjectNameInEditor(component.gameObject, name);
        }

        public static void SetObjectNameInEditor(this GameObject gameObject, string name)
        {
            if (!name.IsNullOrEmpty() && gameObject != null) gameObject.name = name;
        }

        public static TComponent TryGetComponentFromAnyParent<TComponent>(this GameObject gameObject)
            where TComponent : class
        {
            var node = gameObject.transform.parent;
            while (node != null)
            {
                var result = node.GetComponent<TComponent>();
                if (result != null) return result;

                node = node.parent;
            }

            return null;
        }

        public static TComponent TryGetComponentFromAnyParent<TComponent>(this Component component)
            where TComponent : class
        {
            return component.gameObject.TryGetComponentFromAnyParent<TComponent>();
        }

        public static string BuildHierarchyName(this GameObject gameObject, string separator = " / ")
        {
            var names = new List<string>(8);

            var charsCount = 0;
            var node = gameObject.transform;
            while (node != null)
            {
                charsCount += node.name.Length;
                names.Add(node.name);
                node = node.parent;
            }

            var result = new StringBuilder(names.Count * separator.Length + charsCount);
            for (var i = names.Count - 1; i >= 0; i--)
            {
                result.Append(names[i]);
                result.Append(separator);
            }

            return result.ToString();
        }

        public static bool HasParent(this Transform transform)
        {
            return transform.parent != null;
        }

        public static Transform GetTopParent(this Transform transform)
        {
            if (!transform.HasParent()) return null;

            var parentNode = transform.parent;
            while (parentNode.HasParent()) parentNode = parentNode.parent;
            return parentNode;
        }
    }
}