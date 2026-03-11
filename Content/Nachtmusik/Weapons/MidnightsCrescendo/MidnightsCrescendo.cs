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
using MagnumOpus.Content.Nachtmusik.Debuffs;
using MagnumOpus.Content.Nachtmusik.Weapons.MidnightsCrescendo.Projectiles;
using MagnumOpus.Content.Nachtmusik.Weapons.MidnightsCrescendo.Utilities;

namespace MagnumOpus.Content.Nachtmusik.Weapons.MidnightsCrescendo
{
    /// <summary>
    /// Midnight's Crescendo — Rapid alternating sword that builds crescendo stacks.
    /// Each hit adds stacks (max 15), stacks amplify damage/crit/trail intensity.
    /// At 8+ stacks, swings release crescendo wave arcs extending reach.
    /// At max 15, weapon becomes a blinding storm of starlight and cosmic energy.
    /// "The night starts quiet. It does not end that way."
    /// </summary>
    public class MidnightsCrescendo : MeleeSwingItemBase
    {
        protected override int SwingProjectileType => ModContent.ProjectileType<MidnightsCrescendoSwing>();
        protected override int ComboStepCount => 3;

        private int crescendoStacks;
        private int decayTimer;
        private const int MaxStacks = 15;
        private const int DecayTime = 90; // 1.5s before decay starts

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
            damage *= 1f + crescendoStacks * 0.12f; // +12% per stack
        }

        public override void ModifyWeaponCrit(Player player, ref float crit)
        {
            crit += crescendoStacks * 2; // +2% crit per stack
        }

        protected override void OnShoot(Player player, int projectileIndex)
        {
            // At 8+ stacks, release crescendo wave arc
            if (crescendoStacks >= 8)
            {
                Vector2 dir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                var source = player.GetSource_ItemUse(Item);

                float waveDamageMult = crescendoStacks >= MaxStacks ? 1.2f : 0.6f;
                int dmg = (int)(player.GetWeaponDamage(Item) * waveDamageMult);

                Projectile.NewProjectile(source, player.Center, dir * 14f,
                    ModContent.ProjectileType<CrescendoWaveProjectile>(),
                    dmg, Item.knockBack * 0.5f, player.whoAmI,
                    ai0: crescendoStacks / (float)MaxStacks);

                MidnightsCrescendoVFX.WaveReleaseVFX(player.Center, crescendoStacks / (float)MaxStacks);
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
                decayTimer = 15; // Decay one stack per 0.25s
            }
        }

        public override void HoldItem(Player player)
        {
            base.HoldItem(player);

            if (player.ItemAnimationActive)
                decayTimer = DecayTime;

            // Momentum Preservation: 10+ stacks for 5s extends decay to 3s
            // (simplified: just extend decay timer if stacks are high)
            if (crescendoStacks >= 10)
                decayTimer = Math.Max(decayTimer, 180);

            float stackProgress = crescendoStacks / (float)MaxStacks;
            MidnightsCrescendoVFX.HoldItemVFX(player, stackProgress, crescendoStacks);

            // Orbiting starlight at high stacks
            if (crescendoStacks >= 5 && Main.rand.NextBool(3))
            {
                float intensity = stackProgress;
                float angle = Main.GameUpdateCount * 0.05f;
                float radius = 22f + intensity * 18f;
                int orbitCount = 1 + crescendoStacks / 5;
                for (int i = 0; i < orbitCount; i++)
                {
                    float a = angle + MathHelper.TwoPi * i / orbitCount;
                    Vector2 pos = player.Center + a.ToRotationVector2() * radius;
                    Color c = Color.Lerp(new Color(40, 30, 100), new Color(240, 245, 255), intensity);
                    Dust d = Dust.NewDustPerfect(pos, DustID.PurpleTorch, Vector2.Zero, 0, c, 0.6f + intensity * 0.5f);
                    d.noGravity = true;
                }
            }

            Lighting.AddLight(player.Center, new Color(60, 80, 180).ToVector3() * (0.2f + stackProgress * 0.4f));
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            Vector2 pos = Item.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;
            float stackProgress = crescendoStacks / (float)MaxStacks;

            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.05f * (1f + stackProgress * 2f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Glow intensity scales with stacks — from subtle indigo to brilliant stellar white
            Color outerColor = Color.Lerp(new Color(40, 30, 100), new Color(60, 80, 180), stackProgress) with { A = 0 };
            spriteBatch.Draw(tex, pos, null, outerColor * (0.2f + stackProgress * 0.25f),
                rotation, origin, scale * pulse * (1.15f + stackProgress * 0.2f), SpriteEffects.None, 0f);

            if (stackProgress > 0.3f)
            {
                Color midColor = Color.Lerp(new Color(60, 80, 180), new Color(180, 200, 230), stackProgress) with { A = 0 };
                spriteBatch.Draw(tex, pos, null, midColor * 0.2f * stackProgress,
                    rotation, origin, scale * pulse * 1.08f, SpriteEffects.None, 0f);
            }

            if (stackProgress > 0.7f)
            {
                Color coreColor = new Color(240, 245, 255) with { A = 0 };
                spriteBatch.Draw(tex, pos, null, coreColor * (stackProgress - 0.7f),
                    rotation, origin, scale * pulse * 1.03f, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

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

        protected override Color GetLoreColor() => new Color(100, 120, 200);

        protected override void AddWeaponTooltips(List<TooltipLine> tooltips)
        {
            float dmgBonus = crescendoStacks * 12;
            float critBonus = crescendoStacks * 2;
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Rapid 3-phase combo that builds momentum with each hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", $"Crescendo Stacks: {crescendoStacks}/{MaxStacks} (+{dmgBonus:F0}% damage, +{critBonus:F0}% crit)"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At 8+ stacks, each swing releases expanding crescendo wave arcs"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "At max 15 stacks, waves deal double damage — blinding cosmic storm"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Inflicts Celestial Harmony — +10% damage from all Nachtmusik weapons"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The night starts quiet. It does not end that way.'")
            { OverrideColor = new Color(100, 120, 200) });
        }
    }
}
