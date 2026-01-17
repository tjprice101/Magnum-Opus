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
    /// Beam projectile using procedural drawing with layered glows.
    /// Features sinusoidal wavering and red-gold color cycling.
    /// Inspired by Calamity's ProvidenceHolyRay - proper thick beam rendering.
    /// </summary>
    public class Debug2RayBeam : ModProjectile
    {
        // Use soft glow texture for drawing
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";

        // Beam parameters
        private const float MaxBeamLength = 1200f;
        private const float BeamWidth = 50f;
        private const int MaxLifetime = 45;
        
        // Wave parameters for sinusoidal effect
        private const float WaveAmplitude = 12f;     // How much the beam wavers side-to-side
        private const float WaveFrequency = 0.12f;   // How fast the waves occur along length
        private const float WaveSpeed = 0.25f;       // How fast the waves animate
        private const float ColorCycleSpeed = 0.08f; // Speed of red/gold color cycling

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
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // Switch to additive blending for glow effect
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, 
                SamplerState.LinearClamp, DepthStencilState.None, 
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            DrawProceduralBeam(spriteBatch);

            // Return to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, 
                SamplerState.LinearClamp, DepthStencilState.None, 
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false; // Don't draw default sprite
        }

        private void DrawProceduralBeam(SpriteBatch spriteBatch)
        {
            // Use pixel texture for beam body and glow texture for effects
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Texture2D glow = TextureAssets.Extra[98].Value;

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

            // === DRAW BEAM AS LAYERED SEGMENTS ===
            int segmentCount = (int)(beamLength / 8f); // More segments for smoother curve
            
            for (int i = 0; i < segmentCount; i++)
            {
                float t = (float)i / segmentCount;
                float nextT = (float)(i + 1) / segmentCount;
                
                float dist = t * beamLength;
                float nextDist = nextT * beamLength;
                
                // Calculate wave offset at this segment
                float waveOffset = GetWaveOffset(dist);
                float nextWaveOffset = GetWaveOffset(nextDist);
                
                // Position with wave applied
                Vector2 segmentPos = Projectile.Center + direction * dist + perpendicular * waveOffset;
                Vector2 nextPos = Projectile.Center + direction * nextDist + perpendicular * nextWaveOffset;
                Vector2 drawPos = segmentPos - Main.screenPosition;
                
                // Calculate segment direction for proper rotation
                Vector2 segmentDir = (nextPos - segmentPos).SafeNormalize(direction);
                float segmentRotation = segmentDir.ToRotation();
                float segmentLength = Vector2.Distance(segmentPos, nextPos);
                
                // Color cycling along beam length
                float colorPhase = (Main.GameUpdateCount * ColorCycleSpeed + t * 2f) % 1f;
                Color segmentColor = GetBeamColor(colorPhase) * fadeAlpha;
                
                // === LAYER 1: Outer glow (largest, most transparent) ===
                spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), 
                    segmentColor * 0.15f, segmentRotation, 
                    new Vector2(0, 0.5f), new Vector2(segmentLength + 4f, currentWidth * 2.5f), 
                    SpriteEffects.None, 0f);
                
                // === LAYER 2: Mid glow ===
                spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), 
                    segmentColor * 0.35f, segmentRotation, 
                    new Vector2(0, 0.5f), new Vector2(segmentLength + 2f, currentWidth * 1.6f), 
                    SpriteEffects.None, 0f);
                
                // === LAYER 3: Core beam (brightest) ===
                spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), 
                    segmentColor * 0.7f, segmentRotation, 
                    new Vector2(0, 0.5f), new Vector2(segmentLength, currentWidth), 
                    SpriteEffects.None, 0f);
                
                // === LAYER 4: White hot center ===
                spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), 
                    Color.White * fadeAlpha * 0.5f, segmentRotation, 
                    new Vector2(0, 0.5f), new Vector2(segmentLength, currentWidth * 0.4f), 
                    SpriteEffects.None, 0f);
            }

            // === DRAW START GLOW (bright origin point) ===
            {
                float startWave = GetWaveOffset(0);
                Vector2 startPos = Projectile.Center + perpendicular * startWave - Main.screenPosition;
                
                float colorPhase = (Main.GameUpdateCount * ColorCycleSpeed) % 1f;
                Color startColor = GetBeamColor(colorPhase) * fadeAlpha;
                
                // Multiple glow layers at start
                spriteBatch.Draw(glow, startPos, null, startColor * 0.3f, 0f, 
                    glow.Size() / 2f, 1.0f * Projectile.scale, SpriteEffects.None, 0f);
                spriteBatch.Draw(glow, startPos, null, startColor * 0.5f, 0f, 
                    glow.Size() / 2f, 0.6f * Projectile.scale, SpriteEffects.None, 0f);
                spriteBatch.Draw(glow, startPos, null, Color.White * fadeAlpha * 0.6f, 0f, 
                    glow.Size() / 2f, 0.3f * Projectile.scale, SpriteEffects.None, 0f);
            }

            // === DRAW END GLOW ===
            {
                float endWave = GetWaveOffset(beamLength);
                Vector2 endPos = Projectile.Center + direction * beamLength + perpendicular * endWave - Main.screenPosition;
                
                float colorPhase = (Main.GameUpdateCount * ColorCycleSpeed + 1f) % 1f;
                Color endColor = GetBeamColor(colorPhase) * fadeAlpha;
                
                // Glow at end point
                spriteBatch.Draw(glow, endPos, null, endColor * 0.4f, 0f, 
                    glow.Size() / 2f, 0.7f * Projectile.scale, SpriteEffects.None, 0f);
                spriteBatch.Draw(glow, endPos, null, Color.White * fadeAlpha * 0.5f, 0f, 
                    glow.Size() / 2f, 0.35f * Projectile.scale, SpriteEffects.None, 0f);
            }
            
            // === SPARKLE NODES ALONG BEAM ===
            int nodeCount = 6;
            for (int i = 1; i < nodeCount; i++)
            {
                float t = (float)i / nodeCount;
                float dist = t * beamLength;
                float waveOffset = GetWaveOffset(dist);
                Vector2 nodePos = Projectile.Center + direction * dist + perpendicular * waveOffset - Main.screenPosition;
                
                float colorPhase = (Main.GameUpdateCount * ColorCycleSpeed + t * 3f) % 1f;
                Color nodeColor = GetBeamColor(colorPhase) * fadeAlpha;
                
                float nodeScale = 0.25f + (float)Math.Sin(Main.GameUpdateCount * 0.15f + t * MathHelper.TwoPi) * 0.1f;
                spriteBatch.Draw(glow, nodePos, null, nodeColor * 0.6f, 0f, 
                    glow.Size() / 2f, nodeScale * Projectile.scale, SpriteEffects.None, 0f);
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
