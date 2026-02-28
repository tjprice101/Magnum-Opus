using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Particles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Projectiles;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime
{
    /// <summary>
    /// GrandioseChime — Ranged Beam weapon, 240dmg, useTime 6, knockBack 5, uses Bullet ammo.
    /// Every 3rd shot = bellfire barrage (7 burning note projectiles in fan spread).
    /// Every 4th shot = music note mines (3 slow-drifting mines that detonate on enemy proximity).
    /// Kill echoes: recently killed enemies spawn echo burst projectiles.
    /// </summary>
    public class GrandioseChimeItem : ModItem
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/GrandioseChime/GrandioseChime";
        public override string Name => "GrandioseChime";

        public override void SetDefaults()
        {
            Item.damage = 240;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 58;
            Item.height = 28;
            Item.useTime = 6;
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<LaCampanellaRainbowRarity>();
            Item.UseSound = SoundID.Item33;
            Item.shoot = ProjectileID.Bullet;
            Item.useAmmo = AmmoID.Bullet;
            Item.shootSpeed = 20f;
            Item.noMelee = true;
            Item.autoReuse = true;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // Small spread
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(2f));
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var modPlayer = player.GetModPlayer<GrandioseChimePlayer>();
            int shotType = modPlayer.RegisterShot();

            Vector2 muzzlePos = position + Vector2.Normalize(velocity) * 50f;
            float angle = velocity.ToRotation();

            // Muzzle flash
            GrandioseChimeParticleHandler.SpawnParticle(new GrandioseBeamFlashParticle(
                muzzlePos, angle, Main.rand.NextFloat(40f, 60f), Main.rand.Next(5, 10)));

            // Always fire the base beam projectile
            Projectile.NewProjectile(source, muzzlePos, velocity,
                ModContent.ProjectileType<GrandioseBeamProj>(), damage, knockback, player.whoAmI);

            // Barrage (3rd shot or 12th = both)
            if (shotType == 1 || shotType == 3)
            {
                SpawnBellfireBarrage(source, muzzlePos, velocity, damage, knockback, player);
            }

            // Mines (4th shot or 12th = both)
            if (shotType == 2 || shotType == 3)
            {
                SpawnNoteMines(source, muzzlePos, velocity, damage, knockback, player);
            }

            return false;
        }

        private void SpawnBellfireBarrage(EntitySource_ItemUse_WithAmmo source, Vector2 pos, Vector2 vel, int damage, float kb, Player player)
        {
            // 7 burning note projectiles in a fan spread
            float totalSpread = MathHelper.ToRadians(50f);
            float startAngle = vel.ToRotation() - totalSpread / 2f;

            for (int i = 0; i < 7; i++)
            {
                float noteAngle = startAngle + totalSpread / 6f * i;
                Vector2 noteVel = new Vector2((float)Math.Cos(noteAngle), (float)Math.Sin(noteAngle)) * vel.Length() * 0.8f;

                Projectile.NewProjectile(source, pos, noteVel,
                    ModContent.ProjectileType<BellfireNoteProj>(), (int)(damage * 0.6f), kb * 0.5f, player.whoAmI);

                // Barrage burst particles
                GrandioseChimeParticleHandler.SpawnParticle(new BurningNoteParticle(
                    pos, noteVel * 0.15f, Main.rand.Next(20, 35)));
            }
        }

        private void SpawnNoteMines(EntitySource_ItemUse_WithAmmo source, Vector2 pos, Vector2 vel, int damage, float kb, Player player)
        {
            // 3 slow-drifting mines in a triangle spread
            for (int i = 0; i < 3; i++)
            {
                float offset = MathHelper.ToRadians(30f) * (i - 1);
                Vector2 mineVel = vel.RotatedBy(offset) * 0.3f; // Slow drift

                Projectile.NewProjectile(source, pos, mineVel,
                    ModContent.ProjectileType<NoteMineProj>(), (int)(damage * 0.8f), kb, player.whoAmI);
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires powerful beam shots at a moderate pace"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 3rd shot unleashes a bellfire barrage of 7 burning notes"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 4th shot deploys proximity-detonating music note mines"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Enemies slain recently spawn kill echo bursts"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Its grandeur shakes the heavens, each chime a thunderclap of divine fire'")
            {
                OverrideColor = new Color(255, 140, 40)
            });
        }
    }
}
