﻿using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.API.Maps;
using Quaver.Main;

namespace Quaver.States.Gameplay.HitObjects
{
    internal abstract class HitObject
    {
        /// <summary>
        ///     The info of this particular HitObject from the map file.
        /// </summary>
        internal HitObjectInfo Info { get; }

        /// <summary>
        ///     The true start time of the object.
        /// </summary>
        internal float TrueStartTime { get; set; }

        /// <summary>
        ///     The true end time of the object.
        /// </summary>
        internal float TrueEndTime { get; set; }
        
        /// <summary>
        ///     The list of possible beat snaps.
        /// </summary>
        private static int[] BeatSnaps = { 48, 24, 16, 12, 8, 6, 4, 3 };
        
        /// <summary>
        ///     The beat snap index
        ///     (See: BeatSnaps array)
        /// </summary>
        internal int SnapIndex { get; set; }
        
        /// <summary>
        ///     Initializes the HitObject's sprite.
        /// </summary>
        /// <param name="playfield"></param>
        internal abstract void InitializeSprite(IGameplayPlayfield playfield);

        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="info"></param>
        internal HitObject(HitObjectInfo info)
        {
            Info = info;

            TrueStartTime = Info.StartTime;
            TrueEndTime = Info.EndTime;
        }

        /// <summary>
        ///     Gets the timing point this object is in range of.
        /// </summary>
        /// <returns></returns>
        internal TimingPointInfo GetTimingPoint(List<TimingPointInfo> timingPoints)
        {
            // If the start time of the object is greater than the last timing point, then return the last 
            // point.
            if (Info.StartTime >= timingPoints.Last().StartTime)
                return timingPoints.Last();

            // Otherwise loop through all the timing points to find the correct one.
            return timingPoints.Where((t, i) => Info.StartTime < timingPoints[i + 1].StartTime).FirstOrDefault();
        }

        /// <summary>
        ///     Returns color of note beatsnap
        /// </summary>
        /// <param name="timingPoint"></param>
        /// <returns></returns>
        internal int GetBeatSnap(TimingPointInfo timingPoint)
        {
            // Add 2ms offset buffer space to offset and get beat length
            var pos = Info.StartTime - timingPoint.StartTime + 2;
            var beatlength = 60000 / timingPoint.Bpm;

            // subtract pos until it's less than beat length. multiple loops for efficiency
            while (pos >= beatlength * (1 << 16)) pos -= beatlength * (1 << 16);           
            while (pos >= beatlength * (1 << 12)) pos -= beatlength * (1 << 12);
            while (pos >= beatlength * (1 << 8))  pos -= beatlength * (1 << 8);
            while (pos >= beatlength * (1 << 4))  pos -= beatlength * (1 << 4);            
            while (pos >= beatlength)  pos -= beatlength;

            // Calculate Note's snap index
            var index = (int)(Math.Floor(48 * pos / beatlength));

            // Return Color of snap index
            for (var i=0; i< 8; i++)
            {
                if (index % BeatSnaps[i] == 0)
                {
                    return i;
                }
            }

            // If it's not snapped to 1/16 or less, return 1/48 snap color
            return 8;
        }

        /// <summary>
        ///     Destroys the HitObject
        /// </summary>
        internal abstract void Destroy();
    }
}