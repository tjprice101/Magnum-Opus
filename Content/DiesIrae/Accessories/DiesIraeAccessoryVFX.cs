using Microsoft.Xna.Framework;
using Terraria;

namespace MagnumOpus.Content.DiesIrae.Accessories
{

    public static class EmberOfTheCondemnedVFX
    {
        public static void AmbientAura(Vector2 playerCenter, float timer) { }
        public static void HellfireProcVFX(Vector2 enemyCenter) { }
        public static void HitVFX(Vector2 hitPos) { }
    }

    public static class SealOfDamnationVFX
    {
        public static void AmbientAura(Vector2 playerCenter, float timer) { }
        public static void DamnationProcVFX(Vector2 enemyCenter) { }
        public static void HitVFX(Vector2 hitPos) { }
    }

    public static class ChainOfFinalJudgmentVFX
    {
        public static void AmbientAura(Vector2 playerCenter, float timer) { }
        public static void LifestealVFX(Vector2 playerCenter, Vector2 enemyCenter) { }
        public static void ExecuteVFX(Vector2 enemyCenter) { }
        public static void HitVFX(Vector2 hitPos) { }
    }

    public static class RequiemsShackleVFX
    {
        public static void AmbientAura(Vector2 playerCenter, float timer) { }
        public static void MarkProcVFX(Vector2 enemyCenter) { }
        public static void HitVFX(Vector2 hitPos) { }
    }

    public static class DiesIraeAccessoryVFX
    {
        public static void EmberAmbientAura(Vector2 playerCenter, float timer) =>
            EmberOfTheCondemnedVFX.AmbientAura(playerCenter, timer);
        public static void EmberHellfireProcVFX(Vector2 enemyCenter) =>
            EmberOfTheCondemnedVFX.HellfireProcVFX(enemyCenter);
        public static void EmberHitVFX(Vector2 hitPos) =>
            EmberOfTheCondemnedVFX.HitVFX(hitPos);

        public static void SealAmbientAura(Vector2 playerCenter, float timer) =>
            SealOfDamnationVFX.AmbientAura(playerCenter, timer);
        public static void SealDamnationProcVFX(Vector2 enemyCenter) =>
            SealOfDamnationVFX.DamnationProcVFX(enemyCenter);
        public static void SealHitVFX(Vector2 hitPos) =>
            SealOfDamnationVFX.HitVFX(hitPos);

        public static void ChainAmbientAura(Vector2 playerCenter, float timer) =>
            ChainOfFinalJudgmentVFX.AmbientAura(playerCenter, timer);
        public static void ChainLifestealVFX(Vector2 playerCenter, Vector2 enemyCenter) =>
            ChainOfFinalJudgmentVFX.LifestealVFX(playerCenter, enemyCenter);
        public static void ChainExecuteVFX(Vector2 enemyCenter) =>
            ChainOfFinalJudgmentVFX.ExecuteVFX(enemyCenter);
        public static void ChainHitVFX(Vector2 hitPos) =>
            ChainOfFinalJudgmentVFX.HitVFX(hitPos);

        public static void ShackleAmbientAura(Vector2 playerCenter, float timer) =>
            RequiemsShackleVFX.AmbientAura(playerCenter, timer);
        public static void ShackleMarkProcVFX(Vector2 enemyCenter) =>
            RequiemsShackleVFX.MarkProcVFX(enemyCenter);
        public static void ShackleHitVFX(Vector2 hitPos) =>
            RequiemsShackleVFX.HitVFX(hitPos);
    }
}
