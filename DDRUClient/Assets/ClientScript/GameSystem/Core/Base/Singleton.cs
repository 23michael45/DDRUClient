using System;
public class Singleton<T>
{
    static T instance;
    public Singleton() { }
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Activator.CreateInstance<T>();
            }
            return instance;
        }
    }
}
