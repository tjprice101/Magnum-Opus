using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.PetalStormCannon.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.OdeToJoy.Weapons.PetalStormCannon
{
    /// <summary>
    /// Petal Storm Cannon — heavy artillery that fires petal cluster barrages.
    /// 3-cluster spread per shot, clusters explode into persistent vortex zones that merge.
    /// Hurricane Mode after 3 consecutive shots: charged shot sweeps the battlefield.
    /// Seasonal Petals cycle: pink → gold → white.
    /// </summary>
    public class PetalStormCannon : ModItem
    {
        private int _shotCount;
        private int _shotCooldown;

        public override void SetDefaults()
        {
            Item.width = 62;
            Item.height = 32;
            Item.damage = 2900; // Tier 9 (2100-3200 range)
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 50;
            Item.useAnimation = 50;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 12f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item62;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.crit = 20;
            Item.shoot = ModContent.ProjectileType<PetalClusterProjectile>();
            Item.shootSpeed = 10f;
            Item.useAmmo = AmmoID.Rocket;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            _shotCount++;
            int seasonalIndex = _shotCount;

            // 3-cluster spread
            float spreadAngle = MathHelper.ToRadians(12f);
            for (int i = -1; i <= 1; i++)
            {
                Vector2 spreadVel = velocity.RotatedBy(spreadAngle * i);
                int proj = Projectile.NewProjectile(source, position, spreadVel,
                    ModContent.ProjectileType<PetalClusterProjectile>(), damage, knockback, player.whoAmI);
                if (proj >= 0 && proj < Main.maxProjectiles)
                    Main.projectile[proj].ai[0] = seasonalIndex; // seasonal color index
            }

            // After 3 consecutive shots, spawn Hurricane if player holds fire
            if (_shotCount >= 3 && player.channel)
            {
                Vector2 hurricaneVel = velocity.SafeNormalize(Vector2.UnitX) * 6f;
                Projectile.NewProjectile(source, position, hurricaneVel,
                    ModContent.ProjectileType<HurricaneShotProjectile>(), (int)(damage * 2f), knockback * 2f, player.whoAmI);
                _shotCount = 0;
            }

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Converts rockets into 3 petal cluster bombs that create persistent petal vortex zones"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Overlapping vortex zones merge into larger storms with increased damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "After 3 shots, holding fire launches a Hurricane Shot that sweeps the battlefield"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Stand inside your own petal storm for +8% damage, +5% crit for 3s"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The storm does not discriminate. Joy and ruin travel together.'")
            {
                OverrideColor = new Color(255, 200, 50)
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

            OdeToJoyPalette.DrawItemBloom(spriteBatch, tex, pos, origin, rotation, scale, pulse);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, OdeToJoyPalette.GoldenPollen.ToVector3() * 0.35f);
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
            Color glowColor = Color.Lerp(OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.RosePink, cycle) * 0.24f;
            spriteBatch.Draw(tex, position, frame, glowColor with { A = 0 }, 0f, origin, scale * pulse * 1.1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            spriteBatch.Draw(tex, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}