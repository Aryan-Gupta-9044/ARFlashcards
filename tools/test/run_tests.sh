#!/usr/bin/env bash
# Compiles every runtime + editor script against stand-in Unity/Vuforia APIs and runs the logic tests.
# Needs: mono-mcs (apt install mono-mcs mono-runtime). Run from the project root: bash tools/test/run_tests.sh
set -e
cd "$(dirname "$0")/../.."
S=Assets/ARFlashcards; T=tools/test; W="-nowarn:0414,0649,0169,0219,0618"; O=${TMPDIR:-/tmp}
echo "== compile: NO Vuforia, NO input defines (a bare Unity project)"
mcs $W -target:library -out:$O/arf1.dll $T/UnityStubs.cs $S/Scripts/*.cs
echo "== compile: Vuforia + old Input Manager"
mcs -define:ARF_VUFORIA -define:ENABLE_LEGACY_INPUT_MANAGER $W -target:library -out:$O/arf2.dll $T/UnityStubs.cs $T/VuforiaStubs.cs $S/Scripts/*.cs
echo "== compile: Android + Editor (with and without Vuforia define)"
mcs -define:UNITY_EDITOR -define:UNITY_ANDROID $W -target:library -out:$O/arf3.dll $T/UnityStubs.cs $T/AndroidStubs.cs $T/EditorStubs.cs $S/Scripts/*.cs $S/Editor/*.cs
mcs -define:UNITY_EDITOR -define:UNITY_ANDROID -define:ARF_VUFORIA $W -target:library -out:$O/arf4.dll $T/UnityStubs.cs $T/VuforiaStubs.cs $T/AndroidStubs.cs $T/EditorStubs.cs $S/Scripts/*.cs $S/Editor/*.cs
echo "== tests WITHOUT Vuforia package (3D-viewer mode)"
mcs $W -out:$O/arf_t1.exe $T/UnityStubs.cs $T/Tests.cs $S/Scripts/*.cs
mkdir -p $O/arf_export; ARF_RES="$PWD/$S/Resources" mono $O/arf_t1.exe $O/arf_export
echo "== tests WITH Vuforia (simulated)"
mcs -define:ARF_VUFORIA $W -out:$O/arf_t2.exe $T/UnityStubs.cs $T/VuforiaStubs.cs $T/Tests.cs $S/Scripts/*.cs
ARF_RES="$PWD/$S/Resources" mono $O/arf_t2.exe | tail -40
