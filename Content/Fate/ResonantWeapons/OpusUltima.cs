// DEPRECATED: Replaced by OpusUltima/ folder self-contained system
/*
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
    /// Opus Ultima - The final masterwork blade.
    /// Fires a big ball of cosmic energy that explodes into 5 smaller seeker balls on enemy hit.
    /// </summary>
    public class OpusUltima : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/OpusUltima";
        
        public override void SetDefaults()
        {
            Item.damage = 720;
            Item.DamageType = DamageClass.Melee;
            Item.width = 50;
            Item.height = 50;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 52);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<CosmicEnergyBall>();
            Item.shootSpeed = 12f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Swings release a large cosmic energy ball"));
            tooltips.Add(new TooltipLine(Mod, "FateSpecial", "On enemy hit, explodes into 5 homing seeker balls"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The ultimate composition, the magnum opus of destruction'")
            {
                OverrideColor = FatePalette.BrightCrimson
            });
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Spawn the big cosmic energy ball
            Vector2 spawnPos = player.Center + velocity.SafeNormalize(Vector2.Zero) * 40f;
            Projectile.NewProjectile(source, spawnPos, velocity, type, damage, knockback, player.whoAmI);
            
            // Track for star circle effect
            player.GetModPlayer<FateWeaponEffectPlayer>()?.OnFateWeaponAttack(player.Center);
            
            // Cosmic spawn VFX
            FateVFXLibrary.ProjectileImpact(spawnPos, 0.6f);
            FateVFXLibrary.SpawnGlyphBurst(spawnPos, 3, 4f);
            
            SoundEngine.PlaySound(SoundID.Item117 with { Pitch = 0.3f, Volume = 0.9f }, player.Center);
            
            return false;
        }
        
        public override void HoldItem(Player player)
        {
            OpusUltimaVFX.HoldItemVFX(player);
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // === SPECTACULAR SWING SYSTEM - ENDGAME TIER (7-8 arcs + cosmic clouds + glyphs) ===
            SpectacularMeleeSwing.OnSwing(player, hitbox, FatePalette.DarkPink, FatePalette.BrightCrimson,
                SpectacularMeleeSwing.SwingTier.Endgame, SpectacularMeleeSwing.WeaponTheme.Fate);

            OpusUltimaVFX.SwingVFX(hitbox.Center.ToVector2(), player);
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 240);

            // Impact VFX
            OpusUltimaVFX.ImpactVFX(target.Center);

            // === SPAWN SEEKING CRYSTALS - THE MAIN DAMAGE SOURCE ===
            // On hit, release 3-5 homing crystal projectiles that seek nearby enemies
            Vector2 crystalDir = (target.Center - player.Center).SafeNormalize(Vector2.UnitX);
            int crystalCount = hit.Crit ? 5 : 3;
            SeekingCrystalHelper.SpawnFateCrystals(
                player.GetSource_ItemUse(player.HeldItem),
                target.Center,
                crystalDir * 8f,
                (int)(damageDone * 0.4f), // 40% of hit damage per crystal
                2f,
                player.whoAmI,
                crystalCount
            );
        }
    }
}
*/
