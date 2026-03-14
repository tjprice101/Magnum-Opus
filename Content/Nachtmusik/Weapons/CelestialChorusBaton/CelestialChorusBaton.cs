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
using MagnumOpus.Content.Nachtmusik.Weapons.CelestialChorusBaton.Projectiles;
using MagnumOpus.Content.Nachtmusik.Weapons.CelestialChorusBaton.Buffs;

namespace MagnumOpus.Content.Nachtmusik.Weapons.CelestialChorusBaton
{
    /// <summary>
    /// Celestial Chorus Baton ? Summons a Nocturnal Guardian.
    /// The guardian orbits the player and dashes to attack enemies.
    /// An aggressive melee minion that conducts the symphony of the night.
    /// </summary>
    public class CelestialChorusBaton : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.damage = 1250; // Tier 7 (1200-1800 range)
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 44);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<NocturnalGuardianMinion>();
            Item.buffType = ModContent.BuffType<CelestialChorusBatonBuff>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            Vector2 spawnPos = Main.MouseWorld;

            // Entrance VFX ? cosmic summoning burst
// VFX_GUTTED:             CelestialChorusBatonVFX.SummonVFX(spawnPos);

            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);

            return false;
        }

        public override void HoldItem(Player player)
        {
            int minionCount = player.ownedProjectileCounts[ModContent.ProjectileType<NocturnalGuardianMinion>()];
// VFX_GUTTED:             CelestialChorusBatonVFX.HoldItemVFX(player, minionCount);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 position = Item.Center - Main.screenPosition;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.GameUpdateCount * 0.05f);

            // Outer cosmic glow
            spriteBatch.Draw(texture, position, null, NachtmusikPalette.CosmicPurple * 0.4f * pulse, rotation, origin, scale * 1.6f, SpriteEffects.None, 0f);
            // Mid starlit glow
            spriteBatch.Draw(texture, position, null, NachtmusikPalette.StarlitBlue * 0.5f * pulse, rotation, origin, scale * 1.25f, SpriteEffects.None, 0f);
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
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a Nocturnal Guardian to fight for you"));
            tooltips.Add(new TooltipLine(Mod, "Behavior", "The guardian orbits you and dashes to attack enemies"));
            tooltips.Add(new TooltipLine(Mod, "Debuff", "Guardian attacks inflict Celestial Harmony"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Conduct the symphony of the night'")
            {
                OverrideColor = new Color(100, 120, 200)
            });
        }
    }
}
