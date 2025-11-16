using System.Collections.Concurrent;

namespace BlazorSupercookie;

/// <summary>
/// Thread-safe service for managing fingerprint operations.
/// Provides high-performance, concurrent access to fingerprinting functionality.
/// </summary>
public sealed class FingerprintService : IDisposable
{
    private readonly FingerprintEngine _engine;
    private readonly ConcurrentDictionary<string, FingerprintSession> _sessions;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the FingerprintService.
    /// </summary>
    /// <param name="cacheIdentifier">Unique cache identifier</param>
    /// <param name="routeCount">Number of routes (default: 32)</param>
    public FingerprintService(string cacheIdentifier, int routeCount = 32)
    {
        _engine = new FingerprintEngine(cacheIdentifier, routeCount);
        _sessions = new ConcurrentDictionary<string, FingerprintSession>();
    }

    /// <summary>
    /// Gets the underlying fingerprint engine.
    /// </summary>
    public FingerprintEngine Engine => _engine;

    /// <summary>
    /// Creates or retrieves a fingerprint session for a user.
    /// </summary>
    /// <param name="sessionId">Unique session identifier</param>
    /// <param name="identifier">Optional existing identifier for write mode</param>
    /// <returns>The fingerprint session</returns>
    public FingerprintSession GetOrCreateSession(string sessionId, ulong? identifier = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(FingerprintService));
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);

        return _sessions.GetOrAdd(sessionId, _ => new FingerprintSession(sessionId, _engine, identifier));
    }

    /// <summary>
    /// Gets an existing session, or returns null if not found.
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <returns>The session, or null if not found</returns>
    public FingerprintSession? GetSession(string sessionId)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(FingerprintService));
        return _sessions.TryGetValue(sessionId, out var session) ? session : null;
    }

    /// <summary>
    /// Removes a session.
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <returns>True if the session was removed</returns>
    public bool RemoveSession(string sessionId)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(FingerprintService));
        return _sessions.TryRemove(sessionId, out _);
    }

    /// <summary>
    /// Cleans up expired sessions older than the specified duration.
    /// </summary>
    /// <param name="maxAge">Maximum age of sessions to keep</param>
    /// <returns>Number of sessions removed</returns>
    public int CleanupExpiredSessions(TimeSpan maxAge)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(FingerprintService));

        var cutoff = DateTimeOffset.UtcNow - maxAge;
        var removed = 0;

        foreach (var (sessionId, session) in _sessions)
        {
            if (session.CreatedAt < cutoff)
            {
                if (_sessions.TryRemove(sessionId, out _))
                    removed++;
            }
        }

        return removed;
    }

    /// <summary>
    /// Releases all resources used by the FingerprintService.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _sessions.Clear();
        _disposed = true;
    }
}
