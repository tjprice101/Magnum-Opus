using MagnumOpus.Common;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw.Projectiles;
using MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw.Utilities;
using MagnumOpus.Content.SandboxExoblade.Utilities;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw
{
    /// <summary>
    /// Rose Thorn Chainsaw — Ode to Joy's rose-themed melee weapon.
    /// Exoblade-architecture melee with rose pink slash arcs and golden pollen accents.
    /// </summary>
    public class RoseThornChainsaw : ModItem, IOverdriveItem
    {
        public IResonantOverdrive GetOverdrivePlayer(Player player) => player.GetModPlayer<RoseThornChainsawPlayer>();

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 65;
            Item.height = 65;
            Item.scale = 0.09f;
            Item.damage = 260;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useTurn = true;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.knockBack = 7.5f;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.channel = true;
            Item.value = Item.sellPrice(gold: 45);
            Item.shoot = ModContent.ProjectileType<RoseThornChainsawProjectile>();
            Item.shootSpeed = 8f;
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override bool CanShoot(Player player)
        {
            if (player.altFunctionUse == 2)
                return true;

            return player.ownedProjectileCounts[Item.shoot] <= 0;
        }

        public override void HoldItem(Player player)
        {
            player.ExoBlade().rightClickListener = true;
            player.ExoBlade().mouseWorldListener = true;
            player.GetModPlayer<RoseThornChainsawPlayer>().IsHoldingRoseThornChainsaw = true;
        }

        public override bool AltFunctionUse(Player player) => true;
        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Right-click: charge-gated special — activate 5s empowerment + fire 3 orbs
            if (player.altFunctionUse == 2)
            {
                var rtp = player.GetModPlayer<RoseThornChainsawPlayer>();

                if (rtp.IsChargeFull)
                {
                    rtp.ConsumeCharge();
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.1f, Volume = 0.8f }, player.MountedCenter);

                    // Activate 5s empowerment
                    rtp.ProjectileEmpowerTimer = 300;

                    // Fire burst of 3 orbs in tight ±5° cone
                    Vector2 baseDir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
                    float spreadAngle = MathHelper.ToRadians(5f);

                    for (int b = -1; b <= 1; b++)
                    {
                        Vector2 dir = baseDir.RotatedBy(b * spreadAngle);
                        Projectile.NewProjectile(source, player.MountedCenter, dir * 14f,
                            ModContent.ProjectileType<RoseThornChainsawSpecialProj>(),
                            (int)(damage * 1.1f), knockback, player.whoAmI, ai0: 1f);
                    }
                }
                else
                {
                    SoundEngine.PlaySound(SoundID.Item27 with { Pitch = -0.3f, Volume = 0.4f }, player.MountedCenter);
                }
                return false;
            }

            // Normal left-click: spawn swing projectile + fire 3 rapid orbs
            Projectile.NewProjectile(source, player.MountedCenter,
                (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI, 0f, 0);

            // Also fire 3 rapid-stream orbs in tight spread
            {
                var rtp = player.GetModPlayer<RoseThornChainsawPlayer>();
                float empowered = rtp.IsEmpowerActive ? 1f : 0f;
                Vector2 baseDir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
                float spreadAngle = MathHelper.ToRadians(2f);

                for (int b = -1; b <= 1; b++)
                {
                    Vector2 dir = baseDir.RotatedBy(b * spreadAngle);
                    Projectile.NewProjectile(source, player.MountedCenter, dir * 14f,
                        ModContent.ProjectileType<RoseThornChainsawSpecialProj>(),
                        (int)(damage * 0.4f), knockback * 0.3f, player.whoAmI, ai0: empowered);
                }
            }

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
            "Rose Thorn Rend — rapid rose-pink slashes that scatter petals on hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
            "Right-click to dash through enemies with a thorned blade charge"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
            "Each strike plants rose thorns that bloom into damaging petal bursts"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'Every rose has its chainsaw.'")
            { OverrideColor = OdeToJoyPalette.LoreText });
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.2f;
            Texture2D tex = Terraria.GameContent.TextureAssets.Item[Type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);
            spriteBatch.Draw(tex, drawPos, null, OdeToJoyPalette.RosePink with { A = 0 } * pulse,
                rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, OdeToJoyPalette.WhiteBloom with { A = 0 } * (pulse * 0.5f),
                rotation, origin, scale * 1.02f, SpriteEffects.None, 0f);
        }
    }
}
