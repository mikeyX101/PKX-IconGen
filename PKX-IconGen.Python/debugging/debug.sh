#!/bin/bash

# This is used to quickly launch Blender with the PKX-IconGen addon without the need of the C# PKX-IconGen application.
# We use specific debug data using the debug flag in modify_data.py to simulate a launch like in PKX-IconGen.

# You need to extract Ludicolo's model (fsys filename is runpappa) using StarsMmd's tools found here https://github.com/PekanMmd/Pokemon-XD-Code and put the model in this directory
# Up-to-date builds are available in this Discord server, #colosseum-xd-resources: https://discord.gg/xCPjjnv
# Working directory must be this directory
# Pass Blender executable path as first argument

cd ..
cp ./template.blend ./debugging/debug.blend
$1 --debug-python --enable-autoexec --python-exit-code 200 ./debugging/debug.blend --python modify_data.py -- --pkx-debug
