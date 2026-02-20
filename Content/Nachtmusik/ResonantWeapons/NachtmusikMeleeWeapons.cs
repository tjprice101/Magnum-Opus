using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Projectiles;
using MagnumOpus.Content.Nachtmusik.Debuffs;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Nachtmusik.ResonantWeapons
{
    // ========================================================================
    // NOCTURNAL EXECUTIONER — Heavy cosmic greatsword with Execution mechanic
    // ========================================================================
    public class NocturnalExecutioner : MeleeSwingItemBase
    {
        protected override int SwingProjectileType => ModContent.ProjectileType<NocturnalExecutionerSwing>();
        protected override int ComboStepCount => 3;

        private int executionCharge;

        public int ExecutionCharge
        {
            get => executionCharge;
            set => executionCharge = Math.Clamp(value, 0, 100);
        }

        protected override void SetWeaponDefaults()
        {
            Item.damage = 1850;
            Item.knockBack = 7.5f;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.scale = 1.5f;
            Item.crit = 18;
            Item.value = Terraria.Item.sellPrice(gold: 45);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item71 with { Pitch = -0.3f };
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                if (executionCharge < 50)
                    return false;
                Item.useTime = 30;
                Item.useAnimation = 30;
            }
            else
            {
                Item.useTime = 16;
                Item.useAnimation = 16;
            }
            return base.CanUseItem(player);
        }

        protected override void OnShoot(Player player, int projectileIndex)
        {
            if (player.altFunctionUse == 2 && executionCharge >= 50)
            {
                executionCharge = 0;

                Vector2 dir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                var source = player.GetSource_ItemUse(Item);
                int dmg = (int)(Item.damage * 2.5f * player.GetDamage(DamageClass.Melee).Multiplicative);

                for (int i = -2; i <= 2; i++)
                {
                    float angle = dir.ToRotation() + MathHelper.ToRadians(i * 12f);
                    Vector2 vel = angle.ToRotationVector2() * 18f;
                    Projectile.NewProjectile(source, player.Center, vel,
                        ModContent.ProjectileType<NocturnalBladeProjectile>(),
                        dmg, Item.knockBack, player.whoAmI);
                }

                NachtmusikCosmicVFX.SpawnCelestialExplosion(player.Center, 1.5f);
                NachtmusikCosmicVFX.SpawnMusicNoteBurst(player.Center, 5, 4f);
                MagnumScreenEffects.AddScreenShake(10f);
            }
        }

        public override void HoldItem(Player player)
        {
            base.HoldItem(player);

            if (executionCharge >= 50 && Main.rand.NextBool(4))
            {
                float angle = Main.GameUpdateCount * 0.06f;
                float radius = 30f + (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 8f;
                for (int i = 0; i < 3; i++)
                {
                    float a = angle + MathHelper.TwoPi * i / 3f;
                    Vector2 orbitPos = player.Center + a.ToRotationVector2() * radius;
                    float chargeProgress = executionCharge / 100f;
                    Color orbitColor = Color.Lerp(NachtmusikCosmicVFX.DeepPurple, NachtmusikCosmicVFX.Gold, chargeProgress);
                    Dust d = Dust.NewDustPerfect(orbitPos, DustID.PurpleTorch, Vector2.Zero, 0, orbitColor, 0.8f);
                    d.noGravity = true;
                }
            }

            if (Main.rand.NextBool(8))
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.Enchanted_Gold,
                    Vector2.Zero, 0, NachtmusikCosmicVFX.Gold, 0.6f);
                d.noGravity = true;
            }

            Lighting.AddLight(player.Center, NachtmusikCosmicVFX.DeepPurple.ToVector3() * 0.4f);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            Vector2 pos = Item.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;
            Color glow = NachtmusikCosmicVFX.DeepPurple with { A = 0 } * 0.4f;

            spriteBatch.Draw(tex, pos, null, glow, rotation, origin, scale * 1.15f, SpriteEffects.None, 0f);
            return true;
        }

        protected override Color GetLoreColor() => NachtmusikCosmicVFX.Gold;

        protected override void AddWeaponTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Devastating 3-phase combo with cosmic trails"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", $"Execution Charge: {executionCharge}/100"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Builds charge on hit (+8, +15 on crit)"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Right-click at 50+ charge: Spectral blade fan (2.5x damage)"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Inflicts Celestial Harmony on all strikes"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The stars themselves bow to the executioner's decree'")
            { OverrideColor = NachtmusikCosmicVFX.Gold });
        }
    }

    // ========================================================================
    // MIDNIGHT'S CRESCENDO — Ramping damage sword with Crescendo stacks
    // ========================================================================
    public class MidnightsCrescendo : MeleeSwingItemBase
    {
        protected override int SwingProjectileType => ModContent.ProjectileType<MidnightsCrescendoSwing>();
        protected override int ComboStepCount => 3;

        private int crescendoStacks;
        private int decayTimer;
        private const int MaxStacks = 15;
        private const int DecayTime = 90;

        public int CrescendoStacks
        {
            get => crescendoStacks;
            set => crescendoStacks = Math.Clamp(value, 0, MaxStacks);
        }

        public void ResetDecayTimer() => decayTimer = DecayTime;

        protected override void SetWeaponDefaults()
        {
            Item.damage = 1200;
            Item.knockBack = 6f;
            Item.useTime = 14;
            Item.useAnimation = 14;
            Item.scale = 1.3f;
            Item.crit = 15;
            Item.value = Terraria.Item.sellPrice(gold: 40);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item71 with { Pitch = 0.1f };
        }

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            damage *= 1f + crescendoStacks * 0.12f;
        }

        public override void ModifyWeaponCrit(Player player, ref float crit)
        {
            crit += crescendoStacks * 2;
        }

        protected override void OnShoot(Player player, int projectileIndex)
        {
            if (crescendoStacks >= 8)
            {
                Vector2 dir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                var source = player.GetSource_ItemUse(Item);
                int dmg = (int)(player.GetWeaponDamage(Item) * 0.6f);

                Projectile.NewProjectile(source, player.Center, dir * 14f,
                    ModContent.ProjectileType<CrescendoWaveProjectile>(),
                    dmg, Item.knockBack * 0.5f, player.whoAmI);

                NachtmusikCosmicVFX.SpawnCelestialExplosion(player.Center, 0.8f + crescendoStacks * 0.05f);
            }
        }

        public override void UpdateInventory(Player player)
        {
            if (decayTimer > 0)
            {
                decayTimer--;
            }
            else if (crescendoStacks > 0)
            {
                crescendoStacks--;
                decayTimer = 15;
            }
        }

        public override void HoldItem(Player player)
        {
            base.HoldItem(player);

            if (player.ItemAnimationActive)
                decayTimer = DecayTime;

            if (crescendoStacks >= 10 && Main.rand.NextBool(3))
            {
                float intensity = crescendoStacks / (float)MaxStacks;
                float angle = Main.GameUpdateCount * 0.05f;
                float radius = 25f + intensity * 15f;
                for (int i = 0; i < 2; i++)
                {
                    float a = angle + MathHelper.Pi * i;
                    Vector2 pos = player.Center + a.ToRotationVector2() * radius;
                    Color c = Color.Lerp(NachtmusikCosmicVFX.Violet, NachtmusikCosmicVFX.Gold, intensity);
                    Dust d = Dust.NewDustPerfect(pos, DustID.PurpleTorch, Vector2.Zero, 0, c, 0.7f * intensity);
                    d.noGravity = true;
                }
            }

            if (Main.rand.NextBool(10))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.Enchanted_Gold,
                    Vector2.Zero, 0, NachtmusikCosmicVFX.Violet, 0.5f);
                d.noGravity = true;
            }

            Lighting.AddLight(player.Center, NachtmusikCosmicVFX.Violet.ToVector3() * 0.3f);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            Vector2 pos = Item.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;
            Color glow = NachtmusikCosmicVFX.Violet with { A = 0 } * 0.35f;

            spriteBatch.Draw(tex, pos, null, glow, rotation, origin, scale * 1.12f, SpriteEffects.None, 0f);
            return true;
        }

        protected override Color GetLoreColor() => NachtmusikCosmicVFX.Violet;

        protected override void AddWeaponTooltips(List<TooltipLine> tooltips)
        {
            float dmgBonus = crescendoStacks * 12;
            float critBonus = crescendoStacks * 2;
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Rapid 3-phase combo that builds momentum"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", $"Crescendo Stacks: {crescendoStacks}/{MaxStacks} (+{dmgBonus:F0}% damage, +{critBonus:F0}% crit)"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Each hit adds a stack (max 15), stacks decay after 1.5s"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "At 8+ stacks, swings release crescendo waves"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Inflicts Celestial Harmony on all strikes"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each note builds upon the last, until the night itself sings'")
            { OverrideColor = NachtmusikCosmicVFX.Violet });
        }
    }

    // ========================================================================
    // TWILIGHT SEVERANCE — Ultra-fast katana with Dimension Sever ultimate
    // ========================================================================
    public class TwilightSeverance : MeleeSwingItemBase
    {
        protected override int SwingProjectileType => ModContent.ProjectileType<TwilightSeveranceSwing>();
        protected override int ComboStepCount => 3;

        private int twilightCharge;
        private const int MaxTwilightCharge = 100;

        public int TwilightCharge
        {
            get => twilightCharge;
            set => twilightCharge = Math.Clamp(value, 0, MaxTwilightCharge);
        }

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }

        protected override void SetWeaponDefaults()
        {
            Item.damage = 1450;
            Item.knockBack = 4f;
            Item.useTime = 8;
            Item.useAnimation = 8;
            Item.scale = 1.3f;
            Item.crit = 25;
            Item.value = Terraria.Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item71 with { Pitch = 0.4f };
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                if (twilightCharge < MaxTwilightCharge)
                    return false;
                Item.useTime = 25;
                Item.useAnimation = 25;
            }
            else
            {
                Item.useTime = 8;
                Item.useAnimation = 8;
            }
            return base.CanUseItem(player);
        }

        protected override void OnShoot(Player player, int projectileIndex)
        {
            if (player.altFunctionUse == 2 && twilightCharge >= MaxTwilightCharge)
            {
                twilightCharge = 0;

                Vector2 dir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                var source = player.GetSource_ItemUse(Item);
                int dmg = (int)(Item.damage * 3f * player.GetDamage(DamageClass.Melee).Multiplicative);

                for (int i = -2; i <= 2; i++)
                {
                    float angle = dir.ToRotation() + MathHelper.ToRadians(i * 10f);
                    Vector2 vel = angle.ToRotationVector2() * 20f;
                    Projectile.NewProjectile(source, player.Center, vel,
                        ModContent.ProjectileType<TwilightSlashProjectile>(),
                        dmg, Item.knockBack * 1.5f, player.whoAmI);
                }

                NachtmusikCosmicVFX.SpawnCelestialExplosion(player.Center, 1.8f);
                NachtmusikCosmicVFX.SpawnGlyphBurst(player.Center, 8, 5f);
                NachtmusikCosmicVFX.SpawnMusicNoteBurst(player.Center, 6, 5f);
                MagnumScreenEffects.AddScreenShake(10f);
            }
            else
            {
                // Every 3rd normal slash fires perpendicular slash pair
                int phase = (CurrentComboStep + ComboStepCount - 1) % ComboStepCount;
                if (phase == 2)
                {
                    Vector2 dir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                    var source = player.GetSource_ItemUse(Item);
                    int dmg = (int)(player.GetWeaponDamage(Item) * 0.4f);

                    Vector2 perp1 = dir.RotatedBy(MathHelper.PiOver2) * 14f;
                    Vector2 perp2 = dir.RotatedBy(-MathHelper.PiOver2) * 14f;

                    Projectile.NewProjectile(source, player.Center, perp1,
                        ModContent.ProjectileType<TwilightSlashProjectile>(),
                        dmg, Item.knockBack * 0.5f, player.whoAmI);
                    Projectile.NewProjectile(source, player.Center, perp2,
                        ModContent.ProjectileType<TwilightSlashProjectile>(),
                        dmg, Item.knockBack * 0.5f, player.whoAmI);

                    NachtmusikCosmicVFX.SpawnCelestialImpact(player.Center, 0.6f);
                }

                twilightCharge = Math.Min(twilightCharge + 5, MaxTwilightCharge);
            }
        }

        public override void HoldItem(Player player)
        {
            base.HoldItem(player);

            if (!player.ItemAnimationActive && twilightCharge > 0)
                twilightCharge = Math.Max(0, twilightCharge - 1);

            if (twilightCharge >= MaxTwilightCharge && Main.rand.NextBool(3))
            {
                float angle = Main.GameUpdateCount * 0.08f;
                for (int i = 0; i < 2; i++)
                {
                    float a = angle + MathHelper.Pi * i;
                    Vector2 pos = player.Center + a.ToRotationVector2() * 28f;
                    Dust d = Dust.NewDustPerfect(pos, DustID.PurpleTorch, Vector2.Zero, 0, NachtmusikCosmicVFX.DuskViolet, 1f);
                    d.noGravity = true;
                }
            }

            if (Main.rand.NextBool(8))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.Enchanted_Gold,
                    Vector2.Zero, 0, NachtmusikCosmicVFX.StarGold, 0.5f);
                d.noGravity = true;
            }

            Lighting.AddLight(player.Center, NachtmusikCosmicVFX.DuskViolet.ToVector3() * 0.35f);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            Vector2 pos = Item.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;
            Color glow = NachtmusikCosmicVFX.DuskViolet with { A = 0 } * 0.4f;

            spriteBatch.Draw(tex, pos, null, glow, rotation, origin, scale * 1.1f, SpriteEffects.None, 0f);
            return true;
        }

        protected override Color GetLoreColor() => NachtmusikCosmicVFX.Gold;

        protected override void AddWeaponTooltips(List<TooltipLine> tooltips)
        {
            int chargePercent = (int)(twilightCharge / (float)MaxTwilightCharge * 100f);
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Ultra-fast 3-phase katana combo"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", $"Twilight Charge: {chargePercent}%"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Builds charge on swing (+5), decays when idle"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Every 3rd slash fires perpendicular blade waves"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Right-click at full charge: Dimension Sever (3x damage fan)"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Inflicts Celestial Harmony on all strikes"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Between dusk and dawn lies the blade that severs all'")
            { OverrideColor = NachtmusikCosmicVFX.Gold });
        }
    }
}
