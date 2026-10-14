using UnityEngine;

public class NoteIdentifier : MonoBehaviour
{
    [SerializeField] private int sampleRate = 44100;
    [SerializeField] private int recordingLength = 1;
    [SerializeField] private int sampleSize = 8192;

    private AudioClip microphoneClip;
    private string currentMic = "";

    private float[] soundSamplesAmplitude; // each array's element carries audio signal amplitude of each sample

    private bool initialBufferReady = false;

    private void OnEnable()
    {
        if (Microphone.devices.Length > 0)
        {
            currentMic = Microphone.devices[0];

            Debug.Log($"Using microphone: {currentMic}");            

            StartRecording(currentMic);

            soundSamplesAmplitude = new float[sampleSize];
        }
        else
        {
            Debug.LogError("Microphone was not found!");
        }
    }

    private void OnDisable()
    {
        StopRecording(currentMic);
    }

    private void Update()
    {
        if (string.IsNullOrEmpty(currentMic)) return;

        if (!Microphone.IsRecording(currentMic)) return;

        if (microphoneClip == null || soundSamplesAmplitude == null) return;

        int position = Microphone.GetPosition(currentMic);

        if (position < 0) return;


        if (!initialBufferReady)
        {
            if (position < sampleSize)
                return;

            initialBufferReady = true;
        }

        GetLatestSamples(position);

        float frequency = DetectFrequency(soundSamplesAmplitude, sampleRate);

        if (frequency > 0f)
        {
            Debug.Log($"Note frequency: {frequency:F2} Hz");
        }
    }

    private void StartRecording(string microphoneName)
    {
        if (Microphone.IsRecording(microphoneName))
        {
            Microphone.End(microphoneName);
        }
        
        microphoneClip =  Microphone.Start(microphoneName, true, recordingLength, sampleRate);

        Debug.Log("Start record!!!");
    }    

    private void StopRecording(string microphoneName)
    {
        Microphone.End(microphoneName);

        Debug.Log("Stop record!!!");
    }

    private void GetLatestSamples(int position)
    {
        if (microphoneClip == null)
            return;

        int clipLength = microphoneClip.samples;

        if (clipLength < sampleSize)
            return;

        int startPosition = position - sampleSize;

        if (startPosition < 0)
        {
            startPosition += clipLength;
        }

        bool success = microphoneClip.GetData(
            soundSamplesAmplitude,
            startPosition
        );

        if (!success)
        {
            Debug.LogWarning("Failed to get audio samples!");
        }
    }

    private float DetectFrequency(float[] samples, int sampleRate) // Autocorrelation algorithm
    {
        float mean = 0f;

        for (int i = 0; i < samples.Length; i++)
        {
            mean += samples[i];
        }

        mean /= samples.Length;


        float squareSum = 0f;

        for (int i = 0; i < samples.Length; i++)
        {
            float value = samples[i] - mean;
            squareSum += value * value;
        }

        float rms = Mathf.Sqrt(squareSum / samples.Length); // RMS (Root Mean Square)
        
        if (rms < 0.01f)
            return 0f;


        int minLag = Mathf.FloorToInt(sampleRate / 500f);
        int maxLag = Mathf.CeilToInt(sampleRate / 25f);

        float[] correlations = new float[maxLag + 2];

        for (int lag = minLag; lag <= maxLag; lag++)
        {
            float correlation = 0f;
            float energyA = 0f;
            float energyB = 0f;

            for (int i = 0; i < samples.Length - lag; i++)
            {
                float a = samples[i] - mean;
                float b = samples[i + lag] - mean;

                correlation += a * b;
                energyA += a * a;
                energyB += b * b;
            }

            float denominator = Mathf.Sqrt(energyA * energyB);

            if (denominator > 0f)
                correlations[lag] = correlation / denominator;
        }
        
        const float correlationThreshold = 0.65f;

        for (int lag = minLag + 1; lag < maxLag; lag++)
        {
            bool isLocalPeak =
                correlations[lag] >= correlations[lag - 1] &&
                correlations[lag] > correlations[lag + 1];

            if (isLocalPeak &&
                correlations[lag] >= correlationThreshold)
            {
                return (float)sampleRate / lag;
            }
        }        

        return 0f;
    }    
}
