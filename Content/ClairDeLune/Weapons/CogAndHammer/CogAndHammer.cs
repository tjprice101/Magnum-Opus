using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.ClairDeLune.Weapons.CogAndHammer.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.CogAndHammer
{
    /// <summary>
    /// Cog and Hammer — Ranged launcher that lobs clockwork bombs.
    /// 3-tick countdown → detonation spraying gear shrapnel.
    /// Alt fire = Sticky Bomb. Every 8th shot = Master Mechanism (2x radius, spawns 4 sub-bombs).
    /// "The precision of a watchmaker. The philosophy of a demolitions expert."
    /// </summary>
    public class CogAndHammer : ModItem
    {
        private int _shotCounter;

        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 32;
            Item.damage = 3500; // Tier 10 (2800-4200 range), slow ranged
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 12f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item61;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<ClockworkBombProjectile>();
            Item.shootSpeed = 14f;
            Item.useAmmo = AmmoID.Rocket;
            Item.crit = 18;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            _shotCounter++;

            if (player.altFunctionUse == 2)
            {
                // Alt fire: Sticky Bomb
                Projectile.NewProjectile(source, position, velocity * 0.8f,
                    ModContent.ProjectileType<StickyBombProjectile>(),
                    damage, knockback, player.whoAmI);
                return false;
            }

            // Every 8th shot: Master Mechanism
            if (_shotCounter % 8 == 0)
            {
                Projectile.NewProjectile(source, position, velocity * 0.7f,
                    ModContent.ProjectileType<MasterMechanismBombProjectile>(),
                    (int)(damage * 1.5f), knockback * 1.5f, player.whoAmI);

                SoundEngine.PlaySound(SoundID.Item62 with { Pitch = -0.3f, Volume = 0.8f }, position);

                var flash = new BloomParticle(position, Vector2.Zero,
                    ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.5f, 0.4f, 6);
                MagnumParticleHandler.SpawnParticle(flash);
                return false;
            }

            // Normal clockwork bomb
            Projectile.NewProjectile(source, position, velocity,
                ModContent.ProjectileType<ClockworkBombProjectile>(),
                damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Lobs clockwork bombs that tick 3 times before detonating"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Right click fires sticky bombs that attach to surfaces and enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 8th shot fires a Master Mechanism bomb that spawns 4 sub-bombs"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "3+ bombs within 6 tiles trigger Chain Detonation"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The precision of a watchmaker. The philosophy of a demolitions expert.'")
            {
                OverrideColor = ClairDeLunePalette.LoreText
            });
        }
    
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            Vector2 pos = Item.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;

            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 2.2f) * 0.05f
                + (float)Math.Sin(time * 3.8f) * 0.03f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            ClairDeLunePalette.DrawItemBloom(spriteBatch, tex, pos, origin, rotation, scale, pulse);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, ClairDeLunePalette.SoftBlue.ToVector3() * 0.35f);
            return true;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            float time = Main.GameUpdateCount * 0.04f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.06f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            float cycle = (float)Math.Sin(time * 0.7f) * 0.5f + 0.5f;
            Color glowColor = Color.Lerp(ClairDeLunePalette.SoftBlue, ClairDeLunePalette.PearlWhite, cycle) * 0.24f;
            spriteBatch.Draw(tex, position, frame, glowColor with { A = 0 }, 0f, origin, scale * pulse * 1.1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            spriteBatch.Draw(tex, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
