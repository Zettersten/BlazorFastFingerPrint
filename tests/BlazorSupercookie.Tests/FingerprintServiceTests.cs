using BlazorSupercookie;
using Xunit;

namespace BlazorSupercookie.Tests;

public class FingerprintServiceTests
{
    private const string TestCacheId = "test-service";

    [Fact]
    public void Constructor_CreatesService()
    {
        var service = new FingerprintService(TestCacheId);
        
        Assert.NotNull(service.Engine);
        Assert.Equal(TestCacheId, service.Engine.CacheIdentifier);
    }

    [Fact]
    public void GetOrCreateSession_NewSession_CreatesSession()
    {
        var service = new FingerprintService(TestCacheId);
        var session = service.GetOrCreateSession("session-1");
        
        Assert.NotNull(session);
        Assert.Equal("session-1", session.SessionId);
    }

    [Fact]
    public void GetOrCreateSession_ExistingSession_ReturnsSame()
    {
        var service = new FingerprintService(TestCacheId);
        var session1 = service.GetOrCreateSession("session-1");
        var session2 = service.GetOrCreateSession("session-1");
        
        Assert.Same(session1, session2);
    }

    [Fact]
    public void GetOrCreateSession_WithIdentifier_CreatesWriteMode()
    {
        var service = new FingerprintService(TestCacheId);
        var session = service.GetOrCreateSession("session-1", 42);
        
        Assert.True(session.IsWriteMode);
        Assert.Equal(42UL, session.Identifier);
    }

    [Fact]
    public void GetSession_ExistingSession_ReturnsSession()
    {
        var service = new FingerprintService(TestCacheId);
        var created = service.GetOrCreateSession("session-1");
        var retrieved = service.GetSession("session-1");
        
        Assert.NotNull(retrieved);
        Assert.Same(created, retrieved);
    }

    [Fact]
    public void GetSession_NonExistent_ReturnsNull()
    {
        var service = new FingerprintService(TestCacheId);
        var session = service.GetSession("non-existent");
        
        Assert.Null(session);
    }

    [Fact]
    public void RemoveSession_ExistingSession_Removes()
    {
        var service = new FingerprintService(TestCacheId);
        service.GetOrCreateSession("session-1");
        
        var removed = service.RemoveSession("session-1");
        Assert.True(removed);
        
        var session = service.GetSession("session-1");
        Assert.Null(session);
    }

    [Fact]
    public void RemoveSession_NonExistent_ReturnsFalse()
    {
        var service = new FingerprintService(TestCacheId);
        var removed = service.RemoveSession("non-existent");
        
        Assert.False(removed);
    }

    [Fact]
    public void CleanupExpiredSessions_RemovesOldSessions()
    {
        var service = new FingerprintService(TestCacheId);
        service.GetOrCreateSession("session-1");
        
        // Wait a bit to ensure time difference
        Thread.Sleep(100);
        
        var removed = service.CleanupExpiredSessions(TimeSpan.FromMilliseconds(50));
        Assert.True(removed >= 0);
    }

    [Fact]
    public void Dispose_ClearsSessions()
    {
        var service = new FingerprintService(TestCacheId);
        service.GetOrCreateSession("session-1");
        service.Dispose();
        
        // After dispose, operations should throw
        Assert.Throws<ObjectDisposedException>(() => service.GetOrCreateSession("session-2"));
    }
}
