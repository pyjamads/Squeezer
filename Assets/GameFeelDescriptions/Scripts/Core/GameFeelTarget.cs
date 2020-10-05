namespace GameFeelDescriptions
{

    public enum GameFeelTarget
    {
        //Relative
        Self, //Executed on the caller
        //Other, //Executed on the opposing collider in a collision
        EditorValue, //No target (eg. TimeScaleEffect, and effects with direct references in Editor CameraBackgroundColorChangeEffect, MaterialColorChangeEffect)  

        //Tag
        Tag,

        //Component
        ComponentType,

        //List
        List
    }

    public static class GameFeelTargetExtensions
    {
        public static bool IsRelative(this GameFeelTarget target)
        {
            switch (target)
            {
                case GameFeelTarget.Self:
                //case GameFeelTarget.Other:
                case GameFeelTarget.EditorValue:
                    return true;
            }

            return false;
        }

        public static bool ShouldUpdateAtRuntime(this GameFeelTarget target)
        {
            switch (target)
            {
                case GameFeelTarget.Tag:
                case GameFeelTarget.ComponentType:
                //case GameFeelTarget.List:
                    return true;
            }

            return false;
        }
    }
}
