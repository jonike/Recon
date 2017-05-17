﻿using UnityEngine;
using System.Collections;
using Gist.Extensions.AABB;
using System.Collections.Generic;

namespace Recon.BoundingVolumes {

    public class OBB : IConvexPolyhedron {
        public Bounds localBounds;
        public Matrix4x4 modelMatrix;
        public Matrix4x4 modelITMatrix;

        public OBB(Bounds localBounds, Matrix4x4 modelMatrix) {
            this.localBounds = localBounds;
            this.modelMatrix = modelMatrix;
            this.modelITMatrix = modelMatrix.inverse.transpose;
        }

        public Matrix4x4 ModelMatrix() {
            return modelMatrix;
        }
        #region IConvexPolyhedron implementation
        public IEnumerable<Vector3> Normals () {
            yield return modelITMatrix.MultiplyVector(Vector3.right);
            yield return modelITMatrix.MultiplyVector (Vector3.up);
            yield return modelITMatrix.MultiplyVector (Vector3.forward);
        }
        public IEnumerable<Vector3> Edges() {
            return Normals ();
        }
        public IEnumerable<Vector3> Vertices () {
            var extent = localBounds.extents;
            var center = modelMatrix.MultiplyPoint (localBounds.center);
            var x = modelMatrix.MultiplyVector(new Vector3(extent.x, 0f, 0f));
            var y = modelMatrix.MultiplyVector (new Vector3 (0f, extent.y, 0f));
            var z = modelMatrix.MultiplyVector(new Vector3(0f, 0f, extent.z));

            for (var i = 0; i < 8; i++)
                yield return center + ((i & 1) != 0 ? x : -x) + ((i & 2) != 0 ? y : -y) + ((i & 4) != 0 ? z : -z);
        }
        public Bounds LocalBounds() {
            return localBounds;
        }
        public Bounds WorldBounds() {
            return localBounds.EncapsulateInTargetSpace (modelMatrix);
        }
        public IConvexPolyhedron DrawGizmos() {
            var aabb = WorldBounds ();

            Gizmos.matrix = Matrix4x4.identity;

            Gizmos.color = ConvexPolyhedronSettings.GizmoAABBColor;
            Gizmos.DrawWireCube (aabb.center, aabb.size);

			Gizmos.color = ConvexPolyhedronSettings.GizmoLineColor;
            foreach (var v in Vertices())
                Gizmos.DrawSphere (v, ConvexPolyhedronSettings.GizmoVertexSize);

            var max = aabb.max;
            var normalLength = 1f;
            Gizmos.color = Color.red;
            foreach (var n in Normals())
                Gizmos.DrawLine(max, max + normalLength * n.normalized);

            Gizmos.matrix = modelMatrix;

            Gizmos.color = ConvexPolyhedronSettings.GizmoLineColor;
            Gizmos.DrawWireCube (localBounds.center, localBounds.size);

            Gizmos.matrix = Matrix4x4.identity;

            return this;
        }
        #endregion

        #region Static
        public static OBB Create(Transform tr, Bounds localBounds) {
            return new OBB (localBounds, tr.localToWorldMatrix);
        }
        #endregion
    }
}
