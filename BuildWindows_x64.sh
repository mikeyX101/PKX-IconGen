#!/bin/bash

dotnet publish ./PKX-IconGen.AvaloniaUI/PKX-IconGen.AvaloniaUI.csproj --configuration Release --framework net6.0 --self-contained true --arch x64 --os win -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true --output ./Publish/win-x64
