using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell.Particles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell.Projectiles;
using MagnumOpus.Common;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell
{
    /// <summary>
    /// FangOfTheInfiniteBell — Magic staff with empowerment cycle.
    /// Fires homing arcane bell-fire orbs. Every 3rd hit triggers empowerment:
    /// +15% magic damage, +10% attack speed, infinite mana for 10s,
    /// and lightning strikes accompany hits. 20s cooldown after empowerment ends.
    /// </summary>
    public class FangOfTheInfiniteBell : ModItem
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/FangOfTheInfiniteBell/FangOfTheInfiniteBell";

        public override void SetDefaults()
        {
            Item.damage = 280;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.width = 50;
            Item.height = 50;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<LaCampanellaRainbowRarity>();
            Item.shoot = ModContent.ProjectileType<InfiniteBellOrbProj>();
            Item.shootSpeed = 12f;
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.staff[Type] = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var tracker = player.FangOfTheInfiniteBell();

            // When empowered, fire 3 orbs in a spread
            if (tracker.IsEmpowered)
            {
                float baseAngle = velocity.ToRotation();
                for (int i = -1; i <= 1; i++)
                {
                    float angle = baseAngle + i * 0.12f;
                    Vector2 vel = angle.ToRotationVector2() * velocity.Length();
                    int proj = Projectile.NewProjectile(source, position, vel, type, damage, knockback, player.whoAmI);
                    if (proj >= 0 && proj < Main.maxProjectiles)
                        Main.projectile[proj].ai[0] = 1f; // Mark as empowered projectile
                }
                return false;
            }

            return true;
        }

        public override void HoldItem(Player player)
        {
            var tracker = player.FangOfTheInfiniteBell();

            // Empowerment ambient particles
            if (tracker.IsEmpowered && Main.rand.NextBool(3))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(30f, 40f);
                Vector2 vel = Vector2.UnitY * -Main.rand.NextFloat(0.5f, 1.5f);
                FangOfTheInfiniteBellParticleHandler.SpawnParticle(
                    new EmpoweredSparkParticle(pos, vel, 20, 0.3f));
            }

            // Normal ambient
            if (Main.rand.NextBool(8))
            {
                Vector2 handPos = player.Center + new Vector2(player.direction * 18f, -6f);
                Vector2 vel = Main.rand.NextVector2Circular(0.5f, 0.5f) + Vector2.UnitY * -0.3f;
                FangOfTheInfiniteBellParticleHandler.SpawnParticle(
                    new ArcaneOrbParticle(handPos, vel, Main.rand.NextFloat(0.3f, 0.7f), 15, 0.2f));
            }
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D bloomTex = null;
            try { bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow", ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value; } catch { }
            if (bloomTex == null) return;

            Vector2 origin = new(bloomTex.Width / 2f, bloomTex.Height / 2f);
            Vector2 drawPos = Item.Center - Main.screenPosition;
            float pulse = 0.5f + 0.3f * (float)Math.Sin(Main.GameUpdateCount * 0.06f);
            Color glow = FangOfTheInfiniteBellUtils.Additive(new Color(180, 50, 20), 0.25f * pulse);
            spriteBatch.Draw(bloomTex, drawPos, null, glow, 0f, origin, 0.4f * pulse, SpriteEffects.None, 0f);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires homing arcane bell-fire orbs"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every third hit triggers Empowerment for 10 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Empowered: infinite mana, triple shot, lightning strikes on hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "20 second cooldown after empowerment fades"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'The bell has no end — its resonance is infinite, and so is its hunger'")
            {
                OverrideColor = FangOfTheInfiniteBellUtils.LoreColor
            });
        }
    }
}
