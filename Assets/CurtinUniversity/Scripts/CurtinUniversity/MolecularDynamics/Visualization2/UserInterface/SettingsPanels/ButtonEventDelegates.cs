﻿using UnityEngine;

/// All the delegates are listed here rather than their associated classes because 
/// Unity doesn't seem to like delegate declarations in front of component classes 
/// when using namespaces

namespace CurtinUniversity.MolecularDynamics.VisualizationP3 {

    public delegate void OpenResidueDisplayOptionsDelegate(string residueName);
    public delegate void SaveResidueButtonOptionsDelegate(ResidueDisplayOptions options, bool updateButton, bool updateModel = true);
    public delegate void SetButtonTextDelegate(string buttonName);
    public delegate void SetCustomColourButtonColour(Color color);
    public delegate void SetElementDelegate(string elementName, bool enabled);
    public delegate void SetParentDirectoryDelegate();
}
