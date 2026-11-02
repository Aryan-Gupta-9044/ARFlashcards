# Troubleshooting

**Red errors in Console right after opening.** Send a screenshot. The project needs no packages to compile, so any error is a bug I want to see.

**Vuforia will not install (wizard).** Manual route: Window > Package Management > Package Manager; Project Settings > Package Manager > Scoped Registries > +
Name `Vuforia`, URL `https://registry.packages.developer.vuforia.com/`, Scope `com.ptc.vuforia`, Save. Package Manager > My Registries > Vuforia Engine > Install.
Then menu `AR Flashcards > Refresh Vuforia detection`, and in the wizard press Rebuild FlashcardScene.

**After installing Vuforia nothing changes.** Wait for the compile spinner to stop; the define `ARF_VUFORIA` is added automatically (Project Settings > Player > Scripting Define Symbols).

**App shows "Vuforia license key missing or invalid".** Paste the key again (wizard step 2). Use the Development key, not a name/ID. The app runs in 3D viewer mode meanwhile.

**Camera black / permission.** Allow camera in phone settings. Vuforia 11 needs Android 10+, ARM64.

**Card not detected.** Print at 100%, matte paper, 30-50 cm, bright even light, whole card visible. One card at a time first.

**Build fails: Android SDK/JDK.** Unity Hub > Installs > gear > Add modules > Android Build Support + OpenJDK + SDK & NDK Tools.

**Voice (SPEAK) silent.** Phone needs a Text-to-Speech engine with the language (Settings > Language > Text-to-speech). Hindi/Tamil voices may need downloading.

**Text looks tiny/huge.** UI scales to screen width; send a phone screenshot.
