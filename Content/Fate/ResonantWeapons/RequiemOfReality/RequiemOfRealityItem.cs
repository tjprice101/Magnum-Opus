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
using MagnumOpus.Content.SandboxExoblade.Utilities;

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

        /// <summary>Tracks the 4-movement combo: Adagio → Allegro → Scherzo → Finale.
        /// Finale (every 4th swing) summons the autonomous RequiemSpectralBlade.</summary>
        private int movementCounter = 0;

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
            Item.channel = true;
            Item.UseSound = null; // Swing projectile handles its own sounds
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
            float state = player.altFunctionUse == 2 ? 1f : 0f;
            Projectile.NewProjectile(source, player.MountedCenter,
                (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI, state, 0);

            // --- 4-Movement Requiem Combo ---
            // Adagio (0): 2 cosmic notes with long seek delay (slow, drifting)
            // Allegro (1): 4 cosmic notes with short seek delay (fast, aggressive)
            // Scherzo (2): 3 notes in wide spread + 2 flanking notes
            // Finale (3): 5 notes burst + autonomous RequiemSpectralBlade
            Vector2 aimDir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            int movement = movementCounter % 4;
            movementCounter++;
            int noteType = ModContent.ProjectileType<RequiemCosmicNote>();
            int noteDmg = (int)(damage * 0.25f);

            switch (movement)
            {
                case 0: // Adagio — 2 slow drifting notes
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 noteVel = aimDir.RotatedBy(MathHelper.ToRadians(15 * i)) * 5f;
                        Projectile.NewProjectile(source, player.MountedCenter, noteVel,
                            noteType, noteDmg, knockback * 0.3f, player.whoAmI,
                            Main.rand.Next(4), 70f); // Long seek delay
                    }
                    break;

                case 1: // Allegro — 4 fast seeking notes
                    for (int i = 0; i < 4; i++)
                    {
                        float spread = MathHelper.ToRadians(-30 + 20 * i);
                        Vector2 noteVel = aimDir.RotatedBy(spread) * 8f;
                        Projectile.NewProjectile(source, player.MountedCenter, noteVel,
                            noteType, noteDmg, knockback * 0.3f, player.whoAmI,
                            Main.rand.Next(4), 25f); // Short seek delay
                    }
                    break;

                case 2: // Scherzo — 3 wide + 2 flanking
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 noteVel = aimDir.RotatedBy(MathHelper.ToRadians(30 * i)) * 7f;
                        Projectile.NewProjectile(source, player.MountedCenter, noteVel,
                            noteType, noteDmg, knockback * 0.3f, player.whoAmI,
                            Main.rand.Next(4), 40f);
                    }
                    // Flanking notes from the side
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 sideVel = aimDir.RotatedBy(MathHelper.PiOver2 * i) * 6f;
                        Projectile.NewProjectile(source, player.MountedCenter, sideVel,
                            noteType, noteDmg, knockback * 0.3f, player.whoAmI,
                            Main.rand.Next(4), 50f);
                    }
                    break;

                case 3: // Finale — 5 notes burst + autonomous spectral blade
                    for (int i = 0; i < 5; i++)
                    {
                        float angle = MathHelper.TwoPi / 5f * i;
                        Vector2 noteVel = aimDir.RotatedBy(angle) * 6f;
                        Projectile.NewProjectile(source, player.MountedCenter, noteVel,
                            noteType, (int)(damage * 0.3f), knockback * 0.4f, player.whoAmI,
                            Main.rand.Next(4), 30f);
                    }

                    // Summon the autonomous RequiemSpectralBlade
                    Projectile.NewProjectile(source, player.MountedCenter, aimDir * 2f,
                        ModContent.ProjectileType<RequiemSpectralBlade>(),
                        (int)(damage * 0.6f), knockback, player.whoAmI);
                    break;
            }

            return false;
        }

        public override void HoldItem(Player player)
        {
            player.ExoBlade().rightClickListener = true;
            player.ExoBlade().mouseWorldListener = true;

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
