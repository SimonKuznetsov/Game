using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static CharacterInputController;

public class TouchDetector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool _isDrag;

    public bool IsDrag
    {
        get
        {
            if (PhotonNetwork.IsConnected == false)
                return true;

            return _isDrag;
        }
    }

    private void Update()
    {
        if (Input.touchCount == 1)
        {
            if (TrackManager.instance.characterController.m_IsSwiping)
            {
                Vector2 diff = Input.GetTouch(0).position - TrackManager.instance.characterController.m_StartingTouch;

                // Put difference in Screen ratio, but using only width, so the ratio is the same on both
                // axes (otherwise we would have to swipe more vertically...)
                diff = new Vector2(diff.x / Screen.width, diff.y / (Screen.width / 2));

                Debug.Log(diff);

                if (diff.magnitude > 0.01f) //we set the swip distance to trigger movement to 1% of the screen width
                {
                    if (Mathf.Abs(diff.y) > Mathf.Abs(diff.x))
                    {
                        if (TrackManager.instance.characterController.TutorialMoveCheck(2) && diff.y < 0 && IsDrag)
                        {
                            TrackManager.instance.characterController.character.ExecuteState(State.Slide);
                            TrackManager.instance.characterController.Slide();
                        }
                        else if (TrackManager.instance.characterController.TutorialMoveCheck(1) && IsDrag)
                        {
                            TrackManager.instance.characterController.character.ExecuteState(State.Jump);
                            TrackManager.instance.characterController.Jump();
                        }
                    }
                    else if (TrackManager.instance.characterController.TutorialMoveCheck(0) && PhotonNetwork.IsConnected == false)
                    {
                        if (diff.x < 0)
                        {
                            TrackManager.instance.characterController.character.ExecuteState(State.Left);
                            TrackManager.instance.characterController.ChangeLane(-1);
                        }
                        else
                        {
                            TrackManager.instance.characterController.character.ExecuteState(State.Right);
                            TrackManager.instance.characterController.ChangeLane(1);
                        }
                    }

                    TrackManager.instance.characterController.m_IsSwiping = false;
                }
            }

            // Input check is AFTER the swip test, that way if TouchPhase.Ended happen a single frame after the Began Phase
            // a swipe can still be registered (otherwise, m_IsSwiping will be set to false and the test wouldn't happen for that began-Ended pair)
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                TrackManager.instance.characterController.m_StartingTouch = Input.GetTouch(0).position;
                TrackManager.instance.characterController.m_IsSwiping = true;
            }
            else if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                TrackManager.instance.characterController.m_IsSwiping = false;
            }
        }
        else if (Input.touchCount == 2)
        {
            if (TrackManager.instance.characterController.m_IsSwiping)
            {
                Vector2 diff = Input.GetTouch(1).position - TrackManager.instance.characterController.m_StartingTouch;

                // Put difference in Screen ratio, but using only width, so the ratio is the same on both
                // axes (otherwise we would have to swipe more vertically...)
                diff = new Vector2(diff.x / Screen.width, diff.y / Screen.width);

                if (diff.magnitude > 0.01f) //we set the swip distance to trigger movement to 1% of the screen width
                {
                    if (Mathf.Abs(diff.y) > Mathf.Abs(diff.x))
                    {
                        if (PhotonNetwork.IsConnected == false)
                        {
                            if (TrackManager.instance.characterController.TutorialMoveCheck(2) && diff.y < 0)
                            {
                                TrackManager.instance.characterController.character.ExecuteState(State.Slide);
                                TrackManager.instance.characterController.Slide();
                            }
                            else if (TrackManager.instance.characterController.TutorialMoveCheck(1))
                            {
                                TrackManager.instance.characterController.character.ExecuteState(State.Jump);
                                TrackManager.instance.characterController.Jump();
                            }
                        }
                        else
                        {
                            if (TrackManager.instance.characterController.TutorialMoveCheck(2) && diff.y < 0 && IsDrag)
                            {
                                TrackManager.instance.characterController.character.ExecuteState(State.Slide);
                                TrackManager.instance.characterController.Slide();
                            }
                            else if (TrackManager.instance.characterController.TutorialMoveCheck(1) && IsDrag)
                            {
                                TrackManager.instance.characterController.character.ExecuteState(State.Jump);
                                TrackManager.instance.characterController.Jump();
                            }
                        }
                    }
                    else if (TrackManager.instance.characterController.TutorialMoveCheck(0) && PhotonNetwork.IsConnected == false)
                    {
                        if (diff.x < 0)
                        {
                            TrackManager.instance.characterController.character.ExecuteState(State.Left);
                            TrackManager.instance.characterController.ChangeLane(-1);
                        }
                        else
                        {
                            TrackManager.instance.characterController.character.ExecuteState(State.Right);
                            TrackManager.instance.characterController.ChangeLane(1);
                        }
                    }

                    TrackManager.instance.characterController.m_IsSwiping = false;
                }
            }

            // Input check is AFTER the swip test, that way if TouchPhase.Ended happen a single frame after the Began Phase
            // a swipe can still be registered (otherwise, m_IsSwiping will be set to false and the test wouldn't happen for that began-Ended pair)
            if (Input.GetTouch(1).phase == TouchPhase.Began)
            {
                TrackManager.instance.characterController.m_StartingTouch = Input.GetTouch(1).position;
                TrackManager.instance.characterController.m_IsSwiping = true;
            }
            else if (Input.GetTouch(1).phase == TouchPhase.Ended)
            {
                TrackManager.instance.characterController.m_IsSwiping = false;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isDrag = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
		_isDrag = false;
	}
}
