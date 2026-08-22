using Proof;
using System;
using System.IO;
using System.Security.Cryptography;

namespace CrossyBro
{
    public class CharacterHUD : Entity
    {
        public UIPanel PlayerDefaultHUD;
        public UIPanel EndScreenHUD;

        public Material VignetteMaterial;

        public float DeathEffectDuration = 0.45f;

        public float DeathCenterBrightness = 0.8f;
        public float DeathGrayscaleStrength = 0.8f;
        public float DeathVignetteRadius = 0.35f;
        public float DeathVignetteSoftness = 0.35f;
        public float DeathVignetteStrength = 0.9f;

        PlayerHUDComponent m_HUDComponent;
        Variable m_PlayerScoreVar;

        int m_PlayerScore = 0;
        int m_HighScore = 0;

        public int Score => m_PlayerScore;

        private string filePath = "HighScore.txt";

        bool appliedRestart = false;

        bool m_DeathAnimationPlaying = false;
        float m_DeathAnimationTime = 0.0f;


        void OnCreate()
        {
            m_HUDComponent = GetComponent<PlayerHUDComponent>();
            m_PlayerScoreVar = m_HUDComponent.GetRegistryVariable("HUD", PlayerDefaultHUD, "Score");

            if(m_HUDComponent == null)
            {
                Log.Error($"{Name} CharacterHud Script needs a PlayerHUDComponent");
                return;
            }

            if(VignetteMaterial != null)
            {
                SetVignetteEnabled(false);

                SetVignetteFloat("u_MaterialUniform.CenterBrightness", 1.0f);
                SetVignetteFloat("u_MaterialUniform.GrayscaleStrength", 0.0f);
                SetVignetteFloat("u_MaterialUniform.VignetteRadius", 0.0f);
                SetVignetteFloat("u_MaterialUniform.VignetteSoftness", 0.0f);
                SetVignetteFloat("u_MaterialUniform.VignetteStrength", 0.0f);
            }
        }


        void OnUpdate(float dt)
        {
            ScoreHUD();
            RestartHUD(dt);
        }


        void RestartHUD(float dt)
        {
            if(appliedRestart)
                return;

            if(!m_DeathAnimationPlaying)
            {
                if(GetScript<CharacterMovement>().Dead == false)
                    return;

                StartDeathAnimation();
            }

            UpdateDeathAnimation(dt);
        }


        void StartDeathAnimation()
        {
            m_DeathAnimationPlaying = true;
            m_DeathAnimationTime = 0.0f;

            /*
                IMPORTANT:

                Once this panel is removed, m_PlayerScoreVar's HUD storage
                no longer exists.

                ScoreHUD() checks m_DeathAnimationPlaying before calling
                m_PlayerScoreVar.SetData(), so we must set
                m_DeathAnimationPlaying = true BEFORE removing this panel.
            */
            m_HUDComponent.RemovePanel("HUD", PlayerDefaultHUD);

            if(VignetteMaterial == null)
            {
                FinishDeathAnimation();
                return;
            }

            /*
                Start the Game Over effect from basically nothing.

                The shader is enabled, but the visible values start neutral.
                They will animate toward the final Game Over values.
            */
            SetVignetteEnabled(true);

            SetVignetteFloat("u_MaterialUniform.CenterBrightness", 1.0f);
            SetVignetteFloat("u_MaterialUniform.GrayscaleStrength", 0.0f);

            SetVignetteFloat("u_MaterialUniform.VignetteRadius", 0.0f);
            SetVignetteFloat("u_MaterialUniform.VignetteSoftness", 0.0f);
            SetVignetteFloat("u_MaterialUniform.VignetteStrength", 0.0f);
        }


        void UpdateDeathAnimation(float dt)
        {
            if(!m_DeathAnimationPlaying)
                return;

            if(VignetteMaterial == null)
            {
                FinishDeathAnimation();
                return;
            }

            m_DeathAnimationTime += dt;

            float t = m_DeathAnimationTime / DeathEffectDuration;

            if(t > 1.0f)
                t = 1.0f;

            /*
                SmoothStep

                Gives us:

                    slow -> fast -> slow

                instead of a completely linear animation.
            */
            float smoothT = t * t * (3.0f - 2.0f * t);

            float centerBrightness = Lerp(1.0f, DeathCenterBrightness, smoothT);
            float grayscaleStrength = Lerp(0.0f, DeathGrayscaleStrength, smoothT);

            float vignetteRadius = Lerp(0.0f, DeathVignetteRadius, smoothT);
            float vignetteSoftness = Lerp(0.0f, DeathVignetteSoftness, smoothT);
            float vignetteStrength = Lerp(0.0f, DeathVignetteStrength, smoothT);

            SetVignetteFloat("u_MaterialUniform.CenterBrightness", centerBrightness);
            SetVignetteFloat("u_MaterialUniform.GrayscaleStrength", grayscaleStrength);

            SetVignetteFloat("u_MaterialUniform.VignetteRadius", vignetteRadius);
            SetVignetteFloat("u_MaterialUniform.VignetteSoftness", vignetteSoftness);
            SetVignetteFloat("u_MaterialUniform.VignetteStrength", vignetteStrength);

            if(t >= 1.0f)
                FinishDeathAnimation();
        }


        void FinishDeathAnimation()
        {
            m_DeathAnimationPlaying = false;
            appliedRestart = true;

            /*
                Force the exact final material values.
            */
            if(VignetteMaterial != null)
            {
                SetVignetteEnabled(true);

                SetVignetteFloat("u_MaterialUniform.CenterBrightness", DeathCenterBrightness);
                SetVignetteFloat("u_MaterialUniform.GrayscaleStrength", DeathGrayscaleStrength);

                SetVignetteFloat("u_MaterialUniform.VignetteRadius", DeathVignetteRadius);
                SetVignetteFloat("u_MaterialUniform.VignetteSoftness", DeathVignetteSoftness);
                SetVignetteFloat("u_MaterialUniform.VignetteStrength", DeathVignetteStrength);
            }

            /*
                Only show the End Screen AFTER the vignette animation finishes.
            */
            m_HUDComponent.PushPanel("HUD", EndScreenHUD, true);

            var finalScore = m_HUDComponent.GetRegistryVariable("HUD", EndScreenHUD, "Score");
            var highScore = m_HUDComponent.GetRegistryVariable("HUD", EndScreenHUD, "BestScore");

            SaveScore();

            if(finalScore != null)
                finalScore.SetData(m_PlayerScore.ToString("0"));

            if(highScore != null)
                highScore.SetData($"BEST {m_HighScore}");
        }


        void ScoreHUD()
        {
            /*
                IMPORTANT:

                When the death animation begins, PlayerDefaultHUD gets removed.

                m_PlayerScoreVar belongs to that panel, so its storage is gone
                after RemovePanel().

                Do NOT call SetData() on it while the death animation is running.
            */
            if(appliedRestart || m_DeathAnimationPlaying)
                return;

            int newScore = (int)this.WorldTransform.Location.x / (int)WorldData.GridSize;

            if(newScore > m_PlayerScore || (m_PlayerScore == 0 && newScore == 0))
            {
                m_PlayerScore = newScore;
            }

            if(m_PlayerScoreVar != null)
            {
                m_PlayerScoreVar.SetData(m_PlayerScore.ToString("0"));
            }
        }


        void SaveScore()
        {
            if(m_PlayerScore > m_HighScore)
            {
                m_HighScore = m_PlayerScore;

                File.WriteAllText(filePath, m_HighScore.ToString());

                Log.Info("High score saved: " + m_HighScore);
            }
        }


        public void LoadHighScore()
        {
            if(File.Exists(filePath))
            {
                string text = File.ReadAllText(filePath);

                if(int.TryParse(text, out int savedScore))
                {
                    m_HighScore = savedScore;
                    Log.Info("High score loaded: " + m_HighScore);
                }
                else
                {
                    Log.Warn("Failed to parse high score from file.");
                    m_HighScore = 0;
                }
            }
            else
            {
                Log.Warn("No high score file found. Starting fresh.");
                m_HighScore = 0;
            }
        }


        float Lerp(float start, float end, float t)
        {
            return start + (end - start) * t;
        }


        void SetVignetteEnabled(bool enabled)
        {
            VignetteMaterial.SetInput("u_MaterialUniform.Enabled", enabled);
        }


        void SetVignetteFloat(string name, float value)
        {
            VignetteMaterial.SetInput(name, value);
        }
    }
}