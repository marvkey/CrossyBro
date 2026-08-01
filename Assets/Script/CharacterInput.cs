using System;
using Proof;

namespace CrossyBro
{
    public class CharacterInput : Entity
    {
        private CharacterMovement m_CharacterMovement;

        public InputAction MoveAction;
        public InputAction TurnAction;

        PlayerInputComponent m_PlayerInputComponent;

        // OnCreate is called once when the Entity that this script is attached to
        // is instantiated in the world at runtime
        void OnCreate()
        {
            m_CharacterMovement = GetScript<CharacterMovement>();
            m_PlayerInputComponent = GetComponent<PlayerInputComponent>();
            if (m_PlayerInputComponent == null)
            {
                Log.Error($"{Name} PlayerInput Script needs a PlayerInputComponent");
                return;
            }
            BindInputAction(m_PlayerInputComponent,MoveAction,InteractionEvent.Triggered,Move);
            BindInputAction(m_PlayerInputComponent,TurnAction,InteractionEvent.Triggered,Turn);
        }

        // OnUpdate is called once every frame while this script is active in the world
        void OnUpdate(float deltaTime)
        {
/*
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
                */
        }

        // OnPhysicsUpdate is called at a fixed timestep for physics-related logic
        void OnPhysicsUpdate(float fixedPhysicsDeltaTime)
        {
        }

        void Move(InputActionOutput actionOutput)
        {
            Vector2 output = actionOutput.Get<Vector2>();

            if (Mathf.Abs(output.x) > Mathf.Abs(output.y))
            {
                if (output.x > 0.0f)
                    m_CharacterMovement.Jump(Transform.Right);
                else if (output.x < 0.0f)
                    m_CharacterMovement.Jump(-Transform.Right);
            }
            else
            {
                if (output.y > 0.0f)
                    m_CharacterMovement.Jump(Transform.Forward);
                else if (output.y < 0.0f)
                    m_CharacterMovement.Jump(-Transform.Forward);
            }
        }

        void Turn(InputActionOutput actionOutput)
        {
            float output = actionOutput.Get<float>();

            if (output < 0.0f)
                m_CharacterMovement.TurnLeft();
            else if (output > 0.0f)
                m_CharacterMovement.TurnRight();
        }

    }
}