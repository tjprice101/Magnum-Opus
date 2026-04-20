using System;
using System.Collections.Generic;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.ClairDeLune;
using MagnumOpus.Content.ClairDeLune.Weapons.StarfallWhisper.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.StarfallWhisper
{
    public class StarfallWhisper : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 76;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 3100;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.crit = 16;
            Item.shoot = ModContent.ProjectileType<TemporalArrowProjectile>();
            Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Arrow;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Right-click: fire 5 GenericHomingOrbChild in a +/-15 degree fan spread
                float baseAngle = velocity.ToRotation();
                float spread = MathHelper.ToRadians(15f);
                int orbType = ModContent.ProjectileType<GenericHomingOrbChild>();

                for (int i = 0; i < 5; i++)
                {
                    float angle = baseAngle + MathHelper.Lerp(-spread, spread, i / 4f);
                    Vector2 orbVel = angle.ToRotationVector2() * velocity.Length();

                    GenericHomingOrbChild.SpawnChild(
                        source, player.MountedCenter, orbVel,
                        (int)(damage * 0.6f), knockback * 0.5f, player.whoAmI,
                        homingStrength: 0.06f,
                        behaviorFlags: 0,
                        themeIndex: GenericHomingOrbChild.THEME_CLAIRDELUNE,
                        scaleMult: 0.8f,
                        timeLeft: 120
                    );
                }

                return false;
            }

            // Left-click: normal single temporal arrow
            Projectile.NewProjectile(source, player.MountedCenter, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void HoldItem(Player player)
        {
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Color col = ClairDeLunePalette.GetGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.WhiteTorch,
                    new Vector2(0, -0.8f) + Main.rand.NextVector2Circular(0.4f, 0.4f), 0, col, 0.5f);
                d.noGravity = true;
            }

            float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f);
            Lighting.AddLight(player.Center, ClairDeLunePalette.SoftBlue.ToVector3() * 0.35f * pulse);
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.2f;
            Texture2D tex = Terraria.GameContent.TextureAssets.Item[Type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);

            spriteBatch.Draw(tex, drawPos, null, ClairDeLunePalette.SoftBlue with { A = 0 } * pulse,
                rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, ClairDeLunePalette.PearlWhite with { A = 0 } * (pulse * 0.7f),
                rotation, origin, scale * 1.02f, SpriteEffects.None, 0f);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires crystal arrows that create Temporal Fractures on impact"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Fractures replay hits after a brief delay for extra damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Right click to fire 5 fracture arrows simultaneously"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Nearby fractures resonate for chain reaction bursts"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'You hear the whisper only after the arrow has already arrived.'")
            {
                OverrideColor = ClairDeLunePalette.LoreText
            });
        }
    }
}
