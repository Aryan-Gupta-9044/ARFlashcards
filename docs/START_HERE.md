# START HERE - from ZIP to app on your phone

You need: Unity 6 with **Android Build Support** (+ OpenJDK + Android SDK & NDK modules), an Android 10+ phone with a USB cable,
a free Vuforia account, and a printer (or a second screen to show the cards).

## A. Open the project
1. Unzip `ARFlashcards_Project.zip` to a short path without special characters, e.g. `D:\Unity Projects\ARFlashcards_Project`.
2. Unity Hub > **Add > Add project from disk** > select that folder.
3. Open it. If Hub warns about the Editor version, choose your installed Unity 6 and continue (accept "Upgrade").
   First import takes a few minutes. **The Console must show 0 red errors** (yellow warnings are fine).
4. The **AR Flashcards** window opens by itself (or menu `AR Flashcards > Setup Wizard`).

## B. Check it works before Vuforia (30 seconds)
5. In the wizard press **Create FlashcardScene**, then press **Play**. You see a 3D animal on a dark background with
   buttons: tap animal chips, drag to rotate, +/- to zoom, SPEAK, QUIZ, EN/HI/TA. This is the fallback viewer. Stop Play.

## C. Add Vuforia + license key
6. Wizard step 1: **Install Vuforia Engine**. Wait 1-3 minutes (accept the Vuforia EULA if asked). Unity recompiles.
   The tick appears when done. (If it fails: docs/TROUBLESHOOTING.md, "Vuforia will not install".)
7. Get a free key: https://developer.vuforia.com > Develop > License Manager > Get Development Key (name it AR Flashcards).
   Open it, copy the long key text.
8. Wizard step 2: paste the key, **Save key into project**. Then step 3: **Rebuild FlashcardScene** (adds Vuforia to the camera).

## D. Run on your phone
9. Phone: Settings > About > tap Build number 7 times > Developer options > **USB debugging ON**. Plug in, allow the prompt.
   Wizard step 4 **Apply Android settings**, then step 5 **Build & Run on connected phone**. (First build: 5-15 min.)
   No cable? Press **Build APK**, copy `Builds/ARFlashcards.apk` to the phone and install it.
10. Print `Cards/PRINT_ME_A4.pdf` at 100% scale, allow camera permission, point at a card (30-50 cm, good light, no glare).

## What your faculty will see
Animal pops in on the card -> name + fact + sound; EN/HI/TA switch; SPEAK reads it aloud; QUIZ asks "which animal is this?";
AR/3D button swaps between camera mode and viewer; progress counter "Scanned x / 5".

## If something goes wrong
Send a screenshot of the Console (red errors only) and the first line of any error. See docs/TROUBLESHOOTING.md.
