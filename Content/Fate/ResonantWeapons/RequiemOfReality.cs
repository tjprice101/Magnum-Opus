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
                OverrideColor = FatePalette.BrightCrimson
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
                RequiemOfRealityVFX.SpectralBladeComboSpawnVFX(player.Center);
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
            RequiemOfRealityVFX.MusicNoteSpawnVFX(player.Center + velocity.SafeNormalize(Vector2.Zero) * 30f);

            SoundEngine.PlaySound(SoundID.Item26 with { Pitch = 0.5f, Volume = 0.7f }, player.Center);

            return false;
        }

        public override void HoldItem(Player player)
        {
            RequiemOfRealityVFX.HoldItemVFX(player);
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // === SPECTACULAR SWING SYSTEM - ENDGAME TIER (7-8 arcs + cosmic music notes) ===
            SpectacularMeleeSwing.OnSwing(player, hitbox, FatePalette.DarkPink, FatePalette.BrightCrimson,
                SpectacularMeleeSwing.SwingTier.Endgame, SpectacularMeleeSwing.WeaponTheme.Fate);

            RequiemOfRealityVFX.SwingVFX(hitbox.Center.ToVector2(), player);
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);

            // Impact VFX
            RequiemOfRealityVFX.ImpactVFX(target.Center);

            // Spawn seeking crystals on every hit - Fate endgame power
            if (Main.rand.NextBool(3)) // 33% chance per hit
            {
                SeekingCrystalHelper.SpawnFateCrystals(
                    player.GetSource_ItemUse(Item),
                    target.Center,
                    (Main.MouseWorld - target.Center).SafeNormalize(Vector2.UnitX) * 10f,
                    (int)(damageDone * 0.2f),
                    Item.knockBack * 0.4f,
                    player.whoAmI,
                    3);
            }
        }
    }
}
