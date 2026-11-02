# Test Report

## What was verified (automated, reproducible: `bash tools/test/run_tests.sh`)
| Area | Method | Result |
|---|---|---|
| Compilation – old Input Manager path | C# compiler vs. stand-in Unity/Vuforia API | pass |
| Compilation – new Input System path (`ENABLE_INPUT_SYSTEM`) | same | pass |
| Compilation – Android-only code (TTS, permission) and Editor tooling | same + UnityEditor stand-ins | pass |
| Syntax of all 19 C# files | tree-sitter C# parser | 0 errors |
| Deck data | 5 cards × (EN/HI/TA name+fact), unique ids, all image resources present | pass |
| Session logic | newest-visible-card wins, fallback on loss, no double-counting, completion, reset | 8 tests pass |
| Quiz logic | 3 unique options always including the answer, shuffled (≈31 % in slot 0 over 300 rounds), wrong/correct/after-solved, small pools | 7 tests pass |
| Localization | EN→HI→TA cycle + events, image loading | 6 tests pass |
| Cone mesh | winding: side faces outward, base faces down | 3 tests pass |
| Animal builders | 5 models (25–36 parts each), finite transforms, materials, animator wired | 20 tests pass |
| App flow (simulated Vuforia) | init → 5 runtime targets → AR; TRACKED / EXTENDED_TRACKED / NO_POSE events drive the session; AR⇄3D toggle; **licence error ⇒ automatic 3D viewer** | 15 tests pass |
| **Total** | | **63 / 63 pass on both input back-ends** |
| Card quality | OpenCV ORB proxy: 2 000–2 700 features/card, full 4×4 grid coverage; cross-card false matches ≤ 87 RANSAC inliers vs. 1 164 for a card against a tilted copy of itself (13× separation) | pass |
| Model look | geometry exported from the real builder code and rendered (`docs/model_geometry_preview.png`) | reviewed |

## What could NOT be verified here (needs your first run)
| Area | Why | How you verify |
|---|---|---|
| Import / compile inside the real Unity 6 editor | Unity is not available in this environment; Vuforia 11.4 API usage follows Vuforia's published API reference (`ObserverFactory.CreateImageTarget(Texture2D, float, string)`, `VuforiaApplication.OnVuforiaInitialized/Started`, `ObserverBehaviour.OnTargetStatusChanged`) | Open project; Console has no errors |
| Real tracking quality & latency on a phone | needs device + licence | First-run checklist (SETUP_GUIDE §5) |
| Visual polish of UI / lighting on device | needs device | Look; adjust colours in `Ui.cs` |
| Gradle build & APK install | needs Unity Android module | Wizard › Build |
| Text-to-speech, Hindi/Tamil voices | device dependent | Speak button |

## Known limitations
- Procedural primitive models are stylised, not photoreal; swap in prefabs (SETUP_GUIDE §6).
- Sounds are synthesised cues, not real animal calls, until you add files.
- Vuforia development licences add a watermark; one card is tracked at a time by design.
- Quiz wording is English only (animal names follow the selected language).


## Update (v3: dependency-free build)
* UI rewritten with built-in IMGUI; no uGUI / TextMeshPro / Input System / EventSystem needed. Vuforia code compiled only with `ARF_VUFORIA` (set automatically).
* Harness: compiled 4 ways (no Vuforia / Vuforia+legacy input / Editor+Android with and without Vuforia). 66 tests pass without Vuforia (incl. IMGUI draw on 4 screen sizes x 3 languages, quiz flow by simulated taps), 63 with simulated Vuforia.
* Manifest registry insertion tested on 3 manifest shapes.
* NOT run inside real Unity or on a phone (no Unity here). Unity 6.6 and Vuforia 11.x behaviour is therefore unverified - send Console screenshots if anything fails.
