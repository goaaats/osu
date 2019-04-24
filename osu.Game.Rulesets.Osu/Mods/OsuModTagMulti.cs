// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Online.Websocket;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osu.Game.Rulesets.UI;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModTagMulti : Mod, IApplicableToDrawableRuleset<OsuHitObject>, IApplicableFailOverride, IApplicableToBeatmap<OsuHitObject>
    {
        public override string Name => "Tag";
        public override string Description => "Play a map in tag multiplayer!";
        public override string Acronym => "TM";

        public bool AllowFail => false;

        public override IconUsage Icon => FontAwesome.Solid.Globe;
        public override ModType Type => ModType.Fun;

        public override bool Ranked => false;
        public override double ScoreMultiplier => 1.0f;

        private DrawableOsuTagCursors tagCursors;

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            drawableRuleset.Overlays.Add(tagCursors = new DrawableOsuTagCursors(drawableRuleset.Playfield.HitObjectContainer, drawableRuleset.Beatmap));
        }

        public void ApplyToBeatmap(Beatmap<OsuHitObject> beatmap)
        {
            //beatmap.HitObjects.OfType<OsuHitObject>().ForEach(h => h.Column = availableColumns - 1 - h.Column);

            //beatmap.HitObjects = beatmap.HitObjects.Where(o => o.IsApplicableForLocalTagPlayer).ToList();

            // this works i guess
            for (int i = 0; i < beatmap.HitObjects.Count; i++)
                beatmap.HitObjects[i].Index = i;
        }

        /// <summary>
        /// Element for the Blinds mod drawing 2 black boxes covering the whole screen which resize inside a restricted area with some leniency.
        /// </summary>
        public class DrawableOsuTagCursors : Container
        {
            private readonly Beatmap<OsuHitObject> beatmap;
            private readonly float targetBreakMultiplier = 0;

            private readonly CompositeDrawable restrictTo;

            public DrawableOsuTagCursors(CompositeDrawable restrictTo, Beatmap<OsuHitObject> beatmap)
            {
                this.restrictTo = restrictTo;
                this.beatmap = beatmap;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                RelativeSizeAxes = Axes.Both;

                var cursors = new List<OsuCursor>();

                for (int i = 0; i < TagModManager.Instance.PlayersMax; i++)
                {
                    cursors.Add(new OsuCursor
                    {
                        Origin = Anchor.TopRight,
                        Colour = ColourInfo.SingleColour(new SRGBColour()
                        {
                            Linear = new Color4(183, 0, 0, 180)
                        })
                    });
                }

                Children = new List<Drawable>(cursors);
            }

            protected override void Update()
            {
                for (int i = 0; i < TagModManager.Instance.PlayersMax; i++)
                {
                    float containerX = Parent.ToLocalSpace(restrictTo.ScreenSpaceDrawQuad.TopLeft).X;
                    float containerY = Parent.ToLocalSpace(restrictTo.ScreenSpaceDrawQuad.TopLeft).Y;

                    var pos = TagModManager.Instance.PlayerLastPositionMap[i].Item2;

                    Children[i].X = containerX + pos.X;
                    Children[i].Y = containerY + pos.Y;
                }
            }

            protected override void LoadComplete()
            {
                const float break_open_early = 500;
                const float break_close_late = 250;

                base.LoadComplete();

                var firstObj = beatmap.HitObjects[0];
                var startDelay = firstObj.StartTime - firstObj.TimePreempt;

                using (BeginAbsoluteSequence(startDelay + break_close_late, true))
                    leaveBreak();

                foreach (var breakInfo in beatmap.Breaks)
                {
                    if (breakInfo.HasEffect)
                    {
                        using (BeginAbsoluteSequence(breakInfo.StartTime - break_open_early, true))
                        {
                            enterBreak();
                            using (BeginDelayedSequence(breakInfo.Duration + break_open_early + break_close_late, true))
                                leaveBreak();
                        }
                    }
                }
            }

            private void enterBreak() => this.TransformTo(nameof(targetBreakMultiplier), 1f, 1000, Easing.OutSine);

            private void leaveBreak() => this.TransformTo(nameof(targetBreakMultiplier), 0.85f, 2500, Easing.OutBounce);
        }
    }
}
