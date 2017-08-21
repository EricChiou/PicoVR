// The MIT License (MIT)
//
// Copyright (c) 2014, Unity Technologies & Google, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
//   The above copyright notice and this permission notice shall be included in
//   all copies or substantial portions of the Software.
//
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//   THE SOFTWARE.

using UnityEngine;
using UnityEngine.EventSystems;

// An implementation of the BaseInputModule that uses the player's gaze and the magnet trigger
// as a raycast generator.  To use, attach to the scene's EventSystem object.  Set the Canvas
// object's Render Mode to World Space, and set its Event Camera to a (mono) camera that is
// controlled by a PicovrHead.  If you'd like gaze to work with 3D scene objects, add a
// PhysicsRaycaster to the gazing camera, and add a component that implements one of the Event
// interfaces (EventTrigger will work nicely).  The objects must have colliders too.
public class SightInputModule : BaseInputModule
{


    [Tooltip("Optional object to place at raycast intersections as a 3D cursor. " +
             "Be sure it is on a layer that raycasts will ignore.")]
    public GameObject cursor;
    public int trigger = 0;
    [HideInInspector]
    public float clickTime = 0.1f;  // Based on default time for a button to animate to Pressed.


    [HideInInspector]
    public Vector2 hotspot = new Vector2(0.5f, 0.5f);

    private PointerEventData pointerData;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bool ShouldActivateModule()
    {
        if (!base.ShouldActivateModule())
        {
            return false;
        }
        return PicoVRManager.SDK.VRModeEnabled;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void DeactivateModule()
    {
        base.DeactivateModule();
        if (pointerData != null)
        {
            HandlePendingClick();
            HandlePointerExitAndEnter(pointerData, null);
            pointerData = null;
        }
        eventSystem.SetSelectedGameObject(null, GetBaseEventData());
        if (cursor != null)
        {
            cursor.SetActive(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pointerId"></param>
    /// <returns></returns>
    public override bool IsPointerOverGameObject(int pointerId)
    {
        return pointerData != null && pointerData.pointerEnter != null;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void Process()
    {
        CastRayFromGaze();
        UpdateCurrentObject();
        PlaceCursor();
        HandlePendingClick();
        HandleTrigger();
    }

    /// <summary>
    /// 
    /// </summary>
    private void CastRayFromGaze()
    {
        if (pointerData == null)
        {
            pointerData = new PointerEventData(eventSystem);
        }
        pointerData.Reset();
        pointerData.position = new Vector2(hotspot.x * Screen.width, hotspot.y * Screen.height);
        eventSystem.RaycastAll(pointerData, m_RaycastResultCache);
        pointerData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
        m_RaycastResultCache.Clear();
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateCurrentObject()
    {
        // Send enter events and update the highlight.
        var go = pointerData.pointerCurrentRaycast.gameObject;
        HandlePointerExitAndEnter(pointerData, go);
        // Update the current selection, or clear if it is no longer the current object.
        var selected = ExecuteEvents.GetEventHandler<ISelectHandler>(go);
        if (selected == eventSystem.currentSelectedGameObject)
        {
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, GetBaseEventData(),
                                  ExecuteEvents.updateSelectedHandler);
        }
        else {
            eventSystem.SetSelectedGameObject(null, pointerData);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void PlaceCursor()
    {
        if (cursor == null)
            return;
        var go = pointerData.pointerCurrentRaycast.gameObject;
        cursor.SetActive(go != null);
        if (cursor.activeInHierarchy)
        {
            Camera cam = pointerData.enterEventCamera;
            // Note: rays through screen start at near clipping plane.
            float dist = pointerData.pointerCurrentRaycast.distance + cam.nearClipPlane;
            //float dist = pointerData.pointerCurrentRaycast.distance;
            cursor.transform.position = cam.transform.position + cam.transform.forward * dist;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void HandlePendingClick()
    {
        if (!pointerData.eligibleForClick)
        {
            return;
        }
        if (!PicoVRManager.SDK.picovrTriggered
            && Time.unscaledTime - pointerData.clickTime < clickTime)
        {
            return;
        }

        // Send pointer up and click events.
        ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerClickHandler);

        // Clear the click state.
        pointerData.pointerPress = null;
        pointerData.rawPointerPress = null;
        pointerData.eligibleForClick = false;
        pointerData.clickCount = 0;
    }

    /// <summary>
    /// 
    /// </summary>
    private void HandleTrigger()
    {
        if (!PicoVRManager.SDK.picovrTriggered)
        {
            return;
        }
        var go = pointerData.pointerCurrentRaycast.gameObject;
        //---------------------------------------------------------------

        if (go == null || ExecuteEvents.GetEventHandler<IPointerClickHandler>(go) == null)
        {
            return;
        }
        pointerData.pointerPress = ExecuteEvents.GetEventHandler<IPointerClickHandler>(go);
#if PicoInputMethod    
        GameObject target = pointerData.pointerPress;


        if (target.gameObject.GetComponent<inputFieldScript>() != null)
        {
            if (target.gameObject.GetComponent<inputFieldScript>().mTag == "Keyboard")
            {
                pointerData = null;
                trigger = 1;
                return;
            }
        }
#endif
        //------------------------------------------
        pointerData.pressPosition = pointerData.position;
        pointerData.pointerPressRaycast = pointerData.pointerCurrentRaycast;
        pointerData.pointerPress =
            ExecuteEvents.ExecuteHierarchy(go, pointerData, ExecuteEvents.pointerDownHandler)
            ?? ExecuteEvents.GetEventHandler<IPointerClickHandler>(go);

        pointerData.rawPointerPress = go;
        pointerData.eligibleForClick = true;
        pointerData.clickCount = 1;
        pointerData.clickTime = Time.unscaledTime;
    }

}

