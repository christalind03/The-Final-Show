using UnityEngine;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour
{
    [Header("Animation Parameters")]
    [SerializeField] private float _animationDuration;

    [Header("References")]
    [SerializeField] private Slider _statusBar;

    private float _startTime;
    private float _startValue;
    private float _targetValue;
    private bool _isAnimating;

    // TODO: Document
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

    // TODO: Document
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
