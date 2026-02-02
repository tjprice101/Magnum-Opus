using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
using MagnumOpus.Content.Autumn.Materials;
using MagnumOpus.Content.Autumn.Projectiles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Autumn.Weapons
{
    /// <summary>
    /// Decay Bell - Autumn-themed summon weapon (Post-Plantera tier)
    /// A cursed bell that summons spirits of decay.
    /// - Harvest Wraith: Summons spectral reapers that attack enemies (88 damage)
    /// - Death Toll: Wraiths periodically toll, dealing AoE damage
    /// - Soul Reap: Enemies killed by wraiths drop healing orbs
    /// - Decay Synchrony: 3+ wraiths create withering aura around player
    /// </summary>
    public class DecayBell : ModItem
    {
        private static readonly Color DecayPurple = new Color(100, 50, 120);
        private static readonly Color WraithGreen = new Color(120, 180, 100);
        private static readonly Color AutumnOrange = new Color(255, 140, 50);
        private static readonly Color SoulWhite = new Color(240, 240, 255);

        public override void SetStaticDefaults()
        {
            ItemID.Sets.StaffMinionSlotsRequired[Type] = 1f;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 38;
            Item.damage = 88;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 14;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3.5f;
            Item.value = Item.buyPrice(gold: 30);
            Item.rare = ItemRarityID.Lime;
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<HarvestWraithMinion>();
            Item.buffType = ModContent.BuffType<HarvestWraithBuff>();
        }

        public override void HoldItem(Player player)
        {
            // === IRIDESCENT WINGSPAN-STYLE HEAVY DUST TRAILS ===
            // Heavy decay dust trail #1
            float trailProgress1 = Main.rand.NextFloat();
            Color purpleGradient = Color.Lerp(DecayPurple, WraithGreen, trailProgress1);
            Dust heavyDecay = Dust.NewDustDirect(player.position, player.width, player.height, 
                DustID.PurpleTorch, player.velocity.X * 0.3f, player.velocity.Y * 0.3f, 100, purpleGradient, 1.5f);
            heavyDecay.noGravity = true;
            heavyDecay.fadeIn = 1.4f;
            heavyDecay.velocity = heavyDecay.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(1.2f, 1.8f);
            
            // Heavy wraith green dust trail #2
            float trailProgress2 = Main.rand.NextFloat();
            Color greenGradient = Color.Lerp(WraithGreen, SoulWhite, trailProgress2);
            Dust heavyGreen = Dust.NewDustDirect(player.position, player.width, player.height, 
                DustID.CursedTorch, player.velocity.X * 0.25f, player.velocity.Y * 0.25f, 80, greenGradient, 1.4f);
            heavyGreen.noGravity = true;
            heavyGreen.fadeIn = 1.3f;
            heavyGreen.velocity = heavyGreen.velocity.RotatedByRandom(0.25f) * Main.rand.NextFloat(1.1f, 1.6f);
            
            // === CONTRASTING SPARKLES (every 1-in-2 frames) ===
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(28f, 28f);
                Color sparkleColor = Main.rand.NextBool() ? DecayPurple : WraithGreen;
                CustomParticles.PrismaticSparkle(sparklePos, sparkleColor, 0.26f);
            }
            
            // === ORBITING SOUL WISPS ===
            if (Main.rand.NextBool(6))
            {
                float orbitAngle = Main.GameUpdateCount * 0.04f;
                for (int i = 0; i < 3; i++)
                {
                    float angle = orbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 orbitPos = player.Center + angle.ToRotationVector2() * (30f + Main.rand.NextFloat(8f));
                    Color orbitColor = Color.Lerp(DecayPurple, WraithGreen, (float)i / 3f);
                    CustomParticles.GenericFlare(orbitPos, orbitColor * 0.6f, 0.22f, 15);
                }
            }
            
            // === MUSIC NOTES - the death bell's toll ===
            if (Main.rand.NextBool(15))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(22f, 22f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1.2f);
                Color noteColor = Color.Lerp(DecayPurple, WraithGreen, Main.rand.NextFloat());
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.85f, 26);
            }

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.12f + 0.55f;
            Lighting.AddLight(player.Center, DecayPurple.ToVector3() * pulse);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Apply buff
            player.AddBuff(Item.buffType, 2);

            Vector2 summonPos = Main.MouseWorld;
            
            // === SPECTACULAR SUMMONING RITUAL VFX ===
            // Multi-layer flare cascade at summon location
            CustomParticles.GenericFlare(summonPos, Color.White, 0.7f, 18);
            CustomParticles.GenericFlare(summonPos, WraithGreen, 0.6f, 22);
            CustomParticles.GenericFlare(summonPos, DecayPurple, 0.5f, 25);
            
            // Gradient halo rings - summoning circle
            for (int ring = 0; ring < 6; ring++)
            {
                float progress = (float)ring / 6f;
                Color ringColor = Color.Lerp(DecayPurple, WraithGreen, progress);
                CustomParticles.HaloRing(summonPos, ringColor * 0.75f, 0.3f + ring * 0.12f, 14 + ring * 4);
            }
            
            // Heavy soul dust burst
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                float progress = (float)i / 12f;
                Color burstColor = Color.Lerp(DecayPurple, WraithGreen, progress);
                
                var burst = new GenericGlowParticle(summonPos, burstVel, burstColor * 0.6f, 0.28f, 22, true);
                MagnumParticleHandler.SpawnParticle(burst);
                
                // Heavy dust
                Dust soul = Dust.NewDustPerfect(summonPos, DustID.CursedTorch, burstVel * 0.7f, 100, burstColor, 1.3f);
                soul.noGravity = true;
                soul.fadeIn = 1.2f;
            }
            
            // Contrasting sparkle ring
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 sparklePos = summonPos + angle.ToRotationVector2() * 22f;
                Color sparkleColor = i % 2 == 0 ? DecayPurple : WraithGreen;
                CustomParticles.PrismaticSparkle(sparklePos, sparkleColor, 0.32f);
            }
            
            // Music notes - the summoning bell toll
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3f) + new Vector2(0, -1f);
                Color noteColor = Color.Lerp(DecayPurple, SoulWhite, Main.rand.NextFloat());
                ThemedParticles.MusicNote(summonPos, noteVel, noteColor, 0.9f, 28);
            }
            
            // Rising soul wisps
            for (int i = 0; i < 4; i++)
            {
                Vector2 wispPos = summonPos + Main.rand.NextVector2Circular(20f, 10f);
                Vector2 wispVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-2f, -3.5f));
                Color wispColor = Color.Lerp(WraithGreen, SoulWhite, Main.rand.NextFloat());
                var wisp = new GenericGlowParticle(wispPos, wispVel, wispColor * 0.65f, 0.25f, 30, true);
                MagnumParticleHandler.SpawnParticle(wisp);
            }

            // Spawn minion at cursor
            Projectile.NewProjectile(source, summonPos, Vector2.Zero, type, damage, knockback, player.whoAmI);

            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.1f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, position, null, DecayPurple * 0.32f, rotation, origin, scale * pulse * 1.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, WraithGreen * 0.25f, rotation, origin, scale * 1.15f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, AutumnOrange * 0.18f, rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, DecayPurple.ToVector3() * 0.45f);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "MinionSlots", "Uses 1 minion slot") { OverrideColor = Color.Gray });
            tooltips.Add(new TooltipLine(Mod, "HarvestWraith", "Summons spectral harvest wraiths that reap enemies") { OverrideColor = WraithGreen });
            tooltips.Add(new TooltipLine(Mod, "DeathToll", "Wraiths periodically toll, dealing AoE damage") { OverrideColor = DecayPurple });
            tooltips.Add(new TooltipLine(Mod, "SoulReap", "Enemies killed by wraiths drop healing orbs") { OverrideColor = SoulWhite });
            tooltips.Add(new TooltipLine(Mod, "DecaySynchrony", "3+ wraiths create a withering aura around you") { OverrideColor = AutumnOrange });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The bell tolls for all who stand in your way'") { OverrideColor = Color.Lerp(DecayPurple, WraithGreen, 0.5f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<HarvestBar>(), 16)
                .AddIngredient(ModContent.ItemType<AutumnResonantEnergy>(), 1)
                .AddIngredient(ItemID.SoulofFright, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    /// <summary>
    /// Buff for Harvest Wraith minions
    /// </summary>
    public class HarvestWraithBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.CursedInferno;
        
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<HarvestWraithMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;

                // Decay Synchrony: 3+ wraiths create withering aura
                if (player.ownedProjectileCounts[ModContent.ProjectileType<HarvestWraithMinion>()] >= 3)
                {
                    // Aura damage to nearby enemies
                    foreach (NPC npc in Main.npc)
                    {
                        if (!npc.CanBeChasedBy()) continue;
                        if (Vector2.Distance(player.Center, npc.Center) < 200f)
                        {
                            npc.AddBuff(BuffID.CursedInferno, 30);
                        }
                    }

                    // Simple aura VFX - EARLY GAME
                    if (Main.rand.NextBool(12))
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float dist = Main.rand.NextFloat(80f, 160f);
                        Vector2 pos = player.Center + angle.ToRotationVector2() * dist;
                        Vector2 vel = (angle + MathHelper.PiOver2).ToRotationVector2() * 1f + new Vector2(0, -0.3f);
                        Color auraColor = Color.Lerp(new Color(100, 50, 120), new Color(120, 180, 100), Main.rand.NextFloat()) * 0.3f;
                        var aura = new GenericGlowParticle(pos, vel, auraColor, 0.16f, 20, true);
                        MagnumParticleHandler.SpawnParticle(aura);
                    }
                }
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}
