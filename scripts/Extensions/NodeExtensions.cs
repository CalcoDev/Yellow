// !!! DISCLAIMER !!!
// Some of the methods in this class were tkane from Firebelley's
// [GodotUtilities](https://github.com/firebelley/GodotUtilities)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Yellow.Managers;
using Yellow.Misc.Logic;

namespace Yellow.Extensions;

public static class NodeExtension
{
    private static readonly Dictionary<Node, HashSet<Coroutine>> _coroutines = new();

    public static bool IsActive(this Node node) => node.IsProcessing();
    public static void SetActive(this Node node, bool active) {
        node.SetProcess(active);
    }

    public static Coroutine StartCoroutine(this Node node, IEnumerator enumerator)
    {
        if (!_coroutines.ContainsKey(node)) {
            _coroutines.Add(node, new HashSet<Coroutine>());
        }
        
        var c = new Coroutine(enumerator);
        _coroutines[node].Add(c);
        return c;
    }

    public static void StopCoroutine(this Node node, Coroutine coroutine)
    {
        if (!_coroutines.ContainsKey(node) || !_coroutines[node].Contains(coroutine)) {
            return;
        }
        _coroutines[node].Remove(coroutine);
    }

    public static void UpdateCoroutines(this Node node)
    {
        if (!_coroutines.ContainsKey(node)) {
            return;
        }

        foreach (var c in _coroutines[node]) {
            c.Update(Game.DeltaTime);
        }
    }

    /// <summary>
    /// Adds the Node to a group with a name equal to the Node's type name.
    /// </summary>
    /// <param name="node"></param>
    public static void AddToGroup(this Node node) =>
        node.AddToGroup(node.GetType().Name);

    public static T GetSibling<T>(this Node node, int idx) where T : Node =>
        (T)node.GetParent().GetChild(idx);

    public static T GetNode<T>(this Node node) where T : Node =>
        node.GetNode<T>(typeof(T).Name);

    public static T GetAutoLoadNode<T>(this Node node) where T : Node =>
        node.GetNode<T>($"/root/{typeof(T).Name}");

    public static List<T> GetChildren<T>(this Node node) where T : Node =>
        node.GetChildren().Cast<Node>().Select(x => x as T).ToList();

    public static IEnumerable<T> GetChildrenOfType<T>(this Node node) where T : Node =>
        node.GetChildren().OfType<T>();

    public static T GetFirstNodeOfType<T>(this Node node)
    {
        var children = node.GetChildren();
        foreach (var child in children)
        {
            if (child is T t)
            {
                return t;
            }
        }
        return default;
    }

    public static List<T> GetNodesOfType<T>(this Node node)
    {
        var result = new List<T>();
        var children = node.GetChildren();
        foreach (var child in children)
        {
            if (child is T t)
            {
                result.Add(t);
            }
        }
        return result;
    }

    public static void AddChildDeferred(this Node node, Node child) =>
        node.CallDeferred("add_child", child);

    public static T GetNullableNodePath<T>(this Node n, NodePath nodePath) where T : Node
    {
        if (nodePath == null) return null;
        return n.GetNodeOrNull<T>(nodePath);
    }

    /// <summary>
    /// Removes the node's children from the scene tree and then queues them for deletion.
    /// </summary>
    /// <param name="n"></param>
    /// <typeparam name="T"></typeparam>
    public static void RemoveAndQueueFreeChildren(this Node n)
    {
        foreach (var child in n.GetChildren())
        {
            if (child is Node childNode)
            {
                n.RemoveChild(childNode);
                childNode.QueueFree();
            }
        }
    }

    /// <summary>
    /// Queues all child nodes for deletion.
    /// </summary>
    /// <param name="n"></param>
    /// <typeparam name="T"></typeparam>
    public static void QueueFreeChildren(this Node n)
    {
        foreach (var child in n.GetChildren())
        {
            if (child is Node childNode)
            {
                childNode.QueueFree();
            }
        }
    }

    public static T GetAncestor<T>(this Node n) where T : Node
    {
        Node currentNode = n;
        while (currentNode != n.GetTree().Root && currentNode is not T)
        {
            currentNode = currentNode.GetParent();
        }

        return currentNode is T ancestor ? ancestor : null;
    }

    public static Node GetLastChild(this Node n)
    {
        var count = n.GetChildCount();
        if (count == 0) return null;
        return n.GetChild(count - 1);
    }

    public static void QueueFreeDeferred(this Node n) =>
        n.CallDeferred("queue_free");

    public static void QueueFreeAll(this IEnumerable<Node> objects)
    {
        foreach (var obj in objects)
            obj.QueueFree();
    }

    /// <summary>
    /// Checks if the Node is the current game's scene. Useful for checking whether the scene was run using the "Run Current Scene" button.
    /// </summary>
    /// <returns></returns>
    public static bool IsCurrentScene(this Node node) =>
        node.GetTree().CurrentScene.SceneFilePath == node.SceneFilePath;

    public static List<Node> GetAllDescendants(this Node node)
    {
        var result = new List<Node>();
        foreach (var child in node.GetChildren())
        {
            result.AddRange(child.GetAllDescendants());
            result.Add(child);
        }
        return result;
    }
}