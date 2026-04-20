using MagnumOpus.Common;
using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Utilities;
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

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver
{
    /// <summary>
    /// Wrath's Cleaver — Dies Irae's brutal melee cleaver embodying fury incarnate.
    /// Exoblade-architecture weapon item with channel-hold swing and dash attack.
    /// </summary>
    public class WrathsCleaver : ModItem, IOverdriveItem
    {
        public IResonantOverdrive GetOverdrivePlayer(Player player) => player.GetModPlayer<WrathsCleaverPlayer>();

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 80;
            Item.scale = 0.12f;
            Item.damage = 340;
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
            Item.shoot = ModContent.ProjectileType<Projectiles.WrathsCleaverSwing>();
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
            player.GetModPlayer<WrathsCleaverPlayer>().IsHoldingWrathsCleaver = true;
        }

        public override bool AltFunctionUse(Player player) => true;
        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var wp = player.GetModPlayer<WrathsCleaverPlayer>();

            if (player.altFunctionUse == 2)
            {
                // Right-click dash: 1 orb straight ahead at 2x speed, pierce all, short life
                SoundEngine.PlaySound(SoundID.Item45 with { Pitch = -0.3f }, player.Center);
                Vector2 aimDir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
                Vector2 dashVel = aimDir * Item.shootSpeed * 2f;
                int projIdx = Projectile.NewProjectile(source, player.MountedCenter, dashVel,
                    ModContent.ProjectileType<Projectiles.WrathsCleaverSpecialProj>(),
                    (int)(damage * 0.5f), knockback * 0.5f, player.whoAmI, ai0: -1f);
                if (projIdx >= 0 && projIdx < Main.maxProjectiles)
                {
                    Main.projectile[projIdx].penetrate = -1;
                    Main.projectile[projIdx].timeLeft = 40;
                }
                return false;
            }

            // Normal left-click swing
            float state = 0f;
            Projectile.NewProjectile(source, player.MountedCenter,
                (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI, state, 0);

            // Wrath Escalation: fire orbs based on combo phase
            int phase = wp.comboCounter;
            Vector2 aimDirection = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            float baseSpeed = Item.shootSpeed;
            int orbDamage = (int)(damage * 0.4f);
            float orbKB = knockback * 0.5f;
            int specialType = ModContent.ProjectileType<Projectiles.WrathsCleaverSpecialProj>();

            switch (phase)
            {
                case 0:
                    // Phase 1: 1 orb, straight shot, no homing — the warning
                    Projectile.NewProjectile(source, player.MountedCenter,
                        aimDirection * baseSpeed, specialType,
                        orbDamage, orbKB, player.whoAmI, ai0: 0f);
                    break;

                case 1:
                    // Phase 2: 2 orbs, mild homing — judgment approaches
                    for (int i = 0; i < 2; i++)
                    {
                        float angle = MathHelper.ToRadians(-7f + 14f * i);
                        Vector2 vel = aimDirection.RotatedBy(angle) * baseSpeed;
                        Projectile.NewProjectile(source, player.MountedCenter, vel,
                            specialType, orbDamage, orbKB, player.whoAmI, ai0: 1f);
                    }
                    break;

                case 2:
                    // Phase 3: 3 orbs, standard homing, pierce 1 — judgment weighs
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = MathHelper.ToRadians(-10f + 10f * i);
                        Vector2 vel = aimDirection.RotatedBy(angle) * baseSpeed;
                        Projectile.NewProjectile(source, player.MountedCenter, vel,
                            specialType, orbDamage, orbKB, player.whoAmI, ai0: 2f);
                    }
                    break;

                case 3:
                    // Phase 4: 1 orb, aggressive homing, on-hit splits — judgment rendered
                    Projectile.NewProjectile(source, player.MountedCenter,
                        aimDirection * baseSpeed, specialType,
                        orbDamage, orbKB, player.whoAmI, ai0: 3f);
                    break;
            }

            wp.IncrementCombo();
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
            "Wrath Crescendo — 4-phase combo that builds to a devastating Infernal Finale"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
            "Each swing drags fire embers and solar flares along the blade"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
            "Hits inflict Hellfire and spawn radial ember bursts"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
            "Right-click dash attack unleashes a massive fire burst on impact"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'The first blow of wrath is always the loudest \u2014 but the last shakes the earth itself.'")
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
