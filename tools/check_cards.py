"""Proxy for Vuforia's star rating: counts well-spread ORB features per card and checks the
cards cannot be confused with each other (cross-card RANSAC inliers should be ~0)."""
import cv2, numpy as np, itertools, sys
names = ["lion", "elephant", "dog", "cat", "horse"]
orb = cv2.ORB_create(nfeatures=3000)
data = {}
for n in names:
    img = cv2.imread(f"Cards/{n}_card.png", cv2.IMREAD_GRAYSCALE)
    img = cv2.resize(img, (512, 512))
    kp, des = orb.detectAndCompute(img, None)
    pts = np.array([k.pt for k in kp])
    cells = {(int(x // 128), int(y // 128)) for x, y in pts}      # 4x4 grid coverage
    data[n] = (kp, des)
    print(f"{n:9s} keypoints={len(kp):5d}  grid-coverage={len(cells):2d}/16")
bf = cv2.BFMatcher(cv2.NORM_HAMMING)
def inliers(a, b):
    (ka, da), (kb, db) = data[a], data[b]
    good = [m for m, n2 in bf.knnMatch(da, db, k=2) if m.distance < 0.75 * n2.distance]
    if len(good) < 8: return len(good), 0
    src = np.float32([ka[m.queryIdx].pt for m in good]).reshape(-1, 1, 2)
    dst = np.float32([kb[m.trainIdx].pt for m in good]).reshape(-1, 1, 2)
    H, mask = cv2.findHomography(src, dst, cv2.RANSAC, 4.0)
    return len(good), int(mask.sum()) if mask is not None else 0
worst = 0
for a, b in itertools.combinations(names, 2):
    g, i = inliers(a, b); worst = max(worst, i)
    print(f"  {a:8s} vs {b:8s}: ratio-matches={g:3d} ransac-inliers={i:3d}")
# self-match under a perspective warp (simulates a tilted card)
img = cv2.imread("Cards/lion_card.png", cv2.IMREAD_GRAYSCALE); img = cv2.resize(img, (512, 512))
M = cv2.getPerspectiveTransform(np.float32([[0,0],[512,0],[512,512],[0,512]]), np.float32([[40,30],[470,0],[500,500],[10,470]]))
warp = cv2.warpPerspective(img, M, (512, 512))
k2, d2 = orb.detectAndCompute(warp, None)
good = [m for m, n2 in bf.knnMatch(data["lion"][1], d2, k=2) if m.distance < 0.75 * n2.distance]
src = np.float32([data["lion"][0][m.queryIdx].pt for m in good]).reshape(-1,1,2); dst = np.float32([k2[m.trainIdx].pt for m in good]).reshape(-1,1,2)
H, mask = cv2.findHomography(src, dst, cv2.RANSAC, 4.0)
print("lion vs tilted lion: inliers =", int(mask.sum()))
self_inl = int(mask.sum())
ratio = self_inl / max(worst, 1)
print(f"RESULT: self-match {self_inl} vs worst cross-card {worst} -> separation x{ratio:.1f} (want >= 10)")
sys.exit(0 if ratio >= 10 else 1)
