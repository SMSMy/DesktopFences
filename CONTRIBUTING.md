# Contributing to Desktop Fences +

Thank you for your interest in contributing to Desktop Fences +! This document provides guidelines and instructions for contributing.

## 🚀 Getting Started

### Prerequisites

- Windows 10/11
- Visual Studio 2022 or later
- .NET 8.0 SDK
- Git

### Setting Up the Development Environment

1. **Clone the repository:**

   ```bash
   git clone https://github.com/limbo666/DesktopFences.git
   cd DesktopFences
   ```

2. **Open the solution:**

   ```bash
   cd Code
   start "Desktop Fences.sln"
   ```

3. **Restore packages and build:**
   ```bash
   dotnet restore
   dotnet build
   ```

## 📁 Project Structure

```
DesktopFences/
├── Code/
│   ├── Desktop Fences/           # Main application
│   │   ├── Interfaces/           # Interface definitions
│   │   ├── Resources/            # Images, icons, sounds
│   │   ├── FenceManager.cs       # Core fence logic
│   │   ├── SettingsManager.cs    # Settings management
│   │   ├── BackupManager.cs      # Backup/restore
│   │   ├── ThemeManager.cs       # Theme management
│   │   └── ...
│   └── Desktop Fences.Tests/     # Unit tests
├── Exported Fences/              # Sample fence exports
├── Imgs/                         # Documentation images
└── README.md                     # Main documentation
```

## 🔧 Development Guidelines

### Code Style

- Use **C# naming conventions** (PascalCase for public members, camelCase for private)
- Add **XML documentation** for public methods and classes
- Keep methods **under 50 lines** when possible
- Use **meaningful variable names**

### Architecture

The project follows these patterns:

- **Static managers** for global functionality (SettingsManager, ThemeManager)
- **Interfaces** for testability (IFenceManager, IBackupManager)
- **Event-driven** UI interactions

### Adding New Features

1. Create an issue describing the feature
2. Fork the repository
3. Create a feature branch: `git checkout -b feature/your-feature-name`
4. Implement the feature
5. Add tests if applicable
6. Submit a pull request

### Testing

Run tests with:

```bash
cd Code
dotnet test
```

## 🐛 Bug Reports

When reporting bugs, please include:

1. **Description**: Clear description of the issue
2. **Steps to Reproduce**: Numbered steps to reproduce the bug
3. **Expected Behavior**: What should happen
4. **Actual Behavior**: What actually happens
5. **Screenshots**: If applicable
6. **System Info**: Windows version, display scaling, etc.

## 📝 Pull Request Process

1. Update the README.md if needed
2. Add your changes to the changelog section
3. Ensure all tests pass
4. Request review from maintainers

## 🎨 UI/UX Guidelines

- Maintain the dark theme aesthetic
- Use consistent spacing and margins
- Follow Windows 11 design language
- Ensure DPI awareness for all UI elements

## 📄 License

By contributing, you agree that your contributions will be licensed under the MIT License.

## 💬 Questions?

- Open an issue for questions
- Check existing issues for similar questions
- Read the [TIPS.md](TIPS.md) for advanced features

---

Thank you for contributing! 🙏
