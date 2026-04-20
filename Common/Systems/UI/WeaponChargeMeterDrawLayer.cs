using System;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Utilities;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Utilities;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Utilities;
using MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Utilities;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Utilities;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid.Utilities;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence.Utilities;
using MagnumOpus.Content.ClairDeLune.Weapons.Chronologicality.Utilities;
using MagnumOpus.Content.ClairDeLune.Weapons.ClockworkHarmony.Utilities;
using MagnumOpus.Content.ClairDeLune.Weapons.TemporalPiercer.Utilities;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Utilities;
using MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Utilities;
using MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.TheConductorsLastConstellation.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.RequiemOfReality.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.CodaOfAnnihilation.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.UI
{
    /// <summary>
    /// Shared world-space charge meter drawn beneath the player while holding a supported weapon.
    /// Weapons with custom progress bar textures get ornate frame overlays.
    /// Weapons without textures get a generic colored rectangle bar.
    /// </summary>
    public class WeaponChargeMeterDrawLayer : PlayerDrawLayer
    {
        private const int BarYOffset = 38;
        private const int DrawWidth = 80;
        private const int GenericBarHeight = 6;
        private const int GenericBarBorderWidth = 1;

        // Theme fill colors: (low, high, shimmerA, shimmerB)
        private static readonly Color MoonlightFillLow = new(138, 43, 226);
        private static readonly Color MoonlightFillHigh = new(135, 206, 250);
        private static readonly Color MoonlightShimmerA = new(135, 206, 250);
        private static readonly Color MoonlightShimmerB = new(230, 235, 255);

        private static readonly Color EroicaFillLow = new(200, 50, 50);
        private static readonly Color EroicaFillHigh = new(255, 220, 100);
        private static readonly Color EroicaShimmerA = new(255, 220, 100);
        private static readonly Color EroicaShimmerB = new(255, 255, 220);

        private static readonly Color SakuraFillLow = new(180, 60, 80);
        private static readonly Color SakuraFillHigh = new(255, 180, 200);
        private static readonly Color SakuraShimmerA = new(255, 180, 200);
        private static readonly Color SakuraShimmerB = new(255, 240, 245);

        private static readonly Color LaCampanellaFillLow = new(180, 80, 20);
        private static readonly Color LaCampanellaFillHigh = new(255, 180, 60);
        private static readonly Color LaCampanellaShimmerA = new(255, 180, 60);
        private static readonly Color LaCampanellaShimmerB = new(255, 240, 180);

        private static readonly Color SwanLakeFillLow = new(180, 180, 200);
        private static readonly Color SwanLakeFillHigh = new(240, 240, 255);
        private static readonly Color SwanLakeShimmerA = new(240, 240, 255);
        private static readonly Color SwanLakeShimmerB = new(255, 255, 255);

        private static readonly Color EnigmaFillLow = new(80, 30, 120);
        private static readonly Color EnigmaFillHigh = new(140, 60, 200);
        private static readonly Color EnigmaShimmerA = new(140, 60, 200);
        private static readonly Color EnigmaShimmerB = new(50, 220, 100);

        private static readonly Color DiesIraeFillLow = new(150, 30, 20);
        private static readonly Color DiesIraeFillHigh = new(255, 80, 40);
        private static readonly Color DiesIraeShimmerA = new(255, 80, 40);
        private static readonly Color DiesIraeShimmerB = new(255, 200, 80);

        private static readonly Color ClairFillLow = new(80, 120, 180);
        private static readonly Color ClairFillHigh = new(150, 200, 255);
        private static readonly Color ClairShimmerA = new(150, 200, 255);
        private static readonly Color ClairShimmerB = new(230, 240, 255);

        private static readonly Color NachtmusikFillLow = new(50, 60, 120);
        private static readonly Color NachtmusikFillHigh = new(100, 120, 200);
        private static readonly Color NachtmusikShimmerA = new(100, 120, 200);
        private static readonly Color NachtmusikShimmerB = new(200, 210, 255);

        private static readonly Color OdeToJoyFillLow = new(180, 140, 30);
        private static readonly Color OdeToJoyFillHigh = new(255, 200, 50);
        private static readonly Color OdeToJoyShimmerA = new(255, 200, 50);
        private static readonly Color OdeToJoyShimmerB = new(255, 240, 160);

        private static readonly Color FateFillLow = new(120, 20, 50);
        private static readonly Color FateFillHigh = new(180, 40, 80);
        private static readonly Color FateShimmerA = new(180, 40, 80);
        private static readonly Color FateShimmerB = new(255, 200, 220);

        // Texture assets for weapons with ornate bars (lazy loaded)
        private static Asset<Texture2D> _incisorBar;
        private static Asset<Texture2D> _eternalMoonBar;
        private static Asset<Texture2D> _celestialValorBar;
        private static Asset<Texture2D> _sakurasBlossomBar;

        public override Position GetDefaultPosition()
            => new AfterParent(PlayerDrawLayers.FrontAccFront);

        /// <summary>
        /// Tries to resolve charge data from the player. Returns true if a weapon with charge is held.
        /// </summary>
        private static bool TryGetChargeData(Player player, out float charge, out bool isFull,
            out Color fillLow, out Color fillHigh, out Color shimmerA, out Color shimmerB,
            out Texture2D frameTex)
        {
            charge = 0f;
            isFull = false;
            fillLow = fillHigh = shimmerA = shimmerB = Color.White;
            frameTex = null;

            // --- Weapons with ornate texture bars ---
            if (player.GetModPlayer<IncisorPlayer>().IsHoldingIncisor)
            {
                var p = player.GetModPlayer<IncisorPlayer>();
                charge = p.LunarCharge; isFull = p.IsChargeFull;
                fillLow = MoonlightFillLow; fillHigh = MoonlightFillHigh;
                shimmerA = MoonlightShimmerA; shimmerB = MoonlightShimmerB;
                _incisorBar ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/MoonlightSonata/IncisorOfMoonlight/IncisorProgressBar");
                frameTex = _incisorBar.Value;
                return true;
            }
            if (player.GetModPlayer<EternalMoonPlayer>().IsHoldingEternalMoon)
            {
                var p = player.GetModPlayer<EternalMoonPlayer>();
                charge = p.Charge; isFull = p.IsChargeFull;
                fillLow = MoonlightFillLow; fillHigh = MoonlightFillHigh;
                shimmerA = MoonlightShimmerA; shimmerB = MoonlightShimmerB;
                _eternalMoonBar ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/MoonlightSonata/EternalMoon/EternalMoonProgressBar");
                frameTex = _eternalMoonBar.Value;
                return true;
            }
            if (player.GetModPlayer<CelestialValorPlayer>().IsHoldingCelestialValor)
            {
                var p = player.GetModPlayer<CelestialValorPlayer>();
                charge = p.Charge; isFull = p.IsChargeFull;
                fillLow = EroicaFillLow; fillHigh = EroicaFillHigh;
                shimmerA = EroicaShimmerA; shimmerB = EroicaShimmerB;
                _celestialValorBar ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/Eroica/CelestialValor/CelestialValorProgressBar");
                frameTex = _celestialValorBar.Value;
                return true;
            }
            if (player.GetModPlayer<SakurasBlossomPlayer>().IsHoldingSakura)
            {
                var p = player.GetModPlayer<SakurasBlossomPlayer>();
                charge = p.Charge; isFull = p.IsChargeFull;
                fillLow = SakuraFillLow; fillHigh = SakuraFillHigh;
                shimmerA = SakuraShimmerA; shimmerB = SakuraShimmerB;
                _sakurasBlossomBar ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/Eroica/SakurasBlossom/SakurasBlossomProgressBar");
                frameTex = _sakurasBlossomBar.Value;
                return true;
            }

            // --- Generic rectangle bar weapons ---
            // LaCampanella
            if (player.GetModPlayer<DualFatedChimePlayer>().IsHoldingDualFatedChime)
            {
                var p = player.GetModPlayer<DualFatedChimePlayer>();
                charge = p.Charge; isFull = p.IsChargeFull;
                fillLow = LaCampanellaFillLow; fillHigh = LaCampanellaFillHigh;
                shimmerA = LaCampanellaShimmerA; shimmerB = LaCampanellaShimmerB;
                return true;
            }
            // SwanLake
            if (player.GetModPlayer<BlackSwanPlayer>().IsHoldingBlackSwan)
            {
                var p = player.GetModPlayer<BlackSwanPlayer>();
                charge = p.Charge; isFull = p.IsChargeFull;
                fillLow = SwanLakeFillLow; fillHigh = SwanLakeFillHigh;
                shimmerA = SwanLakeShimmerA; shimmerB = SwanLakeShimmerB;
                return true;
            }
            // Enigma
            if (player.GetModPlayer<VoidVariationPlayer>().IsHoldingVariationsOfTheVoid)
            {
                var p = player.GetModPlayer<VoidVariationPlayer>();
                charge = p.Charge; isFull = p.IsChargeFull;
                fillLow = EnigmaFillLow; fillHigh = EnigmaFillHigh;
                shimmerA = EnigmaShimmerA; shimmerB = EnigmaShimmerB;
                return true;
            }
            if (player.GetModPlayer<CadencePlayer>().IsHoldingUnresolvedCadence)
            {
                var p = player.GetModPlayer<CadencePlayer>();
                charge = p.Charge; isFull = p.IsChargeFull;
                fillLow = EnigmaFillLow; fillHigh = EnigmaFillHigh;
                shimmerA = EnigmaShimmerA; shimmerB = EnigmaShimmerB;
                return true;
            }
            // ClairDeLune
            if (player.GetModPlayer<ChronologicalityPlayer>().IsHoldingChronologicality)
            {
                var p = player.GetModPlayer<ChronologicalityPlayer>();
                charge = p.Charge; isFull = p.IsChargeFull;
                fillLow = ClairFillLow; fillHigh = ClairFillHigh;
                shimmerA = ClairShimmerA; shimmerB = ClairShimmerB;
                return true;
            }
            if (player.GetModPlayer<ClockworkHarmonyPlayer>().IsHoldingClockworkHarmony)
            {
                var p = player.GetModPlayer<ClockworkHarmonyPlayer>();
                charge = p.Charge; isFull = p.IsChargeFull;
                fillLow = ClairFillLow; fillHigh = ClairFillHigh;
                shimmerA = ClairShimmerA; shimmerB = ClairShimmerB;
                return true;
            }
            if (player.GetModPlayer<TemporalPiercerPlayer>().IsHoldingTemporalPiercer)
            {
                var p = player.GetModPlayer<TemporalPiercerPlayer>();
                charge = p.Charge; isFull = p.IsChargeFull;
                fillLow = ClairFillLow; fillHigh = ClairFillHigh;
                shimmerA = ClairShimmerA; shimmerB = ClairShimmerB;
                return true;
            }
            // OdeToJoy
            if (player.GetModPlayer<ThornboundReckoningPlayer>().IsHoldingThornboundReckoning)
            {
                var p = player.GetModPlayer<ThornboundReckoningPlayer>();
                charge = p.Charge; isFull = p.IsChargeFull;
                fillLow = OdeToJoyFillLow; fillHigh = OdeToJoyFillHigh;
                shimmerA = OdeToJoyShimmerA; shimmerB = OdeToJoyShimmerB;
                return true;
            }
            if (player.GetModPlayer<TheGardenersFuryPlayer>().IsHoldingTheGardenersFury)
            {
                var p = player.GetModPlayer<TheGardenersFuryPlayer>();
                charge = p.Charge; isFull = p.IsChargeFull;
                fillLow = OdeToJoyFillLow; fillHigh = OdeToJoyFillHigh;
                shimmerA = OdeToJoyShimmerA; shimmerB = OdeToJoyShimmerB;
                return true;
            }
            if (player.GetModPlayer<RoseThornChainsawPlayer>().IsHoldingRoseThornChainsaw)
            {
                var p = player.GetModPlayer<RoseThornChainsawPlayer>();
                charge = p.Charge; isFull = p.IsChargeFull;
                fillLow = OdeToJoyFillLow; fillHigh = OdeToJoyFillHigh;
                shimmerA = OdeToJoyShimmerA; shimmerB = OdeToJoyShimmerB;
                return true;
            }
            // Fate
            if (player.GetModPlayer<FractalPlayer>().IsHoldingFractalOfTheStars)
            {
                var p = player.GetModPlayer<FractalPlayer>();
                charge = p.Charge; isFull = p.IsChargeFull;
                fillLow = FateFillLow; fillHigh = FateFillHigh;
                shimmerA = FateShimmerA; shimmerB = FateShimmerB;
                return true;
            }
            if (player.GetModPlayer<ConstellationConductorPlayer>().IsHoldingConductorsConstellation)
            {
                var p = player.GetModPlayer<ConstellationConductorPlayer>();
                charge = p.Charge; isFull = p.IsChargeFull;
                fillLow = FateFillLow; fillHigh = FateFillHigh;
                shimmerA = FateShimmerA; shimmerB = FateShimmerB;
                return true;
            }
            if (player.GetModPlayer<RequiemPlayer>().IsHoldingRequiemOfReality)
            {
                var p = player.GetModPlayer<RequiemPlayer>();
                charge = p.Charge; isFull = p.IsChargeFull;
                fillLow = FateFillLow; fillHigh = FateFillHigh;
                shimmerA = FateShimmerA; shimmerB = FateShimmerB;
                return true;
            }
            if (player.GetModPlayer<OpusPlayer>().IsHoldingOpusUltima)
            {
                var p = player.GetModPlayer<OpusPlayer>();
                charge = p.Charge; isFull = p.IsChargeFull;
                fillLow = FateFillLow; fillHigh = FateFillHigh;
                shimmerA = FateShimmerA; shimmerB = FateShimmerB;
                return true;
            }
            if (player.GetModPlayer<CodaPlayer>().IsHoldingCodaOfAnnihilation)
            {
                var p = player.GetModPlayer<CodaPlayer>();
                charge = p.Charge; isFull = p.IsChargeFull;
                fillLow = FateFillLow; fillHigh = FateFillHigh;
                shimmerA = FateShimmerA; shimmerB = FateShimmerB;
                return true;
            }

            return false;
        }

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            if (player.dead || player.invis) return false;
            return TryGetChargeData(player, out _, out _, out _, out _, out _, out _, out _);
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;

            if (!TryGetChargeData(player, out float charge, out bool isFull,
                out Color fillLow, out Color fillHigh, out Color shimmerA, out Color shimmerB,
                out Texture2D frameTex))
                return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            if (pixel == null) return;

            Vector2 playerDrawPos = drawInfo.Position - Main.screenPosition + player.Size / 2f;
            float barX = playerDrawPos.X - DrawWidth / 2f;

            if (frameTex != null)
            {
                // === Ornate texture bar (Incisor, EternalMoon, CelestialValor, SakurasBlossom) ===
                float aspect = (float)frameTex.Height / frameTex.Width;
                int drawHeight = (int)(DrawWidth * aspect);
                float barY = playerDrawPos.Y + BarYOffset;

                float fillLeftPct = 0.14f;
                float fillRightPct = 0.86f;
                float fillTopPct = 0.28f;
                float fillBottomPct = 0.72f;

                float fillAreaWidth = (fillRightPct - fillLeftPct) * DrawWidth;
                float fillAreaHeight = (fillBottomPct - fillTopPct) * drawHeight;
                float fillX = barX + fillLeftPct * DrawWidth;
                float fillY = barY + fillTopPct * drawHeight;

                if (charge > 0f)
                {
                    int fillWidth = Math.Max((int)(fillAreaWidth * charge), 1);
                    Color fillColor = isFull
                        ? Color.Lerp(shimmerA, shimmerB, 0.5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * 6f))
                        : Color.Lerp(fillLow, fillHigh, charge);
                    DrawRect(ref drawInfo, pixel, fillX, fillY, fillWidth, (int)fillAreaHeight, fillColor);
                    DrawRect(ref drawInfo, pixel, fillX, fillY, fillWidth, 1, Color.White * 0.3f);
                }

                Vector2 scale = new Vector2((float)DrawWidth / frameTex.Width, (float)drawHeight / frameTex.Height);
                drawInfo.DrawDataCache.Add(new DrawData(frameTex,
                    new Vector2(barX, barY), new Rectangle(0, 0, frameTex.Width, frameTex.Height),
                    Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0));

                if (isFull)
                {
                    float pulse = 0.5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * 5f);
                    Color glowColor = Color.Lerp(shimmerA, shimmerB, pulse) * 0.4f;
                    DrawRect(ref drawInfo, pixel, fillX - 1, fillY - 1, (int)fillAreaWidth + 2, 1, glowColor);
                    DrawRect(ref drawInfo, pixel, fillX - 1, fillY + (int)fillAreaHeight, (int)fillAreaWidth + 2, 1, glowColor);
                    DrawRect(ref drawInfo, pixel, fillX - 1, fillY, 1, (int)fillAreaHeight, glowColor);
                    DrawRect(ref drawInfo, pixel, fillX + (int)fillAreaWidth, fillY, 1, (int)fillAreaHeight, glowColor);
                }
            }
            else
            {
                // === Generic rectangle bar (all other weapons) ===
                float barY = playerDrawPos.Y + BarYOffset;
                int b = GenericBarBorderWidth;

                // Dark background border
                DrawRect(ref drawInfo, pixel, barX, barY, DrawWidth, GenericBarHeight, new Color(20, 20, 30) * 0.8f);

                // Inner fill area
                float innerWidth = DrawWidth - 2 * b;
                float innerHeight = GenericBarHeight - 2 * b;
                float fillX = barX + b;
                float fillY = barY + b;

                // Background
                DrawRect(ref drawInfo, pixel, fillX, fillY, (int)innerWidth, (int)innerHeight, new Color(10, 10, 15) * 0.9f);

                if (charge > 0f)
                {
                    int fillWidth = Math.Max((int)(innerWidth * charge), 1);
                    Color fillColor = isFull
                        ? Color.Lerp(shimmerA, shimmerB, 0.5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * 6f))
                        : Color.Lerp(fillLow, fillHigh, charge);
                    DrawRect(ref drawInfo, pixel, fillX, fillY, fillWidth, (int)innerHeight, fillColor);

                    // Top highlight
                    DrawRect(ref drawInfo, pixel, fillX, fillY, fillWidth, 1, Color.White * 0.25f);
                }

                if (isFull)
                {
                    float pulse = 0.5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * 5f);
                    Color glowColor = Color.Lerp(shimmerA, shimmerB, pulse) * 0.5f;
                    DrawRect(ref drawInfo, pixel, barX - 1, barY - 1, DrawWidth + 2, 1, glowColor);
                    DrawRect(ref drawInfo, pixel, barX - 1, barY + GenericBarHeight, DrawWidth + 2, 1, glowColor);
                    DrawRect(ref drawInfo, pixel, barX - 1, barY, 1, GenericBarHeight, glowColor);
                    DrawRect(ref drawInfo, pixel, barX + DrawWidth, barY, 1, GenericBarHeight, glowColor);
                }
            }
        }

        private static void DrawRect(ref PlayerDrawSet drawInfo, Texture2D pixel,
            float x, float y, int width, int height, Color color)
        {
            drawInfo.DrawDataCache.Add(new DrawData(pixel,
                new Vector2(x, y), new Rectangle(0, 0, 1, 1),
                color, 0f, Vector2.Zero, new Vector2(width, height),
                SpriteEffects.None, 0));
        }
    }
}
