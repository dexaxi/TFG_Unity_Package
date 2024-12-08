namespace DUJAL.Systems.Loading
{
    public class SceneIndex
    {
        public static int Count => (int)SceneType.Count;
        public static SceneIndex FirstScene = SceneType.First_Scene;
        public static SceneIndex LastScene = SceneType.Last_Scene;

        public static SceneIndex NoScene = SceneType.No_Scene;
        public static SceneIndex LoadingScreen = SceneType.Loading_Screen;
        public static SceneIndex ExampleScene1 = SceneType.Example_Scene_1;
        public static SceneIndex ExampleScene2 = SceneType.Example_Scene_2;
        public static SceneIndex ExampleScene3 = SceneType.Example_Scene_3;
        public static SceneIndex ExampleScene4 = SceneType.Example_Scene_4;

        public SceneType Value { get; private set; } = SceneType.No_Scene;
        public string String => GetSceneString();

        public SceneIndex(SceneType Scene)
        {
            Value = Scene;
        }

        public SceneIndex(int Scene)
        {
            Value = (SceneType) Scene;
        }

        // Scene enum. Values here should match the build index. 
        public enum SceneType
        {
            No_Scene = -1,
            Loading_Screen = 0,
            First_Scene = 1,
            
            Example_Scene_1 = First_Scene,
            Example_Scene_2,
            Example_Scene_3,
            Example_Scene_4,

            Last_Scene = Example_Scene_4,

            Count,
        }

        // New scene values should be added to this function too.
        private string GetSceneString() 
        {
            return Value switch
            {
                SceneType.Loading_Screen => "Loading_Sceen",
                SceneType.Example_Scene_1 => "Example_Scene_1",
                SceneType.Example_Scene_2 => "Example_Scene_2",
                SceneType.Example_Scene_3 => "Example_Scene_3",
                SceneType.Example_Scene_4 => "Example_Scene_4",
                _ => "No_Scene",
            };
        }

        public static implicit operator int(SceneIndex Scene)
        {
            return (int) Scene.Value;
        }

        public static implicit operator SceneIndex(int Scene)
        {
            return new SceneIndex(Scene);
        }
        
        public static implicit operator SceneType(SceneIndex Scene)
        {
            return Scene.Value;
        }

        public static implicit operator SceneIndex(SceneType Scene)
        {
            return new SceneIndex(Scene);
        }
    }


}
