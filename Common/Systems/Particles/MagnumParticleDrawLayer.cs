using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.Particles
{
    /// <summary>
    /// ModSystem that integrates MagnumParticleHandler into the game loop.
    /// Uses IL hooks like Calamity does for proper rendering order.
    /// 
    /// This system enables the Calamity-style particle classes like:
    /// - CircularSmearSmokeyVFX
    /// - HeavySmokeParticle
    /// - SemiCircularSmearVFX
    /// - And all other Particle subclasses
    /// </summary>
    public class MagnumParticleDrawLayer : ModSystem
    {
        public override void Load()
        {
            // Initialize the particle handler
            MagnumParticleHandler.Load();
            
            // Hook into the ACTUAL draw events like Calamity does
            // This is the key difference - we need IL hooks, not ModSystem hooks
            On_Main.DrawDust += DrawParticlesAfterDust;
        }

        public override void PostSetupContent()
        {
            // Load all particle type instances after content is set up
            MagnumParticleHandler.LoadModParticleInstances();
        }

        public override void Unload()
        {
            // Unhook
            On_Main.DrawDust -= DrawParticlesAfterDust;
            
            // Clean up the particle handler
            MagnumParticleHandler.Unload();
        }

        public override void PostUpdateDusts()
        {
            // Update all particles each frame (same timing as dust)
            MagnumParticleHandler.Update();
        }

        /// <summary>
        /// Hook that draws particles AFTER dust is drawn.
        /// This is the same approach Calamity uses via GeneralDrawLayerSystem.
        /// </summary>
        private void DrawParticlesAfterDust(On_Main.orig_DrawDust orig, Main self)
        {
            // First, let vanilla draw its dust
            orig(self);
            
            // Now draw our particles
            if (Main.dedServ) return;
            if (MagnumParticleHandler.ActiveParticleCount == 0) return;

            try
            {
                // Call DrawAllParticlesStandalone which handles its own spritebatch state
                MagnumParticleHandler.DrawAllParticlesStandalone(Main.spriteBatch);
            }
            catch (System.Exception ex)
            {
                // Log any errors for debugging
                Mod?.Logger?.Warn($"Particle draw error: {ex.Message}");
            }
        }
    }
}
