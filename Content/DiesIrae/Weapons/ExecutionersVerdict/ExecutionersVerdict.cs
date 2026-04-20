using MagnumOpus.Common;
using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Utilities;
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
using MagnumOpus.Common.Systems.UI;

namespace MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict
{
    /// <summary>
    /// Executioner's Verdict — Dies Irae's precise judicial blade delivering the final sentence.
    /// Exoblade-architecture weapon item with channel-hold swing and dash attack.
    /// </summary>
    public class ExecutionersVerdict : ModItem, IOverdriveItem
    {
        public IResonantOverdrive GetOverdrivePlayer(Player player) => player.GetModPlayer<ExecutionersVerdictPlayer>();

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 75;
            Item.height = 75;
            Item.scale = 0.11f;
            Item.damage = 310;
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
            Item.shoot = ModContent.ProjectileType<Projectiles.ExecutionersVerdictSwing>();
            Item.shootSpeed = 8f;
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
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
            player.GetModPlayer<ExecutionersVerdictPlayer>().IsHoldingExecutionersVerdict = true;
        }

        public override bool AltFunctionUse(Player player) => true;
        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                var wp = player.GetModPlayer<ExecutionersVerdictPlayer>();
                if (wp.IsChargeFull)
                {
                    wp.ConsumeCharge();
                    SoundEngine.PlaySound(SoundID.Item45 with { Pitch = -0.4f }, player.Center);
                    Projectile.NewProjectile(source, player.MountedCenter, Vector2.Zero,
                        ModContent.ProjectileType<Projectiles.ExecutionersVerdictSpecialProj>(),
                        (int)(damage * 1.5f), knockback * 2f, player.whoAmI);
                }
                else
                {
                    SoundEngine.PlaySound(SoundID.Item16 with { Pitch = 0.5f, Volume = 0.5f }, player.Center);
                }
                return false;
            }

            // Normal left-click swing
            float state = 0f;
            Projectile.NewProjectile(source, player.MountedCenter,
                (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI, state, 0);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
            "Judicial Crescendo — precise golden strikes that deliver the final sentence"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
            "Each swing scatters golden judgment sparks with flame accents"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
            "Hits spawn golden flame bursts at the point of impact"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
            "Right-click dash attack unleashes a massive golden judgment burst"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'The verdict was written before you were born.'")
            { OverrideColor = new Color(200, 50, 30) });
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.2f;
            Texture2D tex = Terraria.GameContent.TextureAssets.Item[Type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);
            spriteBatch.Draw(tex, drawPos, null, DiesIraePalette.InfernalRed with { A = 0 } * pulse,
                rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, DiesIraePalette.JudgmentGold with { A = 0 } * (pulse * 0.7f),
                rotation, origin, scale * 1.02f, SpriteEffects.None, 0f);
        }
    }
}
