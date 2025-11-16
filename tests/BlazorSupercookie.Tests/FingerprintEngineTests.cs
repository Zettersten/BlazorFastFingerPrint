using BlazorSupercookie;
using Xunit;

namespace BlazorSupercookie.Tests;

public class FingerprintEngineTests
{
    private const string TestCacheId = "test-cache-id";

    [Fact]
    public void Constructor_ValidParameters_CreatesEngine()
    {
        var engine = new FingerprintEngine(TestCacheId, 32);
        
        Assert.Equal(TestCacheId, engine.CacheIdentifier);
        Assert.Equal(32, engine.RouteCount);
        Assert.Equal((1UL << 32) - 1, engine.MaxIdentifier);
    }

    [Fact]
    public void Constructor_InvalidCacheId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => new FingerprintEngine(null!));
        Assert.Throws<ArgumentException>(() => new FingerprintEngine(""));
        Assert.Throws<ArgumentException>(() => new FingerprintEngine("   "));
    }

    [Fact]
    public void Constructor_InvalidRouteCount_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FingerprintEngine(TestCacheId, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new FingerprintEngine(TestCacheId, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new FingerprintEngine(TestCacheId, 33));
    }

    [Fact]
    public void GetRoutesForIdentifier_Zero_ReturnsEmpty()
    {
        var engine = new FingerprintEngine(TestCacheId, 8);
        var routes = engine.GetRoutesForIdentifier(0);
        
        Assert.Empty(routes);
    }

    [Fact]
    public void GetRoutesForIdentifier_ValidId_ReturnsRoutes()
    {
        var engine = new FingerprintEngine(TestCacheId, 8);
        var routes = engine.GetRoutesForIdentifier(5); // Binary: 101
        
        Assert.NotEmpty(routes);
        Assert.True(routes.Length >= 2); // At least 2 routes for bits 0 and 2
    }

    [Fact]
    public void GetRoutesForIdentifier_MaxId_ReturnsAllRoutes()
    {
        var engine = new FingerprintEngine(TestCacheId, 8);
        var routes = engine.GetRoutesForIdentifier(engine.MaxIdentifier);
        
        Assert.Equal(8, routes.Length);
    }

    [Fact]
    public void GetRoutesForIdentifier_InvalidId_ThrowsException()
    {
        var engine = new FingerprintEngine(TestCacheId, 8);
        
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            engine.GetRoutesForIdentifier(engine.MaxIdentifier + 1));
    }

    [Fact]
    public void GetIdentifierFromRoutes_EmptySet_ReturnsNull()
    {
        var engine = new FingerprintEngine(TestCacheId, 8);
        var identifier = engine.GetIdentifierFromRoutes(new HashSet<string>());
        
        Assert.Null(identifier);
    }

    [Fact]
    public void GetIdentifierFromRoutes_ValidRoutes_ReturnsIdentifier()
    {
        var engine = new FingerprintEngine(TestCacheId, 8);
        var routes = engine.GetRoutesForIdentifier(5);
        var identifier = engine.GetIdentifierFromRoutes(routes.ToHashSet());
        
        Assert.Equal(5UL, identifier);
    }

    [Fact]
    public void GetIdentifierFromRoutes_RoundTrip_Consistent()
    {
        var engine = new FingerprintEngine(TestCacheId, 16);
        
        for (ulong id = 1; id <= 100; id++)
        {
            var routes = engine.GetRoutesForIdentifier(id);
            var recoveredId = engine.GetIdentifierFromRoutes(routes.ToHashSet());
            Assert.Equal(id, recoveredId);
        }
    }

    [Fact]
    public void IsValidRoute_ValidRoute_ReturnsTrue()
    {
        var engine = new FingerprintEngine(TestCacheId, 8);
        var routes = engine.GetRoutesForIdentifier(1);
        
        Assert.True(engine.IsValidRoute(routes[0]));
    }

    [Fact]
    public void IsValidRoute_InvalidRoute_ReturnsFalse()
    {
        var engine = new FingerprintEngine(TestCacheId, 8);
        
        Assert.False(engine.IsValidRoute("invalid-route"));
        Assert.False(engine.IsValidRoute(""));
    }

    [Fact]
    public void GetRouteByIndex_ValidIndex_ReturnsRoute()
    {
        var engine = new FingerprintEngine(TestCacheId, 8);
        var route = engine.GetRouteByIndex(0);
        
        Assert.NotNull(route);
        Assert.True(engine.IsValidRoute(route!));
    }

    [Fact]
    public void GetRouteByIndex_InvalidIndex_ReturnsNull()
    {
        var engine = new FingerprintEngine(TestCacheId, 8);
        
        Assert.Null(engine.GetRouteByIndex(-1));
        Assert.Null(engine.GetRouteByIndex(8));
        Assert.Null(engine.GetRouteByIndex(100));
    }

    [Fact]
    public void GetIndexByRoute_ValidRoute_ReturnsIndex()
    {
        var engine = new FingerprintEngine(TestCacheId, 8);
        var route = engine.GetRouteByIndex(3);
        
        Assert.NotNull(route);
        var index = engine.GetIndexByRoute(route!);
        Assert.Equal(3, index);
    }

    [Fact]
    public void GetIndexByRoute_InvalidRoute_ReturnsNegativeOne()
    {
        var engine = new FingerprintEngine(TestCacheId, 8);
        
        Assert.Equal(-1, engine.GetIndexByRoute("invalid-route"));
    }

    [Fact]
    public void FaviconData_ReturnsValidData()
    {
        var engine = new FingerprintEngine(TestCacheId, 8);
        var data = engine.FaviconData;
        
        Assert.NotEmpty(data);
        // PNG signature
        Assert.Equal(0x89, data[0]);
        Assert.Equal(0x50, data[1]);
        Assert.Equal(0x4E, data[2]);
        Assert.Equal(0x47, data[3]);
    }
}
