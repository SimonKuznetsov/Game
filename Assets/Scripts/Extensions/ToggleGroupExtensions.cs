using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleGroupExtensions : ToggleGroup
{
    public IReadOnlyList<Toggle> Toggles => m_Toggles;
}
