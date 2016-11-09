﻿using UnityEngine;
using System.Collections;
using Gist;
using System.Collections.Generic;
using Recon.SpacePartition;
using Recon.VisibleArea;
using Recon.BoundingVolumes.Behaviour;
using Recon.BoundingVolumes;
using System.Linq;

namespace Recon {
    [ExecuteInEditMode]
    public class Vision : MonoBehaviour, IConvex {
        public Color colorInsight = new Color (0.654f, 1f, 1f);
        public Color colorSpot = new Color (1f, 0.65f, 1f);

        public float range = 10f;
        public float angle = 90f;
        public float vertAngle = 45f;

        public IVolumeEvent InSight;

        Volume[] _selfVolumes;
        ConvexUpdator _convUp;
        Frustum _frustum;

        void Start() {
            _selfVolumes = GetComponentsInChildren<Volume> ();
        }
        void Update() {
            //Debug.LogFormat ("Broadphase count {0}", Broadphase ().Count ());
            #if false
            foreach (var v in NarrowPhase())
                InSight.Invoke (v);
            #endif
        }
        void OnDrawGizmos() {
            if (!isActiveAndEnabled)
                return;
            
            ConvUp.AssureUpdateConvex ();
            _frustum.DrawGizmos ();
            #if false
            foreach (var v in NarrowPhase())
                DrawInsight (v.GetBounds ().center);
            #endif
        }

        #region ConvexUpdator
        public ConvexUpdator ConvUp {
            get { return _convUp == null ? (_convUp = new ConvexUpdator (this)) : _convUp; }
        }
        #endregion

        #region Gizmos
        public void DrawInsight (Vector3 posTo) {
            Gizmos.color = colorInsight;
            Gizmos.DrawLine (transform.position, posTo);
        }
        #endregion

        #region IConvex implementation
        public IConvexPolyhedron GetConvexPolyhedron () {
            ConvUp.AssureUpdateConvex ();
            return _frustum;
        }
        public bool StartConvex () {
            range = Mathf.Clamp (range, 0f, float.MaxValue);
            angle = Mathf.Clamp (angle, 0f, 179f);
            vertAngle = Mathf.Clamp (vertAngle, 0f, 179f);
            return true;
        }
        public bool UpdateConvex () {
            return (_frustum = Frustum.Create (transform.position, transform.rotation, angle, vertAngle, range)) != null;
        }
        #endregion

        #region Intersection
        public bool SelfIntersection(Volume v) {
            foreach (var w in _selfVolumes)
                if (v == w)
                    return true;
            return false;
        }
        public Bounds WorldBounds() {
            ConvUp.AssureUpdateConvex ();
            return ConvUp.GetConvexPolyhedron ().WorldBounds ();
        }
        public IEnumerable<Volume> Broadphase() {
            Reconner r;
            BVHController<Volume> bvh;
            if ((r = Reconner.Instance) == null || (bvh = r.BVH) == null)
                yield break;

            foreach (var v in bvh.Intersect(WorldBounds()))
                yield return v;
        }
        public IEnumerable<Volume> NarrowPhase() {
            var conv = GetConvexPolyhedron ();
            foreach (var v in Broadphase())
                if (v.GetConvexPolyhedron().Intersect(conv))
                    yield return v;
        }
        #endregion
    }
}
