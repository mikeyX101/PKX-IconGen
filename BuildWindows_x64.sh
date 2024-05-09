#!/bin/bash

rm -r ./Publish/win-x64

dotnet publish ./PKX-IconGen.AvaloniaUI/PKX-IconGen.AvaloniaUI.csproj --configuration Release --framework net8.0 --self-contained true --arch x64 --os win -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true --output ./Publish/win-x64

mv ./Publish/win-x64/PKX-IconGen.AvaloniaUI.pdb ./Publish/linux-x64/PKX-IconGen.pdb
mv ./Publish/win-x64/PKX-IconGen.AvaloniaUI.exe ./Publish/linux-x64/PKX-IconGen.exe
