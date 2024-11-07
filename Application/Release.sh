read -p "Enter target directory (default: ./Release/): " target
target=${target:-./Release/}

# For Windows x64
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:AssemblyName=Wordle-win-x64 -p:DebugType=none -o $target

# For Windows ARM64
dotnet publish -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=true -p:AssemblyName=Wordle-win-arm64 -p:DebugType=none -o $target

# For Linux x64
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:AssemblyName=Wordle-linux-x64 -p:DebugType=none -o $target

# For Linux ARM64
dotnet publish -c Release -r linux-arm64 --self-contained true -p:PublishSingleFile=true -p:AssemblyName=Wordle-linux-arm64 -p:DebugType=none -o $target

# For macOS x64
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -p:AssemblyName=Wordle-macos-x64 -p:DebugType=none -o $target

# For macOS ARM64
dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -p:AssemblyName=Wordle-macos-arm64 -p:DebugType=none -o $target