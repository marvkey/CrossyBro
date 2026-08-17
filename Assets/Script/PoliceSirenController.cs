using System;
using System.Text;
using Proof;

namespace CrossyBro
{
    public class PoliceSirenController : Entity
    {
        public Material Red;
        public Material Blue;

        public float FlashSpeed = 0.2f;
        public float EmissionStrength = 5.0f;

        private PbrSurfaceMaterial m_Red;
        private PbrSurfaceMaterial m_Blue;

        private float m_Timer = 0.0f;
        private bool m_RedActive = true;

        void OnCreate()
        {
            m_Red = new PbrSurfaceMaterial(Red);
            m_Blue = new PbrSurfaceMaterial(Blue);

            m_Red.SetEmission(EmissionStrength);
            m_Blue.SetEmission(0.0f);
        }

        void OnUpdate(float timeStep)
        {
            m_Timer += timeStep;

            if (m_Timer < FlashSpeed)
                return;

            m_Timer = 0.0f;
            m_RedActive = !m_RedActive;

            m_Red.SetEmission(m_RedActive ? EmissionStrength : 0.0f);
            m_Blue.SetEmission(m_RedActive ? 0.0f : EmissionStrength);
        }
    }
}