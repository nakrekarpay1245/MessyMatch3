using _Game.Scripts.Data;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

namespace _Game.Scripts._helpers
{
    /// <summary>
    /// Manages the game timer, including starting, stopping, adding extra time, and freezing the timer.
    /// </summary>
    public class TimeManager : MonoSingleton<TimeManager>
    {
        [Header("TimeManager Parameters")]
        [SerializeField, Tooltip("Reference to the game data containing level configuration.")]
        private GameData _gameData;

        private float _currentLevelTime;
        private bool _isTimerRunning;

        public UnityAction<float, float> OnTimerUpdated; // Event triggered when the timer is updated
        public UnityAction OnTimeFinished; // Event triggered when the time runs out

        private void Start()
        {
            StartTimer(_gameData.CurrentLevel.InitialTime);
        }

        /// <summary>
        /// Starts the timer with a specified duration.
        /// </summary>
        /// <param name="timeInSeconds">The time to start the timer with, in seconds.</param>
        public void StartTimer(float timeInSeconds)
        {
            _currentLevelTime = timeInSeconds;
            _isTimerRunning = true;

            OnTimerUpdated?.Invoke(_currentLevelTime, _gameData.CurrentLevel.CriticalTimeThreshold);

            ScheduleTimerUpdate();
        }

        /// <summary>
        /// Schedules the timer update at regular intervals.
        /// </summary>
        private void ScheduleTimerUpdate()
        {
            InvokeRepeating(nameof(UpdateTimer), _gameData.CurrentLevel.UpdateInterval, _gameData.CurrentLevel.UpdateInterval);
        }

        /// <summary>
        /// Updates the timer, reducing the remaining time and handling timer completion.
        /// </summary>
        private void UpdateTimer()
        {
            if (!_isTimerRunning) return;

            _currentLevelTime -= _gameData.CurrentLevel.UpdateInterval;

            if (_currentLevelTime <= 0)
            {
                HandleTimeExpired();
            }
            else
            {
                OnTimerUpdated?.Invoke(_currentLevelTime, _gameData.CurrentLevel.CriticalTimeThreshold);
            }
        }

        /// <summary>
        /// Handles the scenario when the time has expired.
        /// </summary>
        private void HandleTimeExpired()
        {
            _currentLevelTime = 0;
            _isTimerRunning = false;
            CancelInvoke(nameof(UpdateTimer));
            OnTimeFinished?.Invoke();
        }

        /// <summary>
        /// Adds extra time to the current timer.
        /// </summary>
        /// <param name="extraTimeInSeconds">The additional time to add, in seconds.</param>
        public void AddExtraTime(float extraTimeInSeconds)
        {
            _currentLevelTime += extraTimeInSeconds;

            if (!_isTimerRunning)
            {
                ResumeTimer();
            }

            OnTimerUpdated?.Invoke(_currentLevelTime, _gameData.CurrentLevel.CriticalTimeThreshold);
        }

        /// <summary>
        /// Resumes the timer if it was not running.
        /// </summary>
        private void ResumeTimer()
        {
            _isTimerRunning = true;
            ScheduleTimerUpdate();
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void StopTimer()
        {
            _isTimerRunning = false;
            CancelInvoke(nameof(UpdateTimer));
        }

        /// <summary>
        /// Resets the timer to the initial level time.
        /// </summary>
        public void ResetTimer()
        {
            StopTimer();
            StartTimer(_gameData.CurrentLevel.InitialTime);
        }

        /// <summary>
        /// Sets the time scale, controlling the flow of time in the game.
        /// </summary>
        /// <param name="scale">The scale at which time passes. 1 is normal speed.</param>
        public void SetTimeScale(float scale)
        {
            Time.timeScale = scale;
        }

        /// <summary>
        /// Freezes the timer for a specified duration. After the duration, the timer resumes.
        /// </summary>
        /// <param name="duration">The duration for which to freeze the timer, in seconds.</param>
        public void FreezeTimer(float duration)
        {
            if (!_isTimerRunning) return;

            StopTimer(); // Pause the timer

            // Use DOTween to resume the timer after the specified duration
            DOVirtual.DelayedCall(duration, () =>
            {
                if (_currentLevelTime > 0)
                {
                    ResumeTimer();
                }
            });
        }
    }
}