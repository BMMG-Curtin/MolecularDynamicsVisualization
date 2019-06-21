﻿
using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace CurtinUniversity.MolecularDynamics.VisualizationP3 {

    public delegate void OnMoleculeSettingsPanelListItemClick(int moleculeID);
    public delegate void OnMoleculeSettingsPanelListItemDoubleClick(int moleculeID);

    public class MoleculeSettingsPanelListItem : MonoBehaviour {

        [SerializeField]
        private TextMeshProUGUI moleculeIDText;

        [SerializeField]
        private TextMeshProUGUI nameText;

        [SerializeField]
        private Image listItemBackground;

        [SerializeField]
        private Color32 normalColor;

        [SerializeField]
        private Color32 highlightedColor;

        private int moleculeID;
        private int displayID;
        private string displayName;

        private OnMoleculeSettingsPanelListItemClick onClick;
        private OnMoleculeSettingsPanelListItemDoubleClick onDoubleClick;

        private float lastClickTime = 0;
        private float doubleClickTimeout = 0.5f;

        private bool itemHighlighted = false;

        public void Initialise(int moleculeID, string name, OnMoleculeSettingsPanelListItemClick onClick, OnMoleculeSettingsPanelListItemDoubleClick onDoubleClick) {

            this.moleculeID = moleculeID;
            this.displayName = name;
            this.onClick = onClick;
            this.onDoubleClick = onDoubleClick;

            moleculeIDText.text = "";
            nameText.text = name;

            SetHighlighted(false);
        }

        public int DisplayID {

            get {
                return displayID;
            }

            set {
                displayID = value;
                moleculeIDText.text = displayID.ToString();
            }
        }

        public void OnClick() {

            if (onDoubleClick != null) {

                if (Time.time <= lastClickTime + doubleClickTimeout) {

                    onDoubleClick(moleculeID);
                    lastClickTime = Time.time;
                    return;
                }

                lastClickTime = Time.time;
            }


            if (onClick != null) {
                onClick(moleculeID);
            }
        }

        public void SetHighlighted(bool setHighlighted) {

            if(setHighlighted) {
                listItemBackground.color = highlightedColor;
            }
            else {
                listItemBackground.color = normalColor;
            }
        }
    }
}