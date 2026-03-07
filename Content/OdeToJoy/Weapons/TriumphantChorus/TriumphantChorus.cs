using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus.Buffs;
using MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus
{
    /// <summary>
    /// Triumphant Chorus — 4 vocal part summon (Soprano/Alto/Tenor/Bass).
    /// Each additional summon adds a new voice type. Harmony Bonus at all 4.
    /// Ensemble Attack every 10s fires synchronized golden wave.
    /// </summary>
    public class TriumphantChorus : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 46;
            Item.damage = 3000; // Tier 9 (2100-3200 range)
            Item.DamageType = DamageClass.Summon;
            Item.mana = 35;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.crit = 4;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<TriumphantChorusMinion>();
            Item.shootSpeed = 10f;
            Item.buffType = ModContent.BuffType<TriumphantChorusBuff>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            // Count existing chorus minions to assign voice type
            int existingCount = player.ownedProjectileCounts[ModContent.ProjectileType<TriumphantChorusMinion>()];
            int voiceType = existingCount % 4; // 0=Soprano, 1=Alto, 2=Tenor, 3=Bass

            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, voiceType);

            // Summoning VFX
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color c = ChorusTextures.GetVoiceColor(voiceType);
                Dust d = Dust.NewDustDirect(position, 1, 1, DustID.GoldFlame, vel.X, vel.Y, 100, c, 0.8f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a chorus vocalist that orbits you"));
            tooltips.Add(new TooltipLine(Mod, "Voices", "Each summon adds a new vocal part: Soprano, Alto, Tenor, Bass"));
            tooltips.Add(new TooltipLine(Mod, "Harmony", "Having all 4 voice types grants Harmony Bonus (+20% summon damage)"));
            tooltips.Add(new TooltipLine(Mod, "Ensemble", "Every 10 seconds, all chorus members fire a synchronized golden wave"));
            tooltips.Add(new TooltipLine(Mod, "Slots", "Requires 2 minion slots per voice"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'When every voice rings true, the world itself sings back in jubilation'")
            {
                OverrideColor = new Color(255, 200, 50)
            });
        }
    }
}