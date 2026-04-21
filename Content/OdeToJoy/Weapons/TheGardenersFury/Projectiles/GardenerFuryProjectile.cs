using System;
using MagnumOpus.Common.BaseClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.OdeToJoy.Systems;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Projectiles
{
    /// <summary>
    /// GardenerFury swing — Ode to Joy theme melee. Exoblade-architecture swing.
    /// Leaf green trail with scattered nature dust and sun gold highlights.
    /// </summary>
    public class GardenerFuryProjectile : ExobladeStyleSwing
    {
        protected override bool SupportsDash => false;

        protected override float BladeLength => 100f;
        protected override int BaseSwingFrames => 76;
        protected override float TextureDrawScale => 0.94f;
        protected override string GradientLUTPath => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/OdeToJoyGradientLUTandRAMP";
        protected override Color SlashPrimaryColor => OdeToJoyPalette.BudGreen;
        protected override Color SlashSecondaryColor => OdeToJoyPalette.MossShadow;
        protected override Color SlashAccentColor => OdeToJoyPalette.SunlightYellow;

        public override string Texture => "MagnumOpus/Content/OdeToJoy/Weapons/TheGardenersFury/TheGardenersFury";

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(OdeToJoyPalette.LeafGreen, OdeToJoyPalette.SunlightYellow, (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.5f
                ? Color.Lerp(OdeToJoyPalette.LeafGreen, OdeToJoyPalette.VerdantGreen, Main.rand.NextFloat())
                : Color.Lerp(OdeToJoyPalette.BudGreen, OdeToJoyPalette.SunlightYellow, Main.rand.NextFloat());
        }

        protected override void OnSwingFrame()
        {
            if (Progression > 0.3f && Progression < 0.85f && Main.rand.NextFloat() < 0.35f)
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.4f, 1f);
                Vector2 vel = SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction * Main.rand.NextFloat(0.3f, 1.2f)) * Main.rand.NextFloat(1f, 3f);
                vel.Y -= Main.rand.NextFloat(0.5f, 1.5f);
                Dust petal = Dust.NewDustPerfect(pos, DustID.GreenTorch, vel, 80, default, Main.rand.NextFloat(0.6f, 1f));
                petal.noGravity = true;
                petal.fadeIn = 0.8f;
            }
            if (Progression > 0.4f && Main.rand.NextFloat() < 0.15f)
            {
                Vector2 tip = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                Dust spark = Dust.NewDustPerfect(tip, DustID.Enchanted_Gold, Main.rand.NextVector2Circular(1f, 1f), 0, default, 0.5f);
                spark.noGravity = true;
            }
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust petal = Dust.NewDustPerfect(target.Center, DustID.GreenTorch,
                    Main.rand.NextVector2CircularEdge(4f, 4f), 60, default, Main.rand.NextFloat(0.8f, 1.2f));
                petal.noGravity = true;
                petal.fadeIn = 1f;
            }
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = (target.Center - Owner.Center).SafeNormalize(Vector2.UnitX).RotatedByRandom(0.5f) * Main.rand.NextFloat(3f, 6f);
                Dust spark = Dust.NewDustPerfect(target.Center, DustID.Enchanted_Gold, vel, 0, default, 0.6f);
                spark.noGravity = true;
            }
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 12; i++)
            {
                Dust petal = Dust.NewDustPerfect(target.Center, DustID.GreenTorch,
                    Main.rand.NextVector2CircularEdge(6f, 6f), 40, default, Main.rand.NextFloat(1f, 1.5f));
                petal.noGravity = true;
            }
        }
    }

    /// <summary>
    /// Gardener's Fury seed projectile — planted orb that becomes a zone.
    /// Falls with gravity, lands on tiles, creates a 3s zone.
    /// After 1.5s, fires 1 homing child upward. If enemy dies, child is 2x scale.
    /// </summary>
    public class GardenerFurySeedProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private int _zoneTimer = 0;
        private bool _isZone = false;
        private bool _childFired = false;

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300; // 5 seconds max
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            if (!_isZone)
            {
                // Falling phase: gravity 0.20f
                Projectile.velocity.Y += 0.20f;
                Projectile.rotation += 0.1f;

                // Trail VFX
                if (Main.rand.NextBool(4))
                {
                    Color col = Color.Lerp(OdeToJoyPalette.LeafGreen, OdeToJoyPalette.GoldenPollen, 0.4f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch,
                        -Projectile.velocity * 0.1f, 0, col, 0.6f);
                    d.noGravity = true;
                }
            }
            else
            {
                // Zone phase: stationary, count time
                Projectile.velocity = Vector2.Zero;
                _zoneTimer++;

                // Zone VFX: pulsing glow
                float pulse = 0.7f + 0.3f * MathF.Sin(_zoneTimer * 0.1f);
                OdeToJoyVFXLibrary.AddOdeToJoyLight(Projectile.Center, 0.5f * pulse);

                // Periodic music note spawn
                if (_zoneTimer % 15 == 0)
                {
                    OdeToJoyVFXLibrary.SpawnMusicNotes(Projectile.Center, 2, 10f, 0.5f, 0.7f, 18);
                }

                // After 1.5s (90 frames), fire child upward
                if (!_childFired && _zoneTimer >= 90)
                {
                    _childFired = true;
                    FireChild();
                }

                // Zone duration: 3s (180 frames)
                if (_zoneTimer >= 180)
                {
                    Projectile.Kill();
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // On tile contact, become a zone
            if (!_isZone)
            {
                _isZone = true;
                Projectile.velocity = Vector2.Zero;
                Projectile.tileCollide = false;

                // Zone creation VFX
                OdeToJoyVFXLibrary.SpawnPetalScatter(Projectile.Center, 4, 15f, 0.2f);
            }
            return false;
        }

        private void FireChild()
        {
            var source = Projectile.GetSource_FromThis();

            // Check if an enemy died over this zone recently
            float scaleMult = 1f;
            bool enemyDiedNearby = false;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active && Vector2.Distance(npc.Center, Projectile.Center) < 80f)
                {
                    enemyDiedNearby = true;
                    break;
                }
            }

            if (enemyDiedNearby)
                scaleMult = 2f;

            // Fire homing child upward
            GenericHomingOrbChild.SpawnChild(
                source,
                Projectile.Center, Vector2.UnitY * -10f,
                Projectile.damage, Projectile.knockBack, Projectile.owner,
                homingStrength: 0.08f,
                behaviorFlags: GenericHomingOrbChild.FLAG_ACCELERATE,
                themeIndex: GenericHomingOrbChild.THEME_ODETOJOY,
                scaleMult: scaleMult,
                timeLeft: 120);

            // Child spawn VFX
            OdeToJoyVFXLibrary.SpawnBloomBurst(Projectile.Center, 6, scaleMult);
        }

        public override void OnKill(int timeLeft)
        {
            OdeToJoyVFXLibrary.SpawnPetalScatter(Projectile.Center, 3, 20f, 0.15f);
        }
    }
}
