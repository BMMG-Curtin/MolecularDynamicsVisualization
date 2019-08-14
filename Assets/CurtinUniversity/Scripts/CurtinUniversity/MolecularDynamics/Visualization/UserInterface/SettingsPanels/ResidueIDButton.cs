﻿using UnityEngine;
using UnityEngine.UI;

namespace CurtinUniversity.MolecularDynamics.Visualization {

    public class ResidueIDButton : MonoBehaviour {

        [SerializeField]
        private Text buttonIDText;

        public Color32 EnabledColour;
        public Color32 HighlightedColour;
        public Color32 DisabledColour;

        private ColorBlock buttonColours;
        private ResidueDisplayOptions residueOptions;

        SaveResidueButtonOptionsDelegate SaveOptionsCallback;
        OpenResidueDisplayOptionsDelegate OpenDisplayOptionsCallback;

        void Start() {
            buttonColours = GetComponent<Button>().colors;
        }

        public void Initialise(ResidueDisplayOptions options, SaveResidueButtonOptionsDelegate saveOptionsCallback, OpenResidueDisplayOptionsDelegate openDisplayCallback) {

            SaveOptionsCallback = saveOptionsCallback;
            OpenDisplayOptionsCallback = openDisplayCallback;

            UpdateResidueOptions(options);
        }

        public void UpdateResidueOptions(ResidueDisplayOptions options) {

            this.residueOptions = options;

            buttonIDText.text = options.ResidueID.ToString();
            updateButtonColors();
        }

        public void ResidueClick() {

            if (InputManager.Instance.ShiftPressed) {

                OpenDisplayOptionsCallback?.Invoke(residueOptions.ResidueID);
            }
            else {

                if (SaveOptionsCallback != null) {

                    residueOptions.Enabled = !residueOptions.Enabled;
                    updateButtonColors();

                    SaveOptionsCallback(residueOptions, false, true);
                }
            }
        }

        private void updateButtonColors() {

            if (residueOptions.Enabled) {
                if (residueOptions.IsDefault()) {
                    setButtonColours(EnabledColour);
                }
                else {
                    setButtonColours(HighlightedColour);
                }
            }
            else {
                setButtonColours(DisabledColour);
            }
        }

        private void setButtonColours(Color32 color) {

            buttonColours.normalColor = color;
            buttonColours.highlightedColor = color;
            buttonColours.pressedColor = color;
            buttonColours.colorMultiplier = 1f;
            GetComponent<Button>().colors = buttonColours;
        }
    }
}