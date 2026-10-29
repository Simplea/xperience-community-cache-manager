# Xperience Community: Cache Manager

[![Nuget](https://img.shields.io/nuget/v/XperienceCommunity.CacheManager)](https://www.nuget.org/packages/XperienceCommunity.CacheManager)

## Description

This new module found in the Development category allows users to view and manage cache items within the Kentico Xperience administration UI. The Cache Manager displays all cache items in a table format, allowing administrators to inspect cache contents, view detailed information about individual cache entries, and clear the entire cache when needed.

## Library Version Matrix

| Xperience Version | Library Version |
|-------------------|-----------------|
| >= 29.0.0         | >= 1.0.0        |
| >= 30.0.0         | >= 1.1.0        |
| >= 31.7.2         | >= 1.2.1        |

> **Note:** The latest version that has been tested is 31.7.2 (targeting .NET 10)

## ⚙️ Package Installation

Add the package to your application using the .NET CLI

```bash
dotnet add package XperienceCommunity.CacheManager
```

## 🚀 Quick Start

Add the following to your application's startup code:

```csharp
builder.Services.AddCacheManager();
```

### Custom Clear Cache Buttons

Optionally, register up to 10 custom buttons that are displayed next to **Clear Cache**. Each button clears only the cache items whose key contains the given value (case-insensitive), without clearing the entire cache:

```csharp
builder.Services.AddCacheManager(options =>
{
    options.ClearCacheButtons.Add(new ClearCacheButton
    {
        ButtonText = "Clear Content Items",
        CacheKeyContains = "contentitem|"
    });
    options.ClearCacheButtons.Add(new ClearCacheButton
    {
        ButtonText = "Clear Web Pages",
        CacheKeyContains = "webpageitem|"
    });
});
```

Open the new Cache Manager application in the Development category. The module will display all current cache items in a table format.

![Cache Manager](images/list.png)

### Features

- **View Cache Items**: Browse all cache entries with their keys and values
- **Search by Key**: Type in the search box to filter cache items whose key contains the text (case-insensitive)
- **Detailed View**: Click on individual cache items to view their full details in a dialog
- **Cache Statistics**: View the total count of cache items
- **Clear Cache Key**: Remove a single cache item using the broom icon on its row
- **Clear Cache**: Clear all cache items with a single action
- **Custom Clear Cache Buttons**: Clear only the cache items whose key contains a configured value

The results are displayed in a table showing cache keys and values. Click on an individual row to view the full detail of that cache item's content.

![Cache Results](images/details.png)

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License.
