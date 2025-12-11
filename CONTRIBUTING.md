# Contributing to Timeless Tales

Thank you for your interest in contributing to Timeless Tales! This document provides guidelines for contributing to the project.

## How to Contribute

### Reporting Bugs
- Check if the bug has already been reported in Issues
- Include detailed steps to reproduce
- Provide system information (OS, .NET version, GPU)
- Include error messages or stack traces

### Suggesting Features
- Check the GDD.md for planned features
- Describe the feature and its benefits
- Explain how it fits with Vintage Story's mechanics
- Consider implementation complexity

### Code Contributions

#### Before You Start
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Read DEVELOPER.md for technical details
4. Ensure you have .NET 8.0 SDK installed

#### Development Workflow
1. Write clean, documented code
2. Follow existing code style:
   - PascalCase for classes and methods
   - _camelCase for private fields
   - UPPER_SNAKE_CASE for constants
3. Add XML documentation comments for public APIs
4. Test your changes thoroughly
5. Build without errors: `dotnet build`

#### Code Quality Standards
- No compiler errors
- Minimal warnings (nullable reference warnings acceptable)
- Pass CodeQL security scan
- Follow existing architecture patterns
- Keep methods focused and small
- Avoid deep nesting (max 3-4 levels)

#### Submitting Changes
1. Commit with clear messages:
   - Use present tense ("Add feature" not "Added feature")
   - First line: brief summary (50 chars max)
   - Follow with detailed description if needed
2. Push to your fork
3. Create a Pull Request with:
   - Clear description of changes
   - Reference to related issues
   - Screenshots for visual changes
   - Test results

### Areas for Contribution

#### Priority (Needed Now)
- [ ] Block texture system
- [ ] Save/load functionality
- [ ] Day/night cycle
- [ ] Basic crafting (knapping)
- [ ] Performance optimization

#### High Priority
- [ ] Temperature/season system
- [ ] Hunger/nutrition mechanics
- [ ] Ore prospecting system
- [ ] Lighting system
- [ ] Sound effects

#### Medium Priority
- [ ] Advanced crafting (pottery, metallurgy)
- [ ] Hostile entities
- [ ] UI improvements
- [ ] Chiseling system
- [ ] Temporal stability

#### Low Priority
- [ ] Multiplayer support
- [ ] Modding API
- [ ] Advanced graphics (shaders, particles)

### Code Organization

When adding new features:

**New Block Types**: Edit `Blocks/BlockRegistry.cs`
**New Game Mechanics**: Create new folder (e.g., `Crafting/`, `Temperature/`)
**New UI Elements**: Add to `UI/` folder
**New Entities**: Add to `Entities/` folder

### Testing

Before submitting:
1. Build in both Debug and Release modes
2. Test on Windows 10 and/or 11
3. Verify no memory leaks (long gameplay sessions)
4. Check FPS doesn't degrade over time
5. Test edge cases (world boundaries, empty inventory, etc.)

### Documentation

Update documentation when:
- Adding new features → Update README.md
- Changing architecture → Update DEVELOPER.md
- Adding player-facing features → Update QUICKSTART.md
- Making significant changes → Update GDD.md

### Commit Message Guidelines

Good:
```
Add copper ore texture
Implement knapping crafting system
Fix raycast bug at chunk boundaries
```

Bad:
```
Update
Fixed stuff
More changes
```

### Pull Request Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Performance improvement
- [ ] Documentation update

## Testing
- [ ] Tested on Windows 10/11
- [ ] No build errors
- [ ] No new warnings
- [ ] Passes CodeQL scan

## Screenshots (if applicable)
Add screenshots for visual changes

## Related Issues
Fixes #(issue number)
```

### Code Review Process

Your PR will be reviewed for:
1. Code quality and style
2. Adherence to architecture
3. Test coverage
4. Documentation updates
5. Performance impact

### Getting Help

- Read DEVELOPER.md for technical details
- Check existing code for examples
- Ask questions in PR comments
- Reference GDD.md for game design decisions

### License

By contributing, you agree that your contributions will be licensed under the MIT License.

### Recognition

Contributors will be:
- Listed in CONTRIBUTORS.md
- Credited in game credits (future feature)
- Mentioned in release notes

## Thank You!

Every contribution helps make Timeless Tales better. Whether it's code, documentation, bug reports, or ideas - we appreciate your involvement!

---

*Happy coding! 🎮⛏️*
