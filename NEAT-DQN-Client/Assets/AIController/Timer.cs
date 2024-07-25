using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _timer;
    [SerializeField]
    private TMP_Text _totalTimer;
    [SerializeField]
    private TMP_Text _generations;

    private bool _on = false;
    public float _currentTime = 0;
    public float _currentTotalTime = 0;
    public float _currentGeneration = 0;

    private int h;
    private int m;
    private int s;

    private void FixedUpdate()
    {
        if (_on)
        {
            _currentTime += Time.fixedDeltaTime;
            _timer.text = "Time: " + ((int)_currentTime).ToString() + " s";

            _currentTotalTime += Time.fixedDeltaTime;
            h = (int)_currentTotalTime / 3600;
            m = ((int)_currentTotalTime % 3600) / 60;
            s = (int)_currentTotalTime % 60;
            _totalTimer.text = "Total time: " + h.ToString() + ":" + m.ToString() + ":" + s.ToString();
        }
    }

    public void on() 
    {
        _on = true;
    }

    public void off() 
    {
        _on = false;
        _currentTime = 0;  
        _timer.text = "";
        
    }

    public void nextGeneration()
    {
        _currentGeneration++;
        _generations.text = "Generation: " + _currentGeneration;
    }

    public void restartTimers()
    {
        off();
        _currentTotalTime = 0;
        _totalTimer.text = "";
        _currentGeneration = 1;
        _generations.text = "";
    }
}
