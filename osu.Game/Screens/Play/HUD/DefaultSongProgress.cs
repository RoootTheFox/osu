﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class DefaultSongProgress : SongProgress
    {
        private const float bottom_bar_height = 5;
        private const float graph_height = SquareGraph.Column.WIDTH * 6;
        private const float handle_height = 18;

        private static readonly Vector2 handle_size = new Vector2(10, handle_height);

        private const float transition_duration = 200;

        private readonly DefaultSongProgressBar bar;
        private readonly DefaultSongProgressGraph graph;
        private readonly SongProgressInfo info;

        [SettingSource("Show difficulty graph", "Whether a graph displaying difficulty throughout the beatmap should be shown")]
        public Bindable<bool> ShowGraph { get; } = new BindableBool(true);

        [Resolved]
        private Player? player { get; set; }

        public DefaultSongProgress()
        {
            RelativeSizeAxes = Axes.X;
            Anchor = Anchor.BottomRight;
            Origin = Anchor.BottomRight;

            Children = new Drawable[]
            {
                info = new SongProgressInfo
                {
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                },
                graph = new DefaultSongProgressGraph
                {
                    RelativeSizeAxes = Axes.X,
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    Height = graph_height,
                    Margin = new MarginPadding { Bottom = bottom_bar_height },
                },
                bar = new DefaultSongProgressBar(bottom_bar_height, graph_height, handle_size)
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    OnSeek = time => player?.Seek(time),
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            graph.FillColour = bar.FillColour = colours.BlueLighter;
        }

        protected override void LoadComplete()
        {
            Interactive.BindValueChanged(_ => updateBarVisibility(), true);
            ShowGraph.BindValueChanged(_ => updateGraphVisibility(), true);

            base.LoadComplete();
        }

        protected override void UpdateObjects(IEnumerable<HitObject> objects)
        {
            graph.Objects = objects;

            info.StartTime = FirstHitTime;
            info.EndTime = LastHitTime;
            bar.StartTime = FirstHitTime;
            bar.EndTime = LastHitTime;
        }

        protected override void UpdateProgress(double progress, bool isIntro)
        {
            bar.CurrentTime = GameplayClock.CurrentTime;

            if (isIntro)
                graph.Progress = 0;
            else
                graph.Progress = (int)(graph.ColumnCount * progress);
        }

        protected override void Update()
        {
            base.Update();
            Height = bottom_bar_height + graph_height + handle_size.Y + info.Height - graph.Y;
        }

        private void updateBarVisibility()
        {
            bar.Interactive = Interactive.Value;

            updateInfoMargin();
        }

        private void updateGraphVisibility()
        {
            float barHeight = bottom_bar_height + handle_size.Y;

            bar.ResizeHeightTo(ShowGraph.Value ? barHeight + graph_height : barHeight, transition_duration, Easing.In);
            graph.FadeTo(ShowGraph.Value ? 1 : 0, transition_duration, Easing.In);

            updateInfoMargin();
        }

        private void updateInfoMargin()
        {
            float finalMargin = bottom_bar_height + (Interactive.Value ? handle_size.Y : 0) + (ShowGraph.Value ? graph_height : 0);
            info.TransformTo(nameof(info.Margin), new MarginPadding { Bottom = finalMargin }, transition_duration, Easing.In);
        }
    }
}
