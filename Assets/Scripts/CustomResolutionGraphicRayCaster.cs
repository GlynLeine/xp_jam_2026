using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CustomResolutionGraphicRayCaster : GraphicRaycaster
{
    public Canvas canvas;
    [NonSerialized] private List<Graphic> m_raycastResults = new List<Graphic>();

    /// <summary>
    /// A version of Display.RelativeMouseAt that scales the position when the main display has a different rendering resolution to the system resolution.
    /// By default, the mouse position is relative to the main render area, we need to adjust this so it is relative to the system resolution
    /// in order to correctly determine the position on other displays.
    /// </summary>
    /// <returns></returns>
    public static Vector3 RelativeMouseAtScaled(Vector2 position, int displayIndex)
    {
#if !UNITY_EDITOR && !UNITY_WSA
            // For most platforms, if the main display is not the same resolution as the system then we will have to scale the mouse position. (case 1141732)
            var display = Display.main;
#if ENABLE_INPUT_SYSTEM && PACKAGE_INPUTSYSTEM
            // With the new input system, passed positions are always relative to a surface and scaled accordingly to the rendering resolution.

            // If not in fullscreen, assume UaaL multi-view multi-screen multi-touch scenario, where the position is already in the correct scaled coordinates for the displayIndex
            if (!Screen.fullScreen)
            {
                return new Vector3(position.x, position.y, displayIndex);
            }

            // Otherwise, in full screen, we add some padding if rendering and system resolution differs, as for other platforms' main display. (So behavior is unchanged for Android main display, untested for non-main displays)
            if (displayIndex >= Display.displays.Length)
                displayIndex = 0; // use position relative to first display if displayIndex is out of bounds
            display = Display.displays[displayIndex];
#endif
            if (display.renderingWidth != display.systemWidth || display.renderingHeight != display.systemHeight)
            {
                // The system will add padding when in full-screen and using a non-native aspect ratio. (case UUM-7893)
                // For example Rendering 1920x1080 with a systeem resolution of 3440x1440 would create black bars on each side that are 330 pixels wide.
                // we need to account for this or it will offset our coordinates when we are not on the main display.
                var systemAspectRatio = display.systemWidth / (float)display.systemHeight;

                var sizePlusPadding = new Vector2(display.renderingWidth, display.renderingHeight);
                var padding = Vector2.zero;
                if (Screen.fullScreen)
                {
                    var aspectRatio =
 Screen.width / (float)Screen.height; // This assumes aspectRatio is the same for all displays
                    if (display.systemHeight * aspectRatio < display.systemWidth)
                    {
                        // Horizontal padding
                        sizePlusPadding.x = display.renderingHeight * systemAspectRatio;
                        padding.x = (sizePlusPadding.x - display.renderingWidth) * 0.5f;
                    }
                    else
                    {
                        // Vertical padding
                        sizePlusPadding.y = display.renderingWidth / systemAspectRatio;
                        padding.y = (sizePlusPadding.y - display.renderingHeight) * 0.5f;
                    }
                }

                var sizePlusPositivePadding = sizePlusPadding - padding;

                // If we are not inside of the main display then we must adjust the mouse position so it is scaled by
                // the main display and adjusted for any padding that may have been added due to different aspect ratios.
                if (position.y < -padding.y || position.y > sizePlusPositivePadding.y ||
                     position.x < -padding.x || position.x > sizePlusPositivePadding.x)
                {
                    var adjustedPosition = position;

                    if (!Screen.fullScreen)
                    {
                        // When in windowed mode, the window will be centered with the 0,0 coordinate at the top left, we need to adjust so it is relative to the screen instead.
                        adjustedPosition.x -= (display.renderingWidth - display.systemWidth) * 0.5f;
                        adjustedPosition.y -= (display.renderingHeight - display.systemHeight) * 0.5f;
                    }
                    else
                    {
                        // Scale the mouse position to account for the black bars when in a non-native aspect ratio.
                        adjustedPosition += padding;
                        adjustedPosition.x *= display.systemWidth / sizePlusPadding.x;
                        adjustedPosition.y *= display.systemHeight / sizePlusPadding.y;
                    }

                    // fix for UUM-63551: Use the display index provided to this method.  Display.RelativeMouseAt( ) no longer works starting with 2021 LTS and new input system
                    // as the Pointer position is reported in Window coordinates rather than relative to the primary window as Display.RelativeMouseAt( ) expects.
#if ENABLE_INPUT_SYSTEM && PACKAGE_INPUTSYSTEM && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_ANDROID || UNITY_EMBEDDED_LINUX || UNITY_QNX)
                    var relativePos = new Vector3(adjustedPosition.x, adjustedPosition.y, displayIndex);
#else
                    var relativePos = Display.RelativeMouseAt(adjustedPosition);
#endif

                    // If we are not on the main display then return the adjusted position.
                    if (relativePos.z != 0)
                        return relativePos;
                }

                // We are using the main display.
#if ENABLE_INPUT_SYSTEM && PACKAGE_INPUTSYSTEM && (UNITY_ANDROID || UNITY_EMBEDDED_LINUX || UNITY_QNX)
                // On Android, in all cases, it is a surface associated to a given displayIndex, so we need to use the display index
                return new Vector3(position.x, position.y, displayIndex);
#else
                return new Vector3(position.x, position.y, 0);
#endif
            }
#endif

        // fix for UUM-63551: Use the display index provided to this method.  Display.RelativeMouseAt( ) no longer works starting with 2021 LTS and new input system
        // as the Pointer position is reported in Window coordinates rather than relative to the primary window as Display.RelativeMouseAt( ) expects.
#if ENABLE_INPUT_SYSTEM && PACKAGE_INPUTSYSTEM && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_ANDROID || UNITY_EMBEDDED_LINUX || UNITY_QNX)
            return new Vector3(position.x, position.y, displayIndex);
#else
        return Display.RelativeMouseAt(position);
#endif
    }

    static Vector3 GetRelativeMousePositionForRaycast(PointerEventData eventData)
    {
        // The multiple display system is not supported on all platforms, when it is not supported the returned position
        // will be all zeros so when the returned index is 0 we will default to the event data to be safe.
        Vector3 eventPosition = RelativeMouseAtScaled(eventData.position, eventData.displayIndex);
        if (eventPosition == Vector3.zero)
        {
            eventPosition = eventData.position;
#if UNITY_EDITOR
            eventPosition.z = Display.activeEditorGameViewTarget;
#endif
            // We don't really know in which display the event occurred. We will process the event assuming it occurred in our display.
        }

        // We support multiple display on some platforms. When supported:
        //  - InputSystem will set eventData.displayIndex
        //  - Old Input System will set eventPosition.z
        //
        // If the event is on the main display, both displayIndex and eventPosition.z
        // will be 0 so in that case we can leave the eventPosition untouched (see UUM-47650).
#if ENABLE_INPUT_SYSTEM && PACKAGE_INPUTSYSTEM
            if (eventData.displayIndex > 0)
                eventPosition.z = eventData.displayIndex;
#endif

        return eventPosition;
    }

    static bool IsASecondaryDisplayIndex(int displayIndex)
    {
        return displayIndex >= 0 && displayIndex < Display.displays.Length && displayIndex != 0;
    }

    /// <summary>
    /// Perform the raycast against the list of graphics associated with the Canvas.
    /// </summary>
    /// <param name="eventData">Current event data</param>
    /// <param name="resultAppendList">List of hit objects to append new results to.</param>
    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
    {
        if (canvas == null)
            return;

        var canvasGraphics = GraphicRegistry.GetRaycastableGraphicsForCanvas(canvas);
        if (canvasGraphics == null || canvasGraphics.Count == 0)
            return;

        int displayIndex;
        var currentEventCamera = eventCamera; // Property can call Camera.main, so cache the reference

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay || currentEventCamera == null)
            displayIndex = canvas.targetDisplay;
        else
            displayIndex = currentEventCamera.targetDisplay;

        Vector3 eventPosition = GetRelativeMousePositionForRaycast(eventData);

        // Discard events that are not part of this display so the user does not interact with multiple displays at once.
        if ((int)eventPosition.z != displayIndex)
            return;

        // Convert to view space
        
        // Multiple display support only when not the main display. For display 0 the reported
        // resolution is always the desktops resolution since its part of the display API,
        // so we use the standard none multiple display method. (case 741751)
        float w = Screen.width;
        float h = Screen.height;
        if (IsASecondaryDisplayIndex(displayIndex))
        {
#if UNITY_ANDROID
                    // Changed to be coherent for Android which passes display-relative rendering coordinates
                    w = Display.displays[displayIndex].renderingWidth;
                    h = Display.displays[displayIndex].renderingHeight;
#else
            w = Display.displays[displayIndex].systemWidth;
            h = Display.displays[displayIndex].systemHeight;
#endif
        }

        Vector2 pos = new Vector2(eventPosition.x / w, eventPosition.y / h);

        Vector3 originalEventPosition = eventPosition;
        if (currentEventCamera != null)
        {
            eventPosition = currentEventCamera.ViewportToScreenPoint(pos);
        }

        // If it's outside the camera's viewport, do nothing
        if (pos.x < 0f || pos.x > 1f || pos.y < 0f || pos.y > 1f)
            return;

        float hitDistance = float.MaxValue;

        Ray ray = new Ray();

        if (currentEventCamera != null)
            ray = currentEventCamera.ScreenPointToRay(eventPosition);

        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && blockingObjects != BlockingObjects.None)
        {
            float distanceToClipPlane = 100.0f;

            if (currentEventCamera != null)
            {
                float projectionDirection = ray.direction.z;
                distanceToClipPlane = Mathf.Approximately(0.0f, projectionDirection)
                    ? Mathf.Infinity
                    : Mathf.Abs((currentEventCamera.farClipPlane - currentEventCamera.nearClipPlane) /
                                projectionDirection);
            }
#if PACKAGE_PHYSICS
                if (blockingObjects == BlockingObjects.ThreeD || blockingObjects == BlockingObjects.All)
                {
                    if (ReflectionMethodsCache.Singleton.raycast3D != null)
                    {
                        RaycastHit hit;
                        if (ReflectionMethodsCache.Singleton.raycast3D(ray, out hit, distanceToClipPlane, (int)m_BlockingMask))
                        {
                            hitDistance = hit.distance;
                        }
                    }
                }
#endif
#if PACKAGE_PHYSICS2D
                if (blockingObjects == BlockingObjects.TwoD || blockingObjects == BlockingObjects.All)
                {
                    if (ReflectionMethodsCache.Singleton.raycast2D != null)
                    {
                        var hits =
 ReflectionMethodsCache.Singleton.getRayIntersectionAll(ray, distanceToClipPlane, (int)m_BlockingMask);
                        if (hits.Length > 0)
                            hitDistance = hits[0].distance;
                    }
                }
#endif
        }

        m_raycastResults.Clear();

        Raycast(canvas, currentEventCamera, eventPosition, canvasGraphics, m_raycastResults);

        int totalCount = m_raycastResults.Count;
        for (var index = 0; index < totalCount; index++)
        {
            var go = m_raycastResults[index].gameObject;
            bool appendGraphic = true;

            if (ignoreReversedGraphics)
            {
                if (currentEventCamera == null)
                {
                    // If we dont have a camera we know that we should always be facing forward
                    var dir = go.transform.rotation * Vector3.forward;
                    appendGraphic = Vector3.Dot(Vector3.forward, dir) > 0;
                }
                else
                {
                    // If we have a camera compare the direction against the cameras forward.
                    var cameraForward = currentEventCamera.transform.rotation * Vector3.forward *
                                        currentEventCamera.nearClipPlane;
                    appendGraphic =
                        Vector3.Dot(go.transform.position - currentEventCamera.transform.position - cameraForward,
                            go.transform.forward) >= 0;
                }
            }

            if (appendGraphic)
            {
                float distance = 0;
                Transform trans = go.transform;
                Vector3 transForward = trans.forward;

                if (currentEventCamera == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    distance = 0;
                else
                {
                    // http://geomalgorithms.com/a06-_intersect-2.html
                    distance = (Vector3.Dot(transForward, trans.position - ray.origin) /
                                Vector3.Dot(transForward, ray.direction));

                    // Check to see if the go is behind the camera.
                    if (distance < 0)
                        continue;
                }

                if (distance >= hitDistance)
                    continue;

                var castResult = new RaycastResult
                {
                    gameObject = go,
                    module = this,
                    distance = distance,
                    screenPosition = originalEventPosition,
                    displayIndex = displayIndex,
                    index = resultAppendList.Count,
                    depth = m_raycastResults[index].depth,
                    sortingLayer = canvas.sortingLayerID,
                    sortingOrder = canvas.sortingOrder,
                    worldPosition = ray.origin + ray.direction * distance,
                    worldNormal = -transForward
                };
                resultAppendList.Add(castResult);
            }
        }
    }

    /// <summary>
    /// Perform a raycast into the screen and collect all graphics underneath it.
    /// </summary>
    [NonSerialized] static readonly List<Graphic> s_sortedGraphics = new List<Graphic>();

    private static void Raycast(Canvas canvas, Camera eventCamera, Vector2 pointerPosition,
        IList<Graphic> foundGraphics, List<Graphic> results)
    {
        // Necessary for the event system
        int totalCount = foundGraphics.Count;
        for (int i = 0; i < totalCount; ++i)
        {
            Graphic graphic = foundGraphics[i];

            // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
            if (!graphic.raycastTarget || graphic.canvasRenderer.cull || graphic.depth == -1)
                continue;

            if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, pointerPosition, eventCamera,
                    graphic.raycastPadding))
                continue;

            if (eventCamera != null && eventCamera.WorldToScreenPoint(graphic.rectTransform.position).z >
                eventCamera.farClipPlane)
                continue;
            
            if (graphic.Raycast(pointerPosition, eventCamera))
            {
                s_sortedGraphics.Add(graphic);
            }
        }

        s_sortedGraphics.Sort((g1, g2) => g2.depth.CompareTo(g1.depth));
        totalCount = s_sortedGraphics.Count;
        for (int i = 0; i < totalCount; ++i)
            results.Add(s_sortedGraphics[i]);

        s_sortedGraphics.Clear();
    }
}