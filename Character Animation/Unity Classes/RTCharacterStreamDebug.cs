using UnityEngine;

namespace QualisysRealTime.Unity.Skeleton
{
    class RTCharacterStreamDebug : RTCharacterStream
    {
        public Skeleton skeleton;

        void OnDrawGizmos()
        {
            if (Application.isPlaying 
                && rtClient.GetStreamingStatus() 
                && skeleton != null 
                && markerData != null)
            {
                pos = this.transform.position + skeleton.Offset;
                if (skeleton.markers.ShowMarkers)
                {
                    foreach (var lb in markerData)
                    {
                        Gizmos.color = new Color(lb.Color.r, lb.Color.g, lb.Color.b);
                        Gizmos.DrawSphere(lb.Position + pos, skeleton.markers.MarkerScale);
                    }
                }

                if (skeleton.markers.MarkerBones && rtClient.Bones != null)
                {
                    foreach (var lb in rtClient.Bones)
                    {
                        var from = markerData.Find(md => md.Label == lb.From).Position + pos;
                        var to = markerData.Find(md => md.Label == lb.To).Position + pos;
                        Debug.DrawLine(from, to, skeleton.markers.boneColor);
                    }
                }

                if (base.skeleton != null &&
                    (skeleton.showSkeleton || skeleton.showRotationTrace || skeleton.showJoints || skeleton.showConstraints || skeleton.showTwistConstraints))
                {
                    Gizmos.color = skeleton.jointColor;
                    foreach (TreeNode<Bone> b in base.skeleton.Root)
                    {
                        if (skeleton.showSkeleton)
                        {
                            foreach (TreeNode<Bone> child in b.Children)
                            {
                                UnityEngine.Debug.DrawLine(b.Data.Pos.Convert() + pos, child.Data.Pos.Convert() + pos, skeleton.skelettColor);
                            }
                        }
                        if (skeleton.showRotationTrace && (!b.IsLeaf))
                        {
                            UnityDebug.DrawRays(b.Data.Orientation, b.Data.Pos.Convert() + pos, skeleton.traceLength);
                        }
                        if (skeleton.showJoints)
                        {
                            Gizmos.DrawSphere(b.Data.Pos.Convert() + pos, skeleton.jointSize);
                        }
                        if ((skeleton.showConstraints || skeleton.showTwistConstraints) && b.Data.HasConstraints)
                        {
                            OpenTK.Quaternion parentRotation =
                                b.Parent.Data.Orientation * b.Data.ParentPointer;
                            OpenTK.Vector3 poss = b.Data.Pos + pos.Convert();
                            if (skeleton.showConstraints)
                            {
                                UnityDebug.CreateIrregularCone(
                                    b.Data.Constraints, poss,
                                    OpenTK.Vector3.NormalizeFast(
                                        OpenTK.Vector3.Transform(OpenTK.Vector3.UnitY, parentRotation)),
                                    parentRotation,
                                    skeleton.coneResolution,
                                    skeleton.coneSize
                                    );
                            }
                            if (skeleton.showTwistConstraints)
                            {
                                UnityDebug.DrawTwistConstraints(b.Data, b.Parent.Data, poss, skeleton.traceLength);
                            }
                        }
                    }
                }
            }
        }

    }
}
