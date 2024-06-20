using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace FlexibleGameScale;

public class PatchedDayTimeMoneyBox : DayTimeMoneyBox
{
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        if (Game1.player.hasVisibleQuests && questButton.containsPoint(x, y) && Game1.player.CanMove && !Game1.dialogueUp && !Game1.eventUp && Game1.farmEvent == null)
            Game1.activeClickableMenu = new QuestLog();
        if (!Game1.options.zoomButtons)
            return;
        if (zoomInButton.containsPoint(x, y) && Game1.options.desiredBaseZoomLevel < ModEntry.MaxZoom)
        {
            var num = (int) Math.Round(Game1.options.desiredBaseZoomLevel * 100.0);
            Game1.options.desiredBaseZoomLevel = Math.Min(ModEntry.MaxZoom, (num - num % 5 + 5) / 100f);
            Game1.forceSnapOnNextViewportUpdate = true;
            Game1.playSound("drumkit6");
        }
        else
        {
            if (!zoomOutButton.containsPoint(x, y) || Game1.options.desiredBaseZoomLevel <= ModEntry.MinZoom)
                return;
            var num = (int) Math.Round(Game1.options.desiredBaseZoomLevel * 100.0);
            Game1.options.desiredBaseZoomLevel = Math.Max(ModEntry.MinZoom, (num - num % 5 - 5) / 100f);
            Game1.forceSnapOnNextViewportUpdate = true;
            Program.gamePtr.refreshWindowSettings();
            Game1.playSound("drumkit6");
        }
    }

    public override void draw(SpriteBatch b)
    {
        base.draw(b);
        if (!Game1.options.zoomButtons) return;
        zoomInButton.draw(b, Color.White * (Game1.options.desiredBaseZoomLevel >= ModEntry.MaxZoom ? 0.5f : 1f), 1f);
        zoomOutButton.draw(b, Color.White * (Game1.options.desiredBaseZoomLevel <= ModEntry.MinZoom ? 0.5f : 1f), 1f);
    }
}