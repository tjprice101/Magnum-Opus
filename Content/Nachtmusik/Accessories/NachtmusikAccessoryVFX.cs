using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Content.Nachtmusik.Accessories
{
    public static class NachtmusikAccessoryVFX
    {
        public static void AmbientStarlitDrift(Vector2 center) { }
        public static void AmbientMusicNotes(Vector2 center) { }
        public static void AmbientLight(Vector2 center, Color lightColor, float baseIntensity = 0.15f)
        {
            Lighting.AddLight(center, lightColor.ToVector3() * baseIntensity);
        }
        public static void FullAmbientVFX(Vector2 center, Color lightColor)
        {
            AmbientLight(center, lightColor);
        }
        public static void OnHitProcVFX(Vector2 hitPos, Color procColor, float intensity = 1f) { }
        public static void DrawWorldItemBloom(SpriteBatch sb, Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale) { }

        public static void MoonlitSerenadePendantAmbientVFX(Player player) { }
        public static void MoonlitSerenadePendantProcVFX(Vector2 hitPos) { }
        public static void NocturnesEmbraceAmbientVFX(Player player) { }
        public static void NocturnesEmbraceProcVFX(Vector2 hitPos) { }
        public static void RadianceOfTheNightQueenAmbientVFX(Player player) { }
        public static void RadianceOfTheNightQueenProcVFX(Vector2 hitPos) { }
        public static void QueensRadianceActivateVFX(Vector2 pos) { }
        public static void QueensRadianceActiveVFX(Player player) { }
        public static void StarweaversSignetAmbientVFX(Player player) { }
        public static void StarweaversSignetProcVFX(Vector2 hitPos) { }
    }
}
