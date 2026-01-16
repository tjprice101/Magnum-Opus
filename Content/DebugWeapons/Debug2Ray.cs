using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.DataStructures;
using Terraria.Audio;
using ReLogic.Content;

namespace MagnumOpus.Content.DebugWeapons
{
    /// <summary>
    /// Debug weapon to test red and gold beam rendering.
    /// Uses three-part texture system (Start, Mid, End) like Profaned Goddess rays.
    /// On swing, fires a wavering sinusoidal beam with color cycling.
    /// </summary>
    public class Debug2Ray : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.TinBroadsword;

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.damage = 200;
            Item.knockBack = 5f;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.DamageType = DamageClass.Melee;
            Item.autoReuse = true;
            Item.noMelee = false;
            Item.noUseGraphic = false;
            Item.shoot = ModContent.ProjectileType<Debug2RayBeam>();
            Item.shootSpeed = 1f;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(gold: 1);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Spawn beam projectile toward cursor
            Vector2 direction = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            
            int proj = Projectile.NewProjectile(source, player.Center, direction, type, damage, knockback, player.whoAmI);
            
            // Spawn muzzle VFX
            SpawnMuzzleFlash(player.Center);
            
            return false;
        }

        private void SpawnMuzzleFlash(Vector2 position)
        {
            // Red/gold burst at origin
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * 3f;
                Color color = i % 2 == 0 ? new Color(255, 50, 50) : new Color(255, 200, 50);
                Dust d = Dust.NewDustPerfect(position, DustID.Torch, vel, 0, color, 1.5f);
                d.noGravity = true;
            }
            
            // Central flash
            Dust flash = Dust.NewDustPerfect(position, DustID.GoldFlame, Vector2.Zero, 0, Color.White, 2.5f);
            flash.noGravity = true;
            
            Lighting.AddLight(position, 1f, 0.5f, 0.2f);
        }
    }

    /// <summary>
    /// Beam projectile using three-part texture system.
    /// Features sinusoidal wavering and red-gold color cycling.
    /// Inspired by Calamity's ProvidenceHolyRay.
    /// </summary>
    public class Debug2RayBeam : ModProjectile
    {
        // Texture paths for the three-part beam system
        private const string TextureStart = "MagnumOpus/Content/DebugWeapons/Debug2RayStart";
        private const string TextureMid = "MagnumOpus/Content/DebugWeapons/Debug2RayMid";
        private const string TextureEnd = "MagnumOpus/Content/DebugWeapons/Debug2RayEnd";

        // Use a simple texture for the main projectile (won't be drawn normally)
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowRodBullet;

        // Beam parameters
        private const float MaxBeamLength = 1200f;
        private const float BeamWidth = 40f;
        private const int MaxLifetime = 45;
        
        // Wave parameters for sinusoidal effect
        private const float WaveAmplitude = 8f;      // How much the beam wavers side-to-side
        private const float WaveFrequency = 0.15f;   // How fast the waves occur along length
        private const float WaveSpeed = 0.3f;        // How fast the waves animate
        private const float ColorCycleSpeed = 0.08f; // Speed of red/gold color cycling
        
        // Cached textures
        private static Asset<Texture2D> startTex;
        private static Asset<Texture2D> midTex;
        private static Asset<Texture2D> endTex;

        public override void SetStaticDefaults()
        {
            // No trail needed for beam
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = MaxLifetime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            // Keep beam anchored to player
            Projectile.Center = owner.Center;
            
            // Calculate beam direction (locked to initial direction)
            if (Projectile.localAI[0] == 0f)
            {
                // First frame - store initial angle
                Projectile.localAI[0] = 1f;
                Projectile.ai[0] = Projectile.velocity.ToRotation();
            }
            
            // Animation timer for wave effect
            Projectile.localAI[1] += WaveSpeed;
            
            // Calculate actual beam length (with tile collision)
            float beamLength = CalculateBeamLength();
            Projectile.ai[1] = beamLength;
            
            // Scale pulsing (like Profaned Goddess)
            float scalePulse = 1f + (float)Math.Sin(Projectile.localAI[1] * 0.2f) * 0.1f;
            Projectile.scale = scalePulse;
            
            // Spawn particles along beam
            SpawnBeamParticles(beamLength);
            
            // Add lighting along beam
            AddBeamLighting(beamLength);
        }

        private float CalculateBeamLength()
        {
            Vector2 start = Projectile.Center;
            Vector2 direction = Projectile.ai[0].ToRotationVector2();
            
            // Scan for tiles
            float length = MaxBeamLength;
            for (float d = 0; d < MaxBeamLength; d += 16f)
            {
                Vector2 checkPos = start + direction * d;
                Point tileCoords = checkPos.ToTileCoordinates();
                
                if (WorldGen.InWorld(tileCoords.X, tileCoords.Y))
                {
                    Tile tile = Main.tile[tileCoords.X, tileCoords.Y];
                    if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                    {
                        length = d;
                        break;
                    }
                }
            }
            
            return length;
        }

        private void SpawnBeamParticles(float beamLength)
        {
            if (Main.rand.NextBool(2)) return;
            
            Vector2 direction = Projectile.ai[0].ToRotationVector2();
            Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
            
            // Random position along beam
            float t = Main.rand.NextFloat();
            float waveOffset = GetWaveOffset(t * beamLength);
            Vector2 pos = Projectile.Center + direction * t * beamLength + perpendicular * waveOffset;
            
            // Color cycling
            float colorPhase = (Main.GameUpdateCount * ColorCycleSpeed + t) % 1f;
            Color color = GetBeamColor(colorPhase);
            
            // Spawn dust
            Dust dust = Dust.NewDustPerfect(pos, DustID.Torch, 
                Main.rand.NextVector2Circular(2f, 2f), 0, color, 1.2f);
            dust.noGravity = true;
            
            // Occasional golden sparkle
            if (Main.rand.NextBool(3))
            {
                Dust gold = Dust.NewDustPerfect(pos, DustID.GoldCoin, 
                    Main.rand.NextVector2Circular(1f, 1f), 0, default, 0.8f);
                gold.noGravity = true;
            }
        }

        private void AddBeamLighting(float beamLength)
        {
            Vector2 direction = Projectile.ai[0].ToRotationVector2();
            
            int lightCount = (int)(beamLength / 40f);
            for (int i = 0; i <= lightCount; i++)
            {
                float t = (float)i / Math.Max(1, lightCount);
                Vector2 lightPos = Projectile.Center + direction * t * beamLength;
                
                // Color cycling for lighting too
                float colorPhase = (Main.GameUpdateCount * ColorCycleSpeed + t) % 1f;
                Color color = GetBeamColor(colorPhase);
                
                Lighting.AddLight(lightPos, color.R / 255f * 0.8f, color.G / 255f * 0.8f, color.B / 255f * 0.3f);
            }
        }

        /// <summary>
        /// Calculates the sinusoidal wave offset at a given distance along the beam.
        /// </summary>
        private float GetWaveOffset(float distance)
        {
            return (float)Math.Sin(distance * WaveFrequency + Projectile.localAI[1]) * WaveAmplitude;
        }

        /// <summary>
        /// Returns a color interpolated between red and gold based on phase (0-1).
        /// </summary>
        private Color GetBeamColor(float phase)
        {
            // Red: (255, 50, 50)
            // Gold: (255, 200, 50)
            // Use smooth sine interpolation
            float t = (float)Math.Sin(phase * MathHelper.TwoPi) * 0.5f + 0.5f;
            return Color.Lerp(new Color(255, 50, 50), new Color(255, 200, 50), t);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Line collision for beam
            Vector2 direction = Projectile.ai[0].ToRotationVector2();
            float beamLength = Projectile.ai[1];
            Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
            
            // Check collision along beam with wave offset
            for (float d = 0; d < beamLength; d += 16f)
            {
                float waveOffset = GetWaveOffset(d);
                Vector2 checkPos = Projectile.Center + direction * d + perpendicular * waveOffset;
                
                Rectangle checkBox = new Rectangle((int)checkPos.X - 20, (int)checkPos.Y - 20, 40, 40);
                if (checkBox.Intersects(targetHitbox))
                    return true;
            }
            
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Load textures if needed
            startTex ??= ModContent.Request<Texture2D>(TextureStart);
            midTex ??= ModContent.Request<Texture2D>(TextureMid);
            endTex ??= ModContent.Request<Texture2D>(TextureEnd);

            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // Switch to additive blending for glow effect
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, 
                SamplerState.LinearClamp, DepthStencilState.None, 
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            DrawWaveBeam(spriteBatch);

            // Return to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, 
                SamplerState.LinearClamp, DepthStencilState.None, 
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false; // Don't draw default sprite
        }

        private void DrawWaveBeam(SpriteBatch spriteBatch)
        {
            Texture2D startTexture = startTex.Value;
            Texture2D midTexture = midTex.Value;
            Texture2D endTexture = endTex.Value;

            float beamLength = Projectile.ai[1];
            float rotation = Projectile.ai[0];
            Vector2 direction = rotation.ToRotationVector2();
            Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
            
            // Fade in/out based on lifetime
            float lifetimeRatio = (float)Projectile.timeLeft / MaxLifetime;
            float fadeAlpha = lifetimeRatio > 0.8f ? (1f - lifetimeRatio) / 0.2f : 
                             lifetimeRatio < 0.2f ? lifetimeRatio / 0.2f : 1f;
            fadeAlpha = Math.Clamp(fadeAlpha, 0f, 1f);
            
            // Scale with pulse
            float currentWidth = BeamWidth * Projectile.scale;
            float widthScale = currentWidth / midTexture.Height;

            // === DRAW START PIECE ===
            {
                float startWave = GetWaveOffset(0);
                Vector2 startPos = Projectile.Center + perpendicular * startWave - Main.screenPosition;
                
                float colorPhase = (Main.GameUpdateCount * ColorCycleSpeed) % 1f;
                Color startColor = GetBeamColor(colorPhase) * fadeAlpha;
                
                Vector2 startOrigin = new Vector2(0, startTexture.Height / 2f);
                float startScale = widthScale * 1.2f;
                
                // Draw multiple layers for glow
                spriteBatch.Draw(startTexture, startPos, null, startColor * 0.3f, rotation, 
                    startOrigin, startScale * 1.5f, SpriteEffects.None, 0f);
                spriteBatch.Draw(startTexture, startPos, null, startColor * 0.6f, rotation, 
                    startOrigin, startScale * 1.1f, SpriteEffects.None, 0f);
                spriteBatch.Draw(startTexture, startPos, null, Color.White * fadeAlpha * 0.4f, rotation, 
                    startOrigin, startScale * 0.8f, SpriteEffects.None, 0f);
            }

            // === DRAW MID SEGMENTS WITH SINUSOIDAL WAVE ===
            float segmentLength = midTexture.Width * widthScale;
            int segmentCount = (int)Math.Ceiling(beamLength / segmentLength);
            
            for (int i = 0; i < segmentCount; i++)
            {
                float segmentStart = i * segmentLength;
                float segmentEnd = Math.Min((i + 1) * segmentLength, beamLength);
                float segmentMid = (segmentStart + segmentEnd) / 2f;
                
                // Calculate wave offset at this segment
                float waveOffset = GetWaveOffset(segmentMid);
                
                // Position with wave applied
                Vector2 segmentPos = Projectile.Center + direction * segmentStart + perpendicular * waveOffset;
                Vector2 drawPos = segmentPos - Main.screenPosition;
                
                // Color cycling along beam length
                float t = segmentMid / beamLength;
                float colorPhase = (Main.GameUpdateCount * ColorCycleSpeed + t * 2f) % 1f;
                Color segmentColor = GetBeamColor(colorPhase) * fadeAlpha;
                
                // Calculate rotation adjustment for wave tangent
                float waveSlope = (float)Math.Cos(segmentMid * WaveFrequency + Projectile.localAI[1]) * WaveAmplitude * WaveFrequency;
                float adjustedRotation = rotation + (float)Math.Atan(waveSlope);
                
                Vector2 midOrigin = new Vector2(0, midTexture.Height / 2f);
                
                // Draw with multiple glow layers
                spriteBatch.Draw(midTexture, drawPos, null, segmentColor * 0.25f, adjustedRotation, 
                    midOrigin, new Vector2(widthScale, widthScale * 1.6f), SpriteEffects.None, 0f);
                spriteBatch.Draw(midTexture, drawPos, null, segmentColor * 0.5f, adjustedRotation, 
                    midOrigin, new Vector2(widthScale, widthScale * 1.2f), SpriteEffects.None, 0f);
                spriteBatch.Draw(midTexture, drawPos, null, segmentColor * 0.8f, adjustedRotation, 
                    midOrigin, widthScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(midTexture, drawPos, null, Color.White * fadeAlpha * 0.3f, adjustedRotation, 
                    midOrigin, new Vector2(widthScale, widthScale * 0.5f), SpriteEffects.None, 0f);
            }

            // === DRAW END PIECE ===
            {
                float endWave = GetWaveOffset(beamLength);
                Vector2 endPos = Projectile.Center + direction * beamLength + perpendicular * endWave - Main.screenPosition;
                
                float colorPhase = (Main.GameUpdateCount * ColorCycleSpeed + 1f) % 1f;
                Color endColor = GetBeamColor(colorPhase) * fadeAlpha;
                
                Vector2 endOrigin = new Vector2(endTexture.Width, endTexture.Height / 2f);
                float endScale = widthScale * 1.2f;
                
                // Draw with glow layers
                spriteBatch.Draw(endTexture, endPos, null, endColor * 0.3f, rotation, 
                    endOrigin, endScale * 1.5f, SpriteEffects.None, 0f);
                spriteBatch.Draw(endTexture, endPos, null, endColor * 0.6f, rotation, 
                    endOrigin, endScale * 1.1f, SpriteEffects.None, 0f);
                spriteBatch.Draw(endTexture, endPos, null, Color.White * fadeAlpha * 0.4f, rotation, 
                    endOrigin, endScale * 0.8f, SpriteEffects.None, 0f);
            }

            // === ADDITIONAL SINUSOIDAL COLOR OVERLAY ===
            // Draw a second pass with offset color for extra visual richness
            DrawColorOverlay(spriteBatch, beamLength, rotation, direction, perpendicular, fadeAlpha, widthScale);
        }

        /// <summary>
        /// Draws an additional color overlay with shifted phase for richer visuals.
        /// </summary>
        private void DrawColorOverlay(SpriteBatch spriteBatch, float beamLength, float rotation, 
            Vector2 direction, Vector2 perpendicular, float fadeAlpha, float widthScale)
        {
            Texture2D midTexture = midTex.Value;
            float segmentLength = midTexture.Width * widthScale;
            int segmentCount = (int)Math.Ceiling(beamLength / segmentLength);
            
            // Offset phase for secondary color layer
            float phaseOffset = MathHelper.Pi;
            
            for (int i = 0; i < segmentCount; i++)
            {
                float segmentStart = i * segmentLength;
                float segmentEnd = Math.Min((i + 1) * segmentLength, beamLength);
                float segmentMid = (segmentStart + segmentEnd) / 2f;
                
                // Offset wave for overlay
                float waveOffset = GetWaveOffset(segmentMid + 50f);
                
                Vector2 segmentPos = Projectile.Center + direction * segmentStart + perpendicular * waveOffset * 0.5f;
                Vector2 drawPos = segmentPos - Main.screenPosition;
                
                // Offset color phase
                float t = segmentMid / beamLength;
                float colorPhase = (Main.GameUpdateCount * ColorCycleSpeed + t * 2f + 0.5f) % 1f;
                Color overlayColor = GetBeamColor(colorPhase) * fadeAlpha * 0.3f;
                
                float waveSlope = (float)Math.Cos((segmentMid + 50f) * WaveFrequency + Projectile.localAI[1]) * WaveAmplitude * WaveFrequency * 0.5f;
                float adjustedRotation = rotation + (float)Math.Atan(waveSlope);
                
                Vector2 midOrigin = new Vector2(0, midTexture.Height / 2f);
                
                spriteBatch.Draw(midTexture, drawPos, null, overlayColor, adjustedRotation, 
                    midOrigin, new Vector2(widthScale, widthScale * 0.8f), SpriteEffects.None, 0f);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Impact VFX
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * 4f;
                Color color = i % 2 == 0 ? new Color(255, 50, 50) : new Color(255, 200, 50);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.Torch, vel, 0, color, 1.5f);
                d.noGravity = true;
            }
            
            Lighting.AddLight(target.Center, 1f, 0.6f, 0.2f);
        }
    }
}
