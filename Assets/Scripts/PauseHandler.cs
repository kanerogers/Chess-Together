using UnityEngine;

public class PauseHandler : MonoBehaviour {
    public GameObject OVRCameraRig;
    public GameObject[] listOfObjectsToHideOnPause;
    private AudioSource[] audioSources;
    public bool keyboardShowing;
    private void OnEnable() {
        // try to find camera rig automatically, if it's not set in the Inspector..
        if (OVRCameraRig == null)
            OVRCameraRig = GameObject.Find("OVRCameraRig");

        // this subscribes to Oculus events, so when they fire our functions get called in this script
        OVRManager.HMDUnmounted += PauseGame;
        OVRManager.HMDMounted += UnPauseGame;
        OVRManager.VrFocusLost += PauseGame;
        OVRManager.VrFocusAcquired += UnPauseGame;
        OVRManager.InputFocusLost += PauseGame;
        OVRManager.InputFocusAcquired += UnPauseGame;
    }

    private void OnDisable() {
        // this unsubscribes from the Oculus events when they're no longer needed (when this object is disabled etc)
        OVRManager.HMDUnmounted -= PauseGame;
        OVRManager.HMDMounted -= UnPauseGame;
        OVRManager.VrFocusLost -= PauseGame;
        OVRManager.VrFocusAcquired -= UnPauseGame;
        OVRManager.InputFocusLost -= PauseGame;
        OVRManager.InputFocusAcquired -= UnPauseGame;
    }

    private void PauseGame() {
        if (Application.isEditor) return;
        if (keyboardShowing) return;

        // if we have objects to hide, let's hide them..
        for (int i = 0; i < listOfObjectsToHideOnPause.Length; i++) {
            listOfObjectsToHideOnPause[i].SetActive(false);
        }

        // pause time
        Time.timeScale = 0.0f;
    }

    private void UnPauseGame() {
        if (OVRManager.hasVrFocus && OVRManager.isHmdPresent && OVRManager.hasInputFocus) {
            // if we have objects to hide, let's hide them..
            for (int i = 0; i < listOfObjectsToHideOnPause.Length; i++) {
                listOfObjectsToHideOnPause[i].SetActive(true);
            }

            // restart time
            Time.timeScale = 1.0f;
        }
    }

}