# Avae.Browser.Essentials

A port of Microsoft.Maui.Essentials tailored for Avalonia.Browser, bringing essential cross-platform APIs to your Avalonia-based browser applications. This package simplifies access to common device and application features, enabling seamless integration with web technologies.

# Features

Cross-Platform Essentials: Leverage APIs from Microsoft.Maui.Essentials adapted for browser environments.

Automatic Integration: Includes essentials.js, automatically copied to your wwwroot folder during build.

Lightweight Configuration: Minimal setup required for integration into your Avalonia.Browser project.

MIT Licensed: Freely use, modify, and distribute under the permissive MIT License.

# Getting Started

Follow these steps to integrate Avae.Browser.Essentials into your Avalonia.Browser project.

# Prerequisites

An Avalonia.Browser project set up with .NET.

# Installation

1. Add Microsoft.Maui.Essentials to Your Shared Project

In your shared project’s .csproj file, include the Microsoft.Maui.Essentials package. Use one of the following methods
````
<UseMauiEssentials>true</UseMauiEssentials>
````
OR
````
<PackageReference Include="Microsoft.Maui.Essentials">
    <PrivateAssets>all</PrivateAssets>
</PackageReference>
````

2. Configure the Avalonia.Browser Project

In your Avalonia.Browser project’s .csproj file, add the following to enable IndexedDB support for file operations:
````
<EmccExtraLDFlags>-lidbfs.js</EmccExtraLDFlags>
````

3. Register Essentials in Your Application

Modify the Main method in your Avalonia.Browser project to initialize the essentials library:
````
private static Task Main(string[] args) => BuildAvaloniaApp()
    .WithInterFont()
    .StartBrowserAppAsync("out").ContinueWith(async t =>
    {
        await JSInitialize.Initialize();
    });
````

4. Update main.js

Ensure your main.js file exports the .NET runtime correctly:
````
export const dotnetRuntime = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create();
````


5. Set Up index.html

Configure your index.html file to include necessary metadata and scripts. Below is a sample configuration:
````
<!DOCTYPE html>
<html>
<head>
    <title>BrowserTest.Browser</title>
    <meta charset="UTF-8">
    <!-- Used by AppInfo -->
    <meta name="app-version" content="1.2.3" />
    <meta name="build-version" content="1.2.3" />
    <meta name="name" content="myApplicationName" />
    <meta name="package" content="myApplicationPackage" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0">    
    <link rel="stylesheet" href="./app.css" />
</head>
<body style="margin: 0; overflow: hidden">
    <div id="out">
        <div class="avalonia-splash">
            <h2>
                Powered by
                <a href="https://www.avaloniaui.net/" target="_blank">
                    <svg width="266" height="52" viewBox="0 0 266 52" fill="none" xmlns="http://www.w3.org/2000/svg">
                        <!-- SVG content (omitted for brevity, use the provided SVG) -->
                    </svg>
                </a>
            </h2>
        </div>
    </div>
    <script type='module' src="./main.js"></script>
    <script>
        // To return the token when calling the WebAuthenticator
        const channel = new BroadcastChannel('token');
        channel.postMessage(window.location.href);

        // Optionally, close the callback page after sending the token
        if (/Android|iPhone/i.test(navigator.userAgent)) {
            if (window.opener)
                window.close();
        }
        else
            window.close();
    </script>
</body>
</html>
````
# Configuration

No additional configuration is required. The essentials.js file is automatically copied to your wwwroot folder during the build process, ensuring seamless integration.

# Usage

Once installed, you can use Microsoft.Maui.Essentials APIs within your Avalonia.Browser application. For example, access application information via AppInfo or handle web authentication with WebAuthenticator.

# Example: Accessing AppInfo
````
using Microsoft.Maui.Essentials;

string appName = AppInfo.Name; // Retrieves "myApplicationName" from index.html
string appVersion = AppInfo.VersionString; // Retrieves "1.2.3"
````
# Built With

This package builds upon the excellent work of:

Microsoft.Maui.Essentials

AvaloniaUI

BlazorBrowserDetect

# License

Avae.Browser.Essentials is licensed under the MIT License.

# Contributing

Contributions are welcome! Please submit issues or pull requests to the GitHub repository. Ensure your code follows the project’s coding standards and includes appropriate tests.

# Acknowledgments

Thanks to the Avalonia community for their robust UI framework.
Gratitude to the MAUI team for providing cross-platform essentials.