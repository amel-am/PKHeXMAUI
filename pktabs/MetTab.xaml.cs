
using Syncfusion.Maui.Inputs;
using Syncfusion.Maui.DataSource.Extensions;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Input;
using PKHeX.Core;

using static PKHeXMAUI.MainPage;

namespace PKHeXMAUI;

public partial class MetTab : ContentPage
{
    public bool SkipEvent = false;
    public bool FirstLoad = true;
	public MetTab()
	{
		InitializeComponent();
      
        mettabpic.Source = spriteurl;
        origingamepicker.ItemsSource = (System.Collections.IList)datasourcefiltered.Games;
        origingamepicker.ItemDisplayBinding = new Binding("Text");
        battleversionpicker.ItemsSource = GameInfo.Strings.gamelist;
        metlocationpicker.ItemsSource = GameInfo.GetLocationList((GameVersion)pk.Version, pk.Context);
        metlocationpicker.DisplayMemberPath = "Text";
        eggmetpicker.ItemsSource = (System.Collections.IList)GameInfo.GetLocationList(sav.Version, sav.Context, true);
        eggmetpicker.ItemDisplayBinding = new Binding("Text");
        if (PSettings.DisplayLegalBallsOnly)
        {
            var PokemonBalls = BallApplicator.GetLegalBalls(pk);
            if (PokemonBalls.Count() != 0)
                ballpicker.ItemsSource = PokemonBalls.ToList();
            else
                ballpicker.ItemsSource = Enum.GetValues(typeof(Ball));
        }
        else
            ballpicker.ItemsSource = Enum.GetValues(typeof(Ball));


        ICommand refreshCommand = new Command(async () =>
        {

            await applymetinfo(pk);
            MetRefresh.IsRefreshing = false;
        });
        MetRefresh.Command = refreshCommand;
        applymetinfo(pk);
        FirstLoad = false;
    }
    public static string ballspriteurl;
    public async Task applymetinfo(PKM pkm)
    {
        SkipEvent = true;
        eggsprite.IsVisible = pkm.IsEgg;
        if (PSettings.DisplayLegalBallsOnly)
        {
            var PokemonsBalls = BallApplicator.GetLegalBalls(pkm).ToList();
            if (PokemonsBalls.Count != 0)
                ballpicker.ItemsSource = PokemonsBalls;
            else
                ballpicker.ItemsSource = Enum.GetValues(typeof(Ball));
        }
        if (pkm.HeldItem > 0)
        {
            itemsprite.Source = itemspriteurl;
            itemsprite.IsVisible = true;
        }
        else
            itemsprite.IsVisible = false;
        if (pkm.IsShiny)
            shinysparklessprite.IsVisible = true;
        else
            shinysparklessprite.IsVisible = false;
        if (pkm.Species == 0)
            spriteurl = $"a_egg.png";
        else
            spriteurl = $"a_{pkm.Species}{((pkm.Form > 0 && !MainPage.NoFormSpriteSpecies.Contains(pkm.Species)) ? $"_{pkm.Form}" : "")}.png";
        mettabpic.Source = spriteurl;
        origingamepicker.SelectedItem = datasourcefiltered.Games.Where(z => z.Value == pkm.Version).FirstOrDefault();
        if (pkm is IBattleVersion bv)
        {
            battleversionlabel.IsVisible = true;
            battleversionpicker.IsVisible = true;
            battleversionpicker.SelectedIndex = bv.BattleVersion;
        }
        
        metlocationpicker.SelectedItem = GameInfo.GetLocationList((GameVersion)pkm.Version, pkm.Context).Where(z=>z.Value == pkm.Met_Location).FirstOrDefault();
        ballpicker.SelectedItem = pkm.Ball > -1 ? (Ball)pkm.Ball : (Ball)0;
        ballspriteurl = $"{(pkm.Ball>0?$"ball{pkm.Ball}":"ball4")}.png";
        ballimage.Source = ballspriteurl;
       
        var metdate = pkm.MetDate!=null? (DateOnly)pkm.MetDate:DateOnly.MinValue;
        metdatepicker.Date = pkm.MetDate != null?metdate.ToDateTime(TimeOnly.Parse("10:00 PM")) :DateTime.Now;
        metleveldisplay.Text = pkm.Met_Level>-1?pkm.Met_Level.ToString():"0";
        if (pkm is IObedienceLevel ob)
        {
            obediencelevellabel.IsVisible = true;
            obedienceleveldisplay.IsVisible = true;
            obedienceleveldisplay.Text = ob.Obedience_Level.ToString();
        }
        fatefulcheck.IsChecked = pkm.FatefulEncounter;
        eggcheck.IsChecked = pkm.WasEgg;
        
        var eggmetdate = pkm.EggMetDate!=null? (DateOnly)pkm.EggMetDate:DateOnly.MinValue;
        eggdatepicker.Date = pkm.EggMetDate != null?eggmetdate.ToDateTime(TimeOnly.Parse("10:00 PM")):DateTime.Now;
        if(pkm.Egg_Location > -1)
        {
            eggmetpicker.SelectedItem = GameInfo.GetLocationList((GameVersion)sav.Version, sav.Context,true).Where(z => z.Value == pkm.Egg_Location).FirstOrDefault();
        }
        SkipEvent = false;
        
    }

    public void applyorigingame(object sender, EventArgs e)
    {
        if (!SkipEvent)
        {
            var version = (ComboItem)origingamepicker.SelectedItem;
            pk.Version = version.Value;
            metlocationpicker.ItemsSource = GameInfo.GetLocationList((GameVersion)pk.Version, pk.Context);
            metlocationpicker.DisplayMemberPath = "Text";
        }
    }

    private void applybattleversion(object sender, EventArgs e)
    {
        if (!SkipEvent)
        {
            if (pk is IBattleVersion bv)
                bv.BattleVersion = (byte)battleversionpicker.SelectedIndex;
        }
    }

    private void applymetlocation(object sender, EventArgs e)
    {

        if (metlocationpicker.SelectedItem != null && !SkipEvent)
        {
            var metlocation = (ComboItem)metlocationpicker.SelectedItem;
            pk.Met_Location = metlocation.Value;
        }
    }

    private void givebackballs(object sender, EventArgs e)
    {
        if (!SkipEvent)
        {
            pk.Ball = (int)(Ball)ballpicker.SelectedItem;
            ballspriteurl = $"ball{pk.Ball}.png";
            ballimage.Source = ballspriteurl;
        }
    }

    private void applymetdate(object sender, DateChangedEventArgs e)
    {
        if(!SkipEvent)
            pk.MetDate = DateOnly.FromDateTime( metdatepicker.Date);
    }

    private void applymetlevel(object sender, TextChangedEventArgs e)
    {
        if(metleveldisplay.Text.Length > 0 && !SkipEvent)
        {
            pk.Met_Level = int.Parse(metleveldisplay.Text);
        }
    }

    private void applyobediencelevel(object sender, TextChangedEventArgs e)
    {
        if (pk is IObedienceLevel ob && !SkipEvent)
        {
            if (obedienceleveldisplay.Text.Length > 0)
                ob.Obedience_Level = (byte)int.Parse(obedienceleveldisplay.Text);
        }
    }

    private void applyfateful(object sender, CheckedChangedEventArgs e)
    {
        if(!SkipEvent)
            pk.FatefulEncounter = fatefulcheck.IsChecked;
    }

  

    private void applyeggmetlocation(object sender, EventArgs e)
    {
        if (!SkipEvent)
        {
            var egglocation = (ComboItem)eggmetpicker.SelectedItem;
            pk.Egg_Location = egglocation.Value;
        }
    }

    private void applyeggdate(object sender, DateChangedEventArgs e)
    {
        if(!SkipEvent)
        pk.EggMetDate = DateOnly.FromDateTime(eggdatepicker.Date);
    }

    private void wasegg(object sender, CheckedChangedEventArgs e)
    {
        if (!SkipEvent)
        {
            if (eggcheck.IsChecked)
            {
                pk.Egg_Location = EncounterSuggestion.GetSuggestedEncounterEggLocationEgg(pk, true);
                pk.EggMetDate = DateOnly.FromDayNumber(DateTime.Now.Day);
            }
            else
            {
                
                pk.Egg_Location = LocationEdits.GetNoneLocation(pk);
                pk.EggMetDate = null;
            }
        }
    }

    private void refreshmet(object sender, EventArgs e)
    {
        if(pk.Species != 0)
            applymetinfo(pk);
    }

    private void ChangeComboBoxFontColor(object sender, PropertyChangedEventArgs e)
    {
        SfComboBox box = (SfComboBox)sender;
        if (box.IsDropDownOpen)
            box.TextColor = Colors.Black;
        else
            box.TextColor = Colors.White;
    }
}

