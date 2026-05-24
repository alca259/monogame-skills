using MonoGame.Editor.Core.Events;

namespace MonoGame.Editor.Core.UnitTests.Assets;

public sealed class ContentWatcherTests
{
    private sealed class CapturingEventBus : IEditorEventBus
    {
        public readonly List<IEditorEvent> Published = [];

        public void Publish<TEvent>(TEvent e) where TEvent : IEditorEvent
            => Published.Add(e);

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IEditorEvent { }
        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IEditorEvent { }
    }

    [Fact]
    public void Watch_NonExistentDirectory_DoesNotThrow()
    {
        CapturingEventBus bus = new();
        using ContentWatcher watcher = new(bus);

        watcher.Watch(Path.Combine(Path.GetTempPath(), "definitely_missing_dir_xyz"));

        Assert.Empty(watcher.RootPath);
    }

    [Fact]
    public void Watch_ValidDirectory_SetsRootPath()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            CapturingEventBus bus = new();
            using ContentWatcher watcher = new(bus);
            watcher.Watch(tempDir);

            Assert.Equal(tempDir, watcher.RootPath);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Stop_ClearsRootPath()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            CapturingEventBus bus = new();
            using ContentWatcher watcher = new(bus);
            watcher.Watch(tempDir);
            watcher.Stop();

            Assert.Empty(watcher.RootPath);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Watch_FileCreated_PublishesAssetImportedEvent()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            CapturingEventBus bus = new();
            using ContentWatcher watcher = new(bus);
            watcher.Watch(tempDir);

            string filePath = Path.Combine(tempDir, "sprite.png");
            File.WriteAllBytes(filePath, new byte[4]);

            // Allow file system event to propagate
            Thread.Sleep(200);

            Assert.Contains(bus.Published, e =>
                e is AssetImportedEvent evt &&
                string.Equals(evt.Asset.AbsolutePath, filePath, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        CapturingEventBus bus = new();
        ContentWatcher watcher = new(bus);
        watcher.Dispose();
        watcher.Dispose(); // must not throw
    }

    [Fact]
    public void Watch_AfterDispose_ThrowsObjectDisposedException()
    {
        CapturingEventBus bus = new();
        ContentWatcher watcher = new(bus);
        watcher.Dispose();

        Assert.Throws<ObjectDisposedException>(() => watcher.Watch(Path.GetTempPath()));
    }
}
