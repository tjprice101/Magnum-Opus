using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.EnigmaVariations.Debuffs
{
    public class ParadoxBrand : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.Confused;
        
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }
    
    public class ParadoxBrandNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        
        public int paradoxStacks = 0;
        public int paradoxTimer = 0;
        public int surgeTimer = 0;
        private const int MaxStacks = 10;
        private const int StackDuration = 300; // 5 seconds per stack refresh
        
        // Enigma theme colors
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public void AddParadoxStack(NPC npc, int stacks = 1)
        {
            paradoxStacks = Math.Min(paradoxStacks + stacks, MaxStacks);
            paradoxTimer = StackDuration;
            npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), StackDuration);
            
            // Visual feedback for stack
            for (int i = 0; i < 4 + stacks; i++)
            {
                float angle = MathHelper.TwoPi * i / (4 + stacks);
                Vector2 offset = angle.ToRotationVector2() * 20f;
                float progress = (float)i / (4 + stacks);
                Color sparkColor = Color.Lerp(EnigmaPurple, EnigmaGreen, progress);
                CustomParticles.GenericFlare(npc.Center + offset, sparkColor, 0.3f + paradoxStacks * 0.05f, 15);
            }
        }
        
        public override void ResetEffects(NPC npc)
        {
            if (!npc.HasBuff(ModContent.BuffType<ParadoxBrand>()))
            {
                paradoxStacks = 0;
                paradoxTimer = 0;
            }
        }
        
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (npc.HasBuff(ModContent.BuffType<ParadoxBrand>()) && paradoxStacks > 0)
            {
                // DOT scales with stacks - 15 damage per stack per second
                int dot = paradoxStacks * 15;
                npc.lifeRegen -= dot * 2;
                damage = Math.Max(damage, paradoxStacks * 3);
            }
        }
        
        public override void AI(NPC npc)
        {
            if (!npc.HasBuff(ModContent.BuffType<ParadoxBrand>()) || paradoxStacks <= 0) return;
            
            surgeTimer++;
            
            // Enigma Surges - random bursts of swirling energy
            if (surgeTimer >= 90 - paradoxStacks * 5 && Main.rand.NextBool(3))
            {
                surgeTimer = 0;
                TriggerEnigmaSurge(npc);
            }
            
            // Ambient swirling particles
            if (Main.rand.NextBool(8 - paradoxStacks / 2))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 pos = npc.Center + angle.ToRotationVector2() * Main.rand.NextFloat(20f, 40f);
                Vector2 vel = (npc.Center - pos).SafeNormalize(Vector2.Zero) * 2f;
                Color particleColor = Main.rand.NextBool() ? EnigmaPurple : EnigmaGreen;
                
                var glow = new GenericGlowParticle(pos, vel, particleColor * 0.7f, 
                    Main.rand.NextFloat(0.2f, 0.4f), Main.rand.Next(20, 35), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Max stacks explosion
            if (paradoxStacks >= MaxStacks)
            {
                TriggerParadoxExplosion(npc);
                paradoxStacks = 0;
                npc.DelBuff(npc.FindBuffIndex(ModContent.BuffType<ParadoxBrand>()));
            }
        }
        
        private void TriggerEnigmaSurge(NPC npc)
        {
            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.3f, Volume = 0.5f }, npc.Center);
            
            // Swirling glitter burst
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                float progress = (float)i / 12f;
                Color surgeColor = Color.Lerp(EnigmaPurple, EnigmaGreen, progress);
                
                var glow = new GenericGlowParticle(npc.Center, vel, surgeColor, 
                    Main.rand.NextFloat(0.3f, 0.5f), Main.rand.Next(25, 40), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Fractal flare pattern
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 offset = angle.ToRotationVector2() * 25f;
                float progress = (float)i / 6f;
                Color fractalColor = Color.Lerp(EnigmaGreen, EnigmaPurple, progress);
                CustomParticles.GenericFlare(npc.Center + offset, fractalColor, 0.35f, 15);
            }
            
            CustomParticles.HaloRing(npc.Center, EnigmaPurple * 0.7f, 0.4f, 18);
            
            // Random short teleport (confusion effect)
            if (Main.rand.NextBool(3) && !npc.boss)
            {
                Vector2 teleportOffset = Main.rand.NextVector2Circular(50f, 50f);
                npc.position += teleportOffset;
                npc.netUpdate = true;
                
                // Teleport VFX
                CustomParticles.GenericFlare(npc.Center, EnigmaGreen, 0.6f, 20);
            }
        }
        
        private void TriggerParadoxExplosion(NPC npc)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.3f, Volume = 1f }, npc.Center);
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.8f }, npc.Center);
            
            // Stun
            npc.velocity *= 0.1f;
            
            // Massive VFX explosion
            // Single expanding shockwave
            CustomParticles.HaloRing(npc.Center, EnigmaPurple, 0.8f, 25);
            
            // Radial explosion
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 15f);
                float progress = (float)i / 30f;
                Color burstColor = Color.Lerp(EnigmaGreen, EnigmaPurple, progress);
                
                var glow = new GenericGlowParticle(npc.Center, vel, burstColor, 
                    Main.rand.NextFloat(0.5f, 0.8f), Main.rand.Next(35, 55), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Fractal burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 offset = angle.ToRotationVector2() * 50f;
                float progress = (float)i / 8f;
                Color fractalColor = Color.Lerp(EnigmaPurple, EnigmaGreen, progress);
                CustomParticles.GenericFlare(npc.Center + offset, fractalColor, 0.6f, 22);
            }
            
            // Heavy smoke
            for (int i = 0; i < 15; i++)
            {
                var smoke = new HeavySmokeParticle(npc.Center + Main.rand.NextVector2Circular(30f, 30f),
                    Main.rand.NextVector2Circular(4f, 4f) + new Vector2(0, -2f),
                    EnigmaBlack, Main.rand.Next(50, 80), Main.rand.NextFloat(0.6f, 1f),
                    0.6f, 0.015f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // === WATCHING EYES - The paradox observes its destruction ===
            CustomParticles.EnigmaEyeExplosion(npc.Center, EnigmaPurple, 6, 4f);
            CustomParticles.EnigmaEyeFormation(npc.Center, EnigmaGreen, 4, 60f);
            
            // Spread to nearby enemies
            float spreadRadius = 200f;
            foreach (NPC other in Main.npc)
            {
                if (!other.active || other.friendly || other.whoAmI == npc.whoAmI) continue;
                if (Vector2.Distance(npc.Center, other.Center) <= spreadRadius)
                {
                    other.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(other, 3);
                    
                    // Line effect to spread target
                    MagnumVFX.DrawEnigmaLightning(npc.Center, other.Center, 6, 20f, 2, 0.4f);
                }
            }
            
            // Damage burst
            foreach (NPC target in Main.npc)
            {
                if (!target.active || target.friendly) continue;
                if (Vector2.Distance(npc.Center, target.Center) <= spreadRadius)
                {
                    target.SimpleStrikeNPC(150, 0, false, 0f, null, false, 0f, true);
                }
            }
        }
        
        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (npc.HasBuff(ModContent.BuffType<ParadoxBrand>()) && paradoxStacks > 0)
            {
                // Tint with enigma colors
                float intensity = paradoxStacks / (float)MaxStacks;
                drawColor = Color.Lerp(drawColor, EnigmaPurple, intensity * 0.3f);
            }
        }
    }
}
