using System;
using System.Collections.Generic;

public class Trie
{
    public const char TRIE_ROOT = '^';
    public const char WORD_STOP = '=';
    private readonly Node root;

    public Trie() {
        root = new Node(TRIE_ROOT, null);
    }

    public void Insert(string s, GeoObjectInstance p) {
        s = s.ToLower();
        var currentNode = root;
        foreach (var c in s)
        {
            var child = currentNode.FindChildNode(c);
            if (child == null)
            {
                child = new Node(c, currentNode);
                currentNode.children.Add(child);
            }
            currentNode = child;
        }
        currentNode.children.Add(new Node(WORD_STOP, currentNode, p));
    }

    // public void InsertRange(List<string> items) {
    //     for (int i = 0; i < items.Count; i++)
    //         Insert(items[i]);
    // }

    public bool HasExactMatch(string s) {
        s = s.ToLower();
        var currentNode = root;
        foreach (var c in s) {
            Node child = currentNode.FindChildNode(c);
            if (child == null)
                return false;
            currentNode = child;
        }
        return currentNode.FindChildNode(WORD_STOP) != null;
    }

    public List<GeoObjectInstance> GetMatchingPrefixes(string s) {
        s = s.ToLower();
        List<GeoObjectInstance> results = new();
        var currentNode = root;
        foreach (var c in s) {
            Node child = currentNode.FindChildNode(c);
            if (child == null)
                return results;
            currentNode = child;
        }

        if (currentNode.payload != null)
            results.Add(currentNode.payload);
        
        // Find all subchildren with a GeoModelInstance payload
        SearchForPayloadsInChildren(currentNode, ref results);
        return results;
    }

    private void SearchForPayloadsInChildren(Node current, ref List<GeoObjectInstance> results) {
        foreach (var child in current.children) {  
            if (child.payload != null)
                results.Add(child.payload);
            SearchForPayloadsInChildren(child, ref results);
        }
    }

    public void Delete(string s) {
        s = s.ToLower();
        var currentNode = root;
        foreach (var c in s) {
            Node child = currentNode.FindChildNode(c);
            if (child == null)
                return;
            currentNode = child;
        }

        if (currentNode.FindChildNode(WORD_STOP) == null)
            return;

        currentNode.DeleteChildNode(WORD_STOP);

        if (!currentNode.IsLeafNode())
            return;

        currentNode = currentNode.parent;
        for (int i = s.Length - 1; i >= 0; i--) {
            currentNode.DeleteChildNode(s[i]);
            currentNode = currentNode.parent;
        }
    }
}