using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK.Events;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using EloBuddy.SDK.Rendering;
using EloBuddy.SDK.Enumerations;

namespace ApEzreal
{
    class Program
    {
        private static AIHeroClient Ezreal = Player.Instance;
        private static Menu ApEzrealMenu, ComboMenu, HarassMenu, MiscMenu, DrawingsMenu;
        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        private static List<Spell.SpellBase> SpellList = new List<Spell.SpellBase>();
        private static Item hextech_gunblade = new Item(ItemId.Hextech_Gunblade);

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Ezreal.ChampionName != "Ezreal")
            {
                Chat.Print(Ezreal.ChampionName + " is not supported", System.Drawing.Color.WhiteSmoke);
                return;
            }
            else Chat.Print("Good luck and have fun with Ap Ezreal", System.Drawing.Color.WhiteSmoke);

            Q = new Spell.Skillshot(SpellSlot.Q, spellRange: 1150, skillShotType: EloBuddy.SDK.Enumerations.SkillShotType.Linear, spellSpeed: 2000, spellWidth: 60) { AllowedCollisionCount = 1 };
            W = new Spell.Skillshot(SpellSlot.W, spellRange: 1000, skillShotType: EloBuddy.SDK.Enumerations.SkillShotType.Linear, spellSpeed: 1550, spellWidth: 80) { AllowedCollisionCount = -1 };
            E = new Spell.Skillshot(SpellSlot.E, spellRange: 475, skillShotType: EloBuddy.SDK.Enumerations.SkillShotType.Circular, spellSpeed: null) { AllowedCollisionCount = 1 };
            R = new Spell.Skillshot(SpellSlot.R, 3000, SkillShotType.Linear, 1000, 2000, 160);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            //SpellList.Add(R);

            ApEzrealMenu = MainMenu.AddMenu("ApEzreal", "ApEzreal");
            ComboMenu = ApEzrealMenu.AddSubMenu("Combo");

            ComboMenu.Add("Q", new CheckBox("Use Q"));
            ComboMenu.Add("W", new CheckBox("Use W"));
            //ComboMenu.Add("E", new CheckBox("Use E"));
            ComboMenu.Add("R", new CheckBox("Use R"));
            ComboMenu.Add("gunblade", new CheckBox("Use Hextech Gunblade"));

            HarassMenu = ApEzrealMenu.AddSubMenu("AutoHarass");
            HarassMenu.Add("enabled", new CheckBox("Enabled"));
            HarassMenu.Add("W", new CheckBox("Use W"));
            HarassMenu.Add("Q", new CheckBox("Use Q"));
            HarassMenu.Add("manaLimit", new Slider("Mana Limit (%)", defaultValue: 30, minValue: 1, maxValue: 100));

            MiscMenu = ApEzrealMenu.AddSubMenu("Misc");
            MiscMenu.Add("autoUlt", new Slider("AutoUlt if n enemies", defaultValue: 3, minValue: 0, maxValue: 5));
            MiscMenu.Add("antigapcloser", new CheckBox("AntiGapCloser"));

            DrawingsMenu = ApEzrealMenu.AddSubMenu("Drawings");
            DrawingsMenu.Add("enabled", new CheckBox("Enabled"));

            foreach (var spell in SpellList)
            {
                DrawingsMenu.Add(spell.Slot.ToString(), new CheckBox("Draw " + spell.Slot));
            }

            DrawingsMenu.Add("damage", new CheckBox("Damage indicator"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += DrawCircles;
            Drawing.OnEndScene += Damage_Indicator;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender.IsEnemy && e.End.Distance(ObjectManager.Player.Position) < 300 && !sender.IsDead && E.IsReady() && MiscMenu["antigapcloser"].Cast<CheckBox>().CurrentValue)
            {
                E.Cast((ObjectManager.Player.Position.To2D() + (e.End - e.Start).To2D().Normalized() * E.Range).To3D());
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.Equals(Orbwalker.ActiveModes.Combo))
            {
                ComboVombo();
            }

            if (HarassMenu["enabled"].Cast<CheckBox>().CurrentValue)
            {
                HarassTheirAss();
            }

            var autoUtlCount = MiscMenu["autoUlt"].Cast<Slider>().CurrentValue;

            misc(autoUtlCount);

        }

        public static Obj_AI_Turret getTargetTurret(AIHeroClient target)
        {
            var turret =
            EntityManager.Turrets.Enemies.OrderBy(
                x => x.Distance(target.Position) <= 750 && !x.IsAlly && !x.IsDead)
                .FirstOrDefault();
            return turret;
        }

        #region ComboVombo

        private static void ComboVombo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);

            if (target == null) return;

            if (ComboMenu["W"].Cast<CheckBox>().CurrentValue)
            {
                var WPrediction = W.GetPrediction(target);
                if (target.IsValidTarget(W.Range) && target.IsAlive() && !target.IsInvulnerable && W.IsReady() && WPrediction.HitChance >= EloBuddy.SDK.Enumerations.HitChance.High)
                {
                    W.Cast(WPrediction.CastPosition);
                }
            }

            if (ComboMenu["Q"].Cast<CheckBox>().CurrentValue)
            {
                var QPrediction = Q.GetPrediction(target);
                if (target.IsValidTarget(Q.Range) && !target.IsInvulnerable && target.IsAlive() && Q.IsReady() && QPrediction.HitChance >= EloBuddy.SDK.Enumerations.HitChance.High)
                {
                    if (Ezreal.CountEnemyMinionsInRange(Q.Range) > 3) Q.Cast(target);
                    else Q.Cast(QPrediction.CastPosition);
                }
            }

            if (ComboMenu["gunblade"].Cast<CheckBox>().CurrentValue)
                if (hextech_gunblade.IsReady() && hextech_gunblade.IsInRange(target))
                    hextech_gunblade.Cast(target);

            if (ComboMenu["R"].Cast<CheckBox>().CurrentValue)
            {
                if (target.IsAlive() && !target.IsInvulnerable && R.IsReady() && !(Ezreal.CountAllyChampionsInRange(Ezreal.GetAutoAttackRange()) > 1))
                {
                    var RPrediction = R.GetPrediction(target);
                    if (DamageIndicator.Rdamage(target) - target.SpellBlock >= target.Health)
                    {
                        R.Cast(RPrediction.CastPosition);
                    }
                }
            }
        }

        #endregion

        #region HarassAss

        private static void HarassTheirAss()
        {
            if (Ezreal.IsRecalling()) return;
            if (Ezreal.IsUnderEnemyturret()) return;
            if (Orbwalker.ActiveModesFlags.Equals(Orbwalker.ActiveModes.Combo)) return;

            var target = TargetSelector.GetTarget(W.Range, DamageType.Magical);
            if (target == null) return;

            var manaLimit = HarassMenu["manaLimit"].Cast<Slider>().CurrentValue;

            if (HarassMenu["W"].Cast<CheckBox>().CurrentValue)
            {
                if (Ezreal.ManaPercent <= manaLimit)
                {
                    return;
                }

                var WPrediction = W.GetPrediction(target);
                if (target.IsValidTarget(W.Range) && target.IsAlive() && W.IsReady() && WPrediction.HitChance >= EloBuddy.SDK.Enumerations.HitChance.High)
                {
                    W.Cast(WPrediction.CastPosition);
                }
            }

            if (HarassMenu["Q"].Cast<CheckBox>().CurrentValue)
            {
                if (Ezreal.ManaPercent <= manaLimit)
                {
                    return;
                }

                var QPrediction = Q.GetPrediction(target);
                if (target.IsValidTarget(Q.Range) && target.IsAlive() && Q.IsReady() && QPrediction.HitChance >= EloBuddy.SDK.Enumerations.HitChance.High)
                {
                    if (Ezreal.CountEnemyMinionsInRange(Q.Range) > 3) Q.Cast(target);
                    else Q.Cast(QPrediction.CastPosition);
                }
            }

        }

        #endregion

        #region misc

        private static void misc(int count)
        {
            if (count != 0) R.CastIfItWillHit(minTargets: count, minHitchancePercent: 65);
            //DrawingsMenu["skinchanger"].Cast<Slider>().OnValueChange += (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args) =>
            //{
            //};
        }


        #endregion

        #region Drawings

        private static void DrawCircles(EventArgs args)
        {
            if (!DrawingsMenu["enabled"].Cast<CheckBox>().CurrentValue) return;
            foreach (var Spell in SpellList.Where(spell => DrawingsMenu[spell.Slot.ToString()].Cast<CheckBox>().CurrentValue))
            {
                Circle.Draw(Spell.IsReady() ? Color.Chartreuse : Color.OrangeRed, Spell.Range, Ezreal);
            }
        }

        private static void Damage_Indicator(EventArgs args)
        {
            if (DrawingsMenu["enabled"].Cast<CheckBox>().CurrentValue && DrawingsMenu["damage"].Cast<CheckBox>().CurrentValue)
            {
                foreach (var unit in EntityManager.Heroes.Enemies.Where(u => u.IsValidTarget() && u.IsHPBarRendered))
                {
                    var damage = DamageIndicator.Damagefromspell(unit);

                    if (damage <= 0)
                    {
                        continue;
                    }
                    var Special_X = unit.ChampionName == "Jhin" || unit.ChampionName == "Annie" ? -12 : 0;
                    var Special_Y = unit.ChampionName == "Jhin" || unit.ChampionName == "Annie" ? -3 : 9;

                    var DamagePercent = ((unit.TotalShieldHealth() - damage) > 0
                        ? (unit.TotalShieldHealth() - damage)
                        : 0) / (unit.MaxHealth + unit.AllShield + unit.AttackShield + unit.MagicShield);
                    var currentHealthPercent = unit.TotalShieldHealth() / (unit.MaxHealth + unit.AllShield + unit.AttackShield + unit.MagicShield);

                    var StartPoint = new Vector2((int)(unit.HPBarPosition.X + Special_X + DamagePercent * 107) + 1,
                        (int)unit.HPBarPosition.Y + Special_Y);
                    var EndPoint = new Vector2((int)(unit.HPBarPosition.X + Special_X + currentHealthPercent * 107) + 1,
                        (int)unit.HPBarPosition.Y + Special_Y);
                    var Color = System.Drawing.Color.DarkOliveGreen;
                    Drawing.DrawLine(StartPoint, EndPoint, 9.82f, Color);
                }
            }
        }

        #endregion

    }
}
