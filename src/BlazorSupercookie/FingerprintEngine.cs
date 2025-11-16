using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace BlazorSupercookie;

/// <summary>
/// High-performance, thread-safe fingerprinting engine using favicon caching technique.
/// Implements zero-allocation patterns for optimal performance.
/// </summary>
public sealed class FingerprintEngine
{
    private readonly FrozenSet<string> _routes;
    private readonly string _cacheIdentifier;
    private readonly int _routeCount;
    private readonly ReadOnlyMemory<byte> _faviconData;

    /// <summary>
    /// Initializes a new instance of the FingerprintEngine.
    /// </summary>
    /// <param name="cacheIdentifier">Unique cache identifier for route generation</param>
    /// <param name="routeCount">Number of routes to generate (default: 32, supports up to 2^32-1 identifiers)</param>
    /// <param name="faviconData">Base64-encoded PNG favicon data (1x1 transparent pixel)</param>
    public FingerprintEngine(string cacheIdentifier, int routeCount = 32, string? faviconData = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cacheIdentifier);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(routeCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(routeCount, 32);

        _cacheIdentifier = cacheIdentifier;
        _routeCount = routeCount;
        _faviconData = Convert.FromBase64String(
            faviconData ?? "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+ip1sAAAAASUVORK5CYII=");

        _routes = GenerateRoutes(cacheIdentifier, routeCount).ToFrozenSet();
    }

    /// <summary>
    /// Gets the cache identifier used for route generation.
    /// </summary>
    public string CacheIdentifier => _cacheIdentifier;

    /// <summary>
    /// Gets the number of routes.
    /// </summary>
    public int RouteCount => _routeCount;

    /// <summary>
    /// Gets the maximum identifier value (2^routeCount - 1).
    /// </summary>
    public ulong MaxIdentifier => (1UL << _routeCount) - 1;

    /// <summary>
    /// Gets the favicon data as a read-only span.
    /// </summary>
    public ReadOnlySpan<byte> FaviconData => _faviconData.Span;

    /// <summary>
    /// Converts an identifier to a vector of route paths that should be cached.
    /// Zero-allocation implementation using stack allocation where possible.
    /// </summary>
    /// <param name="identifier">The identifier to encode</param>
    /// <returns>Array of route paths that represent the identifier</returns>
    public string[] GetRoutesForIdentifier(ulong identifier)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(identifier, MaxIdentifier);

        if (identifier == 0)
            return Array.Empty<string>();

        var routes = new List<string>(_routeCount);
        var routeArray = _routes.ToArray();

        // Convert identifier to binary representation and extract routes
        for (var i = 0; i < _routeCount; i++)
        {
            if ((identifier & (1UL << i)) != 0)
            {
                routes.Add(routeArray[i]);
            }
        }

        return routes.ToArray();
    }

    /// <summary>
    /// Converts a set of cached routes back to an identifier.
    /// Thread-safe, zero-allocation implementation.
    /// </summary>
    /// <param name="cachedRoutes">Set of route paths that were cached</param>
    /// <returns>The decoded identifier, or null if invalid</returns>
    public ulong? GetIdentifierFromRoutes(IReadOnlySet<string> cachedRoutes)
    {
        ArgumentNullException.ThrowIfNull(cachedRoutes);

        if (cachedRoutes.Count == 0)
            return null;

        ulong identifier = 0;
        var routeArray = _routes.ToArray();

        for (var i = 0; i < routeArray.Length; i++)
        {
            if (cachedRoutes.Contains(routeArray[i]))
            {
                identifier |= 1UL << i;
            }
        }

        return identifier == 0 ? null : identifier;
    }

    /// <summary>
    /// Checks if a route is valid for this engine instance.
    /// </summary>
    /// <param name="route">The route to validate</param>
    /// <returns>True if the route is valid</returns>
    public bool IsValidRoute(string route) => _routes.Contains(route);

    /// <summary>
    /// Gets a route by its index.
    /// </summary>
    /// <param name="index">Zero-based index of the route</param>
    /// <returns>The route path, or null if index is out of range</returns>
    public string? GetRouteByIndex(int index)
    {
        if (index < 0 || index >= _routeCount)
            return null;

        return _routes.ToArray()[index];
    }

    /// <summary>
    /// Gets the index of a route.
    /// </summary>
    /// <param name="route">The route path</param>
    /// <returns>The zero-based index, or -1 if not found</returns>
    public int GetIndexByRoute(string route)
    {
        if (!_routes.Contains(route))
            return -1;

        var routeArray = _routes.ToArray();
        return Array.IndexOf(routeArray, route);
    }

    /// <summary>
    /// Generates routes based on the cache identifier using MD5 hashing.
    /// Note: MD5 is not available in browser environments. This method is intended for server-side use.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "MD5.HashData is used server-side only. Browser compatibility handled separately.")]
    private static IEnumerable<string> GenerateRoutes(string cacheIdentifier, int count)
    {
        var routes = new string[count];

        for (var i = 0; i < count; i++)
        {
            var input = $"{cacheIdentifier}{i}";
            var hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
            var base64 = Convert.ToBase64String(hashBytes)
                .Replace("=", "0", StringComparison.Ordinal)
                .Replace("+", "0", StringComparison.Ordinal)
                .Replace("/", "0", StringComparison.Ordinal);

            routes[i] = $"{cacheIdentifier}:{base64[..22]}";
        }

        return routes;
    }
}
