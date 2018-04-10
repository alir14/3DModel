﻿/*
The MIT License (MIT)
Copyright (c) 2018 Helix Toolkit contributors
*/
using SharpDX;

#if !NETFX_CORE
namespace HelixToolkit.Wpf.SharpDX.Core
#else
namespace HelixToolkit.UWP.Core
#endif
{
    using Shaders;
    using Render;
    /// <summary>
    /// 
    /// </summary>
    public class MeshOutlineRenderCore : PatchMeshRenderCore, IMeshOutlineParams
    {
        #region Properties
        /// <summary>
        /// Outline color
        /// </summary>
        public Color4 Color
        {
            set
            {
                SetAffectsRender(ref modelStruct.Color, value);
            }
            get
            {
                return modelStruct.Color.ToColor4();
            }
        }

        private bool outlineEnabled = false;
        /// <summary>
        /// Enable outline
        /// </summary>
        public bool OutlineEnabled
        {
            set
            {
                SetAffectsRender(ref outlineEnabled, value);
            }
            get
            {
                return outlineEnabled;
            }
        }

        private bool drawMesh = true;
        /// <summary>
        /// Draw original mesh
        /// </summary>
        public bool DrawMesh
        {
            set
            {
                SetAffectsRender(ref drawMesh, value);
            }
            get
            {
                return drawMesh;
            }
        }

        private bool drawOutlineBeforeMesh = false;
        /// <summary>
        /// Draw outline order
        /// </summary>
        public bool DrawOutlineBeforeMesh
        {
            set
            {
                SetAffectsRender(ref drawOutlineBeforeMesh, value);
            }
            get { return drawOutlineBeforeMesh; }
        }

        /// <summary>
        /// Outline fading
        /// </summary>
        public float OutlineFadingFactor
        {
            set
            {
                SetAffectsRender(ref modelStruct.Params.Y, value);
            }
            get { return modelStruct.Params.Y; }
        }

        private string outlinePassName = DefaultPassNames.MeshOutline;
        /// <summary>
        /// Gets or sets the name of the outline pass.
        /// </summary>
        /// <value>
        /// The name of the outline pass.
        /// </value>
        public string OutlinePassName
        {
            set
            {
                if(SetAffectsRender(ref outlinePassName, value) && IsAttached)
                {
                    outlineShaderPass = EffectTechnique[value];
                }
            }
            get
            {
                return outlinePassName;
            }
        }

        #endregion
        /// <summary>
        /// 
        /// </summary>
        protected IShaderPass outlineShaderPass { private set; get; }
        /// <summary>
        /// Initializes a new instance of the <see cref="MeshOutlineRenderCore"/> class.
        /// </summary>
        public MeshOutlineRenderCore()
        {
            OutlineFadingFactor = 1.5f;
        }
        /// <summary>
        /// Called when [attach].
        /// </summary>
        /// <param name="technique">The technique.</param>
        /// <returns></returns>
        protected override bool OnAttach(IRenderTechnique technique)
        {
            outlineShaderPass = technique[OutlinePassName];
            return base.OnAttach(technique);
        }
        /// <summary>
        /// Called when [update per model structure].
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        protected override void OnUpdatePerModelStruct(ref ModelStruct model, IRenderContext context)
        {            
            base.OnUpdatePerModelStruct(ref model, context);
            model.Params.Y = OutlineFadingFactor;
        }
        /// <summary>
        /// Called when [render].
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="deviceContext">The device context.</param>
        protected override void OnRender(IRenderContext context, DeviceContextProxy deviceContext)
        {
            if (DrawOutlineBeforeMesh)
            {
                outlineShaderPass.BindShader(deviceContext);
                outlineShaderPass.BindStates(deviceContext, StateType.BlendState | StateType.DepthStencilState);
                OnDraw(deviceContext, InstanceBuffer);
            }
            if (DrawMesh)
            {
                base.OnRender(context, deviceContext);
            }
            if (!DrawOutlineBeforeMesh)
            {
                outlineShaderPass.BindShader(deviceContext);
                outlineShaderPass.BindStates(deviceContext, StateType.BlendState | StateType.DepthStencilState);
                OnDraw(deviceContext, InstanceBuffer);
            }
        }
    }
}
