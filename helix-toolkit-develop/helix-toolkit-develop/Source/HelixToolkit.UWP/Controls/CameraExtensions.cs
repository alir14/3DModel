﻿using SharpDX;
using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using Point = Windows.Foundation.Point;

namespace HelixToolkit.UWP
{
    /// <summary>
    /// 
    /// </summary>
    public static class CameraExtensions
    {
        /// <summary>
        /// Set the camera target point without changing the look direction.
        /// </summary>
        /// <param name="camera">
        /// The camera.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <param name="animationTime">
        /// The animation time.
        /// </param>
        public static void LookAt(this Camera camera, Vector3 target, double animationTime)
        {
            var projectionCamera = camera as ProjectionCamera;
            if (projectionCamera == null)
            {
                return;
            }

            LookAt(camera, target, projectionCamera.LookDirection, animationTime);
        }

        /// <summary>
        /// Set the camera target point and look direction
        /// </summary>
        /// <param name="camera">
        /// The camera.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <param name="newLookDirection">
        /// The new look direction.
        /// </param>
        /// <param name="animationTime">
        /// The animation time.
        /// </param>
        public static void LookAt(
            this Camera camera, Vector3 target, Vector3 newLookDirection, double animationTime)
        {
            var projectionCamera = camera as ProjectionCamera;
            if (projectionCamera == null)
            {
                return;
            }

            LookAt(camera, target, newLookDirection, projectionCamera.UpDirection, animationTime);
        }

        /// <summary>
        /// Set the camera target point and directions
        /// </summary>
        /// <param name="camera">
        /// The camera.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <param name="newLookDirection">
        /// The new look direction.
        /// </param>
        /// <param name="newUpDirection">
        /// The new up direction.
        /// </param>
        /// <param name="animationTime">
        /// The animation time.
        /// </param>
        public static void LookAt(
            this Camera camera,
            Vector3 target,
            Vector3 newLookDirection,
            Vector3 newUpDirection,
            double animationTime)
        {
            Vector3 newPosition = target - newLookDirection;
            camera.AnimateTo(newPosition, newLookDirection, newUpDirection, animationTime);
        }

        ///// <summary>
        ///// Animates the camera position and directions.
        ///// </summary>
        ///// <param name="camera">
        ///// The camera to animate.
        ///// </param>
        ///// <param name="newPosition">
        ///// The position to animate to.
        ///// </param>
        ///// <param name="newDirection">
        ///// The direction to animate to.
        ///// </param>
        ///// <param name="newUpDirection">
        ///// The up direction to animate to.
        ///// </param>
        ///// <param name="animationTime">
        ///// Animation time in milliseconds.
        ///// </param>
        //public static void AnimateTo(
        //    this Camera camera,
        //    Vector3 newPosition,
        //    Vector3 newDirection,
        //    Vector3 newUpDirection,
        //    double animationTime)
        //{
        //    var projectionCamera = camera as ProjectionCamera;
        //    if (projectionCamera == null)
        //    {
        //        return;
        //    }


        //    projectionCamera.Position = newPosition;
        //    projectionCamera.LookDirection = newDirection;
        //    projectionCamera.UpDirection = newUpDirection;

        //    //if (animationTime > 0)
        //    //{
        //    //    var a1 = new Point3DAnimation(
        //    //        fromPosition, newPosition, new Duration(TimeSpan.FromMilliseconds(animationTime)))
        //    //    {
        //    //        AccelerationRatio = 0.3,
        //    //        DecelerationRatio = 0.5,
        //    //        FillBehavior = FillBehavior.Stop
        //    //    };

        //    //    a1.Completed += (s, a) => { camera.BeginAnimation(ProjectionCamera.PositionProperty, null); };
        //    //    camera.BeginAnimation(ProjectionCamera.PositionProperty, a1);

        //    //    var a2 = new Vector3DAnimation(
        //    //        fromDirection, newDirection, new Duration(TimeSpan.FromMilliseconds(animationTime)))
        //    //    {
        //    //        AccelerationRatio = 0.3,
        //    //        DecelerationRatio = 0.5,
        //    //        FillBehavior = FillBehavior.Stop
        //    //    };
        //    //    a2.Completed += (s, a) => { camera.BeginAnimation(ProjectionCamera.LookDirectionProperty, null); };
        //    //    camera.BeginAnimation(ProjectionCamera.LookDirectionProperty, a2);

        //    //    var a3 = new Vector3DAnimation(
        //    //        fromUpDirection, newUpDirection, new Duration(TimeSpan.FromMilliseconds(animationTime)))
        //    //    {
        //    //        AccelerationRatio = 0.3,
        //    //        DecelerationRatio = 0.5,
        //    //        FillBehavior = FillBehavior.Stop
        //    //    };
        //    //    a3.Completed += (s, a) => { camera.BeginAnimation(ProjectionCamera.UpDirectionProperty, null); };
        //    //    camera.BeginAnimation(ProjectionCamera.UpDirectionProperty, a3);
        //    //}
        //}

        /// <summary>
        /// Zooms the camera to the specified rectangle.
        /// </summary>
        /// <param name="camera">
        /// The camera.
        /// </param>
        /// <param name="viewport">
        /// The viewport.
        /// </param>
        /// <param name="zoomRectangle">
        /// The zoom rectangle.
        /// </param>
        public static void ZoomToRectangle(this Camera camera, Viewport3DX viewport, Rect zoomRectangle)
        {
            var pcam = camera as ProjectionCamera;
            if (pcam == null)
            {
                return;
            }

            var topLeftRay = viewport.UnProjectToRay(new Point(zoomRectangle.Top, zoomRectangle.Left));
            var topRightRay = viewport.UnProjectToRay(new Point(zoomRectangle.Top, zoomRectangle.Right));
            var centerRay =
                viewport.UnProjectToRay(
                    new Point(
                        (zoomRectangle.Left + zoomRectangle.Right) * 0.5,
                        (zoomRectangle.Top + zoomRectangle.Bottom) * 0.5));

            if (topLeftRay == null || topRightRay == null || centerRay == null)
            {
                // could not invert camera matrix
                return;
            }

            var u = topLeftRay.Direction;
            var v = topRightRay.Direction;
            var w = centerRay.Direction;
            u.Normalize();
            v.Normalize();
            w.Normalize();
            var perspectiveCamera = camera as PerspectiveCamera;
            if (perspectiveCamera != null)
            {
                var distance = pcam.LookDirection.Length();

                // option 1: change distance
                var newDistance = distance * zoomRectangle.Width / viewport.ActualWidth;
                var newLookDirection = (float)newDistance * w;
                var newPosition = perspectiveCamera.Position + ((distance - (float)newDistance) * w);
                var newTarget = newPosition + newLookDirection;
                LookAt(pcam, newTarget, newLookDirection, 200);

                // option 2: change fov
                // double newFieldOfView = Math.Acos(Vector3D.DotProduct(u, v));
                // var newTarget = camera.Position + distance * w;
                // pcamera.FieldOfView = newFieldOfView * 180 / Math.PI;
                // LookAt(camera, newTarget, distance * w, 0);
            }

            var orthographicCamera = camera as OrthographicCamera;
            if (orthographicCamera != null)
            {
                orthographicCamera.Width *= zoomRectangle.Width / viewport.ActualWidth;
                var oldTarget = pcam.Position + pcam.LookDirection;
                var distance = pcam.LookDirection.Length();
                var newTarget = centerRay.PlaneIntersection(oldTarget, w);
                if (newTarget != null)
                {
                    orthographicCamera.LookDirection = w * distance;
                    orthographicCamera.Position = newTarget.Value - orthographicCamera.LookDirection;
                }
            }
        }

        /// <summary>
        /// Changes the direction of a camera.
        /// </summary>
        /// <param name="camera">
        /// The camera.
        /// </param>
        /// <param name="newLookDir">
        /// The new look direction.
        /// </param>
        /// <param name="newUpDirection">
        /// The new up direction.
        /// </param>
        /// <param name="animationTime">
        /// The animation time.
        /// </param>
        public static void ChangeDirection(this ProjectionCamera camera, Vector3 newLookDir, Vector3 newUpDirection, double animationTime)
        {
            var target = camera.Position + camera.LookDirection;
            var length = camera.LookDirection.Length();
            newLookDir.Normalize();
            LookAt(camera, target, newLookDir * length, newUpDirection, animationTime);
        }

        /// <summary>
        /// Copies the specified camera, converts field of view/width if necessary.
        /// </summary>
        /// <param name="source">
        /// The source camera.
        /// </param>
        /// <param name="dest">
        /// The destination camera.
        /// </param>
        public static void CopyTo(this Camera source, Camera dest)
        {
            var projectionSource = source as ProjectionCamera;
            var projectionDest = dest as ProjectionCamera;
            if (projectionSource == null || projectionDest == null)
            {
                return;
            }

            projectionDest.LookDirection = projectionSource.LookDirection;
            projectionDest.Position = projectionSource.Position;
            projectionDest.UpDirection = projectionSource.UpDirection;

            var psrc = source as PerspectiveCamera;
            var osrc = source as OrthographicCamera;
            var pdest = dest as PerspectiveCamera;
            var odest = dest as OrthographicCamera;
            if (pdest != null)
            {
                projectionDest.NearPlaneDistance = projectionSource.NearPlaneDistance > 0 ? projectionSource.NearPlaneDistance : 1e-1;
                projectionDest.FarPlaneDistance = projectionSource.FarPlaneDistance;

                double fov = 45;
                if (psrc != null)
                {
                    fov = psrc.FieldOfView;
                }

                if (osrc != null)
                {
                    double dist = projectionSource.LookDirection.Length();
                    fov = Math.Atan2(osrc.Width / 2, dist) * (180 / Math.PI);
                }

                pdest.FieldOfView = fov;
            }

            if (odest != null)
            {
                projectionDest.NearPlaneDistance = projectionSource.NearPlaneDistance;
                projectionDest.FarPlaneDistance = projectionSource.FarPlaneDistance;

                double width = 100;
                if (psrc != null)
                {
                    double dist = projectionSource.LookDirection.Length();
                    width = Math.Tan(psrc.FieldOfView / 180 * Math.PI) * 2 * dist;
                }

                if (osrc != null)
                {
                    width = osrc.Width;
                }

                odest.Width = width;
            }
        }

        /// <summary>
        /// Resets the specified camera.
        /// </summary>
        /// <param name="camera">
        /// The camera.
        /// </param>
        public static void Reset(this Camera camera)
        {
            var projectionCamera = camera as PerspectiveCamera;
            if (projectionCamera != null)
            {
                Reset(projectionCamera);
            }

            var ocamera = camera as OrthographicCamera;
            if (ocamera != null)
            {
                Reset(ocamera);
            }
        }
        /// <summary>
        /// Resets the specified perspective camera.
        /// </summary>
        /// <param name="camera">
        /// The camera.
        /// </param>
        public static void Reset(this PerspectiveCamera camera)
        {
            if (camera == null)
            {
                return;
            }

            camera.Position = new Vector3(20, 10, 40);
            camera.LookDirection = new Vector3(-20, -10, -40);
            camera.UpDirection = new Vector3(0, 1, 0);
            camera.FieldOfView = 45;
            camera.NearPlaneDistance = 0.1;
            camera.FarPlaneDistance = 1000;
        }

        /// <summary>
        /// Resets the specified orthographic camera.
        /// </summary>
        /// <param name="camera">
        /// The camera.
        /// </param>
        public static void Reset(this OrthographicCamera camera)
        {
            if (camera == null)
            {
                return;
            }

            camera.Position = new Vector3(20, 10, 40);
            camera.LookDirection = new Vector3(-20, -10, -40);
            camera.UpDirection = new Vector3(0, 1, 0);
            camera.Width = 40;
            camera.NearPlaneDistance = 0.1;
            camera.FarPlaneDistance = 1000;
        }

        /// <summary>
        /// Zooms to fit the extents of the specified viewport.
        /// </summary>
        /// <param name="camera">
        /// The actual camera.
        /// </param>
        /// <param name="viewport">
        /// The viewport.
        /// </param>
        /// <param name="animationTime">
        /// The animation time.
        /// </param>
        public static void ZoomExtents(
            this Camera camera, Viewport3DX viewport, double animationTime = 0)
        {
            var bounds = viewport.FindBounds();

            if (bounds.Maximum.IsUndefined() || bounds.Maximum == bounds.Minimum)
            {
                return;
            }

            ZoomExtents(camera as ProjectionCamera, viewport, bounds, animationTime);
        }

        /// <summary>
        /// Zooms to fit the specified bounding rectangle.
        /// </summary>
        /// <param name="camera">
        /// The actual camera.
        /// </param>
        /// <param name="viewport">
        /// The viewport.
        /// </param>
        /// <param name="bounds">
        /// The bounding rectangle.
        /// </param>
        /// <param name="animationTime">
        /// The animation time.
        /// </param>
        public static void ZoomExtents(
            this Camera camera, Viewport3DX viewport, BoundingBox bounds, double animationTime = 0)
        {
            var projectionCamera = camera as ProjectionCamera;
            if (projectionCamera == null)
            {
                return;
            }

            var diagonal = bounds.Maximum - bounds.Minimum;
            var center = (bounds.Maximum + bounds.Minimum)/2 + (diagonal * 0.5f);
            var radius = diagonal.Length() * 0.5f;
            ZoomExtents(projectionCamera, viewport, center, radius, animationTime);
        }

        /// <summary>
        /// Zooms to fit the specified sphere.
        /// </summary>
        /// <param name="camera">
        /// The camera.
        /// </param>
        /// <param name="viewport">
        /// The viewport.
        /// </param>
        /// <param name="center">
        /// The center of the sphere.
        /// </param>
        /// <param name="radius">
        /// The radius of the sphere.
        /// </param>
        /// <param name="animationTime">
        /// The animation time.
        /// </param>
        public static void ZoomExtents(
            this Camera camera, Viewport3DX viewport, Vector3 center, double radius, double animationTime = 0)
        {
            var projectionCamera = camera as ProjectionCamera;
            if (projectionCamera == null)
            {
                return;
            }

            // var target = Camera.Position + Camera.LookDirection;
            if (camera is PerspectiveCamera pcam)
            {
                double disth = radius / Math.Tan(0.5 * pcam.FieldOfView * Math.PI / 180);
                double vfov = pcam.FieldOfView / viewport.ActualWidth * viewport.ActualHeight;
                double distv = radius / Math.Tan(0.5 * vfov * Math.PI / 180);

                var dist = (float)Math.Max(disth, distv);
                var dir = projectionCamera.LookDirection;
                dir.Normalize();
                LookAt(projectionCamera, center, dir * dist, animationTime);
            }
            else if (camera is OrthographicCamera orth)
            {
                LookAt(projectionCamera, center, projectionCamera.LookDirection, animationTime);
                double newWidth = radius * 2;

                if (viewport.ActualWidth > viewport.ActualHeight)
                {
                    newWidth = radius * 2 * viewport.ActualWidth / viewport.ActualHeight;
                }

                AnimateWidth(orth, newWidth, animationTime);
            }
        }

        /// <summary>
        /// Animates the orthographic width.
        /// </summary>
        /// <param name="camera">
        /// An orthographic camera.
        /// </param>
        /// <param name="newWidth">
        /// The width to animate to.
        /// </param>
        /// <param name="animationTime">
        /// Animation time in milliseconds
        /// </param>
        public static void AnimateWidth(this OrthographicCamera camera, double newWidth, double animationTime)
        {
            if (animationTime > 0)
            {
                camera.AnimateWidth(newWidth, animationTime);
            }
            else
            {
                camera.Width = newWidth;
            }
        }
    }
}
