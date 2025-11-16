using System.Collections.Frozen;

namespace BlazorSupercookie;

/// <summary>
/// Represents a fingerprinting session for a single user.
/// Thread-safe and optimized for minimal allocations.
/// </summary>
public sealed class FingerprintSession
{
    private readonly FingerprintEngine _engine;
    private readonly FrozenSet<string>? _writeRoutes;
    private readonly HashSet<string> _visitedRoutes;
    private readonly object _lock = new();
    private ulong? _identifier;

    /// <summary>
    /// Initializes a new instance of FingerprintSession.
    /// </summary>
    /// <param name="sessionId">Unique session identifier</param>
    /// <param name="engine">The fingerprint engine</param>
    /// <param name="identifier">Optional identifier for write mode</param>
    public FingerprintSession(string sessionId, FingerprintEngine engine, ulong? identifier = null)
    {
        SessionId = sessionId;
        _engine = engine;
        _identifier = identifier;
        CreatedAt = DateTimeOffset.UtcNow;
        _visitedRoutes = new HashSet<string>(StringComparer.Ordinal);

        if (identifier.HasValue)
        {
            var routes = _engine.GetRoutesForIdentifier(identifier.Value);
            _writeRoutes = routes.ToFrozenSet();
        }
    }

    /// <summary>
    /// Gets the session identifier.
    /// </summary>
    public string SessionId { get; }

    /// <summary>
    /// Gets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Gets whether this session is in write mode (has an identifier).
    /// </summary>
    public bool IsWriteMode => _writeRoutes is not null;

    /// <summary>
    /// Gets whether this session is in read mode (no identifier).
    /// </summary>
    public bool IsReadMode => _writeRoutes is null;

    /// <summary>
    /// Gets the identifier if in write mode.
    /// </summary>
    public ulong? Identifier => _identifier;

    /// <summary>
    /// Records a visited route during read mode.
    /// Thread-safe.
    /// </summary>
    /// <param name="route">The route that was visited</param>
    public void RecordVisitedRoute(string route)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);

        if (!_engine.IsValidRoute(route))
            return;

        lock (_lock)
        {
            _visitedRoutes.Add(route);
        }
    }

    /// <summary>
    /// Checks if a route should be cached based on the identifier (write mode).
    /// </summary>
    /// <param name="route">The route to check</param>
    /// <returns>True if the route should be cached</returns>
    public bool ShouldCacheRoute(string route)
    {
        if (_writeRoutes is null)
            return false;

        return _writeRoutes.Contains(route);
    }

    /// <summary>
    /// Calculates the identifier from visited routes (read mode).
    /// Thread-safe.
    /// </summary>
    /// <returns>The calculated identifier, or null if insufficient data</returns>
    public ulong? CalculateIdentifier()
    {
        if (IsWriteMode)
            return _identifier;

        lock (_lock)
        {
            if (_visitedRoutes.Count == 0)
                return null;

            _identifier = _engine.GetIdentifierFromRoutes(_visitedRoutes);
            return _identifier;
        }
    }

    /// <summary>
    /// Gets the routes that should be visited for reading.
    /// </summary>
    /// <param name="storageSize">Number of bits to read</param>
    /// <returns>Array of route paths</returns>
    public string[] GetRoutesToVisit(int storageSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(storageSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(storageSize, _engine.RouteCount);

        var routes = new List<string>(storageSize);
        for (var i = 0; i < storageSize; i++)
        {
            var route = _engine.GetRouteByIndex(i);
            if (route is not null)
                routes.Add(route);
        }

        return routes.ToArray();
    }

    /// <summary>
    /// Gets the routes that should be cached for writing.
    /// </summary>
    /// <returns>Array of route paths, or empty if in read mode</returns>
    public string[] GetRoutesToCache()
    {
        if (_writeRoutes is null)
            return Array.Empty<string>();

        return _writeRoutes.ToArray();
    }
}
