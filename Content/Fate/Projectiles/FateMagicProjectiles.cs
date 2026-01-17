using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Fate.Debuffs;

namespace MagnumOpus.Content.Fate.Projectiles
{
    #region Fate1Magic Projectiles - Cosmic Electricity

    /// <summary>
    /// Held staff that channels cosmic electricity
    /// </summary>
    public class CosmicElectricityStaff : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.MagnetSphere;

        private HashSet<int> hitEnemies = new HashSet<int>();
        private int zapCooldown = 0;

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!owner.active || owner.dead || !owner.channel)
            {
                Projectile.Kill();
                return;
            }

            // Position at player
            Projectile.Center = owner.Center;
            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;

            zapCooldown--;

            // Find and shock all nearby enemies
            if (zapCooldown <= 0)
            {
                zapCooldown = 8;
                ZapNearbyEnemies(owner);
            }

            // Check for zodiac explosion
            if (hitEnemies.Count >= 3)
            {
                TriggerZodiacExplosion(owner);
                hitEnemies.Clear();
            }

            // Electricity aura around player
            if (Main.rand.NextBool(3))
            {
                FateCosmicVFX.SpawnCosmicElectricity(owner.Center, 2, 60f, 0.6f);
            }

            // Music notes
            if (Main.rand.NextBool(8))
            {
                FateCosmicVFX.SpawnCosmicMusicNotes(owner.Center, 1, 40f, 0.3f);
            }

            Lighting.AddLight(owner.Center, FateCosmicVFX.FateCyan.ToVector3() * 0.8f);
        }

        private void ZapNearbyEnemies(Player owner)
        {
            float range = 350f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;

                float dist = Vector2.Distance(owner.Center, npc.Center);
                if (dist < range)
                {
                    // Draw lightning to enemy
                    FateCosmicVFX.DrawCosmicLightning(owner.Center, npc.Center, 10, 30f, 2, 0.3f, FateCosmicVFX.FateCyan, FateCosmicVFX.FateWhite);

                    // Deal damage
                    npc.SimpleStrikeNPC(Projectile.damage, owner.direction, false, 0f);
                    npc.AddBuff(ModContent.BuffType<DestinyCollapse>(), 120);

                    // Track unique enemies hit
                    hitEnemies.Add(npc.whoAmI);

                    // Impact sparks
                    FateCosmicVFX.SpawnStarSparkles(npc.Center, 4, 20f, 0.2f);
                }
            }

            if (hitEnemies.Count > 0)
            {
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 0.6f }, owner.Center);
            }
        }

        private void TriggerZodiacExplosion(Player owner)
        {
            // Spawn zodiac explosion projectile
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                owner.Center,
                Vector2.Zero,
                ModContent.ProjectileType<ZodiacExplosion>(),
                (int)(Projectile.damage * 3f),
                10f,
                Projectile.owner
            );

            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.2f, Volume = 1.2f }, owner.Center);
        }

        public override bool? CanDamage() => false;

        public override bool PreDraw(ref Color lightColor)
        {
            // Don't draw the staff itself, just use VFX
            return false;
        }
    }

    /// <summary>
    /// Screen-wide zodiac explosion effect
    /// </summary>
    public class ZodiacExplosion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";

        private const int Duration = 60;

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Duration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Projectile.Center = owner.Center;

            float progress = 1f - (float)Projectile.timeLeft / Duration;
            
            // Expanding zodiac circle
            float radius = progress * 600f;
            
            // Update hitbox to match explosion radius
            int size = (int)(radius * 2);
            Projectile.width = Projectile.height = Math.Max(50, size);

            // Zodiac symbols around the circle
            int symbolCount = 12; // 12 zodiac signs
            for (int i = 0; i < symbolCount; i++)
            {
                float angle = MathHelper.TwoPi * i / symbolCount + Main.GameUpdateCount * 0.02f;
                Vector2 symbolPos = owner.Center + angle.ToRotationVector2() * radius;
                
                // Glyph at each zodiac position
                Color glyphColor = FateCosmicVFX.GetCosmicGradient((float)i / symbolCount);
                var glyph = new GenericGlowParticle(symbolPos, angle.ToRotationVector2() * 2f, glyphColor, 0.4f, 8, true);
                MagnumParticleHandler.SpawnParticle(glyph);

                // Star at each position
                var star = new GenericGlowParticle(symbolPos, Vector2.Zero, FateCosmicVFX.FateWhite * 0.8f, 0.3f, 8, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Connecting lines between symbols
            if (Main.GameUpdateCount % 3 == 0)
            {
                for (int i = 0; i < symbolCount; i++)
                {
                    float angle1 = MathHelper.TwoPi * i / symbolCount + Main.GameUpdateCount * 0.02f;
                    float angle2 = MathHelper.TwoPi * ((i + 1) % symbolCount) / symbolCount + Main.GameUpdateCount * 0.02f;
                    Vector2 pos1 = owner.Center + angle1.ToRotationVector2() * radius;
                    Vector2 pos2 = owner.Center + angle2.ToRotationVector2() * radius;
                    
                    FateCosmicVFX.SpawnConstellationLine(pos1, pos2, FateCosmicVFX.FatePurple * 0.5f);
                }
            }

            // Central cosmic explosion building
            float centralIntensity = progress;
            FateCosmicVFX.SpawnCosmicCloudBurst(owner.Center, centralIntensity * 0.5f, 8);
            
            // Music notes bursting outward
            if (Main.rand.NextBool(2))
            {
                float randAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 notePos = owner.Center + randAngle.ToRotationVector2() * radius * 0.5f;
                FateCosmicVFX.SpawnCosmicMusicNotes(notePos, 2, 20f, 0.35f);
            }

            // Lightning effects
            if (Main.rand.NextBool(4))
            {
                float randAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 lightningEnd = owner.Center + randAngle.ToRotationVector2() * radius;
                FateCosmicVFX.DrawCosmicLightning(owner.Center, lightningEnd, 12, 40f, 3, 0.4f);
            }

            Lighting.AddLight(owner.Center, FateCosmicVFX.FateDarkPink.ToVector3() * (1f + progress));
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float progress = 1f - (float)Projectile.timeLeft / Duration;
            float radius = progress * 600f;
            float innerRadius = radius * 0.8f;

            Vector2 targetCenter = targetHitbox.Center.ToVector2();
            float dist = Vector2.Distance(Projectile.Center, targetCenter);

            // Damage enemies in the expanding ring
            return dist >= innerRadius && dist <= radius;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
            FateCosmicVFX.SpawnCosmicExplosion(target.Center, 0.5f);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    #endregion

    #region Fate2Magic Projectiles - Spectral Sword Blades

    /// <summary>
    /// Spectral sword blade that spirals toward cursor
    /// </summary>
    public class SpiralingSpectralBlade : ModProjectile
    {
        // Use random sword texture
        private static readonly int[] SwordTextures = new int[] 
        { 
            ItemID.TrueNightsEdge, 
            ItemID.TrueExcalibur, 
            ItemID.Terragrim,
            ItemID.InfluxWaver,
            ItemID.Meowmere
        };

        private int swordTextureIndex = 0;
        private Vector2 targetPosition;
        private float spiralAngle = 0f;
        private float spiralRadius = 0f;

        public override string Texture => "Terraria/Images/Item_" + ItemID.TrueNightsEdge;

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
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // Pick random sword texture
            swordTextureIndex = Main.rand.Next(SwordTextures.Length);
            
            // Target is cursor position
            targetPosition = Main.MouseWorld;
            
            // Initial spiral parameters
            spiralRadius = Vector2.Distance(Projectile.Center, targetPosition);
            spiralAngle = (targetPosition - Projectile.Center).ToRotation();
        }

        public override void AI()
        {
            // Spiral toward target
            spiralAngle += 0.15f;
            spiralRadius = Math.Max(0, spiralRadius - 8f);

            Vector2 desiredPos = targetPosition + spiralAngle.ToRotationVector2() * spiralRadius;
            Projectile.velocity = (desiredPos - Projectile.Center) * 0.3f;
            
            // Rotate blade to face movement
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Trail
            FateCosmicVFX.SpawnSpectralSwordTrail(Projectile.Center, Projectile.velocity, 0.6f);

            // Music notes occasionally
            if (Main.rand.NextBool(10))
            {
                FateCosmicVFX.SpawnCosmicMusicNotes(Projectile.Center, 1, 15f, 0.25f);
            }

            // Explode when reaching target
            if (spiralRadius < 20f)
            {
                Projectile.Kill();
            }

            Lighting.AddLight(Projectile.Center, FateCosmicVFX.FateDarkPink.ToVector3() * 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);
        }

        public override void OnKill(int timeLeft)
        {
            FateCosmicVFX.SpawnCosmicExplosion(Projectile.Center, 0.7f);
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.7f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("Terraria/Images/Item_" + SwordTextures[swordTextureIndex]).Value;
            Vector2 origin = tex.Size() / 2f;

            // Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Color trailColor = FateCosmicVFX.GetCosmicGradient(progress) * (1f - progress) * 0.5f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(tex, trailPos, null, trailColor, Projectile.oldRot[i], origin, 1f - progress * 0.5f, SpriteEffects.None, 0f);
            }

            // Glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateDarkPink * 0.4f, Projectile.rotation, origin, 1.2f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Main blade
            spriteBatch.Draw(tex, drawPos, null, Color.White, Projectile.rotation, origin, 1f, SpriteEffects.None, 0f);

            return false;
        }
    }

    #endregion

    #region Cinematic Star Circle - Player Attack Effect

    /// <summary>
    /// Player ModPlayer to track weapon usage and trigger star circle effect
    /// </summary>
    public class FateWeaponEffectPlayer : ModPlayer
    {
        private int attackCounter = 0;
        private const int AttacksForStarCircle = 6;

        /// <summary>
        /// Call this when a Fate weapon attacks. Position parameter for potential future use.
        /// </summary>
        public void OnFateWeaponAttack(Vector2 attackPosition)
        {
            attackCounter++;

            if (attackCounter >= AttacksForStarCircle)
            {
                attackCounter = 0;
                FateCosmicVFX.TriggerStarCircleEffect(Player);
            }
        }

        public override void ResetEffects()
        {
            // Reset counter if not attacking for a while
            if (Player.itemAnimation == 0)
            {
                // Decay counter slowly
                if (Main.GameUpdateCount % 60 == 0 && attackCounter > 0)
                {
                    attackCounter--;
                }
            }
        }
    }

    #endregion
}
