using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MagnumOpus.Content.SandboxLastPrism
{
    public class SLPColorInfo
    {
        public String textureLocation = "";

        public float grad1Speed = 2f / 3f;
        public float grad2Speed = 2f / 3f;
        public float grad3Speed = 3.1f / 3f;
        public float grad4Speed = 2.3f / 3f;

        public SLPColorInfo(String TexLocation, float Grad1Speed = 1f, float Grad2Speed = 1f, float Grad3Speed = 1f, float Grad4Speed = 1f)
        {
            textureLocation = TexLocation;
            grad1Speed = Grad1Speed;
            grad2Speed = Grad2Speed;
            grad3Speed = Grad3Speed;
            grad4Speed = Grad4Speed;
        }

        public SLPColorInfo(SLPColorType Type, String TexLocation, float Grad1Speed = 1f, float Grad2Speed = 1f, float Grad3Speed = 1f, float Grad4Speed = 1f)
        {
            //Yandev moment
            if (Type == SLPColorType.None)
            {
                dustColors.Add(Color.Red);
                dustColors.Add(Color.Orange);
                dustColors.Add(Color.Yellow);
                dustColors.Add(Color.LawnGreen);
                dustColors.Add(Color.DeepSkyBlue);
                dustColors.Add(Color.Purple);
            }
            else if (Type == SLPColorType.Lesbian)
            {
                dustColors.Add(Color.Purple);
                dustColors.Add(Color.Orange);
                dustColors.Add(Color.Goldenrod);
                dustColors.Add(Color.HotPink);
                dustColors.Add(Color.HotPink);
            }
            else if (Type == SLPColorType.Bisexual)
            {
                dustColors.Add(Color.DodgerBlue);
                dustColors.Add(Color.DeepPink);
                dustColors.Add(Color.Purple);
            }
            else if (Type == SLPColorType.Trans)
            {
                dustColors.Add(Color.SkyBlue);
                dustColors.Add(Color.LightSkyBlue);
                dustColors.Add(Color.DeepSkyBlue);
                dustColors.Add(Color.HotPink);
                dustColors.Add(Color.Pink);
                dustColors.Add(Color.LightPink);
            }
            else if (Type == SLPColorType.NonBinary)
            {
                dustColors.Add(Color.Gold);
                dustColors.Add(Color.Purple);
                dustColors.Add(Color.Purple * 1f);
            }
            else if (Type == SLPColorType.Asexual)
            {
                dustColors.Add(Color.Purple);
                dustColors.Add(Color.Purple * 1.5f);
            }
            else if (Type == SLPColorType.Aromantic)
            {
                dustColors.Add(Color.ForestGreen);
                dustColors.Add(Color.DarkGreen);
            }
            else if (Type == SLPColorType.Aroace)
            {
                dustColors.Add(Color.Orange);
                dustColors.Add(Color.Gold);
                dustColors.Add(Color.DeepSkyBlue);
                dustColors.Add(Color.DodgerBlue);
                dustColors.Add(Color.DodgerBlue);
                dustColors.Add(Color.Gold);
            }

            textureLocation = TexLocation;

            grad1Speed = Grad1Speed;
            grad2Speed = Grad2Speed;
            grad3Speed = Grad3Speed;
            grad4Speed = Grad4Speed;
        }

        public List<Color> dustColors = new List<Color>();

        public List<Color> laserMainColors = new List<Color>();

        public List<Color> laserBackColors = new List<Color>();
    }

    public enum SLPColorType
    {
        None = 0,
        Lesbian = 1,
        Bisexual = 2,
        Trans = 3,
        NonBinary = 4,
        Asexual = 5,
        Aromantic = 6,
        Aroace = 7
    }
}
