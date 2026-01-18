using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.EnigmaVariations.Debuffs
{
    /// <summary>
    /// Mystery Vapor Debuff - Applied by Riddlemaster's Cauldron minion vapors.
    /// Enemies with this debuff deal 15% reduced damage.
    /// Mysterious vapors swirl around the affected enemy.
    /// </summary>
    public class MysteryVaporDebuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.Weak;
        
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }
    
    public class MysteryVaporDebuffNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        
        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            if (npc.HasBuff(ModContent.BuffType<MysteryVaporDebuff>()))
            {
                // 15% reduced damage dealt
                modifiers.SourceDamage *= 0.85f;
            }
        }
        
        public override void ModifyHitNPC(NPC npc, NPC target, ref NPC.HitModifiers modifiers)
        {
            // For NPCs attacking other NPCs (like town NPCs)
            if (npc.HasBuff(ModContent.BuffType<MysteryVaporDebuff>()))
            {
                modifiers.SourceDamage *= 0.85f;
            }
        }
        
        public override void PostAI(NPC npc)
        {
            if (npc.HasBuff(ModContent.BuffType<MysteryVaporDebuff>()))
            {
                // Swirling vapor effect
                if (Main.rand.NextBool(4))
                {
                    float swirlAngle = Main.GameUpdateCount * 0.08f + Main.rand.NextFloat() * MathHelper.TwoPi;
                    float swirlRadius = npc.width * 0.5f + Main.rand.NextFloat(10f);
                    Vector2 vaporPos = npc.Center + swirlAngle.ToRotationVector2() * swirlRadius;
                    
                    // Gradient color for vapor
                    float progress = Main.rand.NextFloat();
                    Color vaporColor;
                    if (progress < 0.5f)
                        vaporColor = Color.Lerp(EnigmaDeepPurple, EnigmaPurple, progress * 2f);
                    else
                        vaporColor = Color.Lerp(EnigmaPurple, EnigmaGreenFlame, (progress - 0.5f) * 2f);
                    
                    // Upward drifting vapor
                    Vector2 vaporVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f - Main.rand.NextFloat(0.5f));
                    CustomParticles.GenericGlow(vaporPos, vaporColor * 0.5f, 0.2f + Main.rand.NextFloat(0.1f), 20);
                }
                
                // Occasional question-mark like glyph
                if (Main.rand.NextBool(30))
                {
                    CustomParticles.Glyph(npc.Center + Main.rand.NextVector2Circular(npc.width * 0.4f, npc.height * 0.4f), 
                        EnigmaPurple * 0.4f, 0.18f, -1);
                }
                
                // Dim mysterious light
                Lighting.AddLight(npc.Center, EnigmaPurple.ToVector3() * 0.2f);
            }
        }
    }
}
