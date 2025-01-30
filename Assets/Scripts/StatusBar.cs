using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a status bar interface element that smoothly animates between values.
/// </summary>
public class StatusBar : AbstractBillboard
{
    [Header("Animation Parameters")]
    [SerializeField] private float _animationDuration;

    [Header("References")]
    [SerializeField] private Slider _statusBar;

    private float _startTime;
    private float _startValue;
    private float _targetValue;
    private bool _isAnimating;

    /// <summary>
    /// Updates the status bar animation by calculating the progress and adjusting the slider value.
    /// </summary>
    private void Update()
    {
        if (_isAnimating)
        {
            float elapsedTime = Time.time - _startTime;
            float animationProgress = Mathf.Clamp01(elapsedTime / _animationDuration);

            // Ease the slider's value
            _statusBar.value = Mathf.Lerp(_startValue, _targetValue, animationProgress);

            // If the animation is complete, reset the boolean value
            if (animationProgress >= 1f)
            {
                _isAnimating = false;
            }
        }
    }

    /// <summary>
    /// Refreshes the status bar with updated start and target values and starts the animation.
    /// </summary>
    /// <param name="baseValue">The maximum value of the status bar.</param>
    /// <param name="currentValue">The current value of the status bar.</param>
    public void Refresh(float baseValue, float currentValue)
    {
        if (_statusBar != null)
        {
            _startTime = Time.time;
            _startValue = _statusBar.value;
            _targetValue = currentValue / baseValue;
            _isAnimating = true;
        }
    }
}
