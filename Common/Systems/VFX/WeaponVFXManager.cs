using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// WEAPON VFX MANAGER
    /// 
    /// Manages update/render cycles for:
    /// - WeaponFogVFX (unique fog per theme on attacks)
    /// - LightBeamImpactVFX (stretching light rays on tile/enemy hits)
    /// - UniqueProjectileRenderVFX (interpolated projectile rendering per theme)
    /// </summary>
    public class WeaponVFXManager : ModSystem
    {
        public override void PostUpdateEverything()
        {
            // Update fog systems
            WeaponFogVFX.Update();
            LayeredNebulaFog.Update(); // New layered nebula fog system
            
            // Update light beam system
            LightBeamImpactVFX.Update();
        }
        
        public override void PostDrawTiles()
        {
            // Render fog behind players
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            try
            {
                // Render new layered nebula fog first (it handles its own blend states)
                LayeredNebulaFog.Render(Main.spriteBatch);
                
                // Then legacy fog and light beams
                WeaponFogVFX.Render(Main.spriteBatch);
                LightBeamImpactVFX.Render(Main.spriteBatch);
            }
            catch (Exception ex)
            {
                // Fail silently for VFX rendering
                Mod.Logger.Warn($"WeaponVFXManager render error: {ex.Message}");
            }
            
            Main.spriteBatch.End();
        }
    }
    
    /// <summary>
    /// WEAPON ATTACK VFX HOOKS
    /// 
    /// GlobalItem that hooks into weapon attacks to spawn unique fog and light beam effects.
    /// </summary>
    public class WeaponAttackVFXHooks : GlobalItem
    {
        public override bool InstancePerEntity => false;
        
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.ModItem?.Mod == ModContent.GetInstance<MagnumOpus>();
        }
        
        /// <summary>
        /// Spawns theme-appropriate fog on weapon use.
        /// </summary>
        public override void UseAnimation(Item item, Player player)
        {
            if (item.ModItem?.Mod != ModContent.GetInstance<MagnumOpus>())
                return;
            
            string theme = DetectTheme(item);
            
            // Spawn fog on attack initiation
            if (player.itemAnimation == player.itemAnimationMax - 1)
            {
                Vector2 attackPos = player.itemLocation + new Vector2(item.width * 0.5f * player.direction, 0);
                Vector2 attackDirection = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX * player.direction);
                
                // Different fog patterns based on weapon type
                if (item.CountsAsClass(DamageClass.Melee))
                {
                    // Melee: Fog follows swing direction
                    WeaponFogVFX.SpawnAttackFog(attackPos, theme, 0.8f, attackDirection * 2f);
                }
                else if (item.CountsAsClass(DamageClass.Ranged))
                {
                    // Ranged: Muzzle smoke
                    WeaponFogVFX.SpawnAttackFog(attackPos, theme, 0.6f, -attackDirection * 1.5f);
                }
                else if (item.CountsAsClass(DamageClass.Magic))
                {
                    // Magic: Mystical swirl at cast point
                    WeaponFogVFX.SpawnAttackFog(player.Center, theme, 1f, Vector2.Zero);
                }
                else if (item.CountsAsClass(DamageClass.Summon))
                {
                    // Summon: Ethereal mist at player
                    WeaponFogVFX.SpawnAttackFog(player.Center, theme, 0.7f, new Vector2(0, -1f));
                }
            }
            
            // Continuous fog for melee swing
            if (item.CountsAsClass(DamageClass.Melee) && player.itemAnimation > 1)
            {
                float swingProgress = 1f - (float)player.itemAnimation / player.itemAnimationMax;
                WeaponFogVFX.SpawnSwingFog(player, swingProgress, theme, 0.6f);
            }
        }
        
        private string DetectTheme(Item item)
        {
            if (item.ModItem == null) return "generic";
            
            string ns = item.ModItem.GetType().Namespace ?? "";
            string name = item.ModItem.GetType().Name.ToLower();
            
            // Namespace detection
            if (ns.Contains("Eroica")) return "Eroica";
            if (ns.Contains("SwanLake")) return "SwanLake";
            if (ns.Contains("LaCampanella")) return "LaCampanella";
            if (ns.Contains("MoonlightSonata") || ns.Contains("Moonlight")) return "MoonlightSonata";
            if (ns.Contains("EnigmaVariations") || ns.Contains("Enigma")) return "EnigmaVariations";
            if (ns.Contains("Fate")) return "Fate";
            if (ns.Contains("DiesIrae")) return "DiesIrae";
            if (ns.Contains("ClairDeLune")) return "ClairDeLune";
            if (ns.Contains("Nachtmusik")) return "Nachtmusik";
            if (ns.Contains("OdeToJoy")) return "OdeToJoy";
            if (ns.Contains("Spring")) return "Spring";
            if (ns.Contains("Summer")) return "Summer";
            if (ns.Contains("Autumn")) return "Autumn";
            if (ns.Contains("Winter")) return "Winter";
            
            // Name fallback
            if (name.Contains("sakura") || name.Contains("valor")) return "Eroica";
            if (name.Contains("swan") || name.Contains("iridescent")) return "SwanLake";
            if (name.Contains("bell") || name.Contains("campanella")) return "LaCampanella";
            if (name.Contains("moon") || name.Contains("lunar")) return "MoonlightSonata";
            if (name.Contains("enigma") || name.Contains("void")) return "EnigmaVariations";
            if (name.Contains("fate") || name.Contains("cosmic")) return "Fate";
            
            return "generic";
        }
    }
    
    /// <summary>
    /// PROJECTILE IMPACT VFX HOOKS
    /// 
    /// GlobalProjectile that spawns light beam impacts on tile/enemy hits.
    /// </summary>
    public class ProjectileImpactVFXHooks : GlobalProjectile
    {
        public override bool InstancePerEntity => false;
        
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return entity.ModProjectile?.Mod == ModContent.GetInstance<MagnumOpus>();
        }
        
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.ModProjectile?.Mod != ModContent.GetInstance<MagnumOpus>())
                return;
            
            string theme = DetectTheme(projectile);
            
            // Calculate hit position and direction
            Vector2 hitPos = target.Center;
            Vector2 hitDir = (projectile.Center - target.Center).SafeNormalize(Vector2.UnitY);
            
            // Scale based on damage and crit
            float scale = 1f;
            if (hit.Crit) scale *= 1.5f;
            scale *= MathHelper.Clamp(damageDone / 80f, 0.5f, 2f);
            
            // Spawn light beam impact
            LightBeamImpactVFX.SpawnEnemyImpact(hitPos, hitDir, theme, scale);
            
            // Spawn fog burst on heavy hits
            if (damageDone > 50)
            {
                WeaponFogVFX.SpawnAttackFog(hitPos, theme, scale * 0.5f, hitDir * 2f);
            }
        }
        
        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            if (projectile.ModProjectile?.Mod != ModContent.GetInstance<MagnumOpus>())
                return true;
            
            string theme = DetectTheme(projectile);
            
            // Spawn tile impact light beams
            LightBeamImpactVFX.SpawnTileImpact(projectile.Center, oldVelocity, theme, 0.7f);
            
            // Small fog puff on tile hit
            WeaponFogVFX.SpawnAttackFog(projectile.Center, theme, 0.4f, -oldVelocity.SafeNormalize(Vector2.Zero) * 1.5f);
            
            return true;
        }
        
        public override void OnKill(Projectile projectile, int timeLeft)
        {
            if (projectile.ModProjectile?.Mod != ModContent.GetInstance<MagnumOpus>())
                return;
            
            string theme = DetectTheme(projectile);
            
            // Death light beam burst
            LightBeamImpactVFX.SpawnImpact(projectile.Center, theme, 1.2f, projectile.velocity.SafeNormalize(Vector2.UnitY));
            
            // Death fog
            WeaponFogVFX.SpawnAttackFog(projectile.Center, theme, 0.6f, Vector2.Zero);
        }
        
        private string DetectTheme(Projectile projectile)
        {
            if (projectile.ModProjectile == null) return "generic";
            
            string ns = projectile.ModProjectile.GetType().Namespace ?? "";
            string name = projectile.ModProjectile.GetType().Name.ToLower();
            
            // Namespace detection
            if (ns.Contains("Eroica")) return "Eroica";
            if (ns.Contains("SwanLake")) return "SwanLake";
            if (ns.Contains("LaCampanella")) return "LaCampanella";
            if (ns.Contains("MoonlightSonata") || ns.Contains("Moonlight")) return "MoonlightSonata";
            if (ns.Contains("EnigmaVariations") || ns.Contains("Enigma")) return "EnigmaVariations";
            if (ns.Contains("Fate")) return "Fate";
            if (ns.Contains("DiesIrae")) return "DiesIrae";
            if (ns.Contains("ClairDeLune")) return "ClairDeLune";
            if (ns.Contains("Nachtmusik")) return "Nachtmusik";
            if (ns.Contains("OdeToJoy")) return "OdeToJoy";
            if (ns.Contains("Spring")) return "Spring";
            if (ns.Contains("Summer")) return "Summer";
            if (ns.Contains("Autumn")) return "Autumn";
            if (ns.Contains("Winter")) return "Winter";
            
            return "generic";
        }
    }
}
