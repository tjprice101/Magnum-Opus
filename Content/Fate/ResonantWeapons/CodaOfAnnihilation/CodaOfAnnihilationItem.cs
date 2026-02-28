using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Fate.Debuffs;
using MagnumOpus.Content.Fate.ResonantWeapons.CodaOfAnnihilation.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.CodaOfAnnihilation.Particles;
using MagnumOpus.Content.Fate.ResonantWeapons.CodaOfAnnihilation.Projectiles;
// Recipe imports — Moonlight Sonata
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
// Recipe imports — Eroica
using MagnumOpus.Content.Eroica.Weapons.SakurasBlossom;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
// Recipe imports — La Campanella
using MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime;
using MagnumOpus.Content.LaCampanella.ResonanceEnergies;
// Recipe imports — Enigma Variations
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
// Recipe imports — Swan Lake
using MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
// Recipe imports — Fate
using MagnumOpus.Content.Fate.ResonanceEnergies;
using MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima;

namespace MagnumOpus.Content.Fate.ResonantWeapons.CodaOfAnnihilation
{
    /// <summary>
    /// Coda of Annihilation — THE Zenith of MagnumOpus.
    /// A legendary melee weapon that throws spectral copies of all musical themes' melee weapons.
    /// Cycles through 14 weapon indices. Each swing spawns 2-3 flying homing swords + 1 held swing.
    /// Self-contained — uses only Coda systems (CodaParticleHandler, CodaTrailRenderer, CodaUtils).
    /// </summary>
    public class CodaOfAnnihilationItem : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/CodaOfAnnihilation";

        // Track the held swing projectile
        private int heldSwingProjectile = -1;
        private bool swingSpawnedThisUse = false;

        public override void SetDefaults()
        {
            Item.damage = 1350;
            Item.DamageType = DamageClass.Melee;
            Item.width = 54;
            Item.height = 54;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item1 with { Pitch = 0.1f, Volume = 0.8f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<CodaZenithSword>();
            Item.shootSpeed = 22f;
            Item.crit = 15;
            Item.channel = true;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.channel)
            {
                Vector2 toMouse = Main.MouseWorld - player.Center;
                player.itemRotation = toMouse.ToRotation();
                if (player.direction == -1)
                    player.itemRotation += MathHelper.Pi;
            }
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            noHitbox = true;
        }

        public override void HoldItem(Player player)
        {
            // Reset spawn flag when animation ends or new cycle starts
            if (player.itemAnimation <= 0)
            {
                swingSpawnedThisUse = false;
                heldSwingProjectile = -1;
            }
            else if (player.itemAnimation == player.itemAnimationMax - 1)
            {
                swingSpawnedThisUse = false;
            }

            // Spawn held swing projectile ONCE per animation cycle
            if (player.itemAnimation > 0 && !swingSpawnedThisUse)
            {
                swingSpawnedThisUse = true;

                if (Main.myPlayer == player.whoAmI)
                {
                    heldSwingProjectile = Projectile.NewProjectile(
                        player.GetSource_ItemUse(Item),
                        player.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<CodaHeldSwing>(),
                        Item.damage,
                        Item.knockBack,
                        player.whoAmI
                    );
                }
            }

            // Ambient VFX while holding
            if (Main.dedServ) return;
            HoldItemVFX(player);
        }

        private void HoldItemVFX(Player player)
        {
            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Faint constellation orbit
            if (Main.rand.NextBool(8))
            {
                float orbitAngle = time * 0.03f + Main.rand.NextFloat(MathHelper.TwoPi);
                float orbitRadius = 40f + MathF.Sin(time * 0.02f) * 10f;
                Vector2 orbitPos = center + orbitAngle.ToRotationVector2() * orbitRadius;
                Color starCol = CodaUtils.GetAnnihilationGradient(Main.rand.NextFloat());
                CodaParticleHandler.SpawnParticle(new CosmicMoteParticle(
                    orbitPos, Main.rand.NextVector2Circular(0.3f, 0.3f), starCol * 0.5f, 0.18f, 18));
            }

            // Ambient star sparkles
            if (Main.rand.NextBool(8))
            {
                Vector2 sparkPos = center + Main.rand.NextVector2Circular(35f, 35f);
                Color sparkCol = Main.rand.NextBool(3) ? CodaUtils.StarGold : CodaUtils.AnnihilationWhite;
                CodaParticleHandler.SpawnParticle(new CosmicMoteParticle(
                    sparkPos, Main.rand.NextVector2Circular(0.5f, 0.5f), sparkCol * 0.4f, 0.15f, 16));
            }

            // Music notes
            if (Main.rand.NextBool(12))
            {
                Color noteCol = Color.Lerp(CodaUtils.CodaCrimson, CodaUtils.CodaPurple, Main.rand.NextFloat());
                CodaParticleHandler.SpawnParticle(new ZenithNoteParticle(
                    center + Main.rand.NextVector2Circular(20f, 20f),
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -0.8f),
                    noteCol * 0.5f, 0.3f, 25));
            }

            // Glyph accent
            if (Main.rand.NextBool(15))
            {
                CodaParticleHandler.SpawnParticle(new GlyphBurstParticle(
                    center + Main.rand.NextVector2Circular(30f, 30f),
                    CodaUtils.CodaPink * 0.4f, 0.22f, 16));
            }

            // Pulse lighting
            float pulse = 0.3f + MathF.Sin(time * 0.05f) * 0.12f;
            Lighting.AddLight(center, CodaUtils.CodaPurple.ToVector3() * pulse);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Increment weapon cycle index (mod 14)
            var codaPlayer = player.Coda();
            int currentIndex = codaPlayer.WeaponCycleIndex;

            // Spawn 2-3 sword projectiles per swing
            int swordCount = Main.rand.Next(2, 4);

            for (int i = 0; i < swordCount; i++)
            {
                // Zenith-style spawn position — above player with randomness
                Vector2 spawnOffset = new Vector2(
                    Main.rand.NextFloat(-80f, 80f),
                    Main.rand.NextFloat(-100f, -30f)
                );
                Vector2 spawnPos = player.Center + spawnOffset;

                // Direction toward mouse with spread
                Vector2 toMouse = Main.MouseWorld - spawnPos;
                toMouse = toMouse.SafeNormalize(Vector2.UnitY);
                toMouse = toMouse.RotatedByRandom(MathHelper.ToRadians(25f));

                float speed = Main.rand.NextFloat(16f, 22f);
                Vector2 projVelocity = toMouse * speed;

                // Spawn with current weapon index
                Projectile.NewProjectile(
                    source,
                    spawnPos,
                    projVelocity,
                    type,
                    damage,
                    knockback,
                    player.whoAmI,
                    currentIndex, // ai[0] = weapon index
                    0             // ai[1] = target (0 = none)
                );

                // Spawn VFX at spawn location
                Color spawnColor = CodaUtils.GetWeaponColor(currentIndex);
                CodaParticleHandler.SpawnParticle(new AnnihilationFlareParticle(
                    spawnPos, spawnColor, 0.5f, 14));
                CodaParticleHandler.SpawnParticle(new AnnihilationFlareParticle(
                    spawnPos, Color.White, 0.3f, 10));

                // Cycle to next weapon
                currentIndex = (currentIndex + 1) % 14;
            }

            // Update stored index
            codaPlayer.WeaponCycleIndex = currentIndex;

            // Swing sound varies
            if (Main.rand.NextBool(3))
            {
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = Main.rand.NextFloat(-0.2f, 0.3f), Volume = 0.5f }, player.Center);
            }

            return false; // We handled spawning manually
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<IncisorOfMoonlight>()
                .AddIngredient<EternalMoon>()
                .AddIngredient<SakurasBlossom>()
                .AddIngredient<CelestialValor>()
                .AddIngredient<IgnitionOfTheBell>()
                .AddIngredient<DualFatedChime>()
                .AddIngredient<VariationsOfTheVoidItem>()
                .AddIngredient<TheUnresolvedCadenceItem>()
                .AddIngredient<CalloftheBlackSwan>()
                .AddIngredient<TheConductorsLastConstellation.TheConductorsLastConstellationItem>()
                .AddIngredient<RequiemOfReality.RequiemOfRealityItem>()
                .AddIngredient<OpusUltimaItem>()
                .AddIngredient<FractalOfTheStars.FractalOfTheStarsItem>()
                .AddIngredient(ItemID.Zenith)
                .AddIngredient<MoonlightsResonantEnergy>(15)
                .AddIngredient<EroicasResonantEnergy>(15)
                .AddIngredient<LaCampanellaResonantEnergy>(15)
                .AddIngredient<EnigmaResonantEnergy>(15)
                .AddIngredient<SwansResonanceEnergy>(15)
                .AddIngredient<FateResonantEnergy>(15)
                .AddTile<MoonlightAnvilTile>()
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Launches spectral echoes of every score's legendary blades"));
            tooltips.Add(new TooltipLine(Mod, "FateSpecial", "Each swing summons a different weapon from the symphony of fate"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The final movement - a symphony of every blade that came before'")
            {
                OverrideColor = CodaUtils.LoreColor
            });
        }
    }
}
