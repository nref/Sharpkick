using System.Collections.Concurrent;
using System.Reactive.Subjects;

namespace SharpKick.Core.Services;

public interface IEventService
{
    /// <summary>
    /// Publish an event. All subscribed handlers with a matching key will receive the given object.
    /// </summary>
    void Publish(object key, object value);

    /// <summary>
    /// Subscribe to events. Subsequent calls to <see cref="Publish"/>
    /// with a matching key will invoke the given handler with the published object.
    ///  
    /// <para/>
    /// Call <see cref="IDisposable.Dispose()"/> to unsubscribe
    /// </summary>
    IDisposable Subscribe<T>(object key, Action<T> handler);
}

public class EventService : IEventService
{
    private readonly ConcurrentDictionary<object, Subject<object>> _subjects = new();

    public void Publish(object key, object value) => SubjectFor(key).OnNext(value);

    public IDisposable Subscribe<T>(object key, Action<T> handler) => SubjectFor(key).Subscribe(o => handler((T)o));

    private Subject<object> SubjectFor(object key) => _subjects.GetOrAdd(key, key => new Subject<object>());
}
