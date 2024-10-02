using UnityEngine;
using System;
using System.Collections.Generic;
using GameKit.Dependencies.Utilities.Types;

[Serializable]
public class GamePlayerSettings
{
    [Header("View")]
    public float XSensitivity;
    public float YSensitivity;
    public bool XInverted;
    public bool YInverted;
    public float lookXLimit = 45.0f;

    [Header("Movement")]
    public float forwardSpeed;
    public float backwardSpeed;
    public float strafeSpeed;

}
