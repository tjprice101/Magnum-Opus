using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.Debuffs;
using MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Particles;
using MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Primitives;
using ReLogic.Content;

namespace MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Projectiles
{
    /// <summary>
    /// FermataSlashWave — Short-lived slash projectile created during the
    /// synchronized 90-frame slash attack. Travels fast in a straight line,
    /// deals damage, and leaves a crimson-gold trail.
    /// </summary>
    public class FermataSlashWave : ModProjectile
    {
        // Use the item texture (staff) as a stand-in for the slash wave
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/TheFinalFermata";

        private FermataTrailRenderer _trail;
        private int _frameCounter;

        // ─── Bloom Textures (Foundation-tier) ─────────────────────
        private static Asset<Texture2D> _pointBloomTex;
        private static Asset<Texture2D> _softRadialBloomTex;
        private static Asset<Texture2D> _starFlareTex;

        /// <summary>Additive-friendly color with premultiplied alpha and zero alpha channel.</summary>
        private static Color Additive(Color c, float opacity)
            => new Color((int)(c.R * opacity), (int)(c.G * opacity), (int)(c.B * opacity), 0);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 45; // Short-lived
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.alpha = 80;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            _frameCounter++;

            // Initialize trail
            if (_trail == null)
            {
                _trail = new FermataTrailRenderer(FermataTrailSettings.SlashTrail());
                _trail.Reset(Projectile.Center);
            }

            // Record trail
            _trail.RecordPosition(Projectile.Center, Projectile.rotation);

            // Rotation follows velocity
            if (Projectile.velocity != Vector2.Zero)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Slight homing toward nearest enemy
            NPC target = FindNearestEnemy(400f);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center);
                if (toTarget != Vector2.Zero)
                {
                    toTarget.Normalize();
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), 0.04f);
                }
            }

            // Fade out near end of life
            if (Projectile.timeLeft < 15)
            {
                Projectile.alpha = (int)MathHelper.Lerp(80, 255, 1f - Projectile.timeLeft / 15f);
            }

            // === VFX ===
            if (!Main.dedServ)
            {
                // Trailing sparks
                if (_frameCounter % 2 == 0)
                {
                    Vector2 backDir = -Projectile.velocity.SafeNormalize(Vector2.UnitX);
                    FermataParticleTypes.SpawnSpark(
                        Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                        backDir * Main.rand.NextFloat(1f, 3f),
                        Color.Lerp(FermataUtils.FermataCrimson, FermataUtils.TimeGold, Main.rand.NextFloat()),
                        0.14f, 10);
                }

                // Nebula wisps
                if (_frameCounter % 3 == 0)
                {
                    FermataParticleTypes.SpawnNebulaWisp(
                        Projectile.Center,
                        -Projectile.velocity * 0.05f,
                        FermataUtils.PaletteLerp(Main.rand.NextFloat(0.2f, 0.6f)) * 0.6f,
                        0.18f, 20);
                }

                // Vanilla dust
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                    DustID.PurpleTorch,
                    -Projectile.velocity * 0.1f, 0,
                    FermataUtils.FermataCrimson * 0.7f, 1f);
                d.noGravity = true;

                Lighting.AddLight(Projectile.Center,
                    FermataUtils.FermataCrimson.ToVector3() * 0.35f);
            }
        }

        private NPC FindNearestEnemy(float maxDist)
        {
            NPC closest = null;
            float closestDist = maxDist;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);

            if (Main.dedServ) return;

            Vector2 hitPos = target.Center;

            // ═══ MULTI-LAYER SPRITEBATCH BLOOM FLASH ═══
            try
            {
                _pointBloomTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
                _softRadialBloomTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
                _starFlareTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare");

                SpriteBatch sb = Main.spriteBatch;
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Vector2 screenPos = hitPos - Main.screenPosition;
                float time = (float)Main.timeForVisualEffects;

                // Layer 1: Crimson outer haze
                if (_softRadialBloomTex?.IsLoaded == true)
                {
                    var radTex = _softRadialBloomTex.Value;
                    sb.Draw(radTex, screenPos, null,
                        Additive(FermataUtils.FermataCrimson, 0.4f),
                        0f, radTex.Size() * 0.5f, 1.5f, SpriteEffects.None, 0f);
                }

                // Layer 2: Time gold mid glow
                if (_softRadialBloomTex?.IsLoaded == true)
                {
                    var radTex = _softRadialBloomTex.Value;
                    sb.Draw(radTex, screenPos, null,
                        Additive(FermataUtils.TimeGold, 0.45f),
                        0f, radTex.Size() * 0.5f, 1.0f, SpriteEffects.None, 0f);
                }

                // Layer 3: Purple inner
                if (_pointBloomTex?.IsLoaded == true)
                {
                    var ptTex = _pointBloomTex.Value;
                    sb.Draw(ptTex, screenPos, null,
                        Additive(FermataUtils.FermataPurple, 0.5f),
                        0f, ptTex.Size() * 0.5f, 0.65f, SpriteEffects.None, 0f);
                }

                // Layer 4: White-hot core
                if (_pointBloomTex?.IsLoaded == true)
                {
                    var ptTex = _pointBloomTex.Value;
                    sb.Draw(ptTex, screenPos, null,
                        Additive(FermataUtils.FlashWhite, 0.7f),
                        0f, ptTex.Size() * 0.5f, 0.35f, SpriteEffects.None, 0f);
                }

                // Layer 5: StarFlare cross — temporal slash flash
                if (_starFlareTex?.IsLoaded == true)
                {
                    var starTex = _starFlareTex.Value;
                    sb.Draw(starTex, screenPos, null,
                        Additive(FermataUtils.TimeGold, 0.4f),
                        time * 0.1f, starTex.Size() * 0.5f, 0.45f, SpriteEffects.None, 0f);
                    sb.Draw(starTex, screenPos, null,
                        Additive(FermataUtils.FermataCrimson, 0.3f),
                        -time * 0.07f, starTex.Size() * 0.5f, 0.3f, SpriteEffects.None, 0f);
                }

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone,
                    null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch
            {
                try
                {
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                        Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone,
                        null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch { }
            }

            // ═══ ENHANCED PARTICLE IMPACT ═══
            FermataParticleTypes.SyncSlashImpact(hitPos);

            // 12 radial crimson-gold sparks
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f + Main.rand.NextFloat(-0.15f, 0.15f);
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color sparkCol = FermataUtils.PaletteLerp((float)i / 12f);
                FermataParticleTypes.SpawnSpark(hitPos, sparkVel, sparkCol * 0.8f, 0.14f, 14);
            }

            // 6 directional slash marks
            Vector2 slashDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 slashPerp = new Vector2(-slashDir.Y, slashDir.X);
            for (int i = 0; i < 6; i++)
            {
                float spread = (i - 2.5f) / 2.5f;
                Vector2 dirVel = (slashDir * 4f + slashPerp * spread * 5f) * Main.rand.NextFloat(0.8f, 1.2f);
                Color col = Color.Lerp(FermataUtils.FermataCrimson, FermataUtils.FlashWhite, MathF.Abs(spread));
                FermataParticleTypes.SpawnSpark(hitPos, dirVel, col * 0.7f, 0.1f, 12);
            }

            // Time shard burst with glyphs
            FermataParticleTypes.SpawnTimeShardBurst(hitPos, 6, 4f);
            FermataParticleTypes.SpawnGlyph(hitPos, FermataUtils.TimeGold * 0.6f, 0.3f, 24);

            // Dual lighting
            Lighting.AddLight(hitPos, FermataUtils.FermataCrimson.ToVector3() * 0.9f);
            Lighting.AddLight(hitPos + slashDir * 16f, FermataUtils.TimeGold.ToVector3() * 0.6f);

            SoundEngine.PlaySound(SoundID.Item60 with { Pitch = 0.4f, Volume = 0.5f }, hitPos);
        }

        public override void OnKill(int timeLeft)
        {
            // Terminal burst
            FermataParticleTypes.SpawnSparkBurst(Projectile.Center, 6, 3f, FermataUtils.FermataCrimson);
            FermataParticleTypes.SpawnBloomFlare(Projectile.Center, FermataUtils.TimeGold, 0.3f, 12);

            for (int i = 0; i < 3; i++)
            {
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(2f, 2f), 0,
                    FermataUtils.FermataCrimson * 0.5f, 1f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float alpha = 1f - Projectile.alpha / 255f;

            // Draw trail
            _trail?.Draw(sb, alpha);

            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.2f) * 0.08f;
            float breathe = 1f + MathF.Sin(time * 0.06f) * 0.05f;

            // ═══ FOUNDATION-TIER GRADUATED BLOOM ═══
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            _pointBloomTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            _softRadialBloomTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            _starFlareTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare");

            // Layer 1: Outer temporal void haze (SoftRadialBloom)
            if (_softRadialBloomTex?.IsLoaded == true)
            {
                var radTex = _softRadialBloomTex.Value;
                var radOrigin = radTex.Size() * 0.5f;
                sb.Draw(radTex, drawPos, null,
                    Additive(FermataUtils.FermataPurple, 0.2f * alpha),
                    0f, radOrigin, 1.6f * breathe, SpriteEffects.None, 0f);
            }

            // Layer 2: Crimson slash energy (SoftRadialBloom)
            if (_softRadialBloomTex?.IsLoaded == true)
            {
                var radTex = _softRadialBloomTex.Value;
                var radOrigin = radTex.Size() * 0.5f;
                sb.Draw(radTex, drawPos, null,
                    Additive(FermataUtils.FermataCrimson, 0.35f * alpha),
                    0f, radOrigin, 1.1f * pulse, SpriteEffects.None, 0f);
            }

            // Layer 3: Time gold inner glow (PointBloom)
            if (_pointBloomTex?.IsLoaded == true)
            {
                var ptTex = _pointBloomTex.Value;
                var ptOrigin = ptTex.Size() * 0.5f;
                sb.Draw(ptTex, drawPos, null,
                    Additive(FermataUtils.TimeGold, 0.4f * alpha),
                    0f, ptOrigin, 0.6f * pulse, SpriteEffects.None, 0f);
            }

            // Layer 4: Flash white core (PointBloom)
            if (_pointBloomTex?.IsLoaded == true)
            {
                var ptTex = _pointBloomTex.Value;
                var ptOrigin = ptTex.Size() * 0.5f;
                sb.Draw(ptTex, drawPos, null,
                    Additive(FermataUtils.FlashWhite, 0.5f * alpha),
                    0f, ptOrigin, 0.3f * pulse, SpriteEffects.None, 0f);
            }

            // Layer 5: StarFlare rotating cross — the fermata's temporal signature
            if (_starFlareTex?.IsLoaded == true)
            {
                var starTex = _starFlareTex.Value;
                var starOrigin = starTex.Size() * 0.5f;
                sb.Draw(starTex, drawPos, null,
                    Additive(FermataUtils.TimeGold, 0.2f * alpha),
                    time * 0.04f, starOrigin, 0.35f * pulse, SpriteEffects.None, 0f);
                sb.Draw(starTex, drawPos, null,
                    Additive(FermataUtils.FermataCrimson, 0.15f * alpha),
                    -time * 0.025f, starOrigin, 0.22f * pulse, SpriteEffects.None, 0f);
            }

            // Original 2-layer sprite glow (now enhanced as layers 6-7)
            sb.Draw(texture, drawPos, null,
                FermataUtils.FermataCrimson * 0.3f * alpha,
                Projectile.rotation, origin, 1.2f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, drawPos, null,
                FermataUtils.TimeGold * 0.2f * alpha,
                Projectile.rotation, origin, 1.1f * pulse, SpriteEffects.None, 0f);

            // ═══ LEADING-EDGE BLOOM ═══
            Vector2 leadDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 tipPos = drawPos + leadDir * 18f;
            float leadPulse = 1f + MathF.Sin(time * 0.15f) * 0.12f;

            if (_softRadialBloomTex?.IsLoaded == true)
            {
                var radTex = _softRadialBloomTex.Value;
                sb.Draw(radTex, tipPos, null,
                    Additive(FermataUtils.FermataCrimson, 0.3f * alpha),
                    Projectile.rotation, radTex.Size() * 0.5f, 0.5f * leadPulse, SpriteEffects.None, 0f);
            }
            if (_pointBloomTex?.IsLoaded == true)
            {
                var ptTex = _pointBloomTex.Value;
                sb.Draw(ptTex, tipPos, null,
                    Additive(FermataUtils.TimeGold, 0.4f * alpha),
                    0f, ptTex.Size() * 0.5f, 0.25f * leadPulse, SpriteEffects.None, 0f);
                sb.Draw(ptTex, tipPos, null,
                    Additive(FermataUtils.FlashWhite, 0.45f * alpha),
                    0f, ptTex.Size() * 0.5f, 0.12f * leadPulse, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Main sprite
            sb.Draw(texture, drawPos, null,
                Color.White * alpha * 0.85f,
                Projectile.rotation, origin, 0.7f, SpriteEffects.None, 0f);

            }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            return false;
        }
    }
}
