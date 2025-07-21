using UnityEngine;

[CreateAssetMenu(fileName = "NewLiquidData", menuName = "ScriptableObjects/Liquid Data")]
public class LiquidData : ScriptableObject
{
    public Color LiquidColor;
    public Color SurfaceColor;
    public Color FresnelColor;

}
