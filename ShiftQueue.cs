public class ShiftQueue<T> : Queue<T>
{
    int Size;
    public ShiftQueue(int size)
    {
        Size = size;
    }
    public new void Enqueue(T val)
    {
        base.Enqueue(val);
        while (this.Count > Size)
        {
            this.Dequeue();
        }
    }
    public void Enqueue(T[] qArr)
    {
        foreach(T val in qArr)
        {
            this.Enqueue(val);
        }
    }
    public void Enqueue(Span<T> qSpan)
    {
        foreach (T val in qSpan)
        {
            this.Enqueue(val);
        }
    }
    public static implicit operator T[] (ShiftQueue<T> q) => q.ToArray();
    public static implicit operator Span<T> (ShiftQueue<T> q) => q.ToArray();
}

public static class QueueExtensions
{
    public static Queue<T> Fill<T>(this Queue<T> q, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            q.Enqueue(default(T)!);
        }
        return q;
    }
}