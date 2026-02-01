using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons
{
    /// <summary>
    /// Ignition of the Bell - Thrusting spear weapon with bell-music flame waves.
    /// Attack: Thrusts forward, sending streams of blazing musical waves.
    /// Special: Chime Cyclone - hitting enemies 3 times triggers bell-music flame eruption and summons flaming cyclone.
    /// Hold right-click to charge a devastating infernal storm attack!
    /// All attacks apply Resonant Toll debuff.
    /// </summary>
    public class IgnitionOfTheBell : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 340;
            Item.DamageType = DamageClass.Melee;
            Item.width = 70;
            Item.height = 70;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Thrust;
            Item.knockBack = 5.5f;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<LaCampanellaRainbowRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<IgnitionThrustProjectile>();
            Item.shootSpeed = 14f;
            Item.noMelee = false;
            Item.noUseGraphic = false;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Thrusts forward with streams of blazing musical waves"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Hitting enemies 3 times triggers a bell-music flame eruption and flaming cyclone"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Hold right-click to charge a devastating infernal storm attack"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Applies Resonant Toll on hit"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The spark that ignites the symphony of destruction'") { OverrideColor = new Color(255, 140, 40) });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 toMouse = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            
            // Main thrust projectile
            Projectile.NewProjectile(source, player.Center, toMouse * Item.shootSpeed, type, damage, knockback, player.whoAmI);
            
            // Spawn blazing wave projectiles
            for (int i = 0; i < 3; i++)
            {
                float spreadAngle = (i - 1) * 0.15f;
                Vector2 waveVelocity = toMouse.RotatedBy(spreadAngle) * (Item.shootSpeed * 0.8f);
                
                Projectile.NewProjectile(source, player.Center + toMouse * 30f, waveVelocity,
                    ModContent.ProjectileType<BlazingMusicalWave>(), (int)(damage * 0.6f), knockback * 0.5f, player.whoAmI);
            }
            
            // === GUTURAL CHAINSAW BELL SOUND STACK ===
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(0.1f, 0.5f), Volume = 0.55f }, player.Center);
            SoundEngine.PlaySound(SoundID.Item34 with { Pitch = Main.rand.NextFloat(0.2f, 0.5f), Volume = 0.35f }, player.Center);
            SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.4f, Volume = 0.4f }, player.Center);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { Pitch = 0.3f, Volume = 0.2f }, player.Center);
            
            // === UnifiedVFX LA CAMPANELLA THRUST EFFECT ===
            Vector2 thrustPos = player.Center + toMouse * 40f;
            UnifiedVFX.LaCampanella.SwingAura(thrustPos, toMouse, 0.9f);
            
            // === MASSIVE THRUST PARTICLES WITH CUSTOM SYSTEM ===
            ThemedParticles.LaCampanellaSparks(thrustPos, toMouse, 12, 9f);
            ThemedParticles.LaCampanellaBloomBurst(thrustPos, 0.55f);
            ThemedParticles.LaCampanellaMusicNotes(thrustPos, 4, 28f);
            
            // === FLARE TRAIL ALONG THRUST ===
            ThemedParticles.LaCampanellaCrescentWave(thrustPos, toMouse, 0.6f);
            for (int f = 0; f < 5; f++)
            {
                Vector2 flarePos = player.Center + toMouse * (20f + f * 18f);
                Color flareColor = Main.rand.Next(3) switch { 0 => ThemedParticles.CampanellaYellow, 1 => ThemedParticles.CampanellaOrange, _ => ThemedParticles.CampanellaGold };
                CustomParticles.GenericFlare(flarePos + Main.rand.NextVector2Circular(5f, 5f), flareColor, 0.4f + f * 0.08f, 14);
            }
            
            // === PRISMATIC SPARKLES ===
            ThemedParticles.LaCampanellaPrismaticSparkles(thrustPos, 5, 0.5f);
            
            // Halo ring at thrust point
            CustomParticles.HaloRing(thrustPos, ThemedParticles.CampanellaOrange, 0.35f, 12);
            CustomParticles.HaloRing(thrustPos, ThemedParticles.CampanellaYellow, 0.25f, 10);
            
            // Flares along thrust direction
            for (int i = 0; i < 4; i++)
            {
                Vector2 flarePos = player.Center + toMouse * (25f + i * 15f);
                Color flareColor = Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaGold;
                CustomParticles.GenericFlare(flarePos, flareColor, 0.3f + i * 0.05f, 12);
            }
            
            // Fire glow particles
            for (int i = 0; i < 5; i++)
            {
                Color color = Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow;
                var glow = new GenericGlowParticle(thrustPos + Main.rand.NextVector2Circular(15f, 15f),
                    toMouse * Main.rand.NextFloat(3f, 6f) + Main.rand.NextVector2Circular(2f, 2f),
                    color, Main.rand.NextFloat(0.3f, 0.5f), Main.rand.Next(10, 18), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Screen shake
            // REMOVED: Screen shake disabled for La Campanella weapons
            // player.GetModPlayer<ScreenShakePlayer>()?.AddShake(3f, 5);
            
            Lighting.AddLight(thrustPos, 0.9f, 0.45f, 0.12f);
            
            return false;
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(3))
            {
                Vector2 hitCenter = hitbox.Center.ToVector2();
                Color color = Main.rand.NextBool() ? new Color(255, 100, 0) : new Color(255, 180, 50);
                
                Dust flame = Dust.NewDustPerfect(hitCenter + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.Torch, Main.rand.NextVector2Circular(2f, 2f), 100, color, 1.5f);
                flame.noGravity = true;
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Resonant Toll
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
            
            // Track hits for Chime Cyclone
            var cyclonePlayer = player.GetModPlayer<ChimeCyclonePlayer>();
            cyclonePlayer.AddHit(target);
            
            // === UnifiedVFX LA CAMPANELLA IMPACT ===
            Vector2 hitDir = (target.Center - player.Center).SafeNormalize(Vector2.UnitX);
            UnifiedVFX.LaCampanella.Impact(target.Center, 1.1f);
            
            // === BLACK SMOKE SPARKLE MAGIC - SIGNATURE HIT EFFECT! ===
            ThemedParticles.LaCampanellaSignatureHit(target.Center, hitDir, 0.95f);
            
            // === MASSIVE IMPACT EFFECTS WITH SWORD ARCS ===
            ThemedParticles.LaCampanellaSparks(target.Center, hitDir, 10, 7f);
            ThemedParticles.LaCampanellaBloomBurst(target.Center, 0.55f);
            ThemedParticles.LaCampanellaMusicNotes(target.Center, 4, 25f);
            
            // === RADIAL FLARE BURST ON HIT ===
            for (int f = 0; f < 6; f++)
            {
                Vector2 flarePos = target.Center + (MathHelper.TwoPi * f / 6).ToRotationVector2() * 18f;
                Color flareColor = Main.rand.Next(3) switch { 0 => ThemedParticles.CampanellaYellow, 1 => ThemedParticles.CampanellaOrange, _ => ThemedParticles.CampanellaGold };
                CustomParticles.GenericFlare(flarePos, flareColor, 0.45f, 14);
            }
            
            // === PRISMATIC SPARKLES ON HIT ===
            ThemedParticles.LaCampanellaPrismaticSparkles(target.Center, 4, 0.5f);
            
            // Halo rings on hit
            CustomParticles.HaloRing(target.Center, ThemedParticles.CampanellaOrange, 0.45f, 15);
            CustomParticles.HaloRing(target.Center, Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, 0.5f), 0.35f, 13);
            CustomParticles.HaloRing(target.Center, ThemedParticles.CampanellaBlack, 0.25f, 10);
            
            // Flares around impact with BLACK ↁEORANGE gradient
            for (int i = 0; i < 4; i++)
            {
                float progress = (float)i / 4f;
                Color flareColor = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, progress);
                CustomParticles.GenericFlare(target.Center + Main.rand.NextVector2Circular(20f, 20f), flareColor, 0.35f, 12);
            }
            
            // === GLYPH IMPACT - ARCANE RESONANCE ===
            if (CustomParticleSystem.TexturesLoaded)
            {
                for (int i = 0; i < 4; i++)
                {
                    float glyphAngle = MathHelper.TwoPi * i / 4f + Main.GameUpdateCount * 0.05f;
                    Vector2 glyphPos = target.Center + glyphAngle.ToRotationVector2() * 28f;
                    Color glyphColor = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, (float)i / 4f) * 0.65f;
                    CustomParticles.Glyph(glyphPos, glyphColor, 0.24f, -1);
                }
            }
            
            // Screen shake on hit
            // REMOVED: Screen shake disabled for La Campanella weapons
            // player.GetModPlayer<ScreenShakePlayer>()?.AddShake(2f, 4);
            
            Lighting.AddLight(target.Center, 0.8f, 0.4f, 0.1f);
        }

        public override void HoldItem(Player player)
        {
            // === UnifiedVFX LA CAMPANELLA AURA ===
            UnifiedVFX.LaCampanella.Aura(player.Center, 30f, 0.3f);
            
            // === SIGNATURE HOLD AURA - VIBRANT PARTICLES WHILE HELD! ===
            ThemedParticles.LaCampanellaHoldAura(player.Center, 0.85f);
            
            // Ambient fire particles
            if (Main.rand.NextBool(10))
            {
                ThemedParticles.LaCampanellaAura(player.Center, 28f);
            }
            
            // Gradient light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.9f;
            Vector3 lightColor = Color.Lerp(UnifiedVFX.LaCampanella.Black, UnifiedVFX.LaCampanella.Orange, 0.6f).ToVector3();
            Lighting.AddLight(player.Center, lightColor * pulse * 0.55f);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.15f + 1f;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // === ORBITING GLYPHS AROUND WEAPON ===
            if (CustomParticleSystem.TexturesLoaded)
            {
                Texture2D glyphTex = CustomParticleSystem.RandomGlyph().Value;
                for (int i = 0; i < 4; i++)
                {
                    float glyphAngle = Main.GameUpdateCount * 0.04f + MathHelper.TwoPi * i / 4f;
                    Vector2 glyphPos = position + glyphAngle.ToRotationVector2() * 18f;
                    Color glyphColor = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, (float)i / 4f) * 0.6f;
                    spriteBatch.Draw(glyphTex, glyphPos, null, glyphColor, glyphAngle * 2f, glyphTex.Size() / 2f, 0.18f * pulse, SpriteEffects.None, 0f);
                }
            }
            
            // Black ↁEOrange gradient glow layers
            spriteBatch.Draw(texture, position, null, ThemedParticles.CampanellaOrange * 0.4f, rotation, origin, scale * pulse * 1.25f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, 0.4f) * 0.3f, rotation, origin, scale * pulse * 1.15f, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Lighting.AddLight(Item.Center, 0.6f, 0.35f, 0.1f);
            
            return true;
        }
    }

    /// <summary>
    /// Thrust projectile for Ignition of the Bell.
    /// Zenith-style spear thrust with blazing bell-flame trail.
    /// </summary>
    public class IgnitionThrustProjectile : ModProjectile
    {
        // Use the weapon sprite for spectral thrust effect
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/IgnitionOfTheBell";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            
            // === FLAMING DARK SMOKE TRAIL - EXPLOSIVE THRUST! ===
            
            // ☁EMUSICAL NOTATION - Ignition thrust melodic trail
            if (Main.rand.NextBool(5))
            {
                Color noteColor = Color.Lerp(new Color(255, 140, 40), new Color(255, 200, 50), Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.35f, 35);
            }
            
            // Heavy black smoke trail
            if (Main.rand.NextBool(2))
            {
                var smoke = new HeavySmokeParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    ThemedParticles.CampanellaBlack,
                    Main.rand.Next(30, 50),
                    Main.rand.NextFloat(0.35f, 0.5f),
                    0.65f, 0.018f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Golden fire glow trail
            for (int i = 0; i < 2; i++)
            {
                Color trailColor = Main.rand.Next(3) switch
                {
                    0 => ThemedParticles.CampanellaYellow,
                    1 => ThemedParticles.CampanellaGold,
                    _ => ThemedParticles.CampanellaOrange
                };
                var glow = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(1.2f, 1.2f),
                    trailColor, Main.rand.NextFloat(0.28f, 0.45f), Main.rand.Next(12, 20), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Prismatic sparkles along thrust
            if (Main.rand.NextBool(2))
            {
                CustomParticles.PrismaticSparkle(Projectile.Center, ThemedParticles.CampanellaYellow, 0.4f);
            }
            
            // === GLITTERING SPARKLE TRAIL ===
            ThemedParticles.LaCampanellaSparkles(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), 2, 10f);
            ThemedParticles.LaCampanellaPrismaticSparkles(Projectile.Center, 1, 0.45f);
            
            // === BLAZING THRUST TRAIL ===
            ThemedParticles.LaCampanellaTrail(Projectile.Center, Projectile.velocity);
            
            // Extra sparks along thrust path
            if (Main.rand.NextBool(2))
            {
                ThemedParticles.LaCampanellaSparks(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX), 3, 5f);
            }
            
            // Occasional flare burst
            if (Main.rand.NextBool(4))
            {
                Color flareColor = Main.rand.NextBool() ? ThemedParticles.CampanellaYellow : ThemedParticles.CampanellaOrange;
                CustomParticles.GenericFlare(Projectile.Center, flareColor, 0.3f, 12);
            }
            
            // Occasional music note
            if (Main.rand.NextBool(5))
            {
                ThemedParticles.LaCampanellaMusicNotes(Projectile.Center, 1, 18f);
            }
            
            Lighting.AddLight(Projectile.Center, 0.75f, 0.38f, 0.1f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
            
            // Track hits
            Player owner = Main.player[Projectile.owner];
            owner.GetModPlayer<ChimeCyclonePlayer>().AddHit(target);
            
            // === GUTURAL CHAINSAW BELL HIT SOUNDS ===
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(0.3f, 0.6f), Volume = 0.45f }, target.Center);
            SoundEngine.PlaySound(SoundID.Item34 with { Pitch = Main.rand.NextFloat(0.1f, 0.4f), Volume = 0.3f }, target.Center);
            
            // === EXPLOSIVE SMOKE BURST! ===
            for (int i = 0; i < 4; i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2Circular(4f, 4f);
                var smoke = new HeavySmokeParticle(
                    target.Center + Main.rand.NextVector2Circular(12f, 12f),
                    smokeVel,
                    ThemedParticles.CampanellaBlack,
                    Main.rand.Next(35, 55),
                    Main.rand.NextFloat(0.4f, 0.65f),
                    0.65f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // === BLACK SMOKE SPARKLE MAGIC - SIGNATURE HIT EFFECT! ===
            Vector2 hitDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            ThemedParticles.LaCampanellaSignatureHit(target.Center, hitDir, 1.0f);
            
            // ☁EMUSICAL IMPACT - Ignition thrust impact burst
            ThemedParticles.MusicNoteBurst(target.Center, new Color(255, 140, 40), 6, 4f);
            
            // === PRISMATIC BURST ===
            ThemedParticles.LaCampanellaPrismaticBurst(target.Center, 12, 1.0f);
            
            // === MASSIVE IMPACT EFFECTS ===
            ThemedParticles.LaCampanellaSparks(target.Center, hitDir, 14, 8f);
            ThemedParticles.LaCampanellaBloomBurst(target.Center, 0.8f);
            ThemedParticles.LaCampanellaMusicNotes(target.Center, 6, 30f);
            
            // Triple cascading halo rings on hit with BLACK ↁEORANGE
            CustomParticles.HaloRing(target.Center, ThemedParticles.CampanellaOrange, 0.6f, 20);
            CustomParticles.HaloRing(target.Center, Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, 0.6f), 0.45f, 16);
            CustomParticles.HaloRing(target.Center, ThemedParticles.CampanellaBlack, 0.3f, 12);
            
            // === SIGNATURE FRACTAL FLARE BURST - BLACK ↁEORANGE ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 flareOffset = angle.ToRotationVector2() * 30f;
                float progress = (float)i / 6f;
                Color fractalColor = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, progress);
                CustomParticles.GenericFlare(target.Center + flareOffset, fractalColor, 0.45f, 18);
            }
            
            // === GLYPH IMPACT - ARCANE THRUST ===
            if (CustomParticleSystem.TexturesLoaded)
            {
                for (int i = 0; i < 5; i++)
                {
                    float glyphAngle = MathHelper.TwoPi * i / 5f + Main.GameUpdateCount * 0.04f;
                    Vector2 glyphPos = target.Center + glyphAngle.ToRotationVector2() * 32f;
                    Color glyphColor = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, (float)i / 5f) * 0.6f;
                    CustomParticles.Glyph(glyphPos, glyphColor, 0.26f, -1);
                }
            }
            
            // Fire glow burst
            for (int i = 0; i < 6; i++)
            {
                Vector2 glowVel = Main.rand.NextVector2Circular(3.5f, 3.5f);
                Color glowColor = Main.rand.NextBool() ? ThemedParticles.CampanellaYellow : ThemedParticles.CampanellaOrange;
                var glow = new GenericGlowParticle(
                    target.Center + Main.rand.NextVector2Circular(10f, 10f),
                    glowVel, glowColor, Main.rand.NextFloat(0.35f, 0.6f), Main.rand.Next(12, 22), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Screen shake
            // REMOVED: Screen shake disabled for La Campanella weapons
            // owner.GetModPlayer<ScreenShakePlayer>()?.AddShake(3f, 6);
            
            Lighting.AddLight(target.Center, 1.1f, 0.55f, 0.15f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            
            // === SPECTRAL TRAIL ===
            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = 1f - (i / (float)Projectile.oldPos.Length);
                Color trailColor = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, progress) * progress * 0.5f;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.oldRot[i], origin,
                    Projectile.scale * (0.5f + progress * 0.5f), SpriteEffects.None, 0);
            }
            
            // === ADDITIVE GLOW ===
            Vector2 mainPos = Projectile.Center - Main.screenPosition;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Main.EntitySpriteDraw(texture, mainPos, null, ThemedParticles.CampanellaOrange * 0.5f, Projectile.rotation, origin,
                Projectile.scale * 1.3f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, mainPos, null, ThemedParticles.CampanellaYellow * 0.3f, Projectile.rotation, origin,
                Projectile.scale * 1.15f, SpriteEffects.None, 0);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Main sprite
            Main.EntitySpriteDraw(texture, mainPos, null, Color.White, Projectile.rotation, origin,
                Projectile.scale * 0.8f, SpriteEffects.None, 0);
            
            return false;
        }
    }

    /// <summary>
    /// Blazing musical wave projectile.
    /// Pure particle visual with bell-flame effects.
    /// </summary>
    public class BlazingMusicalWave : ModProjectile
    {
        // Uses weapon texture for loading, drawn as particles
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/IgnitionOfTheBell";
        
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 200;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.alpha = Math.Max(0, Projectile.alpha - 5);
            
            // Wave pattern movement
            float waveOffset = (float)Math.Sin(Projectile.ai[0] += 0.3f) * 2f;
            Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);
            Projectile.Center += perpendicular * waveOffset * 0.5f;
            
            // === FLAMING DARK SMOKE TRAIL - BLAZING WAVE! ===
            
            // ☁EMUSICAL NOTATION - Blazing musical wave melodic trail
            if (Main.rand.NextBool(6))
            {
                Color noteColor = Color.Lerp(new Color(255, 140, 40), new Color(255, 200, 50), Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -0.8f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.32f, 32);
            }
            
            // Heavy black smoke trail
            if (Main.rand.NextBool(2))
            {
                var smoke = new HeavySmokeParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                    -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(1f, 1f) + new Vector2(0, -0.5f),
                    ThemedParticles.CampanellaBlack,
                    Main.rand.Next(28, 45),
                    Main.rand.NextFloat(0.28f, 0.4f),
                    0.55f, 0.018f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Fire glow trail
            Color trailColor = Main.rand.Next(3) switch
            {
                0 => ThemedParticles.CampanellaYellow,
                1 => ThemedParticles.CampanellaGold,
                _ => ThemedParticles.CampanellaOrange
            };
            var glow = new GenericGlowParticle(
                Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.8f, 0.8f),
                trailColor, Main.rand.NextFloat(0.22f, 0.35f), Main.rand.Next(10, 16), true);
            MagnumParticleHandler.SpawnParticle(glow);
            
            // === BLAZING WAVE TRAIL ===
            ThemedParticles.LaCampanellaTrail(Projectile.Center, Projectile.velocity);
            
            // Fire sparks
            if (Main.rand.NextBool(2))
            {
                ThemedParticles.LaCampanellaSparks(Projectile.Center, -Projectile.velocity.SafeNormalize(Vector2.UnitX), 3, 4f);
            }
            
            // Prismatic sparkle
            if (Main.rand.NextBool(2))
            {
                CustomParticles.PrismaticSparkle(Projectile.Center, ThemedParticles.CampanellaYellow, 0.3f);
            }
            
            // === GLITTERING SPARKLE TRAIL ===
            ThemedParticles.LaCampanellaSparkles(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), 2, 8f);
            ThemedParticles.LaCampanellaPrismaticSparkles(Projectile.Center, 1, 0.4f);
            
            // Occasional flare
            if (Main.rand.NextBool(5))
            {
                Color flareColor = Main.rand.NextBool() ? ThemedParticles.CampanellaYellow : ThemedParticles.CampanellaOrange;
                CustomParticles.GenericFlare(Projectile.Center, flareColor, 0.22f, 10);
            }
            
            // Occasional music note
            if (Main.rand.NextBool(7))
            {
                ThemedParticles.LaCampanellaMusicNotes(Projectile.Center, 1, 12f);
            }
            
            Lighting.AddLight(Projectile.Center, 0.6f, 0.3f, 0.1f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
            
            Player owner = Main.player[Projectile.owner];
            owner.GetModPlayer<ChimeCyclonePlayer>().AddHit(target);
            
            // === EXPLOSIVE SMOKE BURST! ===
            for (int i = 0; i < 3; i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2Circular(3f, 3f);
                var smoke = new HeavySmokeParticle(
                    target.Center + Main.rand.NextVector2Circular(8f, 8f),
                    smokeVel,
                    ThemedParticles.CampanellaBlack,
                    Main.rand.Next(30, 45),
                    Main.rand.NextFloat(0.35f, 0.5f),
                    0.55f, 0.018f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // === BLACK SMOKE SPARKLE MAGIC - SIGNATURE HIT EFFECT! ===
            Vector2 hitDir = Main.rand.NextVector2Unit();
            ThemedParticles.LaCampanellaSignatureHit(target.Center, hitDir, 0.7f);
            
            // ☁EMUSICAL IMPACT - Blazing wave impact burst
            ThemedParticles.MusicNoteBurst(target.Center, new Color(255, 140, 40), 5, 3.5f);
            
            // === PRISMATIC SPARKLES ===
            ThemedParticles.LaCampanellaPrismaticSparkles(target.Center, 4, 0.5f);
            
            // Impact effects
            ThemedParticles.LaCampanellaSparks(target.Center, hitDir, 8, 5f);
            ThemedParticles.LaCampanellaBloomBurst(target.Center, 0.5f);
            ThemedParticles.LaCampanellaMusicNotes(target.Center, 2, 18f);
            
            // Halo rings
            CustomParticles.HaloRing(target.Center, ThemedParticles.CampanellaOrange, 0.35f, 12);
            CustomParticles.HaloRing(target.Center, ThemedParticles.CampanellaYellow, 0.25f, 10);
            
            // Flares
            for (int i = 0; i < 3; i++)
            {
                Color flareColor = Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaGold;
                CustomParticles.GenericFlare(target.Center + Main.rand.NextVector2Circular(12f, 12f), flareColor, 0.3f, 12);
            }
            
            Lighting.AddLight(target.Center, 0.8f, 0.4f, 0.12f);
        }

        public override void OnKill(int timeLeft)
        {
            // ☁EMUSICAL FINALE - Golden notes on blazing wave death
            ThemedParticles.MusicNoteBurst(Projectile.Center, new Color(218, 165, 32), 4, 3f);
            
            ThemedParticles.LaCampanellaBloomBurst(Projectile.Center, 0.2f);
            ThemedParticles.LaCampanellaSparkles(Projectile.Center, 3, 12f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw as pure particle effect
            return false;
        }
    }

    /// <summary>
    /// Player handler for Chime Cyclone mechanic.
    /// </summary>
    public class ChimeCyclonePlayer : ModPlayer
    {
        private Dictionary<int, int> hitCounts = new Dictionary<int, int>();
        private int cycloneIndex = -1;
        
        public void AddHit(NPC target)
        {
            if (!hitCounts.ContainsKey(target.whoAmI))
                hitCounts[target.whoAmI] = 0;
            
            hitCounts[target.whoAmI]++;
            
            // Check for Chime Cyclone trigger (3 hits)
            if (hitCounts[target.whoAmI] >= 3)
            {
                TriggerChimeCyclone(target);
                hitCounts[target.whoAmI] = 0;
            }
        }

        private void TriggerChimeCyclone(NPC target)
        {
            // Bell-music flame eruption on all affected enemies
            foreach (var kvp in hitCounts)
            {
                NPC affectedNPC = Main.npc[kvp.Key];
                if (affectedNPC.active)
                {
                    // === MASSIVE ERUPTION EFFECTS WITH CUSTOM PARTICLES ===
                    ThemedParticles.LaCampanellaImpact(affectedNPC.Center, 1.5f);
                    ThemedParticles.LaCampanellaMusicalImpact(affectedNPC.Center, 1.2f, true);
                    ThemedParticles.LaCampanellaBellChime(affectedNPC.Center, 1f);
                    
                    // === GRAND IMPACT WITH ALL THE EFFECTS ===
                    ThemedParticles.LaCampanellaGrandImpact(affectedNPC.Center, 1.2f);
                    ThemedParticles.LaCampanellaHaloBurst(affectedNPC.Center, 0.9f);
                    ThemedParticles.LaCampanellaPrismaticBurst(affectedNPC.Center, 10, 0.9f);
                    
                    // === RADIAL FLARE BURST ===
                    for (int f = 0; f < 8; f++)
                    {
                        Vector2 flarePos = affectedNPC.Center + (MathHelper.TwoPi * f / 8).ToRotationVector2() * Main.rand.NextFloat(20f, 40f);
                        Color flareColor = Main.rand.Next(3) switch { 0 => ThemedParticles.CampanellaYellow, 1 => ThemedParticles.CampanellaOrange, _ => ThemedParticles.CampanellaGold };
                        CustomParticles.GenericFlare(flarePos, flareColor, 0.5f, 15);
                    }
                    
                    // Halo rings
                    CustomParticles.HaloRing(affectedNPC.Center, ThemedParticles.CampanellaOrange, 0.6f, 20);
                    CustomParticles.HaloRing(affectedNPC.Center, ThemedParticles.CampanellaYellow, 0.45f, 16);
                    
                    // Flares around affected enemy
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 6f;
                        Vector2 flarePos = affectedNPC.Center + angle.ToRotationVector2() * 30f;
                        Color flareColor = Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaGold;
                        CustomParticles.GenericFlare(flarePos, flareColor, 0.5f, 15);
                    }
                    
                    // Damage eruption
                    if (Player.HeldItem.ModItem is IgnitionOfTheBell)
                    {
                        int damage = (int)(Player.HeldItem.damage * Player.GetDamage(DamageClass.Melee).Multiplicative * 1.5f);
                        affectedNPC.SimpleStrikeNPC(damage, 0, false, 0f, null, false, 0f, true);
                    }
                    
                    Lighting.AddLight(affectedNPC.Center, 1.2f, 0.6f, 0.2f);
                }
            }
            
            // Spawn or extend Chime Cyclone
            if (cycloneIndex >= 0 && Main.projectile[cycloneIndex].active && 
                Main.projectile[cycloneIndex].type == ModContent.ProjectileType<ChimeCycloneProjectile>())
            {
                // Extend duration
                Main.projectile[cycloneIndex].timeLeft = Math.Min(Main.projectile[cycloneIndex].timeLeft + 300, 1500); // Max 25 seconds
            }
            else
            {
                // Spawn new cyclone
                cycloneIndex = Projectile.NewProjectile(Player.GetSource_ItemUse(Player.HeldItem), 
                    target.Center, Vector2.Zero, ModContent.ProjectileType<ChimeCycloneProjectile>(),
                    (int)(Player.HeldItem.damage * 0.8f), 5f, Player.whoAmI);
            }
            
            // === EPIC CHIME CYCLONE TRIGGER SOUNDS ===
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.1f, Volume = 0.8f }, target.Center);
            SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0.3f, Volume = 0.55f }, target.Center);
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = -0.2f, Volume = 0.5f }, target.Center);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Pitch = 0.2f, Volume = 0.35f }, target.Center);
            
            // Screen shake
            // REMOVED: Screen shake disabled for La Campanella weapons
            // Player.GetModPlayer<ScreenShakePlayer>()?.AddShake(8f, 15);
        }

        public override void PostUpdate()
        {
            // Clean up dead NPC entries
            List<int> toRemove = new List<int>();
            foreach (var kvp in hitCounts)
            {
                if (!Main.npc[kvp.Key].active)
                    toRemove.Add(kvp.Key);
            }
            foreach (int id in toRemove)
                hitCounts.Remove(id);
        }
    }

    /// <summary>
    /// Chime Cyclone - stationary flaming cyclone that deals area damage.
    /// Pure particle-based tornado with bell-flame effects.
    /// </summary>
    public class ChimeCycloneProjectile : ModProjectile
    {
        // Uses weapon texture for loading, drawn entirely as particles
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/IgnitionOfTheBell";
        
        private float rotationAngle = 0f;
        
        public override void SetDefaults()
        {
            Projectile.width = 200;
            Projectile.height = 200;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300; // 5 seconds base
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            rotationAngle += 0.15f;
            
            // === MASSIVE CYCLONE VISUAL WITH CUSTOM PARTICLES ===
            // ☁EMUSICAL NOTATION - Chime cyclone melodic swirl
            if (Main.rand.NextBool(4))
            {
                float noteAngle = rotationAngle + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 notePos = Projectile.Center + noteAngle.ToRotationVector2() * Main.rand.NextFloat(30f, 60f);
                Color noteColor = Color.Lerp(new Color(255, 140, 40), new Color(255, 200, 50), Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -1.2f);
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.35f, 40);
            }
            
            // Spiraling fire particles forming tornado shape
            int particleCount = 10;
            for (int i = 0; i < particleCount; i++)
            {
                float angle = rotationAngle + (MathHelper.TwoPi * i / particleCount);
                float heightOffset = (float)Math.Sin(rotationAngle * 2f + i * 0.5f) * 30f;
                float radius = 40f + heightOffset * 0.5f;
                
                Vector2 particlePos = Projectile.Center + angle.ToRotationVector2() * radius + new Vector2(0, heightOffset * 0.3f);
                
                // Fire glow particles
                Color color = Main.rand.Next(3) switch
                {
                    0 => ThemedParticles.CampanellaOrange,
                    1 => ThemedParticles.CampanellaYellow,
                    _ => ThemedParticles.CampanellaRed
                };
                
                var glow = new GenericGlowParticle(particlePos, 
                    angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 2f + new Vector2(0, -2f),
                    color, Main.rand.NextFloat(0.25f, 0.45f), Main.rand.Next(15, 25), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // === FLARES SPIRALING IN CYCLONE ===
            if (Main.rand.NextBool(4))
            {
                float flareAngle = rotationAngle + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 flarePos = Projectile.Center + flareAngle.ToRotationVector2() * Main.rand.NextFloat(25f, 55f);
                Color flareColor = Main.rand.Next(3) switch { 0 => ThemedParticles.CampanellaYellow, 1 => ThemedParticles.CampanellaOrange, _ => ThemedParticles.CampanellaGold };
                CustomParticles.GenericFlare(flarePos, flareColor, 0.4f, 12);
            }
            
            // === PRISMATIC SPARKLES IN CYCLONE ===
            if (Main.rand.NextBool(6))
            {
                Vector2 sparkPos = Projectile.Center + Main.rand.NextVector2Circular(50f, 50f);
                ThemedParticles.LaCampanellaPrismaticSparkles(sparkPos, 2, 0.45f);
            }
            
            // Rising black smoke column
            if (Main.rand.NextBool(2))
            {
                float smokeAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 smokePos = Projectile.Center + smokeAngle.ToRotationVector2() * Main.rand.NextFloat(20f, 50f);
                var smoke = new HeavySmokeParticle(smokePos, 
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-3f, -1.5f)),
                    ThemedParticles.CampanellaBlack, Main.rand.Next(40, 60), 
                    Main.rand.NextFloat(0.4f, 0.6f), 0.5f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Inner core particles - intense fire
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                Color coreColor = Main.rand.NextBool() ? ThemedParticles.CampanellaYellow : ThemedParticles.CampanellaGold;
                var core = new GenericGlowParticle(Projectile.Center + offset,
                    new Vector2(0, Main.rand.NextFloat(-3f, -1.5f)), coreColor,
                    Main.rand.NextFloat(0.3f, 0.5f), Main.rand.Next(15, 25), true);
                MagnumParticleHandler.SpawnParticle(core);
            }
            
            // Occasional spark bursts
            if (Main.rand.NextBool(4))
            {
                Vector2 sparkDir = Main.rand.NextVector2Unit();
                ThemedParticles.LaCampanellaSparks(Projectile.Center + sparkDir * 30f, sparkDir, 3, 4f);
            }
            
            // Occasional music notes spiraling up
            if (Main.rand.NextBool(5))
            {
                ThemedParticles.LaCampanellaMusicNotes(Projectile.Center + Main.rand.NextVector2Circular(50f, 50f), 2, 20f);
            }
            
            // Periodic bell chime with shockwave and flares
            if (Projectile.timeLeft % 60 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.5f, Volume = 0.3f }, Projectile.Center);
                ThemedParticles.LaCampanellaShockwave(Projectile.Center, 0.6f);
                ThemedParticles.LaCampanellaPrismaticBurst(Projectile.Center, 6, 0.6f);
                // Radial flare burst
                for (int f = 0; f < 6; f++)
                {
                    Vector2 flarePos = Projectile.Center + (MathHelper.TwoPi * f / 6).ToRotationVector2() * Main.rand.NextFloat(25f, 45f);
                    Color flareColor = Main.rand.NextBool() ? ThemedParticles.CampanellaYellow : ThemedParticles.CampanellaOrange;
                    CustomParticles.GenericFlare(flarePos, flareColor, 0.45f, 14);
                }
            }
            
            // Halo rings pulsing outward
            if (Projectile.timeLeft % 30 == 0)
            {
                CustomParticles.HaloRing(Projectile.Center, ThemedParticles.CampanellaOrange, 0.5f, 20);
            }
            
            // Dynamic Lighting - pulsing glow
            float lightPulse = 1f + (float)Math.Sin(rotationAngle * 2f) * 0.2f;
            Lighting.AddLight(Projectile.Center, 1.2f * lightPulse, 0.6f * lightPulse, 0.2f * lightPulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
            
            // === BLACK SMOKE SPARKLE MAGIC - SIGNATURE HIT EFFECT! ===
            Vector2 hitDir = Main.rand.NextVector2Unit();
            ThemedParticles.LaCampanellaSignatureHit(target.Center, hitDir, 0.6f);
            
            // ☁EMUSICAL IMPACT - Chime cyclone impact burst
            ThemedParticles.MusicNoteBurst(target.Center, new Color(255, 140, 40), 4, 3f);
            
            ThemedParticles.LaCampanellaSparks(target.Center, hitDir, 4, 5f);
            ThemedParticles.LaCampanellaBloomBurst(target.Center, 0.4f);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float radius = Projectile.width / 2f;
            return Vector2.Distance(Projectile.Center, targetHitbox.Center.ToVector2()) < radius;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false; // Pure particle visual
        }

        public override void OnKill(int timeLeft)
        {
            // ☁EMUSICAL FINALE - Grand golden notes on cyclone dissipation
            ThemedParticles.MusicNoteBurst(Projectile.Center, new Color(218, 165, 32), 8, 5f);
            
            // Final explosion
            ThemedParticles.LaCampanellaImpact(Projectile.Center, 1.5f);
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.2f, Volume = 0.5f }, Projectile.Center);
        }
    }
}
