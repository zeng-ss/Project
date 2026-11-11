//------------------------------------------------------------
// File : ListPool.cs
// Desc : 
//------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

static class TexturePool
{
    private static Dictionary<Vector2Int, Queue<Texture2D>> _pool = new();

    public static Texture2D GetTexture(int width, int height)
    {
        Vector2Int key = new Vector2Int(width, height);
        if (!_pool.ContainsKey(key) || _pool[key].Count == 0)
        {
            // 池中没有则新建
            return new Texture2D(width, height, TextureFormat.RGBA32, false);
        }
        return _pool[key].Dequeue();
    }

    public static void RecycleTexture(Texture2D tex)
    {
        Vector2Int key = new Vector2Int(tex.width, tex.height);
        if (!_pool.ContainsKey(key))
        {
            _pool[key] = new Queue<Texture2D>();
        }
        _pool[key].Enqueue(tex);
    }
}


