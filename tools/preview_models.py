"""Renders the exported primitive geometry of each animal (3/4 view + side view) so proportions can be checked
without Unity. Input: JSON files written by tools/test (Tests.cs). Usage: preview_models.py <export_dir> <out.png>"""
import json, math, sys, os
import numpy as np
from PIL import Image, ImageDraw
from scipy.spatial import ConvexHull

def qmat(q):
    x, y, z, w = q
    return np.array([[1-2*(y*y+z*z), 2*(x*y-z*w), 2*(x*z+y*w)],
                     [2*(x*y+z*w), 1-2*(x*x+z*z), 2*(y*z-x*w)],
                     [2*(x*z-y*w), 2*(y*z+x*w), 1-2*(x*x+y*y)]])

def shape_points(shape):
    pts = []
    if shape == "Sphere":
        for la in np.linspace(0, math.pi, 12):
            for lo in np.linspace(0, 2*math.pi, 20, endpoint=False):
                pts.append([.5*math.sin(la)*math.cos(lo), .5*math.cos(la), .5*math.sin(la)*math.sin(lo)])
    elif shape == "Capsule":     # height 2 (incl. caps), radius .5
        for la in np.linspace(0, math.pi/2, 6):
            for lo in np.linspace(0, 2*math.pi, 20, endpoint=False):
                r, h = .5*math.cos(la), .5*math.sin(la)
                pts.append([r*math.cos(lo), 0.5+h, r*math.sin(lo)]); pts.append([r*math.cos(lo), -0.5-h, r*math.sin(lo)])
        # scale: unity capsule is 2 tall -> stretch the above (which spans 2.0 .. total 1.0+... ) fix below
        pts = [[p[0], p[1]*1.0, p[2]] for p in pts]
    elif shape == "Cylinder":
        for lo in np.linspace(0, 2*math.pi, 24, endpoint=False):
            for y in (-1, 1): pts.append([.5*math.cos(lo), y, .5*math.sin(lo)])
    elif shape == "ARF_Cone":
        for lo in np.linspace(0, 2*math.pi, 24, endpoint=False): pts.append([.5*math.cos(lo), 0, .5*math.sin(lo)])
        pts.append([0, 1, 0])
    else:
        for x in (-.5, .5):
            for y in (-.5, .5):
                for z in (-.5, .5): pts.append([x, y, z])
    return np.array(pts)

def capsule_fix():
    # real Unity capsule: radius .5, total height 2 -> cylinder part 1 tall + two hemispheres of radius .5
    pts = []
    for la in np.linspace(0, math.pi/2, 6):
        for lo in np.linspace(0, 2*math.pi, 20, endpoint=False):
            r, h = .5*math.cos(la), .5*math.sin(la)
            pts.append([r*math.cos(lo), 0.5+h, r*math.sin(lo)]); pts.append([r*math.cos(lo), -0.5-h, r*math.sin(lo)])
    return np.array(pts)

def world(nodes):
    M = []
    for n in nodes:
        R = qmat(n["rot"]); S = np.diag(n["scale"])
        L = np.eye(4); L[:3, :3] = R @ S; L[:3, 3] = n["pos"]
        M.append(L if n["parent"] < 0 else M[n["parent"]] @ L)
    return M

def render(nodes, view, size=520, shade=True):
    M = world(nodes); items = []
    for n, m in zip(nodes, M):
        if not n["shape"]: continue
        p = capsule_fix() if n["shape"] == "Capsule" else shape_points(n["shape"])
        if n["shape"] == "Cylinder": p = p * np.array([1, .5, 1])   # unity cylinder is 2 tall: y in [-1,1]*scale
        if n["shape"] == "Cylinder": p = p * np.array([1, 2, 1])
        P = (m[:3, :3] @ p.T).T + m[:3, 3]
        items.append((P, n["color"]))
    ang = {"34": (35, 25), "side": (90, 0), "front": (0, 0)}[view]
    yaw, pitch = [math.radians(a) for a in ang]
    Ry = np.array([[math.cos(yaw), 0, math.sin(yaw)], [0, 1, 0], [-math.sin(yaw), 0, math.cos(yaw)]])
    Rx = np.array([[1, 0, 0], [0, math.cos(pitch), -math.sin(pitch)], [0, math.sin(pitch), math.cos(pitch)]])
    V = Rx @ Ry
    allp = np.vstack([P for P, _ in items]) @ V.T
    xmin, xmax = allp[:, 0].min(), allp[:, 0].max(); ymin, ymax = allp[:, 1].min(), allp[:, 1].max()
    sc = size*0.88/max(xmax-xmin, ymax-ymin)
    img = Image.new("RGB", (size, size), (28, 34, 56)); d = ImageDraw.Draw(img)
    d.line([(0, size*0.93), (size, size*0.93)], fill=(60, 70, 100))
    order = sorted(items, key=lambda it: (it[0] @ V.T)[:, 2].mean())
    for P, col in reversed(order) if False else order:
        Q = P @ V.T
        pts2 = np.c_[(Q[:, 0]-(xmin+xmax)/2)*sc+size/2, size/2-(Q[:, 1]-(ymin+ymax)/2)*sc]
        try: hull = ConvexHull(pts2); poly = [tuple(pts2[i]) for i in hull.vertices]
        except Exception: continue
        c = tuple(int(255*max(0, min(1, v))) for v in col)
        d.polygon(poly, fill=c, outline=tuple(int(v*0.55) for v in c))
    return img

if __name__ == "__main__":
    src, out = sys.argv[1], sys.argv[2]
    ids = ["lion", "elephant", "dog", "cat", "horse"]
    sheet = Image.new("RGB", (520*len(ids)//1, 520*2), (0, 0, 0))
    for i, cid in enumerate(ids):
        nodes = json.load(open(os.path.join(src, cid + ".json")))
        sheet.paste(render(nodes, "34"), (i*520, 0)); sheet.paste(render(nodes, "side"), (i*520, 520))
    sheet.resize((sheet.width*2//5, sheet.height*2//5)).save(out)
