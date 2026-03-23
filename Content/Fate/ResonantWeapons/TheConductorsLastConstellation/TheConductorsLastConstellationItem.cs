using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Content.Fate.ResonantWeapons.TheConductorsLastConstellation.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.TheConductorsLastConstellation.Particles;
using MagnumOpus.Content.Fate.ResonantWeapons.TheConductorsLastConstellation.Projectiles;
using MagnumOpus.Content.SandboxExoblade.Utilities;

namespace MagnumOpus.Content.Fate.ResonantWeapons.TheConductorsLastConstellation
{
    /// <summary>
    /// The Conductor's Last Constellation — A cosmic blade that IS the conductor's baton.
    ///
    /// SELF-CONTAINED WEAPON SYSTEM (no shared VFX libraries):
    ///   - Own particle system (ConductorParticleHandler)
    ///   - Own GPU trail renderer (ConductorTrailRenderer)
    ///   - Own shader pipeline (ConductorShaderLoader → 4 .fx files, 5 keys)
    ///   - Own ModPlayer state (ConstellationConductorPlayer via player.Conductor())
    ///   - Own projectiles (ConductorSwingProjectile, ConductorSwordBeam)
    ///
    /// ATTACK PATTERN:
    ///   TRUE MELEE — fires ConductorSwingProjectile as a held swing.
    ///   3-phase combo (orchestral movements):
    ///     Movement I  (Downbeat):  Powerful downward sweep + 3 descending beam columns
    ///     Movement II (Crescendo): Rising sweep + intensifying beams
    ///     Movement III (Forte):    Wide horizontal sweep + lightning cascade
    ///   On swing: fires 3 homing ConductorSwordBeam projectiles in 18° spread
    ///   On hit: DestinyCollapse (5s), 3 cosmic lightning strikes, 5 seeking crystal shards at 25% dmg
    ///   On 3rd combo: Convergence — all active beams converge with cosmic lightning storm
    /// </summary>
    public class TheConductorsLastConstellationItem : ModItem, IOverdriveItem
    {
        public IResonantOverdrive GetOverdrivePlayer(Player player) => player.GetModPlayer<ConstellationConductorPlayer>();

        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/TheConductorsLastConstellation";

        private static Asset<Texture2D> _glowTex;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            // === PRESERVED STATS ===
            Item.damage = 780;
            Item.DamageType = DamageClass.Melee;
            Item.width = 50;
            Item.height = 50;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.autoReuse = true;
            Item.shootSpeed = 14f;

            // === HELD PROJECTILE SWING (TRUE MELEE) ===
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<ConductorSwingProjectile>();
            Item.channel = true;
            Item.UseSound = null; // Swing projectile handles sounds
        }

        public override bool CanShoot(Player player)
        {
            bool isDash = player.altFunctionUse == 2;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.owner != player.whoAmI || p.type != Item.shoot)
                    continue;
                if (isDash) return false;
                if (!(p.ai[0] == 1 && p.ai[1] == 1)) return false;
            }
            return true;
        }

        public override bool AltFunctionUse(Player player) => true;
        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "3-phase orchestral combo: Downbeat, Crescendo, Forte"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Each swing fires 3 aggressively homing beams in an 18-degree spread"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "On hit: 3 cosmic lightning bolts + 5 seeking crystal shards at 25% damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Forte's 3rd combo hit triggers Convergence — all beams converge on cursor with cosmic lightning"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "A Star Map constellation builds around you while attacking"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Fully-powered beam kills shatter the constellation into 8-12 homing star projectiles"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The last constellation is the one the conductor draws with their final baton stroke.'")
            {
                OverrideColor = new Color(180, 40, 80) // Cosmic Crimson (Fate theme)
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Right-click: Spectral Blade throw toward mouse
            if (player.altFunctionUse == 2)
            {
                var cp = player.Conductor();
                if (cp.IsChargeFull)
                {
                    cp.ConsumeCharge();
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f }, player.Center);
                    Vector2 toMouse = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX) * 14f;
                    Projectile.NewProjectile(source, player.MountedCenter, toMouse,
                        ModContent.ProjectileType<ConductorSpecialProj>(),
                        (int)(damage * 1.5f), knockback, player.whoAmI);
                }
                else
                    SoundEngine.PlaySound(SoundID.Item16 with { Pitch = 0.5f, Volume = 0.5f }, player.Center);
                return false;
            }

            float state = player.altFunctionUse == 2 ? 1f : 0f;
            Projectile.NewProjectile(source, player.MountedCenter,
                (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI, state, 0);
            return false;
        }

        public override void HoldItem(Player player)
        {
            player.Conductor().IsHoldingConductorsConstellation = true;
            player.ExoBlade().rightClickListener = true;
            player.ExoBlade().mouseWorldListener = true;

            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;
            Vector2 center = player.MountedCenter;

            // Orbiting conductor glyphs — triple orbit at 45f radius
            if (Main.rand.NextBool(8))
            {
                for (int i = 0; i < 3; i++)
                {
                    float orbitAngle = time * 0.035f + MathHelper.TwoPi * i / 3f;
                    float radius = 45f + MathF.Sin(time * 0.05f + i * 2f) * 6f;
                    Vector2 glyphPos = center + orbitAngle.ToRotationVector2() * radius;
                    Color glyphCol = ConductorUtils.PaletteLerp((float)i / 3f + 0.15f);
                    ConductorParticleHandler.SpawnParticle(new ConductorGlyph(
                        glyphPos, glyphCol * 0.6f, 0.2f, 20));
                }
            }

            // Electric mote aura
            if (Main.rand.NextBool(6))
            {
                Vector2 motePos = center + Main.rand.NextVector2Circular(35f, 35f);
                Color moteCol = Main.rand.NextBool(3) ? ConductorUtils.LightningGold : ConductorUtils.ConductorCyan;
                ConductorParticleHandler.SpawnParticle(new ConductorMote(
                    motePos, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    moteCol * 0.55f, 0.18f, 16));
            }

            // Cosmic nebula wisps while moving
            if (player.velocity.Length() > 2f && Main.rand.NextBool(4))
            {
                Vector2 wispPos = center + Main.rand.NextVector2Circular(20f, 20f);
                Color wispCol = Color.Lerp(ConductorUtils.VoidBlack, ConductorUtils.ConductorCyan, Main.rand.NextFloat()) * 0.35f;
                ConductorParticleHandler.SpawnParticle(new ConductorNebulaWisp(
                    wispPos, -player.velocity * 0.08f + Main.rand.NextVector2Circular(0.8f, 0.8f),
                    wispCol, 0.18f, 18));
            }

            // Occasional zigzag lightning spark nearby
            if (Main.rand.NextBool(12))
            {
                Vector2 sparkPos = center + Main.rand.NextVector2Circular(40f, 40f);
                Vector2 sparkVel = Main.rand.NextVector2Circular(2f, 2f);
                ConductorParticleHandler.SpawnParticle(new LightningSpark(
                    sparkPos, sparkVel, ConductorUtils.ConductorCyan * 0.4f,
                    0.12f, 14, 3f, 0.4f));
            }

            // Pulsing conductor light
            float pulse = 0.28f + MathF.Sin(time * 0.05f) * 0.1f;
            Color lightCol = Color.Lerp(ConductorUtils.ConductorCyan, ConductorUtils.StarSilver,
                MathF.Sin(time * 0.03f) * 0.5f + 0.5f);
            Lighting.AddLight(center, lightCol.ToVector3() * pulse);
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            if (Main.dedServ) return;

            _glowTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow");
            if (_glowTex?.Value == null) return;

            Texture2D itemTex = Terraria.GameContent.TextureAssets.Item[Item.type].Value;
            Vector2 origin = itemTex.Size() / 2f;
            Vector2 drawPos = Item.Center - Main.screenPosition;
            float time = (float)Main.timeForVisualEffects;
            float pulse = 0.85f + MathF.Sin(time * 0.05f) * 0.15f;

            // Bloom layers behind item
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            ConductorUtils.DrawItemBloom(spriteBatch, itemTex, drawPos, origin, rotation, scale, pulse);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
