How to use

```
        await BuildAvaloniaApp()
            .StartBrowserAppAsync("out")
            .ContinueWith(async a =>
            {
                await JSInitialize.Initialize();
            });
    

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>().AfterSetup(async builder =>
        {
            IServiceCollection services = new ServiceCollection();
            services.ConfigureEssentials();
            Ioc.Default.ConfigureServices(services.BuildServiceProvider());

        }).UseBrowser();
```

In the main.js add export
```
export const dotnetRuntime = await dotnet
    .withDiagnosticTracing(true)
    .withApplicationArgumentsFromQuery()
    .create();
```

In the index.html
```
    <meta name="app-version" content="1.2.3" />
    <meta name="build-version" content="1.2.3" />

    <script>
        const channel = new BroadcastChannel('token');
        channel.postMessage(window.location.href);
        // Optionally, close the callback page after sending the token
        window.close();
    </script>
```
