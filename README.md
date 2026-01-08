<h1 align="center">Desktop Fences +</h1>
<p align="center"><i>Organize your desktop like magic!</i></p>
<p align="center">
<img src="https://img.shields.io/github/downloads/limbo666/DesktopFences/total?style=flat-square" alt="Total Downloads"/>
<img src="https://img.shields.io/github/stars/limbo666/DesktopFences?style=flat-square" alt="Stars"/>
  <img src="https://img.shields.io/github/forks/limbo666/DesktopFences?style=flat-square" alt="Forks"/>
  <img src="https://img.shields.io/github/issues/limbo666/DesktopFences?style=flat-square" alt="Issues"/>
   <img src="https://img.shields.io/github/last-commit/limbo666/DesktopFences?style=flat-square" alt="Last Commit"/>
</p>

<p align="center">
  <img src="https://github.com/limbo666/DesktopFences/blob/main/Imgs/Desktop_Fences_Social_Media_PNG.png" />

</p>

Desktop Fences + creates **virtual fences** on your desktop, allowing you to group and organize icons in a clean and convenient way. With enhanced visual effects and right-click options, it aims to provide a more polished and customizable user interface.

**Desktop Fences+** is an open-source alternative to Stardock Fences, originally created by HakanKokcu under the name BirdyFences.

This project is a continuation and substantial modification of the original BirdyFences codebase, which was licensed under the MIT License at the time of forking.

Desktop Fences+ has been significantly enhanced and optimized for improved performance, stability, and user experience, while respecting the terms of the original license and acknowledging the original author.


## 🤖 v2.5.4+ AI-Enhanced Updates

> [!IMPORTANT] > **💝 Donations Notice:** All donations should go to the **ORIGINAL DESIGNER** of this program ([limbo666](https://github.com/limbo666)).
>
> These additions were made using **AI (Claude by Anthropic)** for testing and learning purposes.
> Despite being AI-assisted, we achieved beautiful and functional updates! ✨

### ✨ New Features (Fences 6 Inspired)

| Feature                     | Description                                            |
| --------------------------- | ------------------------------------------------------ |
| 🎨 **ThemeManager**         | Light/Dark/Custom themes with color picker             |
| 📑 **DragToTabManager**     | Drag files directly to tab headers                     |
| 👁️ **QuickHideManager**     | Quick hide fences by mouse gesture                     |
| 📂 **AutoOrganizeManager**  | Automatic file organization by type                    |
| 🌀 **PortalFenceManager**   | Enhanced portal fences functionality                   |
| ✨ **LaunchEffectsManager** | Animated launch effects for files                      |
| 📝 **NoteFenceManager**     | Note-taking fences enhancement                         |
| 🌐 **Arabic Localization**  | Full Arabic (العربية) language support with RTL layout |

### 🏗️ Architecture Improvements

| Component            | Description                                                                                       |
| -------------------- | ------------------------------------------------------------------------------------------------- |
| **ServiceLocator**   | Dependency Injection container for better modularity                                              |
| **IFenceManager**    | Interface for FenceManager                                                                        |
| **IBackupManager**   | Interface for backup operations                                                                   |
| **ISettingsManager** | Interface for settings management                                                                 |
| **ITrayManager**     | Interface for System Tray operations                                                              |
| **Handler Classes**  | FenceManager refactored into `FenceIconHandler`, `FenceDragDropHandler`, `FenceNavigationManager` |

### ⚡ Performance Enhancements

| Feature                  | Description                                              |
| ------------------------ | -------------------------------------------------------- |
| **LazyIconLoader**       | Lazy loading for icons to reduce memory usage            |
| **VirtualizationHelper** | UI virtualization for better performance with many items |

### 🔧 Bug Fixes

- Fixed MSB4803 COM reference error for .NET SDK 10 compatibility
- All 24 unit tests passing ✅

## Summary

Desktop Fences + brings a powerful and visually optimized experience for users who want to organize their desktop with flexibility and style. The program is designed to enhance productivity by combining intuitive interactions, aesthetic customization, and practical right-click options.

---

## Release

Get the latest release here:
https://github.com/limbo666/DesktopFences/releases

---

## Installation

Simply extract the executable and run it. The necessary configuration files (`fences.json`, `options.json`) will be created on first run.
Attention: Only user-writable locations are compatible. See (https://github.com/limbo666/DesktopFences/issues/51)

> Compatible with Windows 10/11
> No installation required — fully portable

---

## Stargazers over time

## [![Stargazers over time](https://starchart.cc/limbo666/DesktopFences.svg?background=%23535050&axis=%23f172cb&line=%239c34f7)](https://starchart.cc/limbo666/DesktopFences)

## License

This project is licensed under the [MIT License](License.md).

---

## Credits

Based on the original **BirdyFences** by HakanKokcu
Desktop Fences + is Enhanced and maintained by Nikos Georgousis.
Hand Water Pump 2025
