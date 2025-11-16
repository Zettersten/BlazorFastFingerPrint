using BlazorSupercookie;
using Xunit;

namespace BlazorSupercookie.Tests;

public class FingerprintSessionTests
{
    private const string TestCacheId = "test-session";
    private FingerprintEngine CreateEngine() => new(TestCacheId, 8);

    [Fact]
    public void Constructor_ReadMode_CreatesSession()
    {
        var engine = CreateEngine();
        var session = new FingerprintSession("session-1", engine);
        
        Assert.True(session.IsReadMode);
        Assert.False(session.IsWriteMode);
        Assert.Null(session.Identifier);
        Assert.Equal("session-1", session.SessionId);
    }

    [Fact]
    public void Constructor_WriteMode_CreatesSession()
    {
        var engine = CreateEngine();
        var session = new FingerprintSession("session-1", engine, 42);
        
        Assert.False(session.IsReadMode);
        Assert.True(session.IsWriteMode);
        Assert.Equal(42UL, session.Identifier);
    }

    [Fact]
    public void RecordVisitedRoute_ValidRoute_Records()
    {
        var engine = CreateEngine();
        var session = new FingerprintSession("session-1", engine);
        var route = engine.GetRouteByIndex(0);
        
        Assert.NotNull(route);
        session.RecordVisitedRoute(route!);
        
        var identifier = session.CalculateIdentifier();
        Assert.NotNull(identifier);
    }

    [Fact]
    public void RecordVisitedRoute_InvalidRoute_Ignores()
    {
        var engine = CreateEngine();
        var session = new FingerprintSession("session-1", engine);
        
        session.RecordVisitedRoute("invalid-route");
        
        var identifier = session.CalculateIdentifier();
        Assert.Null(identifier);
    }

    [Fact]
    public void ShouldCacheRoute_WriteMode_ReturnsCorrect()
    {
        var engine = CreateEngine();
        var session = new FingerprintSession("session-1", engine, 5);
        var routes = engine.GetRoutesForIdentifier(5);
        
        foreach (var route in routes)
        {
            Assert.True(session.ShouldCacheRoute(route));
        }
    }

    [Fact]
    public void ShouldCacheRoute_ReadMode_ReturnsFalse()
    {
        var engine = CreateEngine();
        var session = new FingerprintSession("session-1", engine);
        var route = engine.GetRouteByIndex(0);
        
        Assert.NotNull(route);
        Assert.False(session.ShouldCacheRoute(route!));
    }

    [Fact]
    public void CalculateIdentifier_NoRoutes_ReturnsNull()
    {
        var engine = CreateEngine();
        var session = new FingerprintSession("session-1", engine);
        
        var identifier = session.CalculateIdentifier();
        Assert.Null(identifier);
    }

    [Fact]
    public void CalculateIdentifier_WithRoutes_ReturnsIdentifier()
    {
        var engine = CreateEngine();
        var session = new FingerprintSession("session-1", engine);
        var routes = engine.GetRoutesForIdentifier(7);
        
        foreach (var route in routes)
        {
            session.RecordVisitedRoute(route);
        }
        
        var identifier = session.CalculateIdentifier();
        Assert.Equal(7UL, identifier);
    }

    [Fact]
    public void GetRoutesToVisit_ValidSize_ReturnsRoutes()
    {
        var engine = CreateEngine();
        var session = new FingerprintSession("session-1", engine);
        var routes = session.GetRoutesToVisit(5);
        
        Assert.Equal(5, routes.Length);
        Assert.All(routes, route => Assert.NotNull(route));
    }

    [Fact]
    public void GetRoutesToCache_WriteMode_ReturnsRoutes()
    {
        var engine = CreateEngine();
        var session = new FingerprintSession("session-1", engine, 5);
        var routes = session.GetRoutesToCache();
        
        Assert.NotEmpty(routes);
        var expectedRoutes = engine.GetRoutesForIdentifier(5);
        Assert.Equal(expectedRoutes.Length, routes.Length);
    }

    [Fact]
    public void GetRoutesToCache_ReadMode_ReturnsEmpty()
    {
        var engine = CreateEngine();
        var session = new FingerprintSession("session-1", engine);
        var routes = session.GetRoutesToCache();
        
        Assert.Empty(routes);
    }
}
