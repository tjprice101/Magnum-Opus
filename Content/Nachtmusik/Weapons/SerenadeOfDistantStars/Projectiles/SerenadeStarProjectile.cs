using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Nachtmusik.Systems;
using MagnumOpus.Content.Nachtmusik.Debuffs;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Nachtmusik.Weapons.SerenadeOfDistantStars.Projectiles
{
    /// <summary>
    /// Serenade tracer bolt — the second projectile fired alongside the homing orb.
    /// A thin, gold-blue comet needle that registers hits to build Rhythm stacks.
    /// Visual scales with rhythm stacks: brighter/larger at higher stack counts.
    /// </summary>
    public class SerenadeStarProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/ThinTall4PointedStar";

        private static Asset<Texture2D> _bloomTex;
        private static Asset<Texture2D> _starTex;
        private VertexStrip _strip;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Fetch current rhythm stacks for visual scaling
            int stacks = 0;
            if (Main.myPlayer == Projectile.owner)
                stacks = Main.player[Projectile.owner].GetModPlayer<NachtmusikCombatPlayer>().SerenadeRhythmStacks;

            float stackFraction = stacks / 5f;

            // Sparser trail than ConstellationBolt — this is the secondary projectile
            if (Main.rand.NextBool(4))
            {
                Color dustCol = stackFraction > 0.6f
                    ? Color.Lerp(NachtmusikPalette.RadianceGold, NachtmusikPalette.StarlightCore, stackFraction)
                    : Color.Lerp(NachtmusikPalette.StarlitBlue, NachtmusikPalette.RadianceGold, stackFraction);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(0.3f, 0.3f),
                    0, dustCol, Main.rand.NextFloat(0.3f, 0.65f));
                d.noGravity = true;
                d.fadeIn = 0.2f;
            }

            float lightMult = 0.2f + stackFraction * 0.25f;
            Lighting.AddLight(Projectile.Center, NachtmusikPalette.RadianceGold.ToVector3() * lightMult);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.Nachtmusik, ref _strip);

            // Draw the star needle sprite on top as a glowing overlay
            _starTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/ThinTall4PointedStar", AssetRequestMode.ImmediateLoad);
            if (_starTex?.Value != null)
            {
                int stacks = 0;
                if (Main.myPlayer == Projectile.owner)
                    stacks = Main.player[Projectile.owner].GetModPlayer<NachtmusikCombatPlayer>().SerenadeRhythmStacks;
                float stackFraction = stacks / 5f;
                float headScale = (0.35f + stackFraction * 0.2f) * Projectile.scale;

                SpriteBatch sb = Main.spriteBatch;
                try
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                        Main.DefaultSamplerState, DepthStencilState.None,
                        RasterizerState.CullCounterClockwise, null,
                        Main.GameViewMatrix.TransformationMatrix);

                    sb.Draw(_starTex.Value,
                        Projectile.Center - Main.screenPosition,
                        null,
                        (NachtmusikPalette.TwinklingWhite with { A = 0 }) * 0.95f,
                        Projectile.rotation,
                        _starTex.Value.Size() / 2f,
                        headScale, SpriteEffects.None, 0f);
                }
                catch { }
                finally
                {
                    try { sb.End(); } catch { }
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            var combat = owner.GetModPlayer<NachtmusikCombatPlayer>();
            combat.IncrementRhythm();

            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 600);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 1);

            // Hit burst — grows with rhythm stacks
            int stacks = combat.SerenadeRhythmStacks;
            int burstCount = 6 + stacks * 2;
            for (int i = 0; i < burstCount; i++)
            {
                float angle = MathHelper.TwoPi * i / burstCount;
                float speed = Main.rand.NextFloat(2f, 4.5f);
                Color col = i % 2 == 0 ? NachtmusikPalette.RadianceGold : NachtmusikPalette.StarlitBlue;
                Dust d = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch,
                    new Vector2(MathF.Cos(angle) * speed, MathF.Sin(angle) * speed), 0, col, 0.75f);
                d.noGravity = true;
                d.fadeIn = 0.35f;
            }

            // At stack 4+: spawn a child orb from impact
            if (stacks >= 4)
            {
                Vector2 childVel = (target.Center - owner.Center).SafeNormalize(Vector2.UnitX) * 10f;
                GenericHomingOrbChild.SpawnChild(
                    Projectile.GetSource_FromThis(), target.Center, childVel,
                    Projectile.damage / 2, Projectile.knockBack, Projectile.owner,
                    0.08f, 0, GenericHomingOrbChild.THEME_NACHTMUSIK,
                    0.7f, 60);
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Color col = i % 2 == 0 ? NachtmusikPalette.RadianceGold : NachtmusikPalette.CosmicPurple;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2CircularEdge(2.5f, 2.5f), 0, col, 0.5f);
                d.noGravity = true;
            }
        }
    }
}
