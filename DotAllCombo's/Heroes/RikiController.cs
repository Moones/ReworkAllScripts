namespace DotaAllCombo.Heroes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ensage;
    using Ensage.Common;
    using Ensage.Common.Extensions;
    using Ensage.Common.Menu;
    using SharpDX;
    using SharpDX.Direct3D9;

    using Service;
    using Service.Debug;

    internal class RikiController : Variables, IHeroController
    {
        private Ability Q, W, R;
        private Item urn, dagon, diff, mjollnir, orchid, abyssal, mom, Shiva, mail, bkb, satanic, medall, blink;
        public void OnLoadEvent()
        {
            AssemblyExtensions.InitAssembly("VickTheRock", "0.1");

            Print.LogMessage.Success("Into the shadows.");
			Menu.AddItem(new MenuItem("enabled", "Enabled").SetValue(true));
			Menu.AddItem(new MenuItem("orbwalk", "orbwalk").SetValue(true));
			Menu.AddItem(new MenuItem("keyBind", "Combo key").SetValue(new KeyBind('D', KeyBindType.Press)));
            
            Menu.AddItem(
                new MenuItem("Skills", "Skills").SetValue(new AbilityToggler(new Dictionary<string, bool>
                {
                    {"riki_smoke_screen", true},
                    {"riki_blink_strike", true},
                    {"riki_tricks_of_the_trade", true}
                })));
            Menu.AddItem(
                new MenuItem("Items", "Items:").SetValue(new AbilityToggler(new Dictionary<string, bool>
                {
                    {"item_blink", true},
                    {"item_diffusal_blade", true},
                    {"item_diffusal_blade_2", true},
                    {"item_orchid", true},
                    {"item_bloodthorn", true},
                    {"item_urn_of_shadows", true},
                    {"item_abyssal_blade", true},
                    {"item_shivas_guard", true},
                    {"item_blade_mail", true},
                    {"item_black_king_bar", true},
                    {"item_satanic", true},
                    {"item_medallion_of_courage", true},
                    {"item_solar_crest", true}
                })));
            Menu.AddItem(new MenuItem("Heel", "Min targets to BKB").SetValue(new Slider(2, 1, 5)));
            Menu.AddItem(new MenuItem("Ult", "Min targets to Ultimate").SetValue(new Slider(3, 1, 5)));
            Menu.AddItem(new MenuItem("Heelm", "Min targets to BladeMail").SetValue(new Slider(2, 1, 5)));
		}

        public void Combo()
        {
            
            me = ObjectManager.LocalHero;
			
            if (!Menu.Item("enabled").IsActive()) return;
			Active = Game.IsKeyDown(Menu.Item("keyBind").GetValue<KeyBind>().Key);
			e = me.ClosestToMouseTarget(1800);
            if (e == null) return;

            Q = me.Spellbook.SpellQ;
            W = me.Spellbook.SpellW;
            R = me.Spellbook.SpellR;
            Shiva = me.FindItem("item_shivas_guard");
            mom = me.FindItem("item_mask_of_madness");
            diff = me.FindItem("item_diffusal_blade")?? me.FindItem("item_diffusal_blade_2");
            urn = me.FindItem("item_urn_of_shadows");
            dagon = me.Inventory.Items.FirstOrDefault(item => item.Name.Contains("item_dagon"));
            mjollnir = me.FindItem("item_mjollnir");
            orchid = me.FindItem("item_orchid") ?? me.FindItem("item_bloodthorn");
            abyssal = me.FindItem("item_abyssal_blade");
            mail = me.FindItem("item_blade_mail");
            bkb = me.FindItem("item_black_king_bar");
            satanic = me.FindItem("item_satanic");
            blink = me.FindItem("item_blink");
            medall = me.FindItem("item_medallion_of_courage") ?? me.FindItem("item_solar_crest");

            var stoneModif = e.HasModifier("modifier_medusa_stone_gaze_stone");
            
            var v =
                ObjectManager.GetEntities<Hero>()
                    .Where(x => x.Team != me.Team && x.IsAlive && x.IsVisible && !x.IsIllusion && !x.IsMagicImmune())
                    .ToList();

            if (R.IsInAbilityPhase || me.HasModifier("modifier_riki_tricks_of_the_trade_phase")) return;
            if (Active && e.IsAlive)
            {

				if (Menu.Item("orbwalk").GetValue<bool>() && me.Distance2D(e) <= 1900)
				{
					Orbwalking.Orbwalk(e, 0, 1600, true, true);
				}
			}
            if (Active && me.Distance2D(e) <= 1400 && e.IsAlive && (!me.IsInvisible() || me.IsVisibleToEnemies || e.Health <= (e.MaximumHealth * 0.7)))
            {
                if (stoneModif) return;
                    if (
                        Q != null
                        && Q.CanBeCasted()
                        && me.Distance2D(e) <= 300
                        && Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(Q.Name)
                        && Utils.SleepCheck("Q")
                        )
                    {
                        Q.UseAbility(Prediction.InFront(e, 80));
                        Utils.Sleep(200, "Q");
					}
				if (
                        W != null && W.CanBeCasted()
                        && me.Distance2D(e) <= W.CastRange
                        && Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(W.Name)
                        && Utils.SleepCheck("W")
                        )
                    {
                        W.UseAbility(e);
                        Utils.Sleep(200, "W");
                    }
                    if (
                        blink != null
                        && me.CanCast()
                        && blink.CanBeCasted()
                        && me.Distance2D(e) < 1180
                        && me.Distance2D(e) > 300
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(blink.Name)
                        && Utils.SleepCheck("blink")
                        )
                        {
                            blink.UseAbility(e.Position);
                            Utils.Sleep(250, "blink");
                        }

                        if ( // Abyssal Blade
                            abyssal != null
                            && abyssal.CanBeCasted()
                            && me.CanCast()
                            && !e.IsStunned()
                            && !e.IsHexed()
                            && Utils.SleepCheck("abyssal")
                            && me.Distance2D(e) <= 400
                            )
                        {
                            abyssal.UseAbility(e);
                            Utils.Sleep(250, "abyssal");
				} // Abyssal Item end  
				if (diff != null
						&& diff.CanBeCasted()
						&& diff.CurrentCharges > 0
						&& me.Distance2D(e) <= 400
						&& !e.HasModifier("modifier_item_diffusal_blade_slow")
						&& Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(diff.Name) &&
						Utils.SleepCheck("diff"))
                    {
					diff.UseAbility(e);
					Utils.Sleep(4000, "diff");
				}
				if ( // Mjollnir
                           mjollnir != null
                           && mjollnir.CanBeCasted()
                           && me.CanCast()
                           && !e.IsMagicImmune()
                           && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(mjollnir.Name)
                           && Utils.SleepCheck("mjollnir")
                           && me.Distance2D(e) <= 900
                           )
                        {
                            mjollnir.UseAbility(me);
                            Utils.Sleep(250, "mjollnir");
                        } // Mjollnir Item end
                        if ( // Medall
                            medall != null
                            && medall.CanBeCasted()
                            && Utils.SleepCheck("Medall")
                            && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(medall.Name)
                            && me.Distance2D(e) <= 700
                            )
                        {
                            medall.UseAbility(e);
                            Utils.Sleep(250, "Medall");
                        } // Medall Item end

                        if ( // MOM
                            mom != null
                            && mom.CanBeCasted()
                            && me.CanCast()
                            && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(mom.Name)
                            && Utils.SleepCheck("mom")
                            && me.Distance2D(e) <= 700
                            )
                        {
                            mom.UseAbility();
                            Utils.Sleep(250, "mom");
                        }
                        if (orchid != null && orchid.CanBeCasted() && me.Distance2D(e) <= 300 &&
                            Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(orchid.Name) && Utils.SleepCheck("orchid"))
                        {
                            orchid.UseAbility(e);
                            Utils.Sleep(100, "orchid");
                        }

                        if (Shiva != null && Shiva.CanBeCasted() && me.Distance2D(e) <= 600
                            && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(Shiva.Name)
                            && !e.IsMagicImmune() && Utils.SleepCheck("Shiva"))
                        {
                            Shiva.UseAbility();
                            Utils.Sleep(100, "Shiva");
                        }

                        if ( // Dagon
                            me.CanCast()
                            && dagon != null
                            && !e.IsLinkensProtected()
                            && dagon.CanBeCasted()
                            && !e.IsMagicImmune()
                            && !stoneModif
                            && Utils.SleepCheck("dagon")
                            )
                        {
                            dagon.UseAbility(e);
                            Utils.Sleep(200, "dagon");
                        } // Dagon Item end


                        if (urn != null && urn.CanBeCasted() && urn.CurrentCharges > 0 && me.Distance2D(e) <= 400
                            && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(urn.Name) && Utils.SleepCheck("urn"))
                        {
                            urn.UseAbility(e);
                            Utils.Sleep(240, "urn");
                        }
                        if ( // Satanic 
                            satanic != null &&
                            me.Health <= (me.MaximumHealth * 0.3) &&
                            satanic.CanBeCasted() &&
                            me.Distance2D(e) <= me.AttackRange + 50
                            && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(satanic.Name)
                            && Utils.SleepCheck("satanic")
                            )
                        {
                            satanic.UseAbility();
                            Utils.Sleep(240, "satanic");
                        } // Satanic Item end
                        if (mail != null && mail.CanBeCasted() 
                        && ((v.Count(x => x.Distance2D(me) <= 650) >= (Menu.Item("Heelm").GetValue<Slider>().Value)) 
                        || me.HasModifier("modifier_skywrath_mystic_flare_aura_effect")) &&
                            Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(mail.Name) && Utils.SleepCheck("mail"))
                        {
                            mail.UseAbility();
                            Utils.Sleep(100, "mail");
                        }
                        if (bkb != null && bkb.CanBeCasted() && ((v.Count(x => x.Distance2D(me) <= 650) >=
                                                                 (Menu.Item("Heel").GetValue<Slider>().Value))
                        || me.HasModifier("modifier_skywrath_mystic_flare_aura_effect")) &&
                            Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(bkb.Name) && Utils.SleepCheck("bkb"))
                        {
                            bkb.UseAbility();
                            Utils.Sleep(100, "bkb");
                        }
                    if (R != null && R.CanBeCasted()
                        && (v.Count(x => x.Distance2D(me) <= 500)
                        >= (Menu.Item("Ult").GetValue<Slider>().Value))
                        && Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(R.Name)
                        && Utils.SleepCheck("R"))
                    {
                        R.UseAbility();
                        Utils.Sleep(100, "R");
                    }
                }
			
		}

        public void OnCloseEvent()
        {

        }
		
	}
}



