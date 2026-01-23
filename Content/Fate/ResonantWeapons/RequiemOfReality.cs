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
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Fate.Debuffs;
using MagnumOpus.Content.Fate.Projectiles;

namespace MagnumOpus.Content.Fate.ResonantWeapons
{
    /// <summary>
    /// Requiem of Reality - A blade that plays existence's funeral march.
    /// Swings release cosmic music notes that float in place briefly, then seek nearby enemies.
    /// Every 4th swing spawns a spectral blade that performs a combo attack - flies up, spins above head,
    /// explodes, seeks nearest enemy, slashes through twice, and returns.
    /// Notes deal damage with cosmic flames and electricity on impact.
    /// The player continues swinging while the spectral blade performs its combo.
    /// </summary>
    public class RequiemOfReality : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/RequiemOfReality";
        
        // Tracks swings for combo trigger
        private int swingCounter = 0;
        private const int SwingsForCombo = 4;
        
        public override void SetDefaults()
        {
            Item.damage = 740;
            Item.DamageType = DamageClass.Melee;
            Item.width = 58;
            Item.height = 58;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5.5f;
            Item.value = Item.sellPrice(gold: 56);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<CosmicMusicNoteProjectile>();
            Item.shootSpeed = 8f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Swings release cosmic music notes that float, then seek enemies"));
            tooltips.Add(new TooltipLine(Mod, "FateSpecial", "Every 4th swing summons a spectral blade that explodes and slashes through enemies"));
            tooltips.Add(new TooltipLine(Mod, "FateSpecial2", "Notes explode with cosmic flames and electricity on contact"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The final requiem for a dying reality'")
            {
                OverrideColor = FateCosmicVFX.FateBrightRed
            });
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Track for star circle effect
            player.GetModPlayer<FateWeaponEffectPlayer>()?.OnFateWeaponAttack(player.Center);
            
            // Increment swing counter
            swingCounter++;
            
            // Check if we should spawn the spectral blade combo (every 4th swing)
            bool shouldSpawnCombo = swingCounter >= SwingsForCombo && 
                player.ownedProjectileCounts[ModContent.ProjectileType<RequiemSpectralBladeProjectile>()] < 1;
            
            if (shouldSpawnCombo)
            {
                // Reset counter
                swingCounter = 0;
                
                // Spawn the spectral blade combo projectile (independent of player)
                Projectile.NewProjectile(source, player.Center, Vector2.Zero, 
                    ModContent.ProjectileType<RequiemSpectralBladeProjectile>(), damage * 2, knockback, player.whoAmI);
                
                // Dramatic spawn VFX
                FateCosmicVFX.SpawnGlyphBurst(player.Center, 8, 6f, 0.45f);
                FateCosmicVFX.SpawnCosmicMusicNotes(player.Center, 6, 40f, 0.35f);
                FateCosmicVFX.SpawnCosmicCloudBurst(player.Center, 0.6f, 12);
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.8f }, player.Center);
            }
            
            // Always spawn music notes on every swing
            int noteCount = Main.rand.Next(3, 6);
            float baseAngle = velocity.ToRotation();
            
            for (int i = 0; i < noteCount; i++)
            {
                float angleOffset = MathHelper.Lerp(-0.4f, 0.4f, (i + 0.5f) / noteCount);
                Vector2 noteVel = (baseAngle + angleOffset).ToRotationVector2() * velocity.Length() * Main.rand.NextFloat(0.8f, 1.2f);
                Vector2 spawnPos = player.Center + noteVel.SafeNormalize(Vector2.Zero) * 35f;
                
                // ai[0] = type of note (0-3 for different musical notes)
                // ai[1] = delay before seeking (frames)
                Projectile.NewProjectile(source, spawnPos, noteVel, type, damage, knockback, player.whoAmI, 
                    Main.rand.Next(4), // Random note type
                    Main.rand.Next(30, 60) // Random delay before seeking
                );
            }
            
            // Musical spawn VFX
            FateCosmicVFX.SpawnCosmicMusicNotes(player.Center + velocity.SafeNormalize(Vector2.Zero) * 30f, 2, 15f, 0.25f);
            
            SoundEngine.PlaySound(SoundID.Item26 with { Pitch = 0.5f, Volume = 0.7f }, player.Center);
            
            return false;
        }
        
        public override void HoldItem(Player player)
        {
            // === COSMIC SYMPHONY HOLD EFFECT ===
            // Floating music notes orbit
            if (Main.rand.NextBool(6))
            {
                float angle = Main.GameUpdateCount * 0.025f + Main.rand.NextFloat(MathHelper.Pi);
                Vector2 notePos = player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(30f, 50f);
                FateCosmicVFX.SpawnCosmicMusicNotes(notePos, 1, 8f, 0.22f);
            }
            
            // Glyphs in rhythm pattern
            if (Main.rand.NextBool(12))
            {
                float rhythmOffset = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 25f;
                CustomParticles.Glyph(player.Center + new Vector2(rhythmOffset, -20f), FateCosmicVFX.FateDarkPink, 0.3f, -1);
            }
            
            // Star sparkle accompaniment
            if (Main.rand.NextBool(7))
            {
                var star = new GenericGlowParticle(player.Center + Main.rand.NextVector2Circular(35f, 35f),
                    Main.rand.NextVector2Circular(0.6f, 0.6f), FateCosmicVFX.FateWhite, 0.18f, 18, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
            
            // Harmonic light pulse (synced to musical rhythm)
            float rhythmPulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.2f + 0.8f;
            Lighting.AddLight(player.Center, FateCosmicVFX.FateNebulaPurple.ToVector3() * rhythmPulse * 0.4f);
        }
        
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Vector2 swingPos = hitbox.Center.ToVector2();
            
            // Cosmic sparks and music notes from swing
            if (Main.rand.NextBool(2))
            {
                Color sparkColor = FateCosmicVFX.GetCosmicGradient(Main.rand.NextFloat());
                Vector2 sparkVel = new Vector2(player.direction * 3f, Main.rand.NextFloat(-2f, 2f));
                var spark = new GlowSparkParticle(swingPos + Main.rand.NextVector2Circular(15f, 15f), sparkVel, sparkColor, 0.22f, 12);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Music notes scatter from swing
            if (Main.rand.NextBool(4))
            {
                FateCosmicVFX.SpawnCosmicMusicNotes(swingPos, 1, 20f, 0.2f);
            }
        }
        
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
            
            // Impact with cosmic flames and lightning
            FateCosmicVFX.SpawnCosmicExplosion(target.Center, 0.9f);
            FateCosmicVFX.SpawnCosmicLightningStrike(target.Center, 0.8f);
            FateCosmicVFX.SpawnCosmicMusicNotes(target.Center, 5, 40f, 0.35f);
            FateCosmicVFX.SpawnGlyphBurst(target.Center, 4, 5f, 0.35f);
            
            // Star particles
            for (int i = 0; i < 8; i++)
            {
                Vector2 starOffset = Main.rand.NextVector2Circular(35f, 35f);
                var star = new GenericGlowParticle(target.Center + starOffset, Main.rand.NextVector2Circular(2f, 2f), 
                    FateCosmicVFX.FateWhite, 0.28f, 20, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
            
            Lighting.AddLight(target.Center, FateCosmicVFX.FateBrightRed.ToVector3() * 1.3f);
        }
    }
}
