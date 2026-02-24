using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon;

namespace MagnumOpus.Content.MoonlightSonata.Projectiles
{
    /// <summary>
    /// Spinning mana crystal star projectile fired by the Eternal Moon sword (Phase 2 only).
    /// Homes toward enemies with CalamityStyleTrailRenderer.Ice trail,
    /// counter-rotating double flares, and 4-layer bloom body.
    /// </summary>
    public class EternalMoonBeam : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.ManaCrystal;

        private float SpinRotation
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.scale = 1.2f;
        }

        public override void AI()
        {
            // Spin the crystal
            SpinRotation += 0.25f;
            Projectile.rotation = SpinRotation;

            // Dynamic moonlight lighting
            EternalMoonVFX.AddCrescentLight(Projectile.Center, 0.6f);

            // Slight homing toward nearby enemies
            float homingRange = 300f;
            float homingStrength = 0.03f;
            NPC closestNPC = null;
            float closestDist = homingRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestNPC = npc;
                    }
                }
            }

            if (closestNPC != null)
            {
                Vector2 toTarget = closestNPC.Center - Projectile.Center;
                toTarget.Normalize();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity,
                    toTarget * Projectile.velocity.Length(), homingStrength);
            }

            // Music notes periodically
            if (Main.rand.NextBool(8))
            {
                MoonlightVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 12f, 0.7f, 0.9f, 30);
            }

            // Phase-cycling dust trail
            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Main.rand.NextVector2Circular(8f, 8f);
                Color trailColor = EternalMoonVFX.GetLunarPhaseColor(Main.rand.NextFloat(), 0);
                Dust d = Dust.NewDustPerfect(Projectile.Center + offset, DustID.PurpleTorch,
                    -Projectile.velocity * 0.1f, 0, trailColor, 1.3f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = Main.GlobalTimeWrappedHourly;

            // === CALAMITY-STYLE TRAIL (Ice style — crystalline shimmer) ===
            if (Projectile.oldPos.Length > 1)
            {
                Vector2[] trailPos = new Vector2[Projectile.oldPos.Length];
                float[] trailRot = new float[Projectile.oldPos.Length];
                int validCount = 0;

                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) break;
                    trailPos[i] = Projectile.oldPos[i] + Projectile.Size / 2f;
                    trailRot[i] = Projectile.oldRot[i];
                    validCount++;
                }

                if (validCount > 1)
                {
                    if (validCount < trailPos.Length)
                    {
                        Array.Resize(ref trailPos, validCount);
                        Array.Resize(ref trailRot, validCount);
                    }

                    CalamityStyleTrailRenderer.DrawTrailWithBloom(
                        trailPos, trailRot,
                        CalamityStyleTrailRenderer.TrailStyle.Ice,
                        baseWidth: 10f,
                        primaryColor: MoonlightVFXLibrary.IceBlue,
                        secondaryColor: MoonlightVFXLibrary.Lavender,
                        intensity: 0.7f,
                        bloomMultiplier: 1.8f);
                }
            }

            // === COUNTER-ROTATING DOUBLE FLARES ===
            MoonlightVFXLibrary.DrawCounterRotatingFlares(sb, Projectile.Center, 0.4f, time, 0.7f);

            // === 4-LAYER BLOOM STACK BODY ({A=0} pattern) ===
            var bloomTex = MagnumTextureRegistry.GetBloom();
            if (bloomTex != null)
            {
                Vector2 bloomOrigin = bloomTex.Size() * 0.5f;
                float pulse = 1f + MathF.Sin(time * 6f) * 0.1f;
                float bloomScale = 0.3f * pulse;

                // Layer 1: Outer DarkPurple halo
                sb.Draw(bloomTex, drawPos, null,
                    (MoonlightVFXLibrary.DarkPurple with { A = 0 }) * 0.25f,
                    0f, bloomOrigin, bloomScale * 2.2f, SpriteEffects.None, 0f);

                // Layer 2: Violet mid glow
                sb.Draw(bloomTex, drawPos, null,
                    (MoonlightVFXLibrary.Violet with { A = 0 }) * 0.4f,
                    0f, bloomOrigin, bloomScale * 1.5f, SpriteEffects.None, 0f);

                // Layer 3: IceBlue inner
                sb.Draw(bloomTex, drawPos, null,
                    (MoonlightVFXLibrary.IceBlue with { A = 0 }) * 0.6f,
                    0f, bloomOrigin, bloomScale * 0.9f, SpriteEffects.None, 0f);

                // Layer 4: White core
                sb.Draw(bloomTex, drawPos, null,
                    (Color.White with { A = 0 }) * 0.75f,
                    0f, bloomOrigin, bloomScale * 0.35f, SpriteEffects.None, 0f);
            }

            // === MAIN SPRITE (tinted crystal) ===
            Color mainColor = Color.Lerp(MoonlightVFXLibrary.Lavender, Color.White, 0.3f);
            mainColor.A = 200;
            sb.Draw(texture, drawPos, null, mainColor,
                Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

            // White-hot core overlay
            sb.Draw(texture, drawPos, null,
                (Color.White with { A = 0 }) * 0.5f,
                Projectile.rotation, origin, Projectile.scale * 0.5f, SpriteEffects.None, 0f);

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 300);

            // EternalMoon-themed impact — crescent arcs + halo cascade + music notes
            EternalMoonVFX.OnHitImpact(target.Center, 0, hit.Crit);
        }

        public override void OnKill(int timeLeft)
        {
            // Death: bloom flash + music note scatter
            MoonlightVFXLibrary.ProjectileImpact(Projectile.Center, 0.6f);
        }
    }
}
