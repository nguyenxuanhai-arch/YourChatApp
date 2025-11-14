#!/bin/bash
# Script to open YourChatApp solution in Visual Studio

echo "Opening YourChatApp solution in Visual Studio..."

# For macOS
if [[ "$OSTYPE" == "darwin"* ]]; then
    open YourChatApp.sln
    echo "Solution opened in Visual Studio for Mac!"
    exit 0
fi

# For Linux (if Visual Studio Code is preferred)
if command -v code &> /dev/null; then
    code .
    echo "Opened in Visual Studio Code!"
    exit 0
fi

echo "Visual Studio not found!"
echo "Please install Visual Studio or Visual Studio Code"
exit 1
