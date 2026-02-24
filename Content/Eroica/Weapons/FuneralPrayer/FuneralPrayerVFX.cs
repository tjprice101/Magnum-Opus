using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Screen;
using MagnumOpus.Common.Systems.VFX.Trails;

namespace MagnumOpus.Content.Eroica.Weapons.FuneralPrayer
{
    /// <summary>
    /// Static VFX helper for Funeral Prayer — a post-Moon Lord magic staff that fires
    /// 5 beam projectiles. When all 5 beams connect, a divine ricochet beam is born.
    /// Every effect is somber, heavy, and ritualistic — a requiem in particle form.
    ///
    /// The visual language is that of a funeral procession: mourning flames, incense
    /// smoke, tarnished gold remembrance, and the pale glow of departing spirits.
    /// Heroism lives here, but it is tempered by grief.
    ///
    /// All shared VFX routed through EroicaVFXLibrary for canonical palette.
    /// Blade gradients use EroicaPalette.FuneralPrayerBlade.
    /// </summary>
    public static class FuneralPrayerVFX
    {
        // ══════════════════════════════════════════════════════════════
        //  UNIQUE IDENTITY COLORS  (funeral-specific accents)
        // ══════════════════════════════════════════════════════════════

        /// <summary>Deepest mourning shadow — the void beneath the pyre.</summary>
        private static readonly Color FuneralShadow = new Color(25, 8, 12);

        /// <summary>Somber dirge red — dried blood on a battlefield hymn.</summary>
        private static readonly Color DirgeRed = new Color(140, 25, 30);

        /// <summary>Tarnished gold of remembrance — not triumphant, but memorial.</summary>
        private static readonly Color RequiemGold = new Color(200, 160, 60);

        /// <summary>Spirit's pale glow — the last light of the departed.</summary>
        private static readonly Color SpiritWhite = new Color(240, 230, 220);

        /// <summary>Rising incense — smoke from the funeral altar.</summary>
        private static readonly Color IncenseSmoke = new Color(80, 60, 70);

        /// <summary>Funeral pyre embers — the fire that carries prayers skyward.</summary>
        private static readonly Color MourningFlame = new Color(180, 50, 40);

        /// <summary>Eulogy's warm light — words spoken over the fallen, golden and fading.</summary>
        private static readonly Color EulogyGold = new Color(220, 180, 80);

        /// <summary>Cold ash — what remains when the pyre has spoken.</summary>
        private static readonly Color AshGray = new Color(100, 90, 95);

        // ══════════════════════════════════════════════════════════════
        //  FUNERAL GRADIENT HELPER
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Funeral trail color gradient: DirgeRed at head, RequiemGold at tail.
        /// Used for cascading halo rings and beam trail coloring.
        /// </summary>
        private static Color FuneralTrailColor(float progress)
        {
            return Color.Lerp(DirgeRed, RequiemGold, progress);
        }

        // ══════════════════════════════════════════════════════════════
        //  1. CAST VFX  (called from Shoot — prayer formation)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Prayer formation VFX on staff cast: a 5-pointed star of converging funeral
        /// flames, cascading halo rings in dirge-to-gold gradient, bloom flare, somber
        /// music notes, ascending incense smoke, and screen trauma.
        /// The moment the staff is raised, the prayer begins.
        /// </summary>
        public static void CastVFX(Vector2 castPos, Vector2 direction, Player player)
        {
            // Five-pointed star of converging funeral flames
            for (int star = 0; star < 5; star++)
            {
                float starAngle = MathHelper.TwoPi * star / 5f - MathHelper.PiOver2;
                Vector2 starPoint = castPos + starAngle.ToRotationVector2() * 80f;

                for (int p = 0; p < 6; p++)
                {
                    float lerp = p / 6f;
                    Vector2 linePos = Vector2.Lerp(starPoint, castPos, lerp);
                    Color lineColor = FuneralTrailColor(lerp);
                    Dust d = Dust.NewDustPerfect(linePos, DustID.Torch, Vector2.Zero, 0, lineColor, 1.2f);
                    d.noGravity = true;
                }
            }

            // Cascading halo rings — mourning gradient
            for (int i = 0; i < 4; i++)
            {
                float progress = i / 4f;
                Color ringCol = FuneralTrailColor(progress);
                float scale = 0.3f + i * 0.1f;
                var ring = new BloomRingParticle(castPos, Vector2.Zero, ringCol, scale, 28, 0.07f);
                MagnumParticleHandler.SpawnParticle(ring);
            }

            // Bloom flare — funeral pyre ignition
            EroicaVFXLibrary.BloomFlare(castPos, MourningFlame, 0.7f, 20);

            // Somber music notes — the prayer's opening bars
            EroicaVFXLibrary.SpawnMusicNotes(castPos, 4, 30f, 0.7f, 0.95f, 35);

            // Ascending incense smoke
            for (int smoke = 0; smoke < 6; smoke++)
            {
                Vector2 smokePos = castPos + Main.rand.NextVector2Circular(20f, 10f);
                Vector2 smokeVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -2f - Main.rand.NextFloat(1f));
                var smokeParticle = new HeavySmokeParticle(smokePos, smokeVel,
                    IncenseSmoke, Main.rand.Next(35, 55), 0.3f, 0.5f, 0.012f, false);
                MagnumParticleHandler.SpawnParticle(smokeParticle);
            }

            // Screen trauma — the weight of the prayer
            MagnumScreenEffects.AddScreenShake(3f);

            Lighting.AddLight(castPos, MourningFlame.ToVector3() * 1.0f);
        }

        // ══════════════════════════════════════════════════════════════
        //  2. BEAM TRAIL VFX  (per-frame, on-hit, on-death)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Per-frame beam trail: funeral flame dust (DirgeRed to RequiemGold),
        /// mourning smoke wisps rising from the beam path, subtle music notes,
        /// and dynamic lighting with MourningFlame undertone.
        /// Each beam is a verse in the funeral hymn.
        /// </summary>
        public static void BeamTrailFrame(Projectile proj)
        {
            // Funeral flame dust trail — DirgeRed to RequiemGold
            if (Main.rand.NextBool(2))
            {
                Vector2 vel = -proj.velocity * 0.15f + Main.rand.NextVector2Circular(0.6f, 0.6f);
                Color col = Color.Lerp(DirgeRed, RequiemGold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(proj.Center, DustID.Torch, vel, 0, col, 1.4f);
                d.noGravity = true;
            }

            // Mourning smoke wisps — rise upward from beam path
            if (Main.rand.NextBool(3))
            {
                Vector2 smokePos = proj.Center + Main.rand.NextVector2Circular(6f, 6f);
                Vector2 smokeVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -1f - Main.rand.NextFloat(0.5f));
                Dust smoke = Dust.NewDustPerfect(smokePos, DustID.Smoke, smokeVel, 150, AshGray, 1.1f);
                smoke.noGravity = true;
            }

            // Subtle music notes — the hymn carried on the beam (1/6 chance)
            if (Main.rand.NextBool(6))
            {
                EroicaVFXLibrary.SpawnMusicNotes(proj.Center, 1, 8f, 0.6f, 0.85f, 28);
            }

            // Dynamic lighting — mourning flame warmth
            Lighting.AddLight(proj.Center, MourningFlame.ToVector3() * 0.5f);
        }

        /// <summary>
        /// On-NPC-hit: somber impact with requiem gold halo, funeral dust burst,
        /// and a small bloom. Each hit is a toll of the bell.
        /// </summary>
        public static void BeamHitVFX(Vector2 hitPos)
        {
            // Requiem gold halo
            var halo = new BloomRingParticle(hitPos, Vector2.Zero, RequiemGold * 0.7f, 0.35f, 20, 0.05f);
            MagnumParticleHandler.SpawnParticle(halo);

            // Funeral dust burst — radial scatter
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color col = Color.Lerp(DirgeRed, RequiemGold, (float)i / 8f);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.Torch, vel, 0, col, 1.2f);
                d.noGravity = true;
            }

            // Small bloom — the bell's resonance
            EroicaVFXLibrary.DrawBloom(hitPos, 0.3f);

            Lighting.AddLight(hitPos, RequiemGold.ToVector3() * 0.7f);
        }

        /// <summary>
        /// Beam expires: fading ember scatter, ash dust, and a small flare.
        /// The verse ends — embers scatter like dying words.
        /// </summary>
        public static void BeamDeathVFX(Vector2 pos)
        {
            // Fading ember scatter
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f) + new Vector2(0, -0.5f);
                Color col = Color.Lerp(MourningFlame, AshGray, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, Main.rand.NextFloat(0.8f, 1.3f));
                d.noGravity = true;
            }

            // Ash dust — what remains
            for (int i = 0; i < 4; i++)
            {
                Dust ash = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.Smoke, Main.rand.NextVector2Circular(1f, 1f), 120, AshGray, 0.9f);
                ash.noGravity = true;
            }

            // Small flare — last ember
            CustomParticles.EroicaFlare(pos, 0.25f);

            Lighting.AddLight(pos, MourningFlame.ToVector3() * 0.4f);
        }

        // ══════════════════════════════════════════════════════════════
        //  3. PRAYER ANSWERED VFX  (when all 5 beams hit)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// The prayer is answered: MASSIVE divine golden flash, five-pointed star
        /// of funeral flames converging from the mourning field, cascading gradient
        /// halos, music note burst, rising prayer smoke, sakura petal spiral,
        /// god rays, screen distortion, and screen shake.
        /// This is the climax — the moment grief becomes transcendence.
        /// </summary>
        public static void PrayerAnsweredVFX(Vector2 center)
        {
            // MASSIVE divine golden flash — 3-layer ascending brilliance
            EroicaVFXLibrary.BloomFlare(center, SpiritWhite, 1.6f, 28);
            EroicaVFXLibrary.BloomFlare(center, EulogyGold, 1.3f, 32);
            EroicaVFXLibrary.BloomFlare(center, RequiemGold, 1.0f, 35);

            // Five-pointed star of funeral flames converging
            for (int star = 0; star < 5; star++)
            {
                float starAngle = MathHelper.TwoPi * star / 5f - MathHelper.PiOver2;
                Vector2 starPoint = center + starAngle.ToRotationVector2() * 120f;

                for (int p = 0; p < 8; p++)
                {
                    float lerp = p / 8f;
                    Vector2 linePos = Vector2.Lerp(starPoint, center, lerp);
                    Color lineColor = Color.Lerp(DirgeRed, RequiemGold, lerp);
                    Dust d = Dust.NewDustPerfect(linePos, DustID.Torch, Vector2.Zero, 0, lineColor, 1.5f);
                    d.noGravity = true;
                }

                // Flare at each star point
                EroicaVFXLibrary.BloomFlare(starPoint, EulogyGold, 0.6f, 22);
            }

            // Cascading gradient halo rings — 6 rings, DirgeRed to RequiemGold
            for (int i = 0; i < 6; i++)
            {
                float progress = i / 6f;
                Color ringColor = FuneralTrailColor(progress);
                float ringScale = 0.4f + i * 0.15f;
                var ring = new BloomRingParticle(center, Vector2.Zero, ringColor, ringScale, 30 + i * 3, 0.09f);
                MagnumParticleHandler.SpawnParticle(ring);
            }

            // Music note burst — 12 radial notes + 16 scattered
            EroicaVFXLibrary.MusicNoteBurst(center, RequiemGold, 12, 6f);
            EroicaVFXLibrary.SpawnMusicNotes(center, 16, 70f, 0.8f, 1.2f, 40);

            // Rising prayer smoke — 12 incense columns
            for (int smoke = 0; smoke < 12; smoke++)
            {
                float smokeAngle = MathHelper.TwoPi * smoke / 12f;
                Vector2 smokePos = center + smokeAngle.ToRotationVector2() * 45f;
                Vector2 smokeVel = new Vector2(0, -3.5f) + Main.rand.NextVector2Circular(1f, 0.5f);
                var smokeParticle = new HeavySmokeParticle(smokePos, smokeVel,
                    IncenseSmoke, Main.rand.Next(45, 65), 0.45f, 0.65f, 0.014f, false);
                MagnumParticleHandler.SpawnParticle(smokeParticle);
            }

            // Sakura petal spiral — memory of the fallen, petals ascending
            EroicaVFXLibrary.SpawnSakuraPetals(center, 20, 80f);

            // God rays — 8 rays of divine acknowledgment
            GodRaySystem.CreateBurst(center, DirgeRed, 8, 100f, 40,
                GodRaySystem.GodRayStyle.Explosion, RequiemGold);

            // Screen distortion — the world shudders at the prayer's answer
            ScreenDistortionManager.TriggerRipple(center, MourningFlame, 0.9f, 28);

            // Screen shake — heavy, deliberate
            MagnumScreenEffects.AddScreenShake(8f);

            Lighting.AddLight(center, SpiritWhite.ToVector3() * 2.0f);
        }

        // ══════════════════════════════════════════════════════════════
        //  4. RICOCHET BEAM VFX  (enhanced trail, hit, death)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Enhanced trail for the ricochet beam: denser funeral flame dust,
        /// mourning trail with RequiemGold accents, periodic music notes,
        /// and brighter lighting. The answered prayer burns hotter.
        /// </summary>
        public static void RicochetBeamTrailVFX(Projectile proj)
        {
            // Dense funeral flame dust — every frame, heavier than standard beams
            Vector2 vel = -proj.velocity * 0.18f + Main.rand.NextVector2Circular(0.8f, 0.8f);
            Color col = Color.Lerp(DirgeRed, EulogyGold, Main.rand.NextFloat());
            Dust d = Dust.NewDustPerfect(proj.Center, DustID.Torch, vel, 0, col, 1.6f);
            d.noGravity = true;

            // Secondary RequiemGold accent dust
            if (Main.rand.NextBool(2))
            {
                Vector2 accentVel = -proj.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
                Dust accent = Dust.NewDustPerfect(proj.Center + Main.rand.NextVector2Circular(4f, 4f),
                    DustID.GoldFlame, accentVel, 0, RequiemGold, 1.3f);
                accent.noGravity = true;
            }

            // Mourning smoke wisps — rising off the divine beam
            if (Main.rand.NextBool(3))
            {
                Vector2 smokePos = proj.Center + Main.rand.NextVector2Circular(8f, 8f);
                Dust smoke = Dust.NewDustPerfect(smokePos, DustID.Smoke,
                    new Vector2(0, -1.2f) + Main.rand.NextVector2Circular(0.4f, 0.3f),
                    140, IncenseSmoke, 1.0f);
                smoke.noGravity = true;
            }

            // Music notes — every 4th frame, the hymn resonates
            if (Main.GameUpdateCount % 4 == 0)
            {
                EroicaVFXLibrary.SpawnMusicNotes(proj.Center, 1, 10f, 0.75f, 1.0f, 30);
            }

            // Brighter lighting — the answered prayer illuminates
            Lighting.AddLight(proj.Center, EulogyGold.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Ricochet beam on-hit: scaled-up impact with requiem burst, halo cascade,
        /// and music notes. The divine beam strikes with the weight of five prayers.
        /// </summary>
        public static void RicochetBeamHitVFX(Vector2 pos)
        {
            // Heroic impact — scaled up for divine beam
            EroicaVFXLibrary.HeroicImpact(pos, 1.2f);

            // Requiem burst — radial dust ring
            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Color burstCol = Color.Lerp(DirgeRed, RequiemGold, (float)i / 14f);
                Dust bd = Dust.NewDustPerfect(pos, DustID.Torch, burstVel, 0, burstCol, 1.5f);
                bd.noGravity = true;
            }

            // Halo cascade — 3 expanding rings
            for (int h = 0; h < 3; h++)
            {
                Color haloCol = Color.Lerp(MourningFlame, RequiemGold, h / 3f);
                var halo = new BloomRingParticle(pos, Vector2.Zero, haloCol * 0.8f,
                    0.4f + h * 0.12f, 22 + h * 4, 0.06f);
                MagnumParticleHandler.SpawnParticle(halo);
            }

            // Music notes — the chord of impact
            EroicaVFXLibrary.SpawnMusicNotes(pos, 4, 25f, 0.8f, 1.1f, 32);

            Lighting.AddLight(pos, RequiemGold.ToVector3() * 1.2f);
        }

        /// <summary>
        /// Ricochet beam death: full DeathHeroicFlash, funeral finisher slam.
        /// The prayer's final word — complete, absolute.
        /// </summary>
        public static void RicochetBeamDeathVFX(Vector2 pos)
        {
            // Full death heroic flash — the prayer's conclusion
            EroicaVFXLibrary.DeathHeroicFlash(pos, 0.9f);

            // Funeral finisher slam — additional mourning-themed impact
            EroicaVFXLibrary.SpawnRadialDustBurst(pos, 18, 8f, DustID.Torch);

            // Mourning flame dust ring
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, MourningFlame, 1.4f);
                d.noGravity = true;
            }

            // Ash scatter — what remains after the prayer
            for (int i = 0; i < 8; i++)
            {
                Dust ash = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(12f, 12f),
                    DustID.Smoke, Main.rand.NextVector2Circular(2f, 2f), 130, AshGray, 1.1f);
                ash.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.2f, Volume = 0.6f }, pos);

            Lighting.AddLight(pos, SpiritWhite.ToVector3() * 1.5f);
        }

        // ══════════════════════════════════════════════════════════════
        //  5. PROJECTILE PREDRAW  (beam + ricochet beam rendering)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Somber beam rendering: {A=0} trail with FuneralShadow to MourningFlame gradient,
        /// afterimage with DirgeRed opacity fade, 3-layer bloom stack (MourningFlame outer,
        /// RequiemGold mid, SpiritWhite core), and pulsing main sprite.
        /// Returns false to suppress vanilla drawing.
        /// </summary>
        public static bool DrawBeamProjectile(SpriteBatch sb, Projectile proj, ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[proj.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 projScreen = proj.Center - Main.screenPosition;

            // {A=0} bloom trail — FuneralShadow to MourningFlame
            EroicaVFXLibrary.DrawProjectileTrail(sb, proj, MourningFlame);

            // Afterimage trail with DirgeRed opacity fade
            for (int k = 0; k < proj.oldPos.Length; k++)
            {
                if (proj.oldPos[k] == Vector2.Zero) continue;

                Vector2 drawPos = proj.oldPos[k] - Main.screenPosition + new Vector2(proj.width / 2f, proj.height / 2f);
                float progress = (proj.oldPos.Length - k) / (float)proj.oldPos.Length;
                Color trailColor = Color.Lerp(FuneralShadow, DirgeRed, progress);
                trailColor = (trailColor * progress) with { A = 0 };
                float scale = proj.scale * (0.5f + progress * 0.5f);

                sb.Draw(texture, drawPos, null, trailColor, proj.oldRot[k], drawOrigin, scale, SpriteEffects.None, 0f);
            }

            // 3-layer {A=0} bloom stack — mourning palette
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.1f + 1f;
            Color outerGlow = (MourningFlame with { A = 0 }) * 0.4f;
            Color midGlow = (RequiemGold with { A = 0 }) * 0.35f;
            Color innerGlow = (SpiritWhite with { A = 0 }) * 0.25f;

            sb.Draw(texture, projScreen, null, outerGlow, proj.rotation, drawOrigin,
                proj.scale * 1.25f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, projScreen, null, midGlow, proj.rotation, drawOrigin,
                proj.scale * 1.12f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, projScreen, null, innerGlow, proj.rotation, drawOrigin,
                proj.scale * 1.04f * pulse, SpriteEffects.None, 0f);

            // Main sprite — pulsing with mourning warmth
            Color mainColor = new Color(245, 230, 215, 210) * pulse;
            sb.Draw(texture, projScreen, null, mainColor, proj.rotation, drawOrigin, proj.scale, SpriteEffects.None, 0f);

            return false;
        }

        /// <summary>
        /// Enhanced ricochet beam rendering: brighter, wider, with RequiemGold to
        /// SpiritWhite gradient trail, bolder afterimage, larger bloom layers,
        /// and additional counter-rotating flares. The divine beam radiates.
        /// Returns false to suppress vanilla drawing.
        /// </summary>
        public static bool DrawRicochetBeam(SpriteBatch sb, Projectile proj, ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[proj.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 projScreen = proj.Center - Main.screenPosition;

            // {A=0} bloom trail — RequiemGold, brighter than standard
            EroicaVFXLibrary.DrawProjectileTrail(sb, proj, RequiemGold);

            // Enhanced afterimage — RequiemGold to SpiritWhite gradient
            for (int k = 0; k < proj.oldPos.Length; k++)
            {
                if (proj.oldPos[k] == Vector2.Zero) continue;

                Vector2 drawPos = proj.oldPos[k] - Main.screenPosition + new Vector2(proj.width / 2f, proj.height / 2f);
                float progress = (proj.oldPos.Length - k) / (float)proj.oldPos.Length;
                Color trailColor = Color.Lerp(RequiemGold, SpiritWhite, progress);
                trailColor = (trailColor * progress) with { A = 0 };
                float scale = proj.scale * (0.6f + progress * 0.5f);

                sb.Draw(texture, drawPos, null, trailColor, proj.oldRot[k], drawOrigin, scale, SpriteEffects.None, 0f);
            }

            // 3-layer {A=0} bloom stack — brighter, wider
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.1f + 1f;
            Color outerGlow = (MourningFlame with { A = 0 }) * 0.5f;
            Color midGlow = (RequiemGold with { A = 0 }) * 0.45f;
            Color innerGlow = (SpiritWhite with { A = 0 }) * 0.35f;

            sb.Draw(texture, projScreen, null, outerGlow, proj.rotation, drawOrigin,
                proj.scale * 1.35f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, projScreen, null, midGlow, proj.rotation, drawOrigin,
                proj.scale * 1.18f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, projScreen, null, innerGlow, proj.rotation, drawOrigin,
                proj.scale * 1.06f * pulse, SpriteEffects.None, 0f);

            // Counter-rotating flares — divine energy
            EroicaVFXLibrary.DrawCounterRotatingFlares(sb, proj.Center,
                proj.scale * 0.6f, Main.GlobalTimeWrappedHourly * 3f, 0.7f);

            // Main sprite — radiant with answered prayer
            Color mainColor = new Color(255, 240, 225, 200) * pulse;
            sb.Draw(texture, projScreen, null, mainColor, proj.rotation, drawOrigin, proj.scale, SpriteEffects.None, 0f);

            return false;
        }

        // ══════════════════════════════════════════════════════════════
        //  6. AMBIENT HOLD VFX  (called from HoldItem)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Mourning aura while holding the staff: slow orbiting funeral ember motes,
        /// rising incense wisps, occasional somber music notes, subtle sakura petals
        /// (memory of the fallen), and pulsing dirge-red light.
        /// The staff remembers those who were lost.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.gameMenu) return;

            // Slow orbiting funeral ember motes — embers orbit like mourners
            if (Main.rand.NextBool(12))
            {
                float angle = Main.GameUpdateCount * 0.02f;
                float radius = 30f + (float)Math.Sin(Main.GameUpdateCount * 0.035f) * 8f;
                Vector2 motePos = player.Center + angle.ToRotationVector2() * radius;
                Color moteColor = Color.Lerp(DirgeRed, MourningFlame, Main.rand.NextFloat());
                Dust mote = Dust.NewDustPerfect(motePos, DustID.Torch, Vector2.Zero, 0, moteColor, 1.0f);
                mote.noGravity = true;
            }

            // Rising incense wisps — altar smoke
            if (Main.rand.NextBool(18))
            {
                Vector2 smokePos = player.Center + Main.rand.NextVector2Circular(16f, 8f);
                Vector2 smokeVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.8f - Main.rand.NextFloat(0.4f));
                Dust incense = Dust.NewDustPerfect(smokePos, DustID.Smoke, smokeVel, 120, IncenseSmoke, 0.8f);
                incense.noGravity = true;
            }

            // Somber music notes — every 14 frames, the requiem hums
            if (Main.GameUpdateCount % 14 == 0)
            {
                EroicaVFXLibrary.SpawnMusicNotes(player.Center, 1, 20f, 0.6f, 0.85f, 30);
            }

            // Subtle sakura petals — memory of the fallen heroes
            if (Main.rand.NextBool(22))
            {
                EroicaVFXLibrary.SpawnSakuraPetals(
                    player.Center + Main.rand.NextVector2Circular(25f, 25f), 1, 15f);
            }

            // Pulsing dirge-red light — the staff's somber heartbeat
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.12f + 0.88f;
            Vector3 lightCol = Color.Lerp(DirgeRed, MourningFlame, 0.3f).ToVector3();
            Lighting.AddLight(player.Center, lightCol * pulse * 0.5f);
        }

        // ══════════════════════════════════════════════════════════════
        //  7. FUNERAL PROCESSION EFFECTS  (composable sub-effects)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Line of mourning dust particles that march in formation along a direction.
        /// Each particle steps forward like a pallbearer, evenly spaced,
        /// transitioning from DirgeRed at origin to RequiemGold at terminus.
        /// </summary>
        public static void FuneralProcessionDust(Vector2 pos, Vector2 direction)
        {
            Vector2 normalDir = direction.SafeNormalize(Vector2.UnitX);
            int count = 8;
            float spacing = 12f;

            for (int i = 0; i < count; i++)
            {
                float progress = i / (float)count;
                Vector2 dustPos = pos + normalDir * i * spacing;
                Color dustColor = Color.Lerp(DirgeRed, RequiemGold, progress);

                // Each mourner has a slight vertical sway — like walking in step
                float sway = (float)Math.Sin(Main.GameUpdateCount * 0.06f + i * 0.8f) * 2f;
                Vector2 perpendicular = new Vector2(-normalDir.Y, normalDir.X) * sway;

                Dust d = Dust.NewDustPerfect(dustPos + perpendicular, DustID.Torch,
                    normalDir * 0.5f, 0, dustColor, 1.1f + progress * 0.3f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// HeavySmokeParticle incense wisps rising slowly upward. Colors transition
        /// from IncenseSmoke at base to AshGray as they disperse into nothing.
        /// The smoke carries prayers to the heavens.
        /// </summary>
        public static void IncenseRising(Vector2 pos, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 smokePos = pos + Main.rand.NextVector2Circular(15f, 5f);
                Vector2 smokeVel = new Vector2(
                    Main.rand.NextFloat(-0.4f, 0.4f),
                    -1.5f - Main.rand.NextFloat(1.5f));
                Color smokeColor = Color.Lerp(IncenseSmoke, AshGray, Main.rand.NextFloat(0.3f));

                var smoke = new HeavySmokeParticle(smokePos, smokeVel, smokeColor,
                    Main.rand.Next(40, 65), 0.35f, 0.55f, 0.012f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
        }

        /// <summary>
        /// Multiple music notes spawned simultaneously in a chord formation:
        /// three notes at different heights and scales, like a solemn triad.
        /// The requiem's chord — root, third, and fifth.
        /// </summary>
        public static void RequiemChord(Vector2 pos)
        {
            // Root note — lowest, largest
            Vector2 rootPos = pos + new Vector2(-8f, 12f);
            EroicaVFXLibrary.SpawnMusicNotes(rootPos, 1, 4f, 0.9f, 1.1f, 38);

            // Third — middle height
            Vector2 thirdPos = pos + new Vector2(0f, 0f);
            EroicaVFXLibrary.SpawnMusicNotes(thirdPos, 1, 4f, 0.75f, 0.9f, 35);

            // Fifth — highest, smallest
            Vector2 fifthPos = pos + new Vector2(8f, -12f);
            EroicaVFXLibrary.SpawnMusicNotes(fifthPos, 1, 4f, 0.6f, 0.75f, 32);
        }
    }
}
