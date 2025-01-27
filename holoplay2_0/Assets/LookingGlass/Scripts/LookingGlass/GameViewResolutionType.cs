﻿using UnityEngine;

namespace LookingGlass {
    /// <summary>
    /// Describes what resolution the user's GameView(s) will match.
    /// During custom quilt settings, this will need to match the single-view resolution for an accurate quilt rendering.
    /// Otherwise, the calibration can be matched for a rough preview of the native resolution.
    /// </summary>
    public enum GameViewResolutionType {
        //TODO: Document these better
        NativeScreenSize = 0,
        QuiltSingleViewSize = 1
    }
}
