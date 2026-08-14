using Proof;
using System;
using System.IO;
using System.Security.Cryptography;

namespace CrossyBro
{
    public class CharacterHUD  : Entity
    {
     
        public UIPanel PlayerDefaultHUD;
        public UIPanel EndScreenHUD;

        PlayerHUDComponent m_HUDComponent;
        Variable m_PlayerScoreVar;
        int m_PlayerScore =0;
        int m_HighScore = 0;
        public int Score => m_PlayerScore;
        private string filePath = "HighScore.txt";

        void OnCreate()
        {
            m_HUDComponent = GetComponent<PlayerHUDComponent>();
            m_PlayerScoreVar = m_HUDComponent.GetRegistryVariable("HUD", PlayerDefaultHUD, "Score");

            if(m_HUDComponent == null)
            {
                Log.Error($"{Name} CharacterHud Script needs a PlayerHUDComponent");
                return;
            }
        }

        void OnUpdate(float dt)
        { 

            ScoreHUD();
            RestartHUD();
        }

        bool appliedRestart = false;
        void RestartHUD()
        {
            if(appliedRestart)
                return;
            if(GetScript<CharacterMovement>().Dead == true)
            {
                appliedRestart = true;

                m_HUDComponent.RemovePanel("HUD",PlayerDefaultHUD);
                m_HUDComponent.PushPanel("HUD",EndScreenHUD,true);

                var finalScore = m_HUDComponent.GetRegistryVariable("HUD", EndScreenHUD, "Score");
                var highScore = m_HUDComponent.GetRegistryVariable("HUD", EndScreenHUD, "BestScore");
                SaveScore();

                if(finalScore != null)
                finalScore.SetData(m_PlayerScore.ToString("0"));

                if(highScore != null)
                highScore.SetData($"BEST {m_HighScore}");
            }
        }
        void ScoreHUD()
        {
            if(appliedRestart != false)return;
            int newScore = (int)this.WorldTransform.Location.x /(int) WorldData.GridSize;

            if(newScore > m_PlayerScore ||(m_PlayerScore ==0 && newScore == 0))
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
            if (m_PlayerScore > m_HighScore)
            {
                m_HighScore = m_PlayerScore;
                File.WriteAllText(filePath, m_HighScore.ToString());
                Log.Info("High score saved: " + m_HighScore);
            }
        }

        public void LoadHighScore()
        {
            if (File.Exists(filePath))
            {
                string text = File.ReadAllText(filePath);
                if (int.TryParse(text, out int savedScore))
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

    }
}