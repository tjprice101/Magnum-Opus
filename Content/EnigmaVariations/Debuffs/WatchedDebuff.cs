using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.EnigmaVariations.Debuffs
{
    /// <summary>
    /// Watched Debuff - Applied by Ignition of Mystery's mystery burst.
    /// Enemies with this debuff take 15% increased damage from all sources.
    /// An ethereal eye watches them, showing they've been marked by mystery.
    /// </summary>
    public class WatchedDebuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.BrokenArmor;
        
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }
    
    public class WatchedDebuffNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        
        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (npc.HasBuff(ModContent.BuffType<WatchedDebuff>()))
            {
                // 15% increased damage taken
                modifiers.SourceDamage *= 1.15f;
            }
        }
        
        public override void PostAI(NPC npc)
        {
            if (npc.HasBuff(ModContent.BuffType<WatchedDebuff>()))
            {
                // Visual: An eye watches above the enemy
                if (Main.rand.NextBool(12))
                {
                    Vector2 eyePos = npc.Top + new Vector2(Main.rand.NextFloat(-15f, 15f), -25f + Main.rand.NextFloat(-5f, 5f));
                    CustomParticles.EnigmaEyeGaze(eyePos, EnigmaGreenFlame * 0.6f, 0.3f, (npc.Center - eyePos).SafeNormalize(Vector2.UnitY));
                }
                
                // Occasional glyph
                if (Main.rand.NextBool(25))
                {
                    CustomParticles.Glyph(npc.Center + Main.rand.NextVector2Circular(npc.width * 0.5f, npc.height * 0.5f), 
                        EnigmaPurple * 0.5f, 0.2f, -1);
                }
                
                // Purple/green aura motes
                if (Main.rand.NextBool(8))
                {
                    Vector2 motePos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.6f, npc.height * 0.6f);
                    Color moteColor = Main.rand.NextBool() ? EnigmaGreenFlame : EnigmaPurple;
                    CustomParticles.GenericGlow(motePos, moteColor * 0.4f, 0.15f, 15);
                }
                
                // Light
                Lighting.AddLight(npc.Center, EnigmaGreenFlame.ToVector3() * 0.3f);
            }
        }
    }
}
