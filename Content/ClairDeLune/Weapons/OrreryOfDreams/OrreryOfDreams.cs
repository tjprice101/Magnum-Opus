using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.ClairDeLune.Weapons.OrreryOfDreams.Projectiles;
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

namespace MagnumOpus.Content.ClairDeLune.Weapons.OrreryOfDreams
{
    /// <summary>
    /// Orrery of Dreams — Magic weapon with 3 orbiting Dream Spheres.
    /// Inner (rapid), Middle (homing), Outer (AoE). Dream Alignment every 12s.
    /// Alt fire reverses orbit direction. Night bonus: +15% sphere, +30% alignment.
    /// "A clock that counts not hours, but worlds."
    /// </summary>
    public class OrreryOfDreams : ModItem
    {
        private int _alignmentTimer;
        private const int AlignmentCooldown = 720; // 12s

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.damage = 3200; // Tier 10 (2800-4200 range)
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item43;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<DreamSphereProjectile>();
            Item.shootSpeed = 12f;
            Item.crit = 10;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            bool isNight = !Main.dayTime;
            float nightMult = isNight ? 1.15f : 1f;

            if (player.altFunctionUse == 2)
            {
                // Alt = reverse orbits + check if alignment ready
                _alignmentTimer++;

                // Reverse all active spheres
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active && Main.projectile[i].owner == player.whoAmI
                        && Main.projectile[i].type == ModContent.ProjectileType<DreamSphereProjectile>())
                    {
                        Main.projectile[i].ai[1] *= -1; // Reverse orbit direction
                    }
                }

                SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.2f, Volume = 0.4f }, position);
                return false;
            }

            // Ensure all 3 spheres exist
            int sphereCount = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].owner == player.whoAmI
                    && Main.projectile[i].type == ModContent.ProjectileType<DreamSphereProjectile>())
                    sphereCount++;
            }

            // Spawn missing spheres
            for (int s = sphereCount; s < 3; s++)
            {
                int sphereDmg = (int)(damage * nightMult * (s == 0 ? 0.3f : s == 1 ? 0.5f : 0.7f));
                Projectile.NewProjectile(source, player.Center, Vector2.Zero,
                    ModContent.ProjectileType<DreamSphereProjectile>(),
                    sphereDmg, knockback, player.whoAmI, s, 1f); // ai[0]=type, ai[1]=direction
            }

            // Dream Alignment check
            _alignmentTimer++;
            if (_alignmentTimer >= AlignmentCooldown)
            {
                _alignmentTimer = 0;
                float alignDmg = damage * 3f * (isNight ? 1.3f : 1f);

                // Launch Dream Alignment chain toward cursor
                Vector2 dir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                Projectile.NewProjectile(source, player.Center, dir * 16f,
                    ModContent.ProjectileType<DreamAlignmentProjectile>(),
                    (int)alignDmg, knockback * 2f, player.whoAmI);

                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.3f, Volume = 0.8f }, player.Center);

                // Alignment flash
                var flash = new BloomParticle(player.Center, Vector2.Zero,
                    ClairDeLunePalette.PearlWhite with { A = 0 } * 0.5f, 0.6f, 10);
                MagnumParticleHandler.SpawnParticle(flash);
            }

            // Spheres auto-fire at nearest enemies — handled in DreamSphereProjectile AI

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Summons 3 orbiting Dream Spheres — Inner, Middle, Outer"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Each sphere fires unique projectiles at nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Dream Alignment every 12s: combined sphere blast toward cursor"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Right click reverses orbit direction"));
            tooltips.Add(new TooltipLine(Mod, "Night", "At night: +15% sphere damage, +30% alignment damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A clock that counts not hours, but worlds.'")
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
