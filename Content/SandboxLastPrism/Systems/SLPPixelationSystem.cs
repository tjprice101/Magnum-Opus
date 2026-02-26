#region Using directives

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
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

    //This is VERY VERY heavily based on Starlight River's Pixelation system
    //https://github.com/ProjectStarlight/StarlightRiver/blob/master/Core/Systems/PixelationSystem/PixelationSystem.cs

    public class SLPPixelationSystem : ModSystem
    {
        public List<SLPPixelationTarget> pixelationTargets = new();

        public override void Load()
        {
            if (Main.dedServ)
                return;

            On_Main.DrawCachedProjs += DrawTargets;
            On_Main.DrawDust += DrawDustTargets;
        }

        public override void PostSetupContent()
        {
            RegisterScreenTarget(SLPRenderLayer.UnderTiles);

            RegisterScreenTarget(SLPRenderLayer.UnderNPCs);

            RegisterScreenTarget(SLPRenderLayer.UnderProjectiles);

            RegisterScreenTarget(SLPRenderLayer.OverPlayers);

            RegisterScreenTarget(SLPRenderLayer.Dusts);
        }

        public override void Unload()
        {
            if (Main.dedServ)
                return;

            On_Main.DrawCachedProjs -= DrawTargets;
            On_Main.DrawDust -= DrawDustTargets;
        }


        //Calls DrawTarget() on all everything in pixelationTargets, according to what layer they are on
        private void DrawTargets(On_Main.orig_DrawCachedProjs orig, Main self, List<int> projCache, bool startSpriteBatch)
        {
            SpriteBatch sb = Main.spriteBatch;

            orig(self, projCache, startSpriteBatch);

            if (projCache.Equals(Main.instance.DrawCacheProjsBehindNPCsAndTiles))
            {
                foreach (SLPPixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == SLPRenderLayer.UnderTiles))
                {
                    DrawTarget(target, Main.spriteBatch, !startSpriteBatch);
                }
            }

            if (projCache.Equals(Main.instance.DrawCacheProjsBehindNPCs))
            {
                foreach (SLPPixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == SLPRenderLayer.UnderNPCs))
                {
                    DrawTarget(target, Main.spriteBatch, !startSpriteBatch);
                }
            }

            if (projCache.Equals(Main.instance.DrawCacheProjsBehindProjectiles))
            {
                foreach (SLPPixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == SLPRenderLayer.UnderProjectiles))
                {
                    DrawTarget(target, Main.spriteBatch, !startSpriteBatch);
                }
            }

            if (projCache.Equals(Main.instance.DrawCacheProjsOverPlayers))
            {
                foreach (SLPPixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == SLPRenderLayer.OverPlayers))
                {
                    DrawTarget(target, Main.spriteBatch, !startSpriteBatch);
                }
            }
        }


        private void DrawDustTargets(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);

            foreach (SLPPixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == SLPRenderLayer.Dusts))
            {
                DrawTarget(target, Main.spriteBatch, false);
            }
        }

        private void DrawTarget(SLPPixelationTarget target, SpriteBatch sb, bool endSpriteBatch = true)
        {
            if (endSpriteBatch)
            {
                sb.End();
            }

            BlendState blendState = BlendState.AlphaBlend;

            sb.Begin(SpriteSortMode.Immediate, blendState, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            sb.Draw(target.pixelationTarget2.RenderTarget, Vector2.Zero, null, Color.White, 0, new Vector2(0, 0), 2f, SpriteEffects.None, 0);

            sb.End();


            if (endSpriteBatch)
                sb.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Registers a ScreenTarget for use with a drawing action or list of drawing actions.
        /// </summary>
        public void RegisterScreenTarget(SLPRenderLayer renderType = SLPRenderLayer.UnderProjectiles)
        {
            Main.QueueMainThreadAction(() => pixelationTargets.Add(new SLPPixelationTarget(renderType)));
        }

        public void QueueRenderAction(string id, Action renderAction, int order = 0)
        {
            SLPRenderLayer layer;
            if (id == "UnderTiles")
                layer = SLPRenderLayer.UnderTiles;
            else if (id == "UnderNPCs")
                layer = SLPRenderLayer.UnderNPCs;
            else if (id == "UnderProjectiles")
                layer = SLPRenderLayer.UnderProjectiles;
            else if (id == "OverPlayers")
                layer = SLPRenderLayer.OverPlayers;
            else
                layer = SLPRenderLayer.Dusts;



            SLPPixelationTarget target = pixelationTargets.Find(t => t.renderType == layer);

            target.pixelationDrawActions.Add(new Tuple<Action, int>(renderAction, order));
            target.renderTimer = 2;
        }

        public void QueueRenderAction(SLPRenderLayer renderType, Action renderAction, int order = 0)
        {

            SLPPixelationTarget target = pixelationTargets.Find(t => t.renderType == renderType);

            target.pixelationDrawActions.Add(new Tuple<Action, int>(renderAction, order));
            target.renderTimer = 2;
        }
    }

    public class SLPPixelationTarget
    {
        public int renderTimer;

        // list of actions, and their draw order. Default order is zero, but actions with an order of 1 are drawn over 0, etc.
        public List<Tuple<Action, int>> pixelationDrawActions;

        public SLPScreenTarget pixelationTarget;

        public SLPScreenTarget pixelationTarget2;

        public SLPRenderLayer renderType;

        public bool Active => renderTimer > 0;

        public SLPPixelationTarget(SLPRenderLayer renderType)
        {
            pixelationDrawActions = new List<Tuple<Action, int>>();

            pixelationTarget = new(DrawPixelTarget, () => Active, 1f);
            pixelationTarget2 = new(DrawPixelTarget2, () => Active, 1.1f);


            this.renderType = renderType;
        }

        //Draw RenderTarget at half scale
        private void DrawPixelTarget2(SpriteBatch sb)
        {
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            sb.End();
            sb.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, null);

            sb.Draw(pixelationTarget.RenderTarget, Vector2.Zero, null, Color.White, 0, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);

            sb.End();
            sb.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawPixelTarget(SpriteBatch sb)
        {
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            sb.End();
            sb.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, null);

            foreach (Tuple<Action, int> tuple in pixelationDrawActions.OrderBy(t => t.Item2))
            {
                tuple.Item1.Invoke();
            }

            pixelationDrawActions.Clear();
            renderTimer--;

            sb.End();
            sb.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

    }

    public enum SLPRenderLayer : int
    {
        UnderTiles = 1,
        UnderNPCs = 2,
        UnderProjectiles = 3,
        OverPlayers = 4,
        Dusts = 6,
    }
}
