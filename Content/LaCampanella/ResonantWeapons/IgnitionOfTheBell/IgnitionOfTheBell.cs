using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Particles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Projectiles;
using MagnumOpus.Common;
using MagnumOpus.Content.FoundationWeapons.ImpactFoundation;
using MagnumOpus.Content.FoundationWeapons.ExplosionParticlesFoundation;
using MagnumOpus.Content.FoundationWeapons.SmokeFoundation;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell
{
    /// <summary>
    /// IgnitionOfTheBell — Melee thrust weapon (infernal lance).
    /// 3-phase thrust combo:
    ///   Phase 1 — Ignition Strike: Forward thrust + ground geyser pillar at impact point.
    ///   Phase 2 — Tolling Frenzy: Rapid triple thrust (left-center-right), each spawns smaller geyser.
    ///   Phase 3 — Chime Cyclone: Spin attack creating fire cyclone vortex (2s pull + detonation).
    /// Every 3rd Chime Cyclone detonation triggers Chimequake.
    /// </summary>
    public class IgnitionOfTheBell : ModItem
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/IgnitionOfTheBell/IgnitionOfTheBell";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 340;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.width = 60;
            Item.height = 60;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5.5f;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<LaCampanellaRainbowRarity>();
            Item.shoot = ModContent.ProjectileType<IgnitionThrustProj>();
            Item.shootSpeed = 1f;
            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
        }

        public override bool CanUseItem(Player player)
        {
            // Prevent overlapping thrusts
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == player.whoAmI &&
                    p.type == ModContent.ProjectileType<IgnitionThrustProj>())
                    return false;
            }
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var tracker = player.IgnitionOfTheBell();
            int comboStep = tracker.ThrustCombo;
            tracker.AdvanceCombo();

            int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            if (proj >= 0 && proj < Main.maxProjectiles)
                Main.projectile[proj].ai[0] = comboStep;

            return false;
        }

        public override void HoldItem(Player player)
        {
            // Ambient magma embers when held
            if (Main.rand.NextBool(6))
            {
                Vector2 handPos = player.Center + new Vector2(player.direction * 20f, -8f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-1f, -0.3f));
                IgnitionOfTheBellParticleHandler.SpawnParticle(
                    new ThrustEmberParticle(handPos, vel, Main.rand.NextFloat(0.4f, 0.8f), 15, 0.2f));
            }
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D bloomTex = null;
            try
            {
                bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }

            if (bloomTex == null) return;

            Vector2 origin = new Vector2(bloomTex.Width / 2f, bloomTex.Height / 2f);
            Vector2 drawPos = Item.Center - Main.screenPosition;

            float pulse = 0.6f + 0.3f * (float)Math.Sin(Main.GameUpdateCount * 0.08f);
            Color glow = IgnitionOfTheBellUtils.Additive(new Color(200, 40, 0), 0.3f * pulse);

            spriteBatch.Draw(bloomTex, drawPos, null, glow, 0f, origin, 0.5f * pulse, SpriteEffects.None, 0f);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "3-phase thrust combo with infernal geysers and fire cyclones"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Chime Cyclone pulls enemies in before detonating"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 3rd Cyclone detonation triggers a devastating Chimequake"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'The bell does not ring. It ignites.'")
            {
                OverrideColor = IgnitionOfTheBellUtils.LoreColor
            });
        }
    }
}
