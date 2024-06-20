using System.Reflection.Emit;
using Microsoft.Xna.Framework;
using StardewHack;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace FlexibleGameZoom;

public class ModEntry : Hack<ModEntry>
{
    public const float MaxZoom = 2f;
    public const float MinZoom = 0.2f;

    public override void HackEntry(IModHelper modHelper)
    {
        modHelper.ConsoleCommands.Add("dbg", "", OnDebugCommand);
        modHelper.Events.Display.MenuChanged += DisplayOnMenuChanged;
        Patch((DayTimeMoneyBox _) => _.receiveLeftClick(0, 0, false), DayTimeMoneyBox_receiveLeftClick);
        Patch((DayTimeMoneyBox _) => _.draw(null), DayTimeMoneyBox_draw);
    }

    private void DayTimeMoneyBox_receiveLeftClick()
    {
        var ifZoomIn = FindCode(
            Instructions.Call_get(typeof(Game1), "options"),
            Instructions.Callvirt_get(typeof(Options), "desiredBaseZoomLevel"),
            Instructions.Ldc_R4(2),  // To modify
            OpCodes.Bge_Un_S
        );
        var ifZoomOut = FindCode(
            Instructions.Call_get(typeof(Game1), "options"),
            Instructions.Callvirt_get(typeof(Options), "desiredBaseZoomLevel"),
            Instructions.Ldc_R4(0.75f),  // To modify
            OpCodes.Ble_Un_S
        );
        ifZoomIn.Splice(2, 1, Instructions.Ldc_R4(MaxZoom));
        ifZoomOut.Splice(2, 1, Instructions.Ldc_R4(MinZoom));

        var assignDesiredBaseZoomLevelWhenZoomIn = FindCode(
            Instructions.Ldloc_0(),
            Instructions.Ldloc_0(),
            Instructions.Ldc_I4_5(),
            OpCodes.Rem,
            OpCodes.Sub,
            Instructions.Stloc_0(),
            Instructions.Ldloc_0(),
            Instructions.Ldc_I4_5(),
            Instructions.Add(),
            Instructions.Stloc_0(),
            Instructions.Call_get(typeof(Game1), "options"),
            Instructions.Ldc_R4(2),  // To modify
            Instructions.Ldloc_0(),
            Instructions.Conv_R4(),
            Instructions.Ldc_R4(100),
            OpCodes.Div,
            Instructions.Call(typeof(Math), "Min", typeof(float), typeof(float)),
            Instructions.Callvirt_set(typeof(Options), "desiredBaseZoomLevel")
        );
        var assignDesiredBaseZoomLevelWhenZoomOut = FindCode(
            Instructions.Ldloc_2(),
            Instructions.Ldloc_2(),
            Instructions.Ldc_I4_5(), 
            OpCodes.Rem, 
            OpCodes.Sub, 
            Instructions.Stloc_2(), 
            Instructions.Ldloc_2(), 
            Instructions.Ldc_I4_5(), 
            Instructions.Sub(), 
            Instructions.Stloc_2(), 
            Instructions.Call_get(typeof(Game1), "options"), 
            Instructions.Ldc_R4(0.75f),  // To modify
            Instructions.Ldloc_2(), 
            Instructions.Conv_R4(), 
            Instructions.Ldc_R4(100), 
            OpCodes.Div, 
            Instructions.Call(typeof(Math), "Max", typeof(float), typeof(float)), 
            Instructions.Callvirt_set(typeof(Options), "desiredBaseZoomLevel")
            );
        assignDesiredBaseZoomLevelWhenZoomIn.Splice(11, 1, Instructions.Ldc_R4(MaxZoom));
        assignDesiredBaseZoomLevelWhenZoomOut.Splice(11, 1, Instructions.Ldc_R4(MinZoom));
    }

    private void DayTimeMoneyBox_draw()
    {
        var drawZoomInButton = FindCode(
            Instructions.Ldarg_0(),
            Instructions.Ldfld(typeof(DayTimeMoneyBox), "zoomInButton"),
            Instructions.Ldarg_1(),
            Instructions.Call_get(typeof(Color), "White"),
            Instructions.Call_get(typeof(Game1), "options"),
            Instructions.Callvirt_get(typeof(Options), "desiredBaseZoomLevel"),
            Instructions.Ldc_R4(2),
            OpCodes.Bge_S
        );
        var drawZoomOutButton = FindCode(
            Instructions.Ldarg_0(),
            Instructions.Ldfld(typeof(DayTimeMoneyBox), "zoomOutButton"),
            Instructions.Ldarg_1(),
            Instructions.Call_get(typeof(Color), "White"),
            Instructions.Call_get(typeof(Game1), "options"),
            Instructions.Callvirt_get(typeof(Options), "desiredBaseZoomLevel"),
            Instructions.Ldc_R4(0.75f),
            OpCodes.Ble_S
        );

        drawZoomInButton.Splice(6, 1, Instructions.Ldc_R4(MaxZoom));
        drawZoomOutButton.Splice(6, 1, Instructions.Ldc_R4(MinZoom));
    }

    private void DisplayOnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu is not GameMenu gameMenu)
        {
            return;
        }

        foreach (var page in gameMenu.pages)
        {
            if (page is not OptionsPage optionsPage)
            {
                continue;
            }

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator (for performance)
            foreach (var option in optionsPage.options)
            {
                if (option is not OptionsPlusMinus zoomOption || option.whichOption != Options.zoom)
                {
                    continue;
                }

                var optionsList = new List<string>();
                for (var i = MinZoom * 100; i <= MaxZoom * 100; i += 5)
                    optionsList.Add(i + "%");

                zoomOption.options = optionsList;
                zoomOption.displayOptions = optionsList;

                Game1.options.setPlusMinusToProperValue(zoomOption);
            }

            return;
        }
    }

    private void OnDebugCommand(string arg1, string[] arg2)
    {
        switch (arg2[0])
        {
            case "trash":
                Game1.player.removeItemFromInventory(Game1.player.CurrentItem);
                break;

            default:
                Game1.options.desiredBaseZoomLevel = 0.5f;
                break;
        }
    }
}