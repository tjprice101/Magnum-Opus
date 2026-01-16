using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.EnigmaVariations.Debuffs;

namespace MagnumOpus.Content.EnigmaVariations.Bosses
{
    /// <summary>
    /// Fast web projectile shot by Enigma boss
    /// </summary>
    public class EnigmaWebShot : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
            Projectile.penetrate = 1;
        }
        
        public override void AI()
        {
            // Trail particles
            if (Main.rand.NextBool(2))
            {
                Color trailColor = Main.rand.NextBool() ? EnigmaPurple : EnigmaGreen;
                CustomParticles.GenericGlow(Projectile.Center, trailColor * 0.6f, 0.3f, 15);
            }
            
            Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * 0.4f);
        }
        
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 300);
        }
        
        public override void OnKill(int timeLeft)
        {
            UnifiedVFX.EnigmaVariations.Impact(Projectile.Center, 0.5f);
            SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.3f, Volume = 0.5f }, Projectile.Center);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            
            // Core
            Main.spriteBatch.Draw(glow, pos, null, EnigmaGreen, 0f, glow.Size() / 2, 0.5f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(glow, pos, null, EnigmaPurple * 0.6f, 0f, glow.Size() / 2, 0.7f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(glow, pos, null, Color.White * 0.4f, 0f, glow.Size() / 2, 0.3f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    /// <summary>
    /// Glyph projectile that rises from ground eruption
    /// </summary>
    public class EnigmaGlyphProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 120;
            Projectile.penetrate = -1;
        }
        
        public override void AI()
        {
            Projectile.velocity.Y += 0.15f; // Slight gravity
            Projectile.rotation += 0.1f;
            
            // Glyph particles
            if (Main.rand.NextBool(3))
            {
                CustomParticles.Glyph(Projectile.Center + Main.rand.NextVector2Circular(10, 10), EnigmaPurple * 0.7f, 0.3f);
            }
            
            Projectile.alpha = (int)(255 * (1f - Projectile.timeLeft / 120f));
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.3f);
        }
        
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 240);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            float alpha = 1f - Projectile.alpha / 255f;
            
            Main.spriteBatch.Draw(glow, pos, null, EnigmaPurple * alpha, Projectile.rotation, glow.Size() / 2, 0.6f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(glow, pos, null, EnigmaGreen * alpha * 0.5f, Projectile.rotation * 0.5f, glow.Size() / 2, 0.8f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    /// <summary>
    /// Homing eye projectile
    /// </summary>
    public class EnigmaHomingEye : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnigmaEye1";
        
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private int TargetWhoAmI => (int)Projectile.ai[0];
        
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
            Projectile.penetrate = 1;
        }
        
        public override void AI()
        {
            // Homing
            Player target = Main.player[TargetWhoAmI];
            if (target.active && !target.dead)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                float homingStrength = 0.08f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 12f, homingStrength);
            }
            
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Trail
            if (Main.rand.NextBool(3))
            {
                CustomParticles.GenericGlow(Projectile.Center, EnigmaGreen * 0.5f, 0.2f, 15);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * 0.4f);
        }
        
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 300);
        }
        
        public override void OnKill(int timeLeft)
        {
            CustomParticles.EnigmaEyeGaze(Projectile.Center, EnigmaGreen, 0.4f, Projectile.rotation.ToRotationVector2());
            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = -0.2f, Volume = 0.5f }, Projectile.Center);
        }
    }
    
    /// <summary>
    /// Void web line that damages on contact
    /// </summary>
    public class EnigmaVoidWeb : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private float Angle => Projectile.ai[0];
        private float MaxRadius => Projectile.ai[1];
        
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 180;
            Projectile.penetrate = -1;
        }
        
        public override void AI()
        {
            // Draw web line particles
            if (Projectile.timeLeft % 5 == 0)
            {
                float currentRadius = MaxRadius * (1f - Projectile.timeLeft / 180f);
                for (float r = 0; r < currentRadius; r += 30f)
                {
                    Vector2 pos = Projectile.Center + Angle.ToRotationVector2() * r;
                    Color webColor = Color.Lerp(EnigmaPurple, EnigmaGreen, r / MaxRadius) * 0.6f;
                    CustomParticles.GenericGlow(pos, webColor, 0.2f, 8);
                }
            }
        }
        
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float currentRadius = MaxRadius * (1f - Projectile.timeLeft / 180f);
            
            // Line collision
            Vector2 start = Projectile.Center;
            Vector2 end = Projectile.Center + Angle.ToRotationVector2() * currentRadius;
            
            float dist = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 15f, ref dist);
        }
        
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Slow, 120);
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 180);
        }
        
        public override bool PreDraw(ref Color lightColor) => false; // Handled by particles
    }
    
    /// <summary>
    /// Ground shockwave projectile
    /// </summary>
    public class EnigmaShockwave : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private int DelayFrames => (int)Projectile.ai[0];
        
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 80;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 90;
            Projectile.penetrate = -1;
        }
        
        public override void AI()
        {
            // Delay before starting
            if (Projectile.timeLeft > 90 - DelayFrames)
            {
                Projectile.velocity = Vector2.Zero;
                return;
            }
            
            // Ground check - stay on ground
            Vector2 checkPos = Projectile.Bottom + new Vector2(0, 16);
            Point tilePos = checkPos.ToTileCoordinates();
            if (!WorldGen.SolidTile(tilePos.X, tilePos.Y))
            {
                Projectile.velocity.Y += 0.5f;
            }
            else
            {
                Projectile.velocity.Y = 0;
            }
            
            // Visual particles
            if (Main.rand.NextBool(2))
            {
                Vector2 dustPos = Projectile.Bottom + Main.rand.NextVector2Circular(30, 5);
                Color dustColor = Main.rand.NextBool() ? EnigmaPurple : EnigmaGreen;
                var smoke = new HeavySmokeParticle(dustPos, new Vector2(Main.rand.NextFloat(-1f, 1f), -2f), dustColor, Main.rand.Next(15, 25), 0.3f, 0.5f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.4f);
        }
        
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 240);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.timeLeft > 90 - DelayFrames) return false;
            
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            
            // Shockwave effect
            float progress = 1f - Projectile.timeLeft / (90f - DelayFrames);
            float alpha = 1f - progress;
            
            Main.spriteBatch.Draw(glow, pos, null, EnigmaPurple * alpha, 0f, glow.Size() / 2, new Vector2(1.5f, 2f), SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(glow, pos, null, EnigmaGreen * alpha * 0.6f, 0f, glow.Size() / 2, new Vector2(1f, 1.5f), SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    /// <summary>
    /// Mini spider minion spawned by boss
    /// </summary>
    public class EnigmaMiniSpider : ModNPC
    {
        public override string Texture => "Terraria/Images/NPC_" + NPCID.BlackRecluse; // Placeholder
        
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private int ParentBoss => (int)NPC.ai[0];
        
        public override void SetDefaults()
        {
            NPC.width = 40;
            NPC.height = 30;
            NPC.damage = 60;
            NPC.defense = 20;
            NPC.lifeMax = 1500;
            NPC.HitSound = SoundID.NPCHit8;
            NPC.DeathSound = SoundID.NPCDeath10;
            NPC.knockBackResist = 0.3f;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.aiStyle = 3; // Fighter AI
            AIType = NPCID.BlackRecluse;
        }
        
        public override void AI()
        {
            // Despawn if parent boss is dead
            if (ParentBoss >= 0 && ParentBoss < Main.maxNPCs)
            {
                NPC parent = Main.npc[ParentBoss];
                if (!parent.active)
                {
                    NPC.life = 0;
                    NPC.HitEffect(0, 100);
                    NPC.active = false;
                }
            }
            
            // Enigma particles
            if (Main.rand.NextBool(15))
            {
                CustomParticles.GenericGlow(NPC.Center, EnigmaPurple * 0.5f, 0.2f, 20);
            }
            
            Lighting.AddLight(NPC.Center, EnigmaGreen.ToVector3() * 0.2f);
        }
        
        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 180);
        }
        
        public override void OnKill()
        {
            UnifiedVFX.EnigmaVariations.Impact(NPC.Center, 0.4f);
        }
        
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // Tint purple
            return true;
        }
        
        public override Color? GetAlpha(Color drawColor)
        {
            return Color.Lerp(drawColor, EnigmaPurple, 0.4f);
        }
    }
}
