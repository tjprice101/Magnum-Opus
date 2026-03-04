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
using MagnumOpus.Content.Fate.ResonantWeapons.RequiemOfReality.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.RequiemOfReality.Particles;
using MagnumOpus.Content.Fate.ResonantWeapons.RequiemOfReality.Projectiles;

namespace MagnumOpus.Content.Fate.ResonantWeapons.RequiemOfReality
{
    /// <summary>
    /// Requiem of Reality — A blade that plays existence's funeral march.
    ///
    /// SELF-CONTAINED WEAPON SYSTEM (no shared VFX libraries):
    ///   - Own particle system (RequiemParticleHandler)
    ///   - Own GPU trail renderer (RequiemTrailRenderer)
    ///   - Own shader pipeline (RequiemShaderLoader → 4 .fx files)
    ///   - Own ModPlayer state (RequiemPlayer via player.Requiem())
    ///   - Own projectiles (RequiemSwingProjectile, RequiemSpectralBlade, RequiemCosmicNote)
    ///
    /// ATTACK PATTERN:
    ///   Fires RequiemSwingProjectile as a HELD swing (not vanilla useStyle).
    ///   Swing follows 4-movement combo cycle.
    ///   3-5 seeking music notes spawn per swing.
    ///   Every 4th swing = Finale movement + RequiemSpectralBlade autonomous combo.
    /// </summary>
    public class RequiemOfRealityItem : ModItem
    {
        // Texture path: sprite is in this folder alongside this .cs file
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/RequiemOfReality";

        // Glow texture for hold VFX
        private static Asset<Texture2D> _glowTex;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            // === PRESERVED STATS (from original) ===
            Item.damage = 740;
            Item.DamageType = DamageClass.Melee;
            Item.width = 58;
            Item.height = 58;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.knockBack = 5.5f;
            Item.value = Item.sellPrice(gold: 56);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.autoReuse = true;

            // === HELD PROJECTILE SWING (instead of vanilla ItemUseStyleID.Swing) ===
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true; // The swing projectile handles visuals
            Item.shoot = ModContent.ProjectileType<RequiemSwingProjectile>();
            Item.shootSpeed = 1f; // Not used for speed — swing projectile ignores velocity
            Item.channel = false;
            Item.UseSound = null; // Swing projectile handles its own sounds
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Swings release 3-5 cosmic music notes that seek nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "4-movement combo cycle: Adagio, Allegro, Scherzo, Finale"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Finale summons an autonomous spectral blade with a 6-phase attack sequence"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Hits build Spectral Resonance — at 3 stacks, triggers a 2.5x cosmic burst"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "15% chance on hit to tear reality, creating a lingering damage rift"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The cosmos does not mourn. It simply ends, and begins again.'")
            {
                OverrideColor = new Color(180, 40, 80) // Cosmic Crimson
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var rp = player.Requiem();

            // Fire the held swing projectile (RequiemSwingProjectile)
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, damage, knockback, player.whoAmI);

            // Register swing with player tracker
            bool comboTriggered = rp.OnSwing();

            // Spawn spectral blade on combo trigger (4th swing → Finale)
            if (comboTriggered && player.ownedProjectileCounts[ModContent.ProjectileType<RequiemSpectralBlade>()] < 1)
            {
                Projectile.NewProjectile(source, player.Center, Vector2.Zero,
                    ModContent.ProjectileType<RequiemSpectralBlade>(),
                    damage * 2, knockback, player.whoAmI);

                // Dramatic VFX burst at spawn
                SpawnSpectralBladeSpawnVFX(player.Center);
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.8f }, player.Center);
            }

            // Always spawn cosmic music notes (3-5)
            SpawnMusicNotes(source, player, damage, knockback);

            return false; // We already spawned the projectile
        }

        private void SpawnMusicNotes(IEntitySource source, Player player, int damage, float knockback)
        {
            Vector2 toMouse = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            float baseAngle = toMouse.ToRotation();
            int count = Main.rand.Next(3, 6);

            for (int i = 0; i < count; i++)
            {
                float spread = MathHelper.Lerp(-0.4f, 0.4f, (i + 0.5f) / count);
                Vector2 noteVel = (baseAngle + spread).ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                Vector2 spawnPos = player.Center + toMouse * 35f;

                Projectile.NewProjectile(source, spawnPos, noteVel,
                    ModContent.ProjectileType<RequiemCosmicNote>(),
                    (int)(damage * 0.4f), knockback * 0.3f, player.whoAmI,
                    Main.rand.Next(4),          // ai[0] = note type
                    Main.rand.Next(30, 60));     // ai[1] = seek delay
            }

            // Note spawn sound
            SoundEngine.PlaySound(SoundID.Item26 with { Pitch = 0.5f, Volume = 0.5f }, player.Center);
        }

        private void SpawnSpectralBladeSpawnVFX(Vector2 position)
        {
            if (Main.dedServ) return;

            // Big bloom flash
            RequiemParticleHandler.SpawnParticle(new RequiemBloomFlare(
                position, RequiemUtils.SupernovaWhite, 0.8f, 16));
            RequiemParticleHandler.SpawnParticle(new RequiemBloomFlare(
                position, RequiemUtils.BrightCrimson, 0.6f, 14));

            // Radial spark ring
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                RequiemParticleHandler.SpawnParticle(new RequiemSparkParticle(
                    position, sparkVel, RequiemUtils.GetCosmicGradient((float)i / 12f), 0.3f, 14));
            }

            // Glyph accents
            for (int i = 0; i < 4; i++)
            {
                Vector2 glyphPos = position + Main.rand.NextVector2Circular(25f, 25f);
                RequiemParticleHandler.SpawnParticle(new RequiemGlyphParticle(
                    glyphPos, RequiemUtils.DarkPink, 0.35f, 25));
            }
        }

        public override void HoldItem(Player player)
        {
            // Ambient weapon glow while held
            if (Main.dedServ) return;

            float pulse = 0.6f + MathF.Sin((float)Main.timeForVisualEffects * 0.04f) * 0.15f;
            Lighting.AddLight(player.Center, RequiemUtils.BrightCrimson.ToVector3() * 0.3f * pulse);

            // Occasional ambient mote
            if (Main.rand.NextBool(12))
            {
                Vector2 motePos = player.Center + Main.rand.NextVector2Circular(25f, 25f);
                RequiemParticleHandler.SpawnParticle(new RequiemMoteParticle(
                    motePos, new Vector2(0, -0.3f), RequiemUtils.FatePurple, 0.12f, 25));
            }
        }

        /// <summary>Draw additional held-item bloom (vanilla DrawInInventory doesn't do this).</summary>
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            if (Main.dedServ) return;

            _glowTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow");
            if (_glowTex?.Value == null) return;

            Texture2D itemTex = Terraria.GameContent.TextureAssets.Item[Item.type].Value;
            Vector2 origin = itemTex.Size() / 2f;
            Vector2 drawPos = Item.Center - Main.screenPosition;
            float pulse = 0.85f + MathF.Sin((float)Main.timeForVisualEffects * 0.05f) * 0.15f;

            try
            {
                RequiemUtils.BeginAdditive(spriteBatch);
                RequiemUtils.DrawItemBloom(spriteBatch, itemTex, drawPos, origin, rotation, scale, pulse);
                RequiemUtils.EndAdditive(spriteBatch);
            }
            catch
            {
                try { RequiemUtils.EndAdditive(spriteBatch); } catch { }
            }
        }
    }
}
