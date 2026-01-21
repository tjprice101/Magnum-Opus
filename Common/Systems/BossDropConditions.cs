using Terraria.GameContent.ItemDropRules;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Drop condition that requires Eroica (God of Valor) to have been defeated.
    /// Used to gate miniboss essence drops until the main boss is killed.
    /// </summary>
    public class DownedEroicaCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            return MoonlightSonataSystem.DownedEroica;
        }

        public bool CanShowItemDropInUI()
        {
            return true;
        }

        public string GetConditionDescription()
        {
            return "After Eroica, God of Valor has been defeated";
        }
    }

    /// <summary>
    /// Drop condition that requires Swan Lake (The Monochromatic Fractal) to have been defeated.
    /// </summary>
    public class DownedSwanLakeCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            return MoonlightSonataSystem.DownedSwanLake;
        }

        public bool CanShowItemDropInUI()
        {
            return true;
        }

        public string GetConditionDescription()
        {
            return "After Swan Lake, The Monochromatic Fractal has been defeated";
        }
    }

    /// <summary>
    /// Drop condition that requires Enigma (The Hollow Mystery) to have been defeated.
    /// </summary>
    public class DownedEnigmaCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            return MoonlightSonataSystem.DownedEnigma;
        }

        public bool CanShowItemDropInUI()
        {
            return true;
        }

        public string GetConditionDescription()
        {
            return "After Enigma, The Hollow Mystery has been defeated";
        }
    }

    /// <summary>
    /// Drop condition that requires La Campanella (Chime of Life) to have been defeated.
    /// </summary>
    public class DownedLaCampanellaCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            return MoonlightSonataSystem.DownedLaCampanella;
        }

        public bool CanShowItemDropInUI()
        {
            return true;
        }

        public string GetConditionDescription()
        {
            return "After La Campanella, Chime of Life has been defeated";
        }
    }

    /// <summary>
    /// Drop condition that requires Moonlit Maestro to have been defeated.
    /// </summary>
    public class DownedMoonlitMaestroCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            return MoonlightSonataSystem.DownedMoonlitMaestro;
        }

        public bool CanShowItemDropInUI()
        {
            return true;
        }

        public string GetConditionDescription()
        {
            return "After Moonlit Maestro has been defeated";
        }
    }

    /// <summary>
    /// Drop condition that requires Fate (The Warden of Universal Melodies) to have been defeated.
    /// </summary>
    public class DownedFateCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            return MoonlightSonataSystem.FateBossKilledOnce;
        }

        public bool CanShowItemDropInUI()
        {
            return true;
        }

        public string GetConditionDescription()
        {
            return "After Fate, The Warden of Universal Melodies has been defeated";
        }
    }
}
