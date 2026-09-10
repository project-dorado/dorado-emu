// The emulator runtime is intentionally a static singleton (it emulates the
// Zune HD device's single graphics/input surface: PlatformHost.Graphics,
// PlatformHost.Input, PlatformHost.FrameCount, plus Directory.SetCurrentDirectory
// during package extraction). Tests that drive it must not run concurrently or
// they clobber each other's platform state and temp working directory.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
