using System;
using System.Collections.Generic;

public class ObjectPool<T> : IDisposable where T : new()
{
    public int MaxCacheCount = 32;

    private static LinkedList<T> cache;
    private Action<T> onRelease;

    public ObjectPool(Action<T> onRelease)
    {
        cache = new LinkedList<T>();
        this.onRelease = onRelease;
    }
    /// <summary>
    /// 获取
    /// </summary>
    /// <returns></returns>

    public T Obtain()
    {
        T value;
        if (cache.Count == 0)
        {
            value = new T();
        }
        else
        {
            value = cache.First.Value;
            cache.RemoveFirst();
        }
        return value;//返回获取值
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public void Release(T value)
    {
        if (cache.Count >= MaxCacheCount)
            return;

        onRelease?.Invoke(value);//实施
        cache.AddLast(value);//缓冲区添加
    }
    /// <summary>
    /// 清除
    /// </summary>
    public void Clear()
    {
        cache.Clear();
    }

    /// <summary>
    /// 丢弃
    /// </summary>
    public void Dispose()
    {
        cache = null;
        onRelease = null;
    }
}

public class QueuePool<T>
{
    private static ObjectPool<Queue<T>> pool = new ObjectPool<Queue<T>>((value) => value.Clear());
    public static Queue<T> Obtain() => pool.Obtain();
    public static void Release(Queue<T> value) => pool.Release(value);
    public static void Clear() => pool.Clear();
}

public class ListPool<T>
{
    private static ObjectPool<List<T>> pool = new ObjectPool<List<T>>((value) => value.Clear());
    public static List<T> Obtain() => pool.Obtain();
    public static void Release(List<T> value) => pool.Release(value);
    public static void Clear() => pool.Clear();
}
public class HashSetPool<T>
{
    private static ObjectPool<HashSet<T>> pool = new ObjectPool<HashSet<T>>((value) => value.Clear());
    public static HashSet<T> Obtain() => pool.Obtain();
    public static void Release(HashSet<T> value) => pool.Release(value);
    public static void Clear() => pool.Clear();
}
public class DictionaryPool<K, V>
{
    private static ObjectPool<Dictionary<K, V>> pool = new ObjectPool<Dictionary<K, V>>((value) => value.Clear());
    public static Dictionary<K, V> Obtain() => pool.Obtain();
    public static void Release(Dictionary<K, V> value) => pool.Release(value);
    public static void Clear() => pool.Clear();
}
