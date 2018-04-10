﻿/*
The MIT License (MIT)
Copyright (c) 2018 Helix Toolkit contributors
*/

using SharpDX;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;

#if NETFX_CORE
namespace HelixToolkit.UWP.Model.Scene2D
#else

namespace HelixToolkit.Wpf.SharpDX.Model.Scene2D
#endif
{
    using Core2D;
    using System.Runtime.CompilerServices;

    /// <summary>
    ///
    /// </summary>
    public abstract partial class SceneNode2D : DisposeObject, IHitable2D
    {
        public sealed class UpdateEventArgs : EventArgs
        {
            public IRenderContext2D Context { private set; get; }

            public UpdateEventArgs(IRenderContext2D context)
            {
                Context = context;
            }
        }

        /// <summary>
        /// Gets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid GUID { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public SceneNode2D Parent { set; get; }

        private Visibility visibility = Visibility.Visible;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SceneNode2D"/> is visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if visible; otherwise, <c>false</c>.
        /// </value>
        public Visibility Visibility
        {
            set
            {
                if (Set(ref visibility, value))
                { InvalidateVisual(); }
            }
            get { return visibility; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is hit test visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is hit test visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsHitTestVisible { set; get; } = true;

        /// <summary>
        /// Gets or sets the wrapper source used to link the external wrapper with the node.
        /// </summary>
        /// <value>
        /// The hit test source.
        /// </value>
        public object WrapperSource { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is attached.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is attached; otherwise, <c>false</c>.
        /// </value>
        public bool IsAttached { private set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is renderable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is renderable; otherwise, <c>false</c>.
        /// </value>
        public bool IsRenderable { private set; get; } = true;

        private RenderCore2D renderCore;

        /// <summary>
        /// Gets or sets the render core.
        /// </summary>
        /// <value>
        /// The render core.
        /// </value>
        public RenderCore2D RenderCore
        {
            private set
            {
                if (renderCore != value)
                {
                    if (renderCore != null)
                    {
                        renderCore.OnInvalidateRenderer -= RenderCore_OnInvalidateRenderer;
                    }
                    renderCore = value;
                    if (renderCore != null)
                    {
                        renderCore.OnInvalidateRenderer += RenderCore_OnInvalidateRenderer;
                    }
                }
            }
            get
            {
                if (renderCore == null)
                {
                    RenderCore = CreateRenderCore();
                }
                return renderCore;
            }
        }

        /// <summary>
        /// Gets or sets the render host.
        /// </summary>
        /// <value>
        /// The render host.
        /// </value>
        protected IRenderHost RenderHost { private set; get; }

        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        public virtual IList<SceneNode2D> Items { get; } = Constants.EmptyRenderable2D;

        private Matrix3x2 modelMatrix = Matrix3x2.Identity;

        /// <summary>
        /// Gets or sets the model matrix.
        /// </summary>
        /// <value>
        /// The model matrix.
        /// </value>
        public Matrix3x2 ModelMatrix
        {
            set
            {
                if (Set(ref modelMatrix, value))
                {
                    RenderCore.LocalTransform = value;
                    InvalidateVisual();
                }
            }
            get { return modelMatrix; }
        }

        private Matrix3x2 layoutTranslate = Matrix3x2.Identity;

        /// <summary>
        /// Gets or sets the layout translate.
        /// </summary>
        /// <value>
        /// The layout translate.
        /// </value>
        public Matrix3x2 LayoutTranslate
        {
            set
            {
                if (Set(ref layoutTranslate, value))
                {
                    InvalidateRender();
                }
            }
            get { return layoutTranslate; }
        }

        private Matrix3x2 parentMatrix = Matrix3x2.Identity;

        /// <summary>
        /// Gets or sets the parent matrix.
        /// </summary>
        /// <value>
        /// The parent matrix.
        /// </value>
        public Matrix3x2 ParentMatrix
        {
            set
            {
                if (Set(ref parentMatrix, value))
                {
                    IsTransformDirty = true;
                }
            }
            get { return parentMatrix; }
        }

        private Matrix3x2 totalTransform = Matrix3x2.Identity;

        /// <summary>
        /// Gets or sets the total model matrix.
        /// </summary>
        /// <value>
        /// The total model matrix.
        /// </value>
        public Matrix3x2 TotalModelMatrix
        {
            private set
            {
                if (Set(ref totalTransform, value))
                {
                    for (int i = 0; i < Items.Count; ++i)
                    {
                        Items[i].ParentMatrix = totalTransform;
                    }
                    TransformChanged(ref value);
                    OnTransformChanged?.Invoke(this, new Transform2DArgs(ref value));
                }
            }
            get
            {
                return totalTransform;
            }
        }

        /// <summary>
        /// Gets or sets the transform matrix relative to its parent
        /// </summary>
        /// <value>
        /// The relative matrix.
        /// </value>
        private Matrix3x2 RelativeMatrix
        { set; get; }

        /// <summary>
        /// Gets or sets the layout bound with transform.
        /// </summary>
        /// <value>
        /// The layout bound with transform.
        /// </value>
        public RectangleF LayoutBoundWithTransform
        {
            private set; get;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is mouse over.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is mouse over; otherwise, <c>false</c>.
        /// </value>
        public bool IsMouseOver
        {
            set { RenderCore.IsMouseOver = value; }
            get { return RenderCore.IsMouseOver; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneNode2D"/> class.
        /// </summary>
        public SceneNode2D()
        {
            WrapperSource = this;
        }

        /// <summary>
        /// Creates the render core.
        /// </summary>
        /// <returns></returns>
        protected virtual RenderCore2D CreateRenderCore() { return new EmptyRenderCore2D(); }

        /// <summary>
        /// <para>Attaches the element to the specified host. To overide Attach, please override <see cref="OnAttach(IRenderHost)"/> function.</para>
        /// <para>Attach Flow: Set RenderHost -> Get Effect ->
        /// <see cref="OnAttach(IRenderHost)"/> -> <see cref="OnAttach"/> -> <see cref="InvalidateRender"/></para>
        /// </summary>
        /// <param name="host">The host.</param>
        public void Attach(IRenderHost host)
        {
            if (IsAttached || host == null)
            {
                return;
            }
            RenderHost = host;
            IsAttached = OnAttach(host);
            if (IsAttached)
            {
                OnAttached?.Invoke(this, EventArgs.Empty);
            }
            InvalidateAll();
        }

        /// <summary>
        /// To override Attach routine, please override this.
        /// </summary>
        /// <param name="host"></param>
        /// <returns>Return true if attached</returns>
        protected virtual bool OnAttach(IRenderHost host)
        {
            RenderCore.Attach(host);
            return true;
        }

        /// <summary>
        /// Detaches this instance.
        /// </summary>
        public void Detach()
        {
            if (IsAttached)
            {
                IsAttached = false;
                RenderCore.Detach();
                Disposer.RemoveAndDispose(ref bitmapCache);
                OnDetach();
                OnDetached?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called when [detach].
        /// </summary>
        protected virtual void OnDetach()
        {
            RenderHost = null;
        }

        /// <summary>
        /// Updates the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Update(IRenderContext2D context)
        {
            OnUpdate?.Invoke(this, new UpdateEventArgs(context));
            IsRenderable = CanRender(context);
        }

        #region Handling Transforms

        /// <summary>
        /// Transforms the changed.
        /// </summary>
        /// <param name="totalTransform">The total transform.</param>
        protected virtual void TransformChanged(ref Matrix3x2 totalTransform)
        {
        }

        /// <summary>
        /// Occurs when [on transform changed].
        /// </summary>
        public event EventHandler<Transform2DArgs> OnTransformChanged;

        #endregion Handling Transforms

        #region Events;

        /// <summary>
        /// Occurs when [on attached].
        /// </summary>
        public event EventHandler OnAttached;

        /// <summary>
        /// Occurs when [on detached].
        /// </summary>
        public event EventHandler OnDetached;

        /// <summary>
        /// Occurs when [on update].
        /// </summary>
        public event EventHandler<UpdateEventArgs> OnUpdate;

        #endregion Events;

        #region Rendering

        /// <summary>
        /// <para>Determine if this can be rendered.</para>
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual bool CanRender(IRenderContext2D context)
        {
            return Visibility == Visibility.Visible && IsAttached;
        }

        /// <summary>
        /// <para>Renders the element in the specified context. To override Render, please override <see cref="OnRender"/></para>
        /// <para>Uses <see cref="CanRender"/>  to call OnRender or not. </para>
        /// </summary>
        /// <param name="context">The context.</param>
        public void Render(IRenderContext2D context)
        {
            if (!IsRenderable)
            { return; }
            if (IsTransformDirty)
            {
                RelativeMatrix = Matrix3x2.Translation(-RenderSize * RenderTransformOrigin)
                    * ModelMatrix * Matrix3x2.Translation(RenderSize * RenderTransformOrigin)
                    * LayoutTranslate;
                TotalModelMatrix = RelativeMatrix * ParentMatrix;
                IsTransformDirty = false;
                InvalidateVisual();
            }

            LayoutBoundWithTransform = LayoutBound.Translate(TotalModelMatrix.TranslationVector);

#if DISABLEBITMAPCACHE
            IsBitmapCacheValid = false;
#else
            EnsureBitmapCache(context, new Size2((int)Math.Ceiling(LayoutClipBound.Width), (int)Math.Ceiling(LayoutClipBound.Height)), context.DeviceContext.MaximumBitmapSize);
#endif
            if (EnableBitmapCache && IsBitmapCacheValid)
            {
                if (IsVisualDirty)
                {
#if DEBUGDRAWING
                    Debug.WriteLine("Redraw bitmap cache");
#endif
                    context.PushRenderTarget(bitmapCache, true);
                    context.DeviceContext.Transform = Matrix3x2.Identity;
                    context.PushRelativeTransform(Matrix3x2.Identity);
                    RenderCore.Transform = context.RelativeTransform;
                    OnRender(context);
                    context.PopRelativeTransform();
                    context.PopRenderTarget();
                    IsVisualDirty = false;
                }
                if (context.HasTarget)
                {
                    context.DeviceContext.Transform = context.RelativeTransform * RelativeMatrix;
                    context.DeviceContext.DrawImage(bitmapCache, new Vector2(0, 0), LayoutClipBound,
                        InterpolationMode.Linear, global::SharpDX.Direct2D1.CompositeMode.SourceOver);
                }
            }
            else if (context.HasTarget)
            {
                context.PushRelativeTransform(context.RelativeTransform * RelativeMatrix);
                RenderCore.Transform = context.RelativeTransform;
                OnRender(context);
                context.PopRelativeTransform();
                IsVisualDirty = false;
            }
        }

        /// <summary>
        /// Renders the bitmap cache to a render target only.
        /// </summary>
        /// <param name="context">The context.</param>
        public void RenderBitmapCache(IRenderContext2D context)
        {
            if (IsRenderable && EnableBitmapCache && IsBitmapCacheValid && !IsVisualDirty && context.HasTarget)
            {
                context.DeviceContext.Transform = RelativeMatrix;
                context.DeviceContext.DrawImage(bitmapCache, new Vector2(0, 0), new RectangleF(0, 0, RenderSize.X, RenderSize.Y),
                    InterpolationMode.Linear, global::SharpDX.Direct2D1.CompositeMode.SourceOver);
            }
            else
            {
                Render(context);
            }
        }

        /// <summary>
        /// Called when [render].
        /// </summary>
        /// <param name="context">The context.</param>
        protected virtual void OnRender(IRenderContext2D context)
        {
            RenderCore.Render(context);
            for (int i = 0; i < this.Items.Count; ++i)
            {
                Items[i].Render(context);
            }
        }

        #endregion Rendering

        /// <summary>
        /// Determines whether this instance [can hit test].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can hit test]; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool CanHitTest()
        {
            return IsAttached && IsHitTestVisible;
        }

        /// <summary>
        /// Called when [hit test].
        /// </summary>
        /// <param name="mousePoint">The mouse point.</param>
        /// <param name="hitResult">The hit result.</param>
        /// <returns></returns>
        protected abstract bool OnHitTest(ref Vector2 mousePoint, out HitTest2DResult hitResult);

        /// <summary>
        /// Hits the test.
        /// </summary>
        /// <param name="mousePoint">The mouse point.</param>
        /// <param name="hitResult">The hit result.</param>
        /// <returns></returns>
        public bool HitTest(Vector2 mousePoint, out HitTest2DResult hitResult)
        {
            if (CanHitTest())
            {
                return OnHitTest(ref mousePoint, out hitResult);
            }
            else
            {
                hitResult = null;
                return false;
            }
        }

        /// <summary>
        /// Use InvalidateVisual if render update required.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void RenderCore_OnInvalidateRenderer(object sender, EventArgs e)
        {
            InvalidateVisual();
        }

        /// <summary>
        /// Invalidates the render.
        /// </summary>
        public void InvalidateRender()
        {
            RenderHost?.InvalidateRender();
        }

        protected override void OnDispose(bool disposeManagedResources)
        {
            renderCore?.Dispose();
            renderCore = null;
            base.OnDispose(disposeManagedResources);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="backingField"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected bool SetAffectsRender<T>(ref T backingField, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingField, value))
            {
                return false;
            }

            backingField = value;
            this.RaisePropertyChanged(propertyName);
            InvalidateRender();
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="backingField"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected bool SetAffectsMeasure<T>(ref T backingField, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingField, value))
            {
                return false;
            }

            backingField = value;
            this.RaisePropertyChanged(propertyName);
            InvalidateMeasure();
            return true;
        }
    }
}