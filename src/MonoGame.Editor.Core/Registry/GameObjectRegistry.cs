using System.Reflection;

namespace MonoGame.Editor.Core.Registry;

/// <summary>
/// Scans loaded assemblies for concrete types whose base-class chain includes a type
/// named <c>GameBehaviour</c>. Populated at startup or after a hot-reload.
/// </summary>
public sealed class GameObjectRegistry
{
    private readonly Dictionary<string, Type> _types = new(StringComparer.Ordinal);

    /// <summary>All discovered <c>GameBehaviour</c> subclasses keyed by their full type name.</summary>
    public IReadOnlyDictionary<string, Type> RegisteredTypes => _types;

    /// <summary>Scans all assemblies currently loaded in <see cref="AppDomain.CurrentDomain"/>.</summary>
    public void Scan() => Scan(AppDomain.CurrentDomain.GetAssemblies());

    /// <summary>Scans a specific set of <paramref name="assemblies"/> (primarily for unit testing).</summary>
    public void Scan(Assembly[] assemblies)
    {
        _types.Clear();
        for (int i = 0; i < assemblies.Length; i++)
        {
            try
            {
                Type[] types = assemblies[i].GetTypes();
                for (int j = 0; j < types.Length; j++)
                {
                    Type t = types[j];
                    if (t.IsAbstract || !t.IsClass) continue;
                    if (IsGameBehaviour(t))
                        _types[t.FullName ?? t.Name] = t;
                }
            }
            catch (ReflectionTypeLoadException) { }
            catch (NotSupportedException) { }
        }
    }

    /// <summary>Walks the inheritance chain and returns <c>true</c> if any base type is named <c>GameBehaviour</c>.</summary>
    private static bool IsGameBehaviour(Type type)
    {
        Type? current = type.BaseType;
        while (current is not null)
        {
            if (current.Name == "GameBehaviour") return true;
            current = current.BaseType;
        }
        return false;
    }
}
