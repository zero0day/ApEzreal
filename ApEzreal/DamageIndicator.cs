using EloBuddy;
using EloBuddy.SDK;

namespace ApEzreal
{
    class DamageIndicator
    {
        private static float Qdamage(Obj_AI_Base target)
        {
            if (Program.Q.IsReady())
                return Player.Instance.CalculateDamageOnUnit(target, DamageType.Physical,
                    (float)(new[] { 0, 35, 55, 75, 95, 115 }[Program.Q.Level] + 1.1f * Player.Instance.FlatPhysicalDamageMod + 0.4f * Player.Instance.FlatMagicDamageMod
                        ));
            else return 0f;
        }

        private static float Wdamage(Obj_AI_Base target)
        {
            if (Program.W.IsReady())
                return Player.Instance.CalculateDamageOnUnit(target, DamageType.Magical,
                    (float)(new[] { 0, 70, 115, 160, 205, 250 }[Program.W.Level] + 0.8f * Player.Instance.FlatMagicDamageMod
                        ));
            else return 0f;
        }

        private static float Edamage(Obj_AI_Base target)
        {
            if (Program.E.IsReady())
                return Player.Instance.CalculateDamageOnUnit(target, DamageType.Magical,
                    (float)(new[] { 0, 75, 125, 175, 225, 275 }[Program.E.Level] + 0.5f * Player.Instance.FlatPhysicalDamageMod + 0.75f * Player.Instance.FlatMagicDamageMod
                        ));
            else return 0f;
        }

        public static float Rdamage(Obj_AI_Base target)
        {
            if (Program.R.IsReady())
                return Player.Instance.CalculateDamageOnUnit(target, DamageType.Magical,
                    (float)(new[] { 0, 350, 500, 650 }[Program.R.Level] + 1.0f * Player.Instance.FlatPhysicalDamageMod + 0.9f * Player.Instance.FlatMagicDamageMod
                        ));
            else return 0f;
        }

        public static float Damagefromspell(Obj_AI_Base target)
        {
            if (target == null)
            {
                return 0f;
            }
            else
            {
                return Qdamage(target) + Wdamage(target) + Edamage(target) + Rdamage(target);
            }
        }

    }
}
