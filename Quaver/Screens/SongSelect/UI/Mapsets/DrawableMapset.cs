﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Resources;
using Quaver.Scheduling;
using Quaver.Screens.Loading;
using Quaver.Skinning;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Window;

namespace Quaver.Screens.SongSelect.UI.Mapsets
{
    public class DrawableMapset : Button
    {
        /// <summary>
        ///     The container to scroll for maps.
        /// </summary>
        public MapsetScrollContainer Container { get; }

        /// <summary>
        ///     The mapset this drawable is currently representing.
        /// </summary>
        public Mapset Mapset { get; private set; }

        /// <summary>
        ///     The index of the set in Screen.AvailableMapsets
        /// </summary>
        public int MapsetIndex { get; set; }

        /// <summary>
        ///     The thumbnail of the mapset.
        /// </summary>
        public Sprite Thumbnail { get; }

        /// <summary>
        ///    The title of the song.
        /// </summary>
        public SpriteText Title { get; }

        /// <summary>
        ///     The artist of the song
        /// </summary>
        public SpriteText Artist { get; }

        /// <summary>
        ///     The creator of the mapset.
        /// </summary>
        public SpriteText Creator { get; }

        /// <summary>
        ///     The height of the drawable mapset.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static int HEIGHT { get; } = 104;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public DrawableMapset(MapsetScrollContainer container)
        {
            Container = container;
            Size = new ScalableVector2(414, HEIGHT);
            Tint = Color.Black;
            AddBorder(Color.White, 2);

            Thumbnail = new Sprite()
            {
                Parent = this,
                Size = new ScalableVector2(HEIGHT * 0.85f + 40, HEIGHT * 0.85f),
                Alignment = Alignment.MidLeft,
                X = 10,
                Y = 2,
                Alpha = 0,
                SetChildrenAlpha = true
            };

            Thumbnail.AddBorder(Color.White);
            Thumbnail.Border.Alpha = Thumbnail.Alpha;

            Title = new SpriteText(BitmapFonts.Exo2SemiBold, " ", 13)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(15, 18)
            };

            Artist = new SpriteText(BitmapFonts.Exo2SemiBold, " ", 12, false)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(Title.X, Title.Y + Title.Height + 5)
            };

            Creator = new SpriteText(BitmapFonts.Exo2Medium, " ", 10, false)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(Title.X, Artist.Y + Artist.Height + 5),
                Tint = Colors.MainAccent
            };

            Clicked += OnClicked;
        }

        /// <summary>
        ///     Updates the mapset this drawable represents.
        /// </summary>
        public void UpdateWithNewMapset(Mapset set, int mapsetIndex)
        {
            Mapset = set;
            MapsetIndex = mapsetIndex;

            Title.Text = Mapset.Title;
            Artist.Text = Mapset.Artist;
            Creator.Text = "By: " + Mapset.Creator;
        }

        /// <summary>
        ///     Displays the mapset as selected.
        /// </summary>
        public void DisplayAsSelected(Map map)
        {
            lock (Animations)
            lock (Title.Animations)
            lock (Artist.Animations)
            lock (Creator.Animations)
            lock (Border.Animations)
            {
                // Change the width of the set outwards to appear it as selected.
                Animations.Clear();
                ChangeWidthTo(514, Easing.OutQuint, 400);

                Title.Animations.Clear();
                Artist.Animations.Clear();
                Creator.Animations.Clear();

                var targetX = 15 + Thumbnail.Width + 10;

                Title.MoveToX(targetX, Easing.OutQuint, 400);
                Artist.MoveToX(targetX, Easing.OutQuint, 400);
                Creator.MoveToX(targetX, Easing.OutQuint, 400);

                Border.Animations.Clear();
                Border.FadeToColor(Colors.MainAccent, Easing.Linear, 200);
            }

            LoadThumbnail(map);
        }

        /// <summary>
        ///     Displays the mapset as deselected
        /// </summary>
        public void DisplayAsDeselected()
        {
            // Push set outwards to make it appear as selected.
            lock (Animations)
            lock (Title.Animations)
            lock (Artist.Animations)
            lock (Creator.Animations)
            lock (Border.Animations)
            lock (Thumbnail.Animations)
            {
                Animations.Clear();
                ChangeWidthTo(414, Easing.OutQuint, 400);

                Title.Animations.Clear();
                Artist.Animations.Clear();
                Creator.Animations.Clear();

                const int targetX = 15;

                Title.MoveToX(targetX, Easing.OutQuint, 400);
                Artist.MoveToX(targetX, Easing.OutQuint, 400);
                Creator.MoveToX(targetX, Easing.OutQuint, 400);

                Border.Animations.Clear();
                Border.FadeToColor(Color.White, Easing.Linear, 200);

                Thumbnail.Animations.Clear();
                Thumbnail.Alpha = 0;

                if (Thumbnail.Image != null && Thumbnail.Image != WobbleAssets.WhiteBox)
                    Thumbnail.Image.Dispose();
            }
        }

        /// <summary>
        ///     Called when the button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClicked(object sender, EventArgs e)
        {
            // If the user clicks on the mapset again while its already selected, then we can
            // assume they want to play the map.
            if (Container.SelectedMapsetIndex == MapsetIndex)
            {
                // TODO: Scores.
                QuaverScreenManager.ChangeScreen(new MapLoadingScreen(null));
                return;
            }

            var map = Mapset.PreferredMap ?? Mapset.Maps.First();
            Container.SelectMap(MapsetIndex, map);
        }

        /// <inheritdoc />
        /// <summary>
        ///     In this case, we only want buttons to be clickable if they're in the bounds of the scroll container.
        /// </summary>
        /// <returns></returns>
        protected override bool IsMouseInClickArea()
        {
            var newRect = Rectangle.Intersect(ScreenRectangle.ToRectangle(), Container.ScreenRectangle.ToRectangle());
            return GraphicsHelper.RectangleContains(newRect, MouseManager.CurrentState.Position);
        }

        /// <summary>
        ///     Loads and updates the thumbnail of the mapset in a separate thread
        ///
        ///     Handles (poorly) the edge case of then the mapset isn't selected anymore,
        ///     and disposes of the loaded texture in case it took too long.
        /// </summary>
        private void LoadThumbnail(Map map) => Scheduler.RunThread(() =>
        {
            try
            {
                var tex = AssetLoader.LoadTexture2DFromFile(MapManager.GetBackgroundPath(map));

                lock (Thumbnail.Animations)
                {
                    Thumbnail.Animations.Clear();

                    // Check to see if the selected map is still the same.
                    // if it is, then we'll want to display it.
                    if (Container.SelectedMapsetIndex == MapsetIndex)
                    {
                        Thumbnail.Image = tex;
                        Thumbnail.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Thumbnail.Alpha, 1, 300));
                    }
                    // Otherwise dispose of the texture as it's no longer needed.
                    else
                        tex.Dispose();
                }
            }
            catch (Exception)
            {
                // ignored
            }
        });
    }
}