using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling.Projectiles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling.Utilities;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling
{
    /// <summary>
    /// Infernal Chimes' Calling — La Campanella summon staff.
    /// "The choir sings not hymns of peace, but anthems of annihilation."
    /// 
    /// Summons spectral bell minions (1 per use, max 5). Bells hover in arc formation.
    /// Bells attack sequentially with 0.3s stagger, each firing a shockwave.
    /// Harmonic Convergence: overlapping shockwaves deal 2x damage.
    /// Every 12s: Infernal Crescendo — synchronized massive barrage.
    /// Right-click: Bell Sacrifice — one bell detonates in AoE, respawns 15s.
    /// </summary>
    public class InfernalChimesCallingItem : ModItem
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/InfernalChimesCalling/InfernalChimesCalling";
        public override string Name => "InfernalChimesCalling";

        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 145;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<LaCampanellaRainbowRarity>();
            Item.UseSound = SoundID.Item44;
            Item.shoot = ModContent.ProjectileType<CampanellaChoirMinion>();
            Item.buffType = ModContent.BuffType<CampanellaChoirBuff>();
            Item.noMelee = true;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Bell Sacrifice: need at least 3 bells and no active sacrifice cooldown
                var icPlayer = player.InfernalChimesCalling();
                int bellCount = player.ownedProjectileCounts[ModContent.ProjectileType<CampanellaChoirMinion>()];
                return bellCount >= 3 && icPlayer.SacrificeCooldown <= 0;
            }
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Bell Sacrifice: find the last bell and trigger its sacrifice
                var icPlayer = player.InfernalChimesCalling();
                icPlayer.SacrificeCooldown = InfernalChimesCallingPlayer.SacrificeRespawnTime;

                // Find the highest-index bell to sacrifice
                int sacrificeIndex = -1;
                for (int i = Main.maxProjectiles - 1; i >= 0; i--)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<CampanellaChoirMinion>())
                    {
                        sacrificeIndex = i;
                        break;
                    }
                }

                if (sacrificeIndex >= 0)
                {
                    Projectile bell = Main.projectile[sacrificeIndex];
                    // Spawn sacrifice explosion at bell position
                    int sacrificeDmg = (int)(damage * 3f); // 3x damage for sacrifice
                    Projectile.NewProjectile(source, bell.Center, Vector2.Zero,
                        ModContent.ProjectileType<MinionShockwaveProj>(),
                        sacrificeDmg, 10f, player.whoAmI, 0f, 1f); // ai[1]=1 for sacrifice variant

                    bell.Kill(); // Destroy the sacrificed bell
                }
                return false;
            }

            // Normal summon: spawn one bell at cursor position, max 5
            int maxBells = 5;
            int bellCount = player.ownedProjectileCounts[ModContent.ProjectileType<CampanellaChoirMinion>()];
            if (bellCount >= maxBells) return false;

            player.AddBuff(Item.buffType, 18000);
            position = Main.MouseWorld;
            Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Summons spectral bell minions that hover in formation (max 5)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Bells attack sequentially with staggered shockwaves"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Overlapping waves achieve Harmonic Convergence for double damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Right click to sacrifice one bell in a devastating explosion"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The choir sings in flame. The encore is silence.'")
            {
                OverrideColor = InfernalChimesCallingUtils.LoreColor
            });
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.2f;
            Texture2D tex = Terraria.GameContent.TextureAssets.Item[Type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);
            spriteBatch.Draw(tex, drawPos, null, new Color(255, 140, 40, 0) * pulse,
                rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, new Color(255, 200, 60, 0) * (pulse * 0.7f),
                rotation, origin, scale * 1.02f, SpriteEffects.None, 0f);
        }
    }
}
