using UnityEngine;
using System.Collections;

public class NoteIdentifier : MonoBehaviour
{
    private string currentMic = "";
    private AudioClip audioClip;

    private void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            foreach (string deviceName in Microphone.devices)
            {
                Debug.Log($"Microphone: {deviceName}");
            }

            currentMic = Microphone.devices[0];

            StartRecording(currentMic);
        }
    }

    private void StartRecording(string microphoneName)
    {
        Microphone.Start(microphoneName, true, 4, 44100);
    }

    //private void StopRecording()
    //{

    //}

    //private IEnumerator Recording()
    //{
    //    StartRecording();
    //    yield return new WaitForSeconds(3f);
    //    StopRecording();
    //}

}
