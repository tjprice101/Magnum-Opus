using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Effects;
using MagnumOpus.Common.Systems.VFX.Screen;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.Nachtmusik;

namespace MagnumOpus.Content.Nachtmusik.Weapons.ConstellationPiercer.Systems
{
    /// <summary>
    /// Tracks Star Points on enemies hit by Constellation Piercer bolts.
    /// When 3+ Star Points exist, chain resonance arcs flow between them.
    /// At 5+ Star Points, Stellar Conduit activates for 3 seconds of sustained AoE.
    /// Hooks into On_Main.DrawProjectiles to render constellation overlay after projectiles.
    /// </summary>
    public class StarPointSystem : ModSystem
    {
        private static readonly List<StarPoint> _activePoints = new();
        private static float _conduitTimer = 0f;
        private static bool _conduitActive = false;

        // Pooled connection list to avoid per-frame allocation
        private static readonly List<(StarPoint, StarPoint)> _connectionCache = new(32);

        private const int MaxStarPoints = 8;
        private const float StarPointDuration = 240f; // 4 seconds
        private const float ChainResonanceTickRate = 15f; // damage tick every 15 frames
        private const float ChainResonanceRange = 500f; // max distance between connected points
        private const int ConduitThreshold = 5;
        private const float ConduitDuration = 180f; // 3 seconds
        private const int ConduitDamage = 200;
        private const float ChainResonanceDamage = 80f;

        public override void Load()
        {
            On_Main.DrawProjectiles += DrawConstellationOverlay;
        }

        public override void Unload()
        {
            On_Main.DrawProjectiles -= DrawConstellationOverlay;
        }

        /// <summary>
        /// On_Main.DrawProjectiles hook -- renders star point brands, constellation lines,
        /// and conduit rivers AFTER all projectiles have drawn.
        /// </summary>
        private void DrawConstellationOverlay(On_Main.orig_DrawProjectiles orig, Main self)
        {
            orig(self);

            if (Main.dedServ || _activePoints.Count == 0) return;

            SpriteBatch sb = Main.spriteBatch;
            try
            {
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                DrawStarPoints(sb);

                // Also render VerletConstellationSystem lines created by chain resonance
                VerletConstellationSystem.RenderAll(sb);

                sb.End();
            }
            catch
            {
                try { sb.End(); } catch { }
            }
        }

        public struct StarPoint
        {
            public int NpcIndex;
            public Vector2 Position;
            public float TimeRemaining;
            public float MaxTime;
            public int OwnerPlayer;

            public float Progress => 1f - (TimeRemaining / MaxTime);
            public bool IsValid => TimeRemaining > 0 && NpcIndex >= 0 && NpcIndex < Main.maxNPCs
                && Main.npc[NpcIndex].active;
        }

        public override void PostUpdateNPCs()
        {
            // Update star point positions and lifetimes
            for (int i = _activePoints.Count - 1; i >= 0; i--)
            {
                var point = _activePoints[i];
                point.TimeRemaining--;

                if (!point.IsValid)
                {
                    _activePoints.RemoveAt(i);
                    continue;
                }

                // Track NPC position
                point.Position = Main.npc[point.NpcIndex].Center;
                _activePoints[i] = point;
            }

            // Chain resonance: damage enemies along constellation lines
            if (_activePoints.Count >= 3)
            {
                ProcessChainResonance();
            }

            // Stellar conduit
            if (_conduitActive)
            {
                _conduitTimer--;
                ProcessStellarConduit();

                if (_conduitTimer <= 0)
                {
                    _conduitActive = false;
                }
            }
        }

        private void ProcessChainResonance()
        {
            if (Main.GameUpdateCount % (int)ChainResonanceTickRate != 0) return;

            var connections = GetConnections();
            foreach (var (a, b) in connections)
            {
                // Deal damage to both connected NPCs
                if (Main.netMode != Terraria.ID.NetmodeID.MultiplayerClient)
                {
                    var npcA = Main.npc[a.NpcIndex];
                    var npcB = Main.npc[b.NpcIndex];
                    int owner = a.OwnerPlayer;

                    if (npcA.active && owner >= 0 && owner < Main.maxPlayers)
                    {
                        Main.player[owner].ApplyDamageToNPC(npcA, (int)ChainResonanceDamage, 0f, 0, false);
                    }
                    if (npcB.active && owner >= 0 && owner < Main.maxPlayers)
                    {
                        Main.player[owner].ApplyDamageToNPC(npcB, (int)ChainResonanceDamage, 0f, 0, false);
                    }
                }

                // VFX: chain lightning arc particles along the line
                SpawnChainResonanceVFX(a.Position, b.Position);
            }
        }

        private void ProcessStellarConduit()
        {
            if (_activePoints.Count < ConduitThreshold) return;
            if (Main.GameUpdateCount % 10 != 0) return;

            var connections = GetConnections();
            foreach (var (a, b) in connections)
            {
                // Stronger damage during conduit
                if (Main.netMode != Terraria.ID.NetmodeID.MultiplayerClient)
                {
                    var npcA = Main.npc[a.NpcIndex];
                    var npcB = Main.npc[b.NpcIndex];
                    int owner = a.OwnerPlayer;

                    if (npcA.active && owner >= 0 && owner < Main.maxPlayers)
                    {
                        Main.player[owner].ApplyDamageToNPC(npcA, ConduitDamage, 0f, 0, false);
                    }
                    if (npcB.active && owner >= 0 && owner < Main.maxPlayers)
                    {
                        Main.player[owner].ApplyDamageToNPC(npcB, ConduitDamage, 0f, 0, false);
                    }
                }

                // Intensified VFX during conduit
                SpawnConduitRiverVFX(a.Position, b.Position);
            }

            // Music note scatter during conduit
            if (Main.GameUpdateCount % 20 == 0)
            {
                foreach (var point in _activePoints)
                {
                    if (!point.IsValid) continue;
                    SpawnConduitMusicNotes(point.Position);
                }
            }

            // Subtle screen shake during conduit
            if (_conduitTimer > ConduitDuration - 10)
            {
                ScreenEffectSystem.AddScreenShake(2f);
            }
        }

        public static void AddStarPoint(int npcIndex, int ownerPlayer)
        {
            // Check if this NPC already has a star point -- refresh it
            for (int i = 0; i < _activePoints.Count; i++)
            {
                if (_activePoints[i].NpcIndex == npcIndex)
                {
                    var existing = _activePoints[i];
                    existing.TimeRemaining = StarPointDuration;
                    _activePoints[i] = existing;
                    return;
                }
            }

            // Remove oldest if at capacity
            if (_activePoints.Count >= MaxStarPoints)
            {
                _activePoints.RemoveAt(0);
            }

            _activePoints.Add(new StarPoint
            {
                NpcIndex = npcIndex,
                Position = Main.npc[npcIndex].Center,
                TimeRemaining = StarPointDuration,
                MaxTime = StarPointDuration,
                OwnerPlayer = ownerPlayer
            });

            // Check for stellar conduit activation
            if (_activePoints.Count >= ConduitThreshold && !_conduitActive)
            {
                _conduitActive = true;
                _conduitTimer = ConduitDuration;
                ScreenEffectSystem.AddScreenShake(4f);
            }
        }

        public static void RefreshStarPoint(int npcIndex)
        {
            for (int i = 0; i < _activePoints.Count; i++)
            {
                if (_activePoints[i].NpcIndex == npcIndex)
                {
                    var point = _activePoints[i];
                    point.TimeRemaining = StarPointDuration;
                    _activePoints[i] = point;
                    return;
                }
            }
        }

        public static List<StarPoint> GetActivePoints() => _activePoints;
        public static bool IsConduitActive => _conduitActive;
        public static float ConduitProgress => _conduitActive ? 1f - (_conduitTimer / ConduitDuration) : 0f;

        /// <summary>
        /// Returns a random active Star Point's NPC, or -1 if none exist.
        /// Used by Starfall mechanic.
        /// </summary>
        public static int GetRandomStarPointNPC()
        {
            // Count valid points first to avoid allocation
            int validCount = 0;
            for (int i = 0; i < _activePoints.Count; i++)
            {
                if (_activePoints[i].IsValid) validCount++;
            }
            if (validCount == 0) return -1;

            // Pick a random valid one
            int target = Main.rand.Next(validCount);
            int seen = 0;
            for (int i = 0; i < _activePoints.Count; i++)
            {
                if (_activePoints[i].IsValid)
                {
                    if (seen == target) return _activePoints[i].NpcIndex;
                    seen++;
                }
            }
            return -1;
        }

        private static List<(StarPoint, StarPoint)> GetConnections()
        {
            _connectionCache.Clear();
            for (int i = 0; i < _activePoints.Count; i++)
            {
                if (!_activePoints[i].IsValid) continue;
                for (int j = i + 1; j < _activePoints.Count; j++)
                {
                    if (!_activePoints[j].IsValid) continue;
                    float dist = Vector2.Distance(_activePoints[i].Position, _activePoints[j].Position);
                    if (dist <= ChainResonanceRange)
                    {
                        _connectionCache.Add((_activePoints[i], _activePoints[j]));
                    }
                }
            }
            return _connectionCache;
        }

        #region VFX Methods

        public static void DrawStarPoints(SpriteBatch spriteBatch)
        {
            if (_activePoints.Count == 0) return;

            float time = Main.GlobalTimeWrappedHourly;

            // Draw constellation lines (dotted star trail)
            if (_activePoints.Count >= 3)
            {
                DrawConstellationLines(spriteBatch, time);
            }

            // Draw individual star point brands on enemies
            foreach (var point in _activePoints)
            {
                if (!point.IsValid) continue;
                DrawStarPointBrand(spriteBatch, point, time);
            }
        }

        private static void DrawConstellationLines(SpriteBatch spriteBatch, float time)
        {
            for (int i = 0; i < _activePoints.Count; i++)
            {
                if (!_activePoints[i].IsValid) continue;
                for (int j = i + 1; j < _activePoints.Count; j++)
                {
                    if (!_activePoints[j].IsValid) continue;

                    float dist = Vector2.Distance(_activePoints[i].Position, _activePoints[j].Position);
                    if (dist > ChainResonanceRange) continue;

                    Vector2 from = _activePoints[i].Position - Main.screenPosition;
                    Vector2 to = _activePoints[j].Position - Main.screenPosition;
                    Vector2 direction = (to - from);
                    float length = direction.Length();
                    if (length < 1f) continue;
                    direction /= length;

                    // Dotted star trail: small star particles at intervals
                    float dotSpacing = 18f;
                    int dotCount = (int)(length / dotSpacing);
                    float intensity = _conduitActive ? 0.8f : 0.45f;

                    var starTex = MagnumTextureRegistry.GetBloom();

                    for (int d = 0; d <= dotCount; d++)
                    {
                        float t = (float)d / Math.Max(dotCount, 1);
                        Vector2 dotPos = Vector2.Lerp(from, to, t);

                        // Subtle wave offset perpendicular to line
                        Vector2 perp = new(-direction.Y, direction.X);
                        float wave = MathF.Sin(t * MathHelper.TwoPi * 2f + time * 3f) * 3f;
                        dotPos += perp * wave;

                        // Pulsing brightness that travels along the line
                        float pulse = MathF.Sin(t * MathHelper.Pi + time * 4f) * 0.3f + 0.7f;
                        float dotScale = 0.015f * pulse * intensity;

                        // Alternate between constellation blue and star white
                        Color dotColor = (d % 3 == 0)
                            ? NachtmusikPalette.Additive(NachtmusikPalette.StarGold, 0.5f * intensity * pulse)
                            : NachtmusikPalette.Additive(NachtmusikPalette.ConstellationBlue, 0.4f * intensity * pulse);

                        spriteBatch.Draw(starTex, dotPos, null, dotColor,
                            time + t * MathHelper.Pi, new Vector2(starTex.Width, starTex.Height) * 0.5f,
                            dotScale, SpriteEffects.None, 0f);
                    }

                    // During conduit: flowing particle river overlay
                    if (_conduitActive)
                    {
                        DrawConduitRiverLine(spriteBatch, from, to, direction, length, time);
                    }
                }
            }
        }

        private static void DrawConduitRiverLine(SpriteBatch spriteBatch, Vector2 from, Vector2 to, Vector2 dir, float length, float time)
        {
            var bloomTex = MagnumTextureRegistry.GetBloom();
            Vector2 perp = new(-dir.Y, dir.X);

            // Flowing particles along the line
            int particleCount = (int)(length / 8f);
            for (int p = 0; p < particleCount; p++)
            {
                // Staggered flow: each particle slides along the line
                float baseT = (float)p / particleCount;
                float flowT = (baseT + time * 1.5f) % 1f;
                Vector2 flowPos = Vector2.Lerp(from, to, flowT);

                // Perpendicular oscillation for river width
                float riverWave = MathF.Sin(flowT * MathHelper.TwoPi * 3f + time * 5f + p * 0.7f) * 6f;
                flowPos += perp * riverWave;

                float fade = MathF.Sin(flowT * MathHelper.Pi); // fade at endpoints
                float scale = 0.02f * fade;

                Color col = NachtmusikPalette.Additive(
                    Color.Lerp(NachtmusikPalette.StarlitBlue, NachtmusikPalette.StarWhite, flowT),
                    0.35f * fade);

                spriteBatch.Draw(bloomTex, flowPos, null, col,
                    0f, new Vector2(bloomTex.Width, bloomTex.Height) * 0.5f,
                    scale, SpriteEffects.None, 0f);
            }
        }

        private static void DrawStarPointBrand(SpriteBatch spriteBatch, StarPoint point, float time)
        {
            Vector2 pos = point.Position - Main.screenPosition;
            var bloomTex = MagnumTextureRegistry.GetBloom();
            Vector2 origin = new(bloomTex.Width * 0.5f, bloomTex.Height * 0.5f);

            float fade = Math.Min(point.TimeRemaining / 30f, 1f); // fade out in last 0.5s
            float pulse = 0.8f + 0.2f * MathF.Sin(time * 4f + point.NpcIndex * 2f);
            float brandIntensity = _conduitActive ? 1.2f : 0.7f;

            // Outer constellation blue halo
            spriteBatch.Draw(bloomTex, pos, null,
                NachtmusikPalette.Additive(NachtmusikPalette.ConstellationBlue, 0.25f * fade * pulse * brandIntensity),
                0f, origin, 0.06f * pulse, SpriteEffects.None, 0f);

            // Inner star white core
            spriteBatch.Draw(bloomTex, pos, null,
                NachtmusikPalette.Additive(NachtmusikPalette.StarWhite, 0.4f * fade * pulse * brandIntensity),
                0f, origin, 0.025f * pulse, SpriteEffects.None, 0f);

            // Gold accent sparkle (rotating cross)
            float crossRot = time * 2f + point.NpcIndex;
            float crossPulse = MathF.Pow(MathF.Sin(time * 6f + point.NpcIndex * 1.3f) * 0.5f + 0.5f, 3f);
            spriteBatch.Draw(bloomTex, pos, null,
                NachtmusikPalette.Additive(NachtmusikPalette.StarGold, 0.3f * fade * crossPulse * brandIntensity),
                crossRot, origin, new Vector2(0.04f, 0.01f) * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(bloomTex, pos, null,
                NachtmusikPalette.Additive(NachtmusikPalette.StarGold, 0.3f * fade * crossPulse * brandIntensity),
                crossRot + MathHelper.PiOver2, origin, new Vector2(0.04f, 0.01f) * pulse, SpriteEffects.None, 0f);

            // Ambient light
            Lighting.AddLight(point.Position, NachtmusikPalette.ConstellationBlue.ToVector3() * 0.3f * fade * brandIntensity);
        }

        private static void SpawnChainResonanceVFX(Vector2 from, Vector2 to)
        {
            Vector2 mid = (from + to) * 0.5f;
            Vector2 dir = (to - from).SafeNormalize(Vector2.UnitX);
            float dist = Vector2.Distance(from, to);

            // 3-5 sparkle particles along the line
            int sparkCount = Math.Clamp((int)(dist / 80f), 3, 6);
            for (int i = 0; i < sparkCount; i++)
            {
                float t = Main.rand.NextFloat();
                Vector2 pos = Vector2.Lerp(from, to, t);
                Vector2 perp = new(-dir.Y, dir.X);
                pos += perp * Main.rand.NextFloat(-8f, 8f);

                Color col = Color.Lerp(NachtmusikPalette.ConstellationBlue, NachtmusikPalette.StarWhite,
                    Main.rand.NextFloat());
                Vector2 vel = perp * Main.rand.NextFloat(-1f, 1f);

                GlowDustSystem.SpawnGlowPixel(pos, vel, col, 0.6f, 20);
            }

            // Create constellation line visual
            VerletConstellationSystem.CreateLine(from, to, 8,
                NachtmusikPalette.ConstellationBlue, 0.7f, 20);
        }

        private static void SpawnConduitRiverVFX(Vector2 from, Vector2 to)
        {
            Vector2 dir = (to - from).SafeNormalize(Vector2.UnitX);
            Vector2 perp = new(-dir.Y, dir.X);
            float dist = Vector2.Distance(from, to);

            // Dense particle river
            int particleCount = Math.Clamp((int)(dist / 40f), 5, 12);
            for (int i = 0; i < particleCount; i++)
            {
                float t = Main.rand.NextFloat();
                Vector2 pos = Vector2.Lerp(from, to, t);
                pos += perp * Main.rand.NextFloat(-12f, 12f);

                Vector2 vel = dir * Main.rand.NextFloat(1f, 3f) + perp * Main.rand.NextFloat(-1f, 1f);
                Color col = Color.Lerp(NachtmusikPalette.StarlitBlue, NachtmusikPalette.TwinklingWhite,
                    Main.rand.NextFloat());

                GlowDustSystem.SpawnGlowPixelRise(pos, vel, col, 0.8f, 25);
            }
        }

        private static void SpawnConduitMusicNotes(Vector2 position)
        {
            // Spawn real music note particles with wobble and bloom
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-2.5f, -1f));
                Color col = Color.Lerp(NachtmusikPalette.StarGold, NachtmusikPalette.StarWhite,
                    Main.rand.NextFloat());

                MagnumParticleHandler.SpawnParticle(new MusicNoteParticle(
                    position, vel, col, Main.rand.NextFloat(0.3f, 0.5f), 40));
            }

            // Gold accent flare at point
            CustomParticles.GenericFlare(position, NachtmusikPalette.StarGold, 0.25f, 30);
        }

        #endregion

        public override void OnWorldUnload()
        {
            _activePoints.Clear();
            _conduitActive = false;
            _conduitTimer = 0;
        }
    }
}
