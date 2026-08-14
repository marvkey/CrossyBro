using Proof;

namespace CrossyBro
{
    public class MainMenu  : Entity
    {
        void OnCreate()
        {
                 
        }

        void OnUpdate(float dt)
        {
            if(Input.IsKeyClicked(KeyBoardKey.P))
            {
                 World.OpenWorld(12011726453359119438);
            }

            if(Input.IsKeyClicked(KeyBoardKey.Q))
            {
                Application.Shutdown();
            }
        }
    }
}