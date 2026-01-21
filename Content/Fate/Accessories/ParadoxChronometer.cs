using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Fate.ResonanceEnergies;
using MagnumOpus.Content.Fate.HarmonicCores;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Fate.Accessories
{
    /// <summary>
    /// Paradox Chronometer - Melee accessory for Fate theme.
    /// Manipulates the flow of time to enhance melee combat.
    /// Every 7th melee strike triggers a temporal echo that repeats the attack.
    /// </summary>
    public class ParadoxChronometer : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<ParadoxChronometerPlayer>();
            modPlayer.hasParadoxChronometer = true;
            
            // +18% melee damage
            player.GetDamage(DamageClass.Melee) += 0.18f;
            
            // +20% melee speed
            player.GetAttackSpeed(DamageClass.Melee) += 0.20f;
            
            // +10% melee critical strike chance
            player.GetCritChance(DamageClass.Melee) += 10;
            
            // Temporal chronometer ambient particles
            if (!hideVisual)
            {
                // Clock-hand style rotating particles
                float handAngle = Main.GameUpdateCount * 0.05f;
                
                if (Main.rand.NextBool(8))
                {
                    // Hour hand position
                    Vector2 hourPos = player.Center + (handAngle * 0.1f).ToRotationVector2() * 15f;
                    Dust hourDust = Dust.NewDustPerfect(hourPos, DustID.Enchanted_Pink, 
                        Vector2.Zero, 100, FateCosmicVFX.FateDarkPink, 0.6f);
                    hourDust.noGravity = true;
                    
                    // Minute hand position
                    Vector2 minPos = player.Center + handAngle.ToRotationVector2() * 25f;
                    Dust minDust = Dust.NewDustPerfect(minPos, DustID.Enchanted_Pink, 
                        Vector2.Zero, 100, FateCosmicVFX.FateBrightRed, 0.5f);
                    minDust.noGravity = true;
                }
                
                // Temporal glyphs
                if (Main.rand.NextBool(15))
                {
                    CustomParticles.Glyph(player.Center + Main.rand.NextVector2Circular(30f, 30f),
                        FateCosmicVFX.FatePurple, 0.2f, -1);
                }
            }
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "MeleeBoost", "+18% melee damage")
            {
                OverrideColor = new Color(255, 150, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "SpeedBoost", "+20% melee speed")
            {
                OverrideColor = new Color(255, 180, 120)
            });
            
            tooltips.Add(new TooltipLine(Mod, "CritBoost", "+10% melee critical strike chance")
            {
                OverrideColor = new Color(255, 200, 140)
            });
            
            tooltips.Add(new TooltipLine(Mod, "TemporalEcho", "Every 7th melee strike triggers a temporal echo")
            {
                OverrideColor = FateCosmicVFX.FateDarkPink
            });
            
            tooltips.Add(new TooltipLine(Mod, "EchoEffect", "Temporal echoes repeat the strike for 75% damage")
            {
                OverrideColor = FateCosmicVFX.FateBrightRed
            });
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'Time bends to the rhythm of your blade'")
            {
                OverrideColor = new Color(255, 150, 180)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<FateResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfFate>(), 5)
                .AddIngredient(ItemID.SoulofMight, 10)
                .AddIngredient(ItemID.FragmentSolar, 8)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    public class ParadoxChronometerPlayer : ModPlayer
    {
        public bool hasParadoxChronometer = false;
        public int meleeStrikeCounter = 0;
        
        private const int StrikesForEcho = 7;
        
        public override void ResetEffects()
        {
            hasParadoxChronometer = false;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasParadoxChronometer) return;
            if (!item.CountsAsClass(DamageClass.Melee)) return;
            
            ProcessMeleeHit(target, damageDone, hit.Crit);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasParadoxChronometer) return;
            if (!proj.CountsAsClass(DamageClass.Melee)) return;
            if (proj.minion || proj.sentry) return;
            
            ProcessMeleeHit(target, damageDone, hit.Crit);
        }

        private void ProcessMeleeHit(NPC target, int damage, bool wasCrit)
        {
            meleeStrikeCounter++;
            
            // Show counter progress with subtle particles
            if (meleeStrikeCounter >= StrikesForEcho - 2)
            {
                float intensity = (float)(meleeStrikeCounter - (StrikesForEcho - 3)) / 3f;
                CustomParticles.GenericFlare(target.Center, FateCosmicVFX.FateDarkPink * intensity, 0.3f, 10);
            }
            
            if (meleeStrikeCounter >= StrikesForEcho)
            {
                meleeStrikeCounter = 0;
                TriggerTemporalEcho(target, damage, wasCrit);
            }
        }

        private void TriggerTemporalEcho(NPC target, int originalDamage, bool wasCrit)
        {
            if (!target.active) return;
            
            // Temporal echo VFX - time distortion effect
            // Afterimage-style burst
            for (int i = 0; i < 5; i++)
            {
                float progress = i / 5f;
                Vector2 echoOffset = Main.rand.NextVector2Circular(20f, 20f);
                Color echoColor = Color.Lerp(FateCosmicVFX.FateWhite, FateCosmicVFX.FateDarkPink, progress);
                CustomParticles.GenericFlare(target.Center + echoOffset, echoColor * (1f - progress * 0.5f), 0.4f, 15);
            }
            
            // Temporal glyph burst
            CustomParticles.GlyphBurst(target.Center, FateCosmicVFX.FatePurple, 5, 4f);
            
            // Halo ring - time ripple
            CustomParticles.HaloRing(target.Center, FateCosmicVFX.FateDarkPink, 0.5f, 18);
            CustomParticles.HaloRing(target.Center, FateCosmicVFX.FateWhite * 0.6f, 0.35f, 15);
            
            // Lightning crackle effect - temporal energy
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 endPos = target.Center + angle.ToRotationVector2() * 50f;
                FateCosmicVFX.DrawCosmicLightning(target.Center, endPos, 5, 15f, 1, 0.4f);
            }
            
            // Apply echo damage (75% of original)
            if (Main.myPlayer == Player.whoAmI)
            {
                int echoDamage = (int)(originalDamage * 0.75f);
                Player.ApplyDamageToNPC(target, echoDamage, 0f, 0, wasCrit);
            }
            
            // Sound effect
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 0.6f }, target.Center);
        }
    }
}
