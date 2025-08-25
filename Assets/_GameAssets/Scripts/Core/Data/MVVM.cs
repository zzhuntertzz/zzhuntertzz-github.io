using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
public abstract class ViewModel: INotifyPropertyChanged
{
    // [ShowInInspector]
    private Dictionary<Behaviour, Dictionary<string, Action>> binds = new();
    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        foreach(var key in binds.Keys.ToArray())
            if (key == null)
                binds.Remove(key);
        foreach (var action in binds.Values.ToArray().SelectMany(bind => bind.Where(action => action.Key == propertyName)))
            action.Value?.Invoke();
    }
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    public void Bind(Behaviour component, string property, Action action)
    {
        if (!binds.ContainsKey(component)) binds.Add(component, new Dictionary<string, Action>());
        binds[component][property] = action;
        action?.Invoke();
    }

    public void UnBind(Behaviour component)
    {
        binds.Remove(component);
    }
}

public class ViewModel<T> : ViewModel
{
    protected T _value;
    public T Value
    {
        get => _value;
        set => SetField(ref _value, value);
    }
}

[ShowInInspector]
public class ViewModelFetch<T> : ViewModel<T>
{
    private Func<UniTask<T>> _fetchTask;
    public ViewModelFetch(Func<UniTask<T>> fetchTask,T value=default)
    {
        _fetchTask = fetchTask;
        Value = value;
    }
    public async UniTask<T> Fetch()
    {
        Value = await _fetchTask();
        return Value;
    }
}

[Serializable]
public class ObservableList<T> : IList<T>,INotifyCollectionChanged
{
    public event NotifyCollectionChangedEventHandler CollectionChanged;
    [SerializeField] private List<T> list = new();
    #region IList[T] implementation
    public int IndexOf(T value)
    {
        return list.IndexOf(value);
    }

    public void Insert(int index, T value)
    {
        list.Insert(index, value);
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value, index));
    }

    public void RemoveAt(int index)
    {
        var item = list[index];
        list.RemoveAt(index);
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
    }
    public T this[int index]
    {
        get => list[index];
        set
        {
            if (EqualityComparer<T>.Default.Equals(list[index], value)) return;
            list[index] = value;
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, list[index], index));
        }
    }
    #endregion

    #region IEnumerable implementation

    // Return List<T> explicit Enumerator to avoid garbage allocated in foreach loops
    public List<T>.Enumerator GetEnumerator()
    {
        return list.GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion

    #region ICollection[T] implementation

    public void Add(T item)
    {
        list.Add(item);
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, list.Count - 1));
    }
    public void Clear()
    {
        list.Clear();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public bool Contains(T item)
    {
        return list.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        list.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
        var idx = list.IndexOf(item);
        if (idx > -1) RemoveAt(idx);
        return idx > -1;
    }
    
    public int Count => list.Count;
    public bool IsReadOnly => false;

    #endregion
}


