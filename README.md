# Avae.Browser.Essentials

This repository contains source code for the integration of Maui essentials in Avalonia.Browser
1. How to register essentials in your project .Browser

```` 
private static Task Main(string[] args) => BuildAvaloniaApp()
            .WithInterFont()
            .StartBrowserAppAsync("out").ContinueWith(async t =>
            {
                await JSInitialize.Initialize();
            });
````
2. It doesn't works with a Directory.Packages.props
3. In your shared avalonia project add this to csproj
````
<UseMauiEssentials>true</UseMauiEssentials>
````
## What's Included
The package automatically copies `essentials.js` to your `wwwroot` folder during build.

## Configuration
No configuration required. The file will be copied automatically on build.

## Built With

https://github.com/erossini/BlazorBrowserDetect

## Licence

Avae.Browser.Essentials is licenced under the [MIT licence](Licence.md).