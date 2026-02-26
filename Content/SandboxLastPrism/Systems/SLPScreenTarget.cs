#region Using directives

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using System.Collections.Generic;
using System;
using Terraria.Graphics.Effects;
using System.Linq;
using System.Threading;
using ReLogic.Content;

#endregion

namespace MagnumOpus.Content.SandboxLastPrism.Systems
{
    public class SLPScreenTarget
    {
        /// <summary>
        /// What gets rendered to this screen target. Spritebatch is automatically started and RT automatically set, you only need to write the code for what you are rendering.
        /// </summary>
        public Action<SpriteBatch> drawFunct;

        /// <summary>
        /// If this render target should be rendered. Make sure this it as restrictive as possible to prevent uneccisary rendering work.
        /// </summary>
        public Func<bool> activeFunct;

        /// <summary>
        /// Optional function that runs when the screen is resized. Returns the size the render target should be. Return null to prevent resizing.
        /// </summary>
        public Func<Vector2, Vector2?> onResize;

        /// <summary>
        /// Where this render target should fall in the order of rendering. Important if you want to render something to chain into another RT.
        /// </summary>
        public float order;

        public RenderTarget2D RenderTarget { get; set; }

        public SLPScreenTarget(Action<SpriteBatch> draw, Func<bool> active, float order, Func<Vector2, Vector2?> onResize = null)
        {
            if (Main.dedServ)
                return;

            drawFunct = draw;
            activeFunct = active;
            this.order = order;
            this.onResize = onResize;

            Vector2? initialDims = onResize is null ? new Vector2(Main.screenWidth, Main.screenHeight) : onResize(new Vector2(Main.screenWidth, Main.screenHeight));
            Main.QueueMainThreadAction(() => RenderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, (int)initialDims?.X, (int)initialDims?.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents));

            SLPScreenTargetHandler.AddTarget(this);
        }

        /// <summary>
        /// Foribly resize a target to a new size
        /// </summary>
        /// <param name="size"></param>
        public void ForceResize(Vector2 size)
        {
            if (Main.dedServ)
                return;

            RenderTarget.Dispose();
            RenderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, (int)size.X, (int)size.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        }
    }

    internal class SLPScreenTargetHandler : ModSystem
    {
        public static List<SLPScreenTarget> targets = new();
        public static Semaphore targetSem = new(1, 1);

        private static int firstResizeTime = 0;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                On_Main.CheckMonoliths += RenderScreens;
                Main.OnResolutionChanged += ResizeScreens;
            }
        }

        public override void Unload()
        {
            if (!Main.dedServ)
            {
                On_Main.CheckMonoliths -= RenderScreens;
                Main.OnResolutionChanged -= ResizeScreens;

                Main.QueueMainThreadAction(() =>
                {
                    if (targets != null)
                    {
                        targets.ForEach(n => n.RenderTarget?.Dispose());
                        targets.Clear();
                        targets = null;
                    }
                });
            }
        }

        /// <summary>
        /// Registers a new screen target and orders it into the list. Called automatically by the constructor of SLPScreenTarget!
        /// </summary>
        /// <param name="toAdd"></param>
        public static void AddTarget(SLPScreenTarget toAdd)
        {
            targetSem.WaitOne();

            targets.Add(toAdd);
            targets.Sort((a, b) => a.order.CompareTo(b.order));

            targetSem.Release();
        }

        /// <summary>
        /// Removes a screen target from the targets list. Should not normally need to be used.
        /// </summary>
        /// <param name="toRemove"></param>
        public static void RemoveTarget(SLPScreenTarget toRemove)
        {
            targetSem.WaitOne();

            targets.Remove(toRemove);
            targets.Sort((a, b) => a.order - b.order > 0 ? 1 : -1);

            targetSem.Release();
        }

        public static void ResizeScreens(Vector2 obj)
        {
            if (Main.gameMenu || Main.dedServ)
                return;

            targetSem.WaitOne();

            targets.ForEach(n =>
            {
                Vector2? size = obj;

                if (n.onResize != null)
                    size = n.onResize(obj);

                if (size != null)
                {
                    n.RenderTarget?.Dispose();
                    n.RenderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, (int)size?.X, (int)size?.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
                }
            });

            targetSem.Release();
        }

        private void RenderScreens(On_Main.orig_CheckMonoliths orig)
        {
            orig();

            if (Main.gameMenu || Main.dedServ)
                return;

            RenderTargetBinding[] bindings = Main.graphics.GraphicsDevice.GetRenderTargets();

            targetSem.WaitOne();

            foreach (SLPScreenTarget target in targets)
            {
                if (target.drawFunct is null) //allows for RTs which dont draw in the default loop, like the lighting tile buffers
                    continue;

                Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default);
                Main.graphics.GraphicsDevice.SetRenderTarget(target.RenderTarget);
                Main.graphics.GraphicsDevice.Clear(Color.Transparent);

                if (target.activeFunct())
                    target.drawFunct(Main.spriteBatch);

                Main.spriteBatch.End();
            }

            Main.graphics.GraphicsDevice.SetRenderTargets(bindings);

            targetSem.Release();
        }

        public override void PostUpdateEverything()
        {
            if (Main.gameMenu)
                firstResizeTime = 0;
            else
                firstResizeTime++;

            if (firstResizeTime == 20)
                ResizeScreens(new Vector2(Main.screenWidth, Main.screenHeight));
        }
    }
}
