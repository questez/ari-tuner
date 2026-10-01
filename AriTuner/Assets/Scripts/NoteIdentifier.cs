using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class NoteIdentifier : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

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
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            StartRecording(currentMic);
        }

        if (keyboard.ctrlKey.wasPressedThisFrame)
        {
            StopRecording(currentMic);
        }
    }

    private void StartRecording(string microphoneName)
    {
        Debug.Log("Start record!!!");

        if (Microphone.IsRecording(microphoneName))
        {
            Microphone.End(microphoneName);
        }
        
        audioClip =  Microphone.Start(microphoneName, true, 4, 44100);        
    }    

    private void StopRecording(string microphoneName)
    {
        Debug.Log("Stop record!!!");

        Microphone.End(microphoneName);
        
        audioSource.clip = audioClip;
        audioSource.Play();
    }

    //private IEnumerator Recording()
    //{
    //    StartRecording();
    //    yield return new WaitForSeconds(3f);
    //    StopRecording();
    //}

}
