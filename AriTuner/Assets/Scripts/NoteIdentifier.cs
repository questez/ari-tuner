using UnityEngine;

public class NoteIdentifier : MonoBehaviour
{
    [SerializeField] private int sampleRate = 44100;
    [SerializeField] private int recordingLength = 1;
    [SerializeField] private int sampleSize = 8192;

    private AudioClip microphoneClip;
    private string currentMic = "";

    private float[] soundSamplesAmplitude;
    
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
        if (!Microphone.IsRecording(currentMic)) return;

        int position = Microphone.GetPosition(currentMic);

        if (position < sampleSize) return;

        GetLatestSamples(position);

        Debug.Log($"Note frequency: {DetectFrequency(soundSamplesAmplitude, sampleRate)} Hz");
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
        int startPosition = position - sampleSize;

        if (startPosition >= 0)
        {
            microphoneClip.GetData(soundSamplesAmplitude, startPosition);
            return;
        }

        int samplesFromEnd = -startPosition;

        float[] endSamples = new float[samplesFromEnd];
        float[] startSamples = new float[sampleSize - samplesFromEnd];

        microphoneClip.GetData(endSamples, microphoneClip.samples - samplesFromEnd);

        microphoneClip.GetData(startSamples, 0);

        System.Array.Copy(endSamples, 0, soundSamplesAmplitude, 0, endSamples.Length);

        System.Array.Copy(startSamples, 0, soundSamplesAmplitude, endSamples.Length, startSamples.Length);
    }

    private float DetectFrequency(float[] samples, int sampleRate) // Autocorrelation algorithm
    {
        float bestCorrelation = 0f;
        int bestLag = 0;

        int minLag = Mathf.FloorToInt(sampleRate / 500f);
        int maxLag = Mathf.CeilToInt(sampleRate / 25f);

        for (int lag = minLag; lag <= maxLag; lag++)
        {
            float correlation = 0f;

            for (int i = 0; i < samples.Length - lag; i++)
            {
                correlation += samples[i] * samples[i + lag];
            }

            if (correlation > bestCorrelation)
            {
                bestCorrelation = correlation;
                bestLag = lag;
            }
        }

        if (bestLag == 0)
            return 0f;

        return (float)sampleRate / bestLag;
    }    
}
