# Viva Q&A

**What is AR vs VR?** AR overlays virtual content on the live camera view; VR replaces the view with a fully virtual world.

**How does Vuforia image tracking work?** The target image is analysed for distinctive feature points. At run time each camera frame is matched against them and the 6-DoF camera-to-target pose is computed; Unity places child objects in that pose.

**Why create targets at run time?** `ObserverFactory.CreateImageTarget(texture, width, name)` builds targets from textures on the device, so no cloud Target Manager database has to be built and imported.

**Why do cards need to be detailed and different from each other?** Tracking quality depends on the number and spread of features; cards that share structure can be confused. We measured ORB features (~2 000+/card) and cross-card matches (13× lower than self-matches).

**What happens if AR cannot start?** The status pill shows the reason (licence, permission, timeout) and the app switches to a 3D viewer, so the demo never goes blank.

**Why is Hindi/Tamil text an image?** Unity's legacy text does not do complex-script shaping; rendering offline with HarfBuzz guarantees correct conjuncts and vowel signs.

**How is the code organised for testing?** Game state (`FlashcardSession`, `QuizRound`) has no Unity dependency and is unit-tested; Vuforia use is isolated in two files.

**How do you handle both Unity input systems?** Dragging uses IMGUI events, which work with either system; pinch uses the legacy Input only when `ENABLE_LEGACY_INPUT_MANAGER` is defined; +/- buttons and mouse wheel cover the rest.

**What would you improve?** Real photogrammetry models, recorded animal sounds, more decks, Ground Plane placement, score persistence, analytics.

**Limitations?** One card tracked at a time, development-licence watermark, needs light and a printed/displayed card, Android 10+.
