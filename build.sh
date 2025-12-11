#!/bin/bash
# Build script for Timeless Tales

echo "Building Timeless Tales..."
cd TimelessTales

# Clean previous builds
echo "Cleaning previous builds..."
dotnet clean

# Build the project
echo "Building project..."
dotnet build -c Release

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Build successful!"
    echo ""
    echo "To run the game:"
    echo "  cd TimelessTales"
    echo "  dotnet run"
    echo ""
    echo "Or run the executable directly:"
    echo "  ./TimelessTales/bin/Release/net8.0/TimelessTales"
else
    echo "❌ Build failed!"
    exit 1
fi
