﻿using UnityEngine;

using SpaceNavigatorDriver;

namespace CurtinUniversity.MolecularDynamics.Visualization {

    public class MoleculeInputController : MonoBehaviour {

        private float mouseMoveSensitivity = 4;
        private float mouseRotateSensitivity = 100;

        private bool spaceNavigatorInputEnabled;

        private Camera sceneCamera;
        private bool warned = false;

        private void Awake() {
            spaceNavigatorInputEnabled = false;
        }

        private void Start() {
            this.sceneCamera = SceneCamera.Instance.GetCamera();
        }

        private void Update() {

            if (!Input.GetMouseButton(1)) {
                handleMouseInput();
            }

            if (spaceNavigatorInputEnabled) {
                handleSpaceNavigator();
            }
        }

        public void EnableSpaceNavigatorInput(bool enable) {
            spaceNavigatorInputEnabled = enable;
        }

        private void handleMouseInput() {

            if(sceneCamera == null) {
                if(!warned) {
                    Debug.Log("Scene camera is null in molecule input. Cancelling mouse movement");
                    warned = true;
                }
                return;
            }

            float horizontalAxis = Input.GetAxis("Mouse X");
            float verticalAxis = Input.GetAxis("Mouse Y");

            // molecule movement in relation to camera
            if (InputManager.Instance.ShiftPressed) {

                Vector3 forward = sceneCamera.transform.forward;
                Vector3 right = sceneCamera.transform.right;
                Vector3 up = sceneCamera.transform.up;

                forward.y = 0f;
                right.y = 0f;
                forward.Normalize();
                right.Normalize();

                Vector3 moveDirection;
                if (InputManager.Instance.ControlPressed) {
                    moveDirection = forward * verticalAxis + right * horizontalAxis;
                }
                else {
                    moveDirection = up * verticalAxis + right * horizontalAxis;
                }

                transform.Translate(moveDirection * mouseMoveSensitivity * Time.deltaTime, Space.World);
            }

            // molecule rotation around camera forward and horizontal axis 
            if (InputManager.Instance.ControlPressed && !InputManager.Instance.ShiftPressed) {

                transform.RotateAround(transform.position, sceneCamera.transform.forward, horizontalAxis * Time.deltaTime * mouseRotateSensitivity);
                transform.RotateAround(transform.position, sceneCamera.transform.right, verticalAxis * Time.deltaTime * mouseRotateSensitivity);
            }

            // molecule rotation around world vertical axis rotation
            if (InputManager.Instance.AltPressed && !InputManager.Instance.ShiftPressed) {
                transform.RotateAround(transform.position, Vector3.up, horizontalAxis * Time.deltaTime * mouseRotateSensitivity * -1);
            }
        }

        private void handleSpaceNavigator() {

            if (InputManager.Instance.ShiftPressed) {

                Vector3 translation = SpaceNavigator.Translation * 0.5f;
                transform.Translate(translation, Space.World);
            }

            if(InputManager.Instance.ControlPressed) {

                Vector3 rotation = SpaceNavigator.Rotation.eulerAngles;
                transform.Rotate(rotation, Space.World);
            }
        }
    }
}
