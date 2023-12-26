using System;
namespace Bit.Core
{
    public static class State
    {
        public static bool IsAuthed { get; set; } = false;

        public static bool IsLocked { get; set; } = false;
    }
}

