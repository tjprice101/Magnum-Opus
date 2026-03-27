using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Nachtmusik.Weapons.GalacticOverture.Projectiles;
using MagnumOpus.Content.Nachtmusik.Weapons.GalacticOverture.Buffs;

namespace MagnumOpus.Content.Nachtmusik.Weapons.GalacticOverture
{
    /// <summary>
    /// Galactic Overture ? Summons a Celestial Muse.
    /// The muse hovers near the player and fires musical projectiles at enemies.
    /// A ranged-attack minion with golden/indigo cosmic aesthetic.
    /// </summary>
    public class GalacticOverture : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.damage = 1200; // Tier 7 (1200-1800 range)
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

            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);

            return false;
        }

        public override void HoldItem(Player player)
        {
            int minionCount = player.ownedProjectileCounts[ModContent.ProjectileType<CelestialMuseMinion>()];
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 position = Item.Center - Main.screenPosition;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

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


        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            float time = Main.GameUpdateCount * 0.04f;
            float twinkle = 1f + (float)Math.Sin(time * 2.3f) * 0.07f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            float cycle = (float)Math.Sin(time * 0.7f) * 0.5f + 0.5f;
            Color glowColor = Color.Lerp(NachtmusikPalette.StarlitBlue, NachtmusikPalette.StarGold, cycle) * 0.24f;
            spriteBatch.Draw(tex, position, frame, glowColor, 0f, origin, scale * twinkle * 1.1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            spriteBatch.Draw(tex, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
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
