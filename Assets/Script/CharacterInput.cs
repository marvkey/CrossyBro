using System;
using Proof;

namespace CrossyBro
{
    public class CharacterInput : Entity
    {
        private CharacterMovement m_CharacterMovement;

        public InputAction MoveAction;
        public InputAction TurnAction;
        public InputAction RotateAction;

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
            BindInputAction(m_PlayerInputComponent,RotateAction,InteractionEvent.Triggered,Rotate);
            //BindInputAction(m_PlayerInputComponent,TurnAction,InteractionEvent.Triggered,Turn);
            Mouse.SetCursorMode(MouseCursorMode.Locked);
        }

        // OnUpdate is called once every frame while this script is active in the world
        void OnUpdate(float deltaTime)
        {
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
                    m_CharacterMovement.Jump(m_CharacterMovement.Transform.Right);
                else if (output.x < 0.0f)
                    m_CharacterMovement.Jump(-m_CharacterMovement.Transform.Right);
            }
            else
            {
                if (output.y > 0.0f)
                    m_CharacterMovement.Jump(m_CharacterMovement.Transform.Forward);
                else if (output.y < 0.0f)
                    m_CharacterMovement.Jump(-m_CharacterMovement.Transform.Forward);
            }
        }

        void Turn(InputActionOutput actionOutput)
        {
            /*
            float output = actionOutput.Get<float>();

            if (output < 0.0f)
                m_CharacterMovement.TurnLeft();
            else if (output > 0.0f)
                m_CharacterMovement.TurnRight();
                */
        }

        void Rotate(InputActionOutput actionOutput)
        {
            if (m_CharacterMovement != null)
            {
                m_CharacterMovement.Rotate(actionOutput.Get<Vector2>());
            }
        }

    }
}