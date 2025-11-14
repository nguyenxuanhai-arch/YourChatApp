#!/bin/bash
# Build and run script for YourChatApp

echo "Building YourChatApp Solution..."

# Build Shared library
echo "[1/3] Building Shared library..."
cd Shared
dotnet build
cd ..

# Build Server
echo "[2/3] Building Server..."
cd Server
dotnet build
cd ..

# Build Client
echo "[3/3] Building Client..."
cd Client
dotnet build
cd ..

echo "Build complete!"
echo ""
echo "To run Server: cd Server && dotnet run"
echo "To run Client: cd Client && dotnet run"
