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
    /// The Conductor's Last Constellation - A cosmic blade that sends out 3 homing spectral beams.
    /// Creates glass-like visual distortion on swing.
    /// On hit, cosmic lightning strikes the target 3 times.
    /// </summary>
    public class TheConductorsLastConstellation : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/TheConductorsLastConstellation";
        
        public override void SetDefaults()
        {
            Item.damage = 780;
            Item.DamageType = DamageClass.Melee;
            Item.width = 50;
            Item.height = 50;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<SpectralSwordBeam>();
            Item.shootSpeed = 14f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Each swing releases 3 homing spectral beams"));
            tooltips.Add(new TooltipLine(Mod, "FateSpecial", "Strikes call down cosmic lightning three times"));
            tooltips.Add(new TooltipLine(Mod, "FateVisual", "Swings create a glass-shattering distortion effect"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The final symphony, written in starlight'")
            {
                OverrideColor = FatePalette.BrightCrimson
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Spawn 3 homing spectral Terrablade-style beams in a spread
            float baseAngle = velocity.ToRotation();
            float spreadAngle = MathHelper.ToRadians(18f);

            for (int i = 0; i < 3; i++)
            {
                float angleOffset = MathHelper.Lerp(-spreadAngle, spreadAngle, (i + 0.5f) / 3f);
                Vector2 projVelocity = (baseAngle + angleOffset).ToRotationVector2() * velocity.Length();
                Vector2 spawnPos = player.Center + projVelocity.SafeNormalize(Vector2.Zero) * 40f;

                Projectile.NewProjectile(source, spawnPos, projVelocity, type, damage, knockback, player.whoAmI);
            }

            // Spawn glass distortion effect at swing origin
            Vector2 distortionPos = player.Center + velocity.SafeNormalize(Vector2.Zero) * 50f;
            Projectile.NewProjectile(source, distortionPos, velocity * 0.5f,
                ModContent.ProjectileType<GlassDistortionEffect>(), 0, 0f, player.whoAmI,
                velocity.ToRotation());

            // Track for star circle effect
            player.GetModPlayer<FateWeaponEffectPlayer>()?.OnFateWeaponAttack(player.Center);

            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.4f, Volume = 0.9f }, player.Center);

            return false;
        }

        public override void HoldItem(Player player)
        {
            TheConductorsLastConstellationVFX.HoldItemVFX(player);
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            TheConductorsLastConstellationVFX.SwingVFX(hitbox.Center.ToVector2(), player);
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Fate debuff
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);

            // Call down cosmic lightning 3 times with staggered timing
            for (int strike = 0; strike < 3; strike++)
            {
                Vector2 strikeOffset = Main.rand.NextVector2Circular(25f, 25f);
                TheConductorsLastConstellationVFX.LightningStrikeVFX(target.Center + strikeOffset, 1.0f + strike * 0.15f);
            }

            // Impact VFX
            TheConductorsLastConstellationVFX.ImpactVFX(target.Center);

            // Spawn seeking crystals on every hit - Fate ultimate melee power
            SeekingCrystalHelper.SpawnFateCrystals(
                player.GetSource_ItemUse(Item),
                target.Center,
                (Main.MouseWorld - target.Center).SafeNormalize(Vector2.UnitX) * 10f,
                (int)(damageDone * 0.25f),
                Item.knockBack * 0.5f,
                player.whoAmI,
                5); // 5 crystals for ultimate weapon
        }
    }
}
