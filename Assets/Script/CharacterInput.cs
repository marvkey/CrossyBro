using System;
using Proof;

namespace CrossyBro
{
    public class CharacterInput : Entity
    {
        private CharacterMovement m_CharacterMovement;

        // OnCreate is called once when the Entity that this script is attached to
        // is instantiated in the world at runtime
        void OnCreate()
        {
            m_CharacterMovement = GetScript<CharacterMovement>();
        }

        // OnUpdate is called once every frame while this script is active in the world
        void OnUpdate(float deltaTime)
        {
            if (Input.IsKeyPressed(KeyBoardKey.W))
                m_CharacterMovement.Jump(Transform.Forward);
            else if (Input.IsKeyPressed(KeyBoardKey.S))
                m_CharacterMovement.Jump(-Transform.Forward);
            else if (Input.IsKeyPressed(KeyBoardKey.A))
                m_CharacterMovement.Jump(-Transform.Right);
            else if (Input.IsKeyPressed(KeyBoardKey.D))
                m_CharacterMovement.Jump(Transform.Right);
            else if (Input.IsMouseButtonPressed(MouseButton.Button0))
                m_CharacterMovement.TurnLeft();
            else if (Input.IsMouseButtonPressed(MouseButton.Button1))
                m_CharacterMovement.TurnRight();
        }

        // OnPhysicsUpdate is called at a fixed timestep for physics-related logic
        void OnPhysicsUpdate(float fixedPhysicsDeltaTime)
        {
        }
    }
}