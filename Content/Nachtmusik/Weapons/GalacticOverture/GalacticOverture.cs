using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Nachtmusik.Weapons.GalacticOverture.Projectiles;
using MagnumOpus.Content.Nachtmusik.Weapons.GalacticOverture.Buffs;
using MagnumOpus.Content.Nachtmusik.Weapons.GalacticOverture.Utilities;

namespace MagnumOpus.Content.Nachtmusik.Weapons.GalacticOverture
{
    /// <summary>
    /// Galactic Overture — Summons a Celestial Muse.
    /// The muse hovers near the player and fires musical projectiles at enemies.
    /// A ranged-attack minion with golden/indigo cosmic aesthetic.
    /// </summary>
    public class GalacticOverture : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.damage = 980;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 18;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 42);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<CelestialMuseMinion>();
            Item.buffType = ModContent.BuffType<GalacticOvertureBuff>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            Vector2 spawnPos = Main.MouseWorld;

            // Musical entrance VFX
            GalacticOvertureVFX.SummonVFX(spawnPos);

            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);

            return false;
        }

        public override void HoldItem(Player player)
        {
            int minionCount = player.ownedProjectileCounts[ModContent.ProjectileType<CelestialMuseMinion>()];
            GalacticOvertureVFX.HoldItemVFX(player, minionCount);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 position = Item.Center - Main.screenPosition;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.GameUpdateCount * 0.06f);

            // Outer golden glow
            spriteBatch.Draw(texture, position, null, NachtmusikPalette.RadianceGold * 0.35f * pulse, rotation, origin, scale * 1.55f, SpriteEffects.None, 0f);
            // Mid indigo glow
            spriteBatch.Draw(texture, position, null, NachtmusikPalette.DeepBlue * 0.45f * pulse, rotation, origin, scale * 1.25f, SpriteEffects.None, 0f);
            // Core shimmer
            spriteBatch.Draw(texture, position, null, NachtmusikPalette.StarWhite * 0.3f, rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a Celestial Muse to fight for you"));
            tooltips.Add(new TooltipLine(Mod, "Behavior", "The muse hovers nearby and fires musical projectiles"));
            tooltips.Add(new TooltipLine(Mod, "Debuff", "Attacks inflict Celestial Harmony"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Let the overture begin'")
            {
                OverrideColor = new Color(100, 120, 200)
            });
        }
    }
}
